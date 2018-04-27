using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;
using BabylonExport.Entities;
using MayaBabylon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        private List<MFnSkinCluster> skins = new List<MFnSkinCluster>();
        private Dictionary<MObject, int> indexByNode = new Dictionary<MObject, int>();

        private void ExportSkin(MFnSkinCluster skin, BabylonScene babylonScene)
        {
            int skinIndex = skins.IndexOf(skin);
            BabylonSkeleton babylonSkeleton = new BabylonSkeleton {
                id = skinIndex,
                name = "skeleton #" + skinIndex,
                //BabylonBone[] bones 
                needInitialSkinMatrix = true
            };
            List<BabylonBone> bones = new List<BabylonBone>();

            RaiseMessage(babylonSkeleton.name, 1);

            // get the root node
            MObject rootNode = getRootNode(skin);
            
            // Travel the DAG
            MItDag dagIterator = new MItDag();
            dagIterator.reset(rootNode);
            for (int index = 0 ; !dagIterator.isDone ; dagIterator.next())
            {
                // current node
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                MObject currentNode = mDagPath.node;
                MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                indexByNode.Add(currentNode, index);

                // parent node
                MFnDagNode mFnDagNode = new MFnDagNode(currentNode);
                MObject firstParent = mFnDagNode.parent(0);
                int parentIndex = (index == 0) ? -1 : indexByNode[firstParent];

                // create the bone
                BabylonBone bone = new BabylonBone()
                {
                    name = currentNodeTransform.name,
                    index = index,
                    parentBoneIndex = parentIndex,
                    matrix = currentNodeTransform.transformationMatrix.toArray(),
                    //animation = GetAnimationsFrameByFrameMatrix(currentNodeTransform)
                };
                bones.Add(bone);


                RaiseMessage($"Bone: name={bone.name}, index={bone.index}, parentBoneIndex={bone.parentBoneIndex}, matrix={string.Join(" ",bone.matrix)}", (int)dagIterator.depth + 1);
            }
            babylonSkeleton.bones = bones.ToArray();
            babylonScene.SkeletonsList.Add(babylonSkeleton);

            RaiseMessage($"{indexByNode.Count} bone(s) exported", 1);

            // clear the dictionary
            indexByNode.Clear();
        }


        private MObject getRootNode(MFnSkinCluster skin)
        {
            MObject rootJoint = null;

            MPlugArray connections = new MPlugArray();
            try
            {
                skin.getConnections(connections);
                // Search a joint in the connections
                int connectionsCount = connections.Count;
                int connectionIndex = 0;
                while(connectionIndex < connectionsCount && rootJoint == null)
                {
                    MObject source = connections[connectionIndex].source.node;
                    if (source != null && source.hasFn(MFn.Type.kJoint))
                    {
                        rootJoint = source;
                    }
                    connectionIndex++;
                }

                // Check the joint parent until it's a kWorld
                MFnDagNode mFnDagNode = new MFnDagNode(rootJoint);
                MObject firstParent = mFnDagNode.parent(0);
                MFnDependencyNode node = new MFnDependencyNode(firstParent);
                while(!firstParent.apiType.Equals(MFn.Type.kWorld))
                {
                    rootJoint = firstParent;
                    mFnDagNode = new MFnDagNode(rootJoint);
                    firstParent = mFnDagNode.parent(0);
                    node = new MFnDependencyNode(firstParent);
                }
            }
            catch (Exception e)
            {
            }

            return rootJoint;
        }
    }
}