﻿using BabylonExport.Entities;
using GLTFExport.Entities;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        // Skeletons, aka group of nodes, are re-used when exporting same babylon skeleton
        // Only the inverseBindMatrices change, as it is linked to the mesh of the gltf node the skin is applied to
        // This dictionary is reset everytime a scene is exported
        private Dictionary<BabylonSkeleton, BabylonSkeletonExportData> alreadyExportedSkeletons = new Dictionary<BabylonSkeleton, BabylonSkeletonExportData>();

        private GLTFSkin ExportSkin(BabylonSkeleton babylonSkeleton, GLTF gltf, GLTFNode gltfNode)
        {
            RaiseMessage("GLTFExporter.Skin | Export skin of node '" + gltfNode.name + "' based on skeleton '" + babylonSkeleton.name + "'", 2);

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
                    translationBabylon.Z *= -1;
                    rotationQuatBabylon.X *= -1;
                    rotationQuatBabylon.Y *= -1;
                    boneLocalMatrix = BabylonMatrix.Compose(scale, rotationQuatBabylon, translationBabylon);

                    babylonBone.matrix = boneLocalMatrix.m;
                }
            }
            var babylonSkeletonExportData = alreadyExportedSkeletons[babylonSkeleton];

            // Skin
            var nameSuffix = babylonSkeletonExportData.nb != 0 ? "_" + babylonSkeletonExportData.nb : "";
            GLTFSkin gltfSkin = new GLTFSkin
            {
                name = babylonSkeleton.name + nameSuffix
            };
            gltfSkin.index = gltf.SkinsList.Count;
            gltf.SkinsList.Add(gltfSkin);
            babylonSkeletonExportData.nb++;

            var bones = new List<BabylonBone>(babylonSkeleton.bones);

            // Compute and store world matrix of each bone
            var bonesWorldMatrices = new Dictionary<int, BabylonMatrix>();
            foreach (var babylonBone in babylonSkeleton.bones)
            {
                if (!bonesWorldMatrices.ContainsKey(babylonBone.index))
                {
                    BabylonMatrix boneWorldMatrix = _getBoneWorldMatrix(babylonBone, bones);
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
                    gltfBoneNode = _exportBone(babylonBone, gltf, babylonSkeleton, bones);
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

                var inverseBindMatrices = nodeWorldMatrix * BabylonMatrix.Invert(boneWorldMatrix);

                // Populate accessor
                List<float> matrix = new List<float>(inverseBindMatrices.m);
                matrix.ForEach(n => accessorInverseBindMatrices.bytesList.AddRange(BitConverter.GetBytes(n)));
                accessorInverseBindMatrices.count++;
            }
            gltfSkin.joints = gltfJoints.ToArray();

            return gltfSkin;
        }

        private GLTFNode _exportBone(BabylonBone babylonBone, GLTF gltf, BabylonSkeleton babylonSkeleton, List<BabylonBone> bones)
        {
            var nodeNodePair = nodeToGltfNodeMap.FirstOrDefault(pair => pair.Key.id.Equals(babylonBone.id));
            if (nodeNodePair.Key != null)
            {
                return nodeNodePair.Value;
            }

            var boneNodePair = boneToGltfNodeMap.FirstOrDefault(pair => pair.Key.id.Equals(babylonBone.id));
            if (boneNodePair.Key != null)
            {
                return boneNodePair.Value;
            }

            // Node
            var gltfNode = new GLTFNode
            {
                name = GetUniqueNodeName(babylonBone.name)
            };
            gltfNode.index = gltf.NodesList.Count;
            gltf.NodesList.Add(gltfNode);
            boneToGltfNodeMap.Add(babylonBone, gltfNode);

            // Hierarchy
            if (babylonBone.parentBoneIndex >= 0)
            {
                var babylonParentBone = bones.Find(_babylonBone => _babylonBone.index == babylonBone.parentBoneIndex);
                var gltfParentNode = _exportBone(babylonParentBone, gltf, babylonSkeleton, bones);
                RaiseMessage("GLTFExporter.Skin | Add " + babylonBone.name + " as child to " + gltfParentNode.name, 3);
                gltfParentNode.ChildrenList.Add(gltfNode.index);
                gltfNode.parent = gltfParentNode;
            }
            else
            {
                // It's a root node
                // Only root nodes are listed in a gltf scene
                RaiseMessage("GLTFExporter.Skin | Add " + babylonBone.name + " as root node to scene", 3);
                gltf.scenes[0].NodesList.Add(gltfNode.index);
            }

            // Transform
            // Bones transform are exported through translation/rotation/scale (TRS) rather than matrix
            // Because gltf node animation can only target TRS properties, not the matrix one
            // Create matrix from array
            var babylonMatrix = new BabylonMatrix();
            babylonMatrix.m = babylonBone.matrix;
            // Decompose matrix into TRS
            var translationBabylon = new BabylonVector3();
            var rotationQuatBabylon = new BabylonQuaternion();
            var scaleBabylon = new BabylonVector3();
            babylonMatrix.decompose(scaleBabylon, rotationQuatBabylon, translationBabylon);
            // Store TRS values
            gltfNode.translation = translationBabylon.ToArray();
            gltfNode.rotation = rotationQuatBabylon.ToArray();
            gltfNode.scale = scaleBabylon.ToArray();

            return gltfNode;
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
            RaiseWarning(name + ".translation=[" + translation.X + ", " + translation.Y + ", " + translation.Z + "]", lvl);
            RaiseWarning(name + ".rotation=[" + rotation.X + ", " + rotation.Y + ", " + rotation.Z + "]", lvl);
            RaiseWarning(name + ".scale=[" + scale.X + ", " + scale.Y + ", " + scale.Z + "]", lvl);
        }
    }
}
