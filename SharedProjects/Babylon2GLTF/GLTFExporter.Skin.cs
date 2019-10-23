using BabylonExport.Entities;
using GLTFExport.Entities;
using GLTFExport.Tools;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Babylon2GLTF
{
    partial class GLTFExporter
    {
        // Skeletons, aka group of nodes, are re-used when exporting same babylon skeleton
        // Only the inverseBindMatrices change, as it is linked to the mesh of the gltf node the skin is applied to
        // This dictionary is reset everytime a scene is exported
        private Dictionary<BabylonSkeleton, BabylonSkeletonExportData> alreadyExportedSkeletons = new Dictionary<BabylonSkeleton, BabylonSkeletonExportData>();

        private GLTFSkin ExportSkin(BabylonSkeleton babylonSkeleton, GLTF gltf, GLTFNode gltfNode, GLTFMesh gltfMesh)
        {
            logger.RaiseMessage("GLTFExporter.Skin | Export skin of node '" + gltfNode.name + "' based on skeleton '" + babylonSkeleton.name + "'", 2);

            // Retreive gltf skeleton data if babylon skeleton has already been exported
            if (!alreadyExportedSkeletons.ContainsKey(babylonSkeleton))
            {
                alreadyExportedSkeletons.Add(babylonSkeleton, new BabylonSkeletonExportData());

                // Switch coordinate system at object level
                foreach (var babylonBone in babylonSkeleton.bones)
                {
                    var boneLocalMatrix = new BabylonMatrix();
                    boneLocalMatrix.m = babylonBone.matrix;

                    var translationBabylon = new BabylonVector3();
                    var rotationQuatBabylon = new BabylonQuaternion();
                    var scale = new BabylonVector3();
                    boneLocalMatrix.decompose(scale, rotationQuatBabylon, translationBabylon);
                    translationBabylon *= exportParameters.scaleFactor;
                    translationBabylon.Z *= -1;
                    rotationQuatBabylon.X *= -1;
                    rotationQuatBabylon.Y *= -1;
                    boneLocalMatrix = BabylonMatrix.Compose(scale, rotationQuatBabylon, translationBabylon);

                    babylonBone.matrix = boneLocalMatrix.m;
                }
            }
            var babylonSkeletonExportData = alreadyExportedSkeletons[babylonSkeleton];

            // Skin

            // if this mesh is sharing a skin with another mesh, use the already exported skin
            var sharedSkinnedMeshesByOriginalPair = sharedSkinnedMeshesByOriginal.Where(skinSharingMeshPair => skinSharingMeshPair.Value.Contains(gltfMesh)).Select(kvp => (KeyValuePair<GLTFMesh, List<GLTFMesh>>?) kvp).FirstOrDefault();
            if (sharedSkinnedMeshesByOriginalPair != null)
            {
                logger.RaiseMessage("GLTFExporter.Skin | Sharing skinning information from mesh '" + sharedSkinnedMeshesByOriginalPair.Value.Key.name + "'", 3);
                var skeletonExportData = alreadyExportedSkeletons[babylonSkeleton];
                gltfNode.skin = skeletonExportData.skinIndex;
                return gltf.SkinsList[(int)gltfNode.skin];
            }

            // otherwise create a new GLTFSkin
            var nameSuffix = babylonSkeletonExportData.nb != 0 ? "_" + babylonSkeletonExportData.nb : "";
            GLTFSkin gltfSkin = new GLTFSkin
            {
                name = babylonSkeleton.name + nameSuffix
            };
            gltfSkin.index = gltf.SkinsList.Count;
            gltf.SkinsList.Add(gltfSkin);
            babylonSkeletonExportData.nb++;
            babylonSkeletonExportData.skinIndex = gltfSkin.index;

            var bones = new List<BabylonBone>(babylonSkeleton.bones);

            // Compute and store world matrix of each bone
            var bonesWorldMatrices = new Dictionary<int, BabylonMatrix>();
            foreach (var babylonBone in babylonSkeleton.bones)
            {
                if (!bonesWorldMatrices.ContainsKey(babylonBone.index))
                {
                    var nodePair = nodeToGltfNodeMap.First(pair => pair.Key.id.Equals(babylonBone.id));
                    BabylonMatrix boneWorldMatrix = _getNodeWorldMatrix(nodePair.Value);
                    bonesWorldMatrices.Add(babylonBone.index, boneWorldMatrix);
                }
            }

            // Buffer
            var buffer = GLTFBufferService.Instance.GetBuffer(gltf);

            // Accessor - InverseBindMatrices
            var accessorInverseBindMatrices = GLTFBufferService.Instance.CreateAccessor(
                gltf,
                GLTFBufferService.Instance.GetBufferViewFloatMat4(gltf, buffer),
                "accessorInverseBindMatrices",
                GLTFAccessor.ComponentType.FLOAT,
                GLTFAccessor.TypeEnum.MAT4
            );
            gltfSkin.inverseBindMatrices = accessorInverseBindMatrices.index;

            // World matrix of the node
            var nodeWorldMatrix = _getNodeWorldMatrix(gltfNode);

            var gltfJoints = new List<int>();

            foreach (var babylonBone in babylonSkeleton.bones)
            {
                GLTFNode gltfBoneNode = null;
                if (!babylonSkeletonExportData.nodeByBone.ContainsKey(babylonBone))
                {
                    // Export bone as a new node
                    gltfBoneNode = nodeToGltfNodeMap.FirstOrDefault(pair => pair.Key.id.Equals(babylonBone.id)).Value;//_exportBone(babylonBone, gltf, babylonSkeleton, bones);
                    babylonSkeletonExportData.nodeByBone.Add(babylonBone, gltfBoneNode);
                }
                gltfBoneNode = babylonSkeletonExportData.nodeByBone[babylonBone];

                gltfJoints.Add(gltfBoneNode.index);

                // Set this bone as skeleton if it is a root
                // Meaning of 'skeleton' here is the top root bone
                if (babylonBone.parentBoneIndex == -1)
                {
                    gltfSkin.skeleton = gltfBoneNode.index;
                }

                // Compute inverseBindMatrice for this bone when attached to this node
                var boneLocalMatrix = new BabylonMatrix();
                boneLocalMatrix.m = babylonBone.matrix;
                //printMatrix("boneLocalMatrix[" + babylonBone.name + "]", boneLocalMatrix);

                BabylonMatrix boneWorldMatrix = null;
                if (babylonBone.parentBoneIndex == -1)
                {
                    boneWorldMatrix = boneLocalMatrix;
                }
                else
                {
                    var parentWorldMatrix = bonesWorldMatrices[babylonBone.parentBoneIndex];
                    // Remove scale of parent
                    // This actually enable to take into account the scale of the bones, except for the root one
                    parentWorldMatrix = _removeScale(parentWorldMatrix);

                    boneWorldMatrix = boneLocalMatrix * parentWorldMatrix;
                }
                //printMatrix("boneWorldMatrix[" + babylonBone.name + "]", boneWorldMatrix);

                var inverseBindMatrices = nodeWorldMatrix * BabylonMatrix.Invert(boneWorldMatrix);

                // Populate accessor
                List<float> matrix = new List<float>(inverseBindMatrices.m);
                matrix.ForEach(n => accessorInverseBindMatrices.bytesList.AddRange(BitConverter.GetBytes(n)));
                accessorInverseBindMatrices.count++;
            }
            gltfSkin.joints = gltfJoints.ToArray();

            ExportGLTFExtension(babylonSkeleton, ref gltfNode,gltf);

            return gltfSkin;
        }

        private BabylonNode BoneToNode(BabylonBone babylonBone)
        {
            BabylonNode babylonNode = new BabylonNode();
            babylonNode.id = babylonBone.id;
            babylonNode.parentId = babylonBone.parentNodeId;
            babylonNode.name = babylonBone.name;
            
            
            babylonNode.animations = new[] {babylonBone.animation};

            var tm_babylon = new BabylonMatrix();
            tm_babylon.m = babylonBone.matrix.ToArray();
            var s_babylon = new BabylonVector3();
            var q_babylon = new BabylonQuaternion();
            var t_babylon = new BabylonVector3();
            tm_babylon.decompose(s_babylon, q_babylon, t_babylon);
            babylonNode.position = t_babylon.ToArray();
            babylonNode.rotationQuaternion = q_babylon.ToArray();
            babylonNode.scaling = s_babylon.ToArray();

            return babylonNode;
        }

        private BabylonMatrix _getNodeLocalMatrix(GLTFNode gltfNode)
        {
            return BabylonMatrix.Compose(
                BabylonVector3.FromArray(gltfNode.scale),
                BabylonQuaternion.FromArray(gltfNode.rotation),
                BabylonVector3.FromArray(gltfNode.translation)
            );
        }

        private BabylonMatrix _getNodeWorldMatrix(GLTFNode gltfNode)
        {
            if (gltfNode.parent == null)
            {
                return _getNodeLocalMatrix(gltfNode);
            }
            else
            {
                return _getNodeLocalMatrix(gltfNode) * _getNodeWorldMatrix(gltfNode.parent);
            }
        }

        private BabylonMatrix _getBoneWorldMatrix(BabylonBone babylonBone, List<BabylonBone> bones)
        {
            var boneLocalMatrix = new BabylonMatrix();
            boneLocalMatrix.m = babylonBone.matrix;
            if (babylonBone.parentBoneIndex == -1)
            {
                return boneLocalMatrix;
            }
            else
            {
                var parentBabylonBone = bones.Find(bone => bone.index == babylonBone.parentBoneIndex);
                var parentWorldMatrix = _getBoneWorldMatrix(parentBabylonBone, bones);
                return boneLocalMatrix * parentWorldMatrix;
            }
        }

        // TODO clean up
        private BabylonMatrix _removeScale(BabylonMatrix boneWorldMatrix)
        {
            //var translation = new BabylonVector3();
            //var rotation = new BabylonQuaternion();
            //var scale = new BabylonVector3();
            //boneWorldMatrix.decompose(scale, rotation, translation);
            //scale.X = 1;
            //scale.Y = 1;
            //scale.Z = 1;
            //return BabylonMatrix.Compose(scale, rotation, translation);
            return boneWorldMatrix;
        }

        /// <summary>
        /// Class used to temporary store gltf skeleton, aka group of nodes, binded to a babylon skeleton
        /// </summary>
        private class BabylonSkeletonExportData
        {
            /// <summary>
            /// Number of times the skeleton has been exported
            /// </summary>
            public int nb = 0;

            /// <summary>
            /// Which skin index is used for this skeleton
            /// </summary>
            public int skinIndex = -1;

            /// <summary>
            /// Each glTF bone is binded to a babylon bone
            /// </summary>
            public Dictionary<BabylonBone, GLTFNode> nodeByBone = new Dictionary<BabylonBone, GLTFNode>();
        }

        private void printMatrix(string name, BabylonMatrix matrix)
        {
            // Decompose matrix into TRS
            var translation = new BabylonVector3();
            var rotationQuat = new BabylonQuaternion();
            var scale = new BabylonVector3();
            matrix.decompose(scale, rotationQuat, translation);
            var rotation = rotationQuat.toEulerAngles();
            rotation *= (float)(180 / Math.PI);

            var lvl = 3;
            logger.RaiseWarning(name + ".translation=[" + translation.X + ", " + translation.Y + ", " + translation.Z + "]", lvl);
            logger.RaiseWarning(name + ".rotation=[" + rotation.X + ", " + rotation.Y + ", " + rotation.Z + "]", lvl);
            logger.RaiseWarning(name + ".scale=[" + scale.X + ", " + scale.Y + ", " + scale.Z + "]", lvl);
        }
    }
}