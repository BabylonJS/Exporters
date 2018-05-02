using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        private List<MFnSkinCluster> skins = new List<MFnSkinCluster>();
        private Dictionary<string, int> indexByNodeName = new Dictionary<string, int>();    // contains the node (joint and parents) name and its index

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
            MItDag dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(rootNode);
            for (; !dagIterator.isDone ; dagIterator.next())
            {
                // current node
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                MObject currentNode = mDagPath.node;
                MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                string currentName = currentNodeTransform.name;
                //indexByNodeName.Add(currentName, index);

                // parent node
                int parentIndex = -1;
                if (indexByNodeName[currentName] > 0)
                {
                    MFnDagNode mFnDagNode = new MFnDagNode(currentNode);
                    MFnTransform firstParentTransform = new MFnTransform(mFnDagNode.parent(0));
                    parentIndex = indexByNodeName[firstParentTransform.name];
                }

                // create the bone
                BabylonBone bone = new BabylonBone()
                {
                    name = currentNodeTransform.name,
                    index = indexByNodeName[currentName],
                    parentBoneIndex = parentIndex,
                    matrix = ConvertMayaToBabylonMatrix(currentNodeTransform.transformationMatrix).m.ToArray(),
                    animation = GetAnimationsFrameByFrameMatrix(currentNodeTransform)
                };
                bones.Add(bone);

                RaiseVerbose($"Bone: name={bone.name}, index={bone.index}, parentBoneIndex={bone.parentBoneIndex}, matrix={string.Join(" ",bone.matrix)}", (int)dagIterator.depth + 1);
            }
            babylonSkeleton.bones = bones.ToArray();
            babylonScene.SkeletonsList.Add(babylonSkeleton);

            RaiseMessage($"{indexByNodeName.Count} bone(s) exported", 1);
        }


        // Init the dictionary
        private void initIndexByNodeNameDictionary(MFnSkinCluster skin)
        {
            // get the root node
            MObject rootNode = getRootNode(skin);
            // Travel the DAG
            MItDag dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(rootNode);
            int index = 0;
            while( !dagIterator.isDone )
            {
                // current node
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                MObject currentNode = mDagPath.node;
                MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                string currentName = currentNodeTransform.name;
                indexByNodeName.Add(currentName, index);

                // increment iter and index
                dagIterator.next();
                index++;
            }

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