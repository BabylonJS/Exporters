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
        private List<MFnSkinCluster> skins = new List<MFnSkinCluster>();                    // contains the skins to export
        private Dictionary<string, int> indexByNodeName = new Dictionary<string, int>();    // contains the node (joint and parents of the current skin) fullPathName and its index
        private List<MObject> skeletalRoots = new List<MObject>();                          // contains the root node of each skeleton (skin)

        private void ExportSkin(MFnSkinCluster skin, BabylonScene babylonScene)
        {
            int logRank = 1;
            int skinIndex = skins.IndexOf(skin);
            BabylonSkeleton babylonSkeleton = new BabylonSkeleton {
                id = skinIndex,
                name = "skeleton #" + skinIndex,
                //BabylonBone[] bones 
                needInitialSkinMatrix = true
            };
            List<BabylonBone> bones = new List<BabylonBone>();

            RaiseMessage(babylonSkeleton.name, logRank);

            // TODO OPTIMIZATION stock the dictionary with the skinCluster
            InitIndexByNodeNameDictionary(skin);

            // Travel the DAG
            MItDag dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(GetRootNode(skin));                               // start from the root node of the skin
            for (; !dagIterator.isDone ; dagIterator.next())
            {
                try
                {
                    // current node
                    MDagPath mDagPath = new MDagPath();
                    dagIterator.getPath(mDagPath);
                    MObject currentNode = mDagPath.node;
                    MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                    string currentFullPathName = currentNodeTransform.fullPathName;

                    // find the parent node to get its index
                    int parentIndex = -1;
                    if (indexByNodeName[currentFullPathName] > 0)
                    {
                        MFnDagNode mFnDagNode = new MFnDagNode(currentNode);
                        MFnTransform firstParentTransform = new MFnTransform(mFnDagNode.parent(0));
                        parentIndex = indexByNodeName[firstParentTransform.fullPathName];
                    }

                    // create the bone
                    BabylonBone bone = new BabylonBone()
                    {
                        name = currentFullPathName,
                        index = indexByNodeName[currentFullPathName],
                        parentBoneIndex = parentIndex,
                        matrix = ConvertMayaToBabylonMatrix(currentNodeTransform.transformationMatrix).m.ToArray(),
                        animation = GetAnimationsFrameByFrameMatrix(currentNodeTransform)
                    };
                    bones.Add(bone);

                    // The RaiseMessage is only here to prevent Maya from freezing...
                    RaiseMessage($"Bone: name={bone.name}, index={bone.index}, parentBoneIndex={bone.parentBoneIndex}, matrix={string.Join(" ", bone.matrix)}", (int)dagIterator.depth + logRank+1);
                }
                catch
                {
                }
            }
            babylonSkeleton.bones = bones.ToArray();
            babylonScene.SkeletonsList.Add(babylonSkeleton);

            RaiseMessage($"{indexByNodeName.Count} bone(s) exported", logRank + 1);
        }


        // Init the dictionary that contains the fullPathName and its index of each bone
        private void InitIndexByNodeNameDictionary(MFnSkinCluster skin)
        {
            // TODO OPTIMIZATION stock the dictionary with the skinCluster
            indexByNodeName.Clear();

            // get the root node
            MObject rootNode = GetRootNode(skin);
            // Travel the DAG
            MItDag dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(rootNode); // TODO think about what to export
            int index = 0;
            while( !dagIterator.isDone && isSkinExportSuccess )
            {
                // current node
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                MObject currentNode = mDagPath.node;

                try
                {
                    MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                    string currentFullPathName = currentNodeTransform.fullPathName;

                    try
                    {
                        // TODO: add only revelant node
                        indexByNodeName.Add(currentFullPathName, index);
                        index++;
                    }
                    catch (ArgumentException e) // Exception raised when adding nodes with same names
                    {
                        isSkinExportSuccess = false;
                        RaiseError($"Two items have the same name: {currentFullPathName} - partial name: {mDagPath.partialPathName}.", 2);
                        // TOTO OPTIMIZATION
                        //RaiseError("Or the same skeleton is used by two different mesh...", 2);
                    }
                }
                catch   // When it's not a kTransform or kJoint node. For exemple a kLocator.
                {
                    RaiseMessage($"{currentNode.apiType} is not supported. It will not be exported: {mDagPath.fullPathName}", System.Drawing.Color.Orange , 2);
                }
                
                // increment iter and index
                dagIterator.next();
            }
        }

        private MObject GetRootNode(MFnSkinCluster skin)
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
            catch
            {
            }

            return rootJoint;
        }

        private void ConvertBoneNameToFullPathName(MFnSkinCluster skin, MStringArray allMayaInfluenceNames)
        {
            int logRank = 3;
            RaiseVerbose($"getBoneFullPathName({skin}, {allMayaInfluenceNames})", logRank);
            List<string> boneFullPathNames = new List<string>();
            MPlugArray connections = new MPlugArray();

            // Get the bones connected to the skin cluster
            skin.getConnections(connections);
            foreach (MPlug connection in connections)
            {
                MObject source = connection.source.node;
                if (source != null && source.hasFn(MFn.Type.kJoint))
                {
                    MFnDagNode node = new MFnDagNode(source);
                    if(! boneFullPathNames.Contains(node.fullPathName))
                    {
                        boneFullPathNames.Add(node.fullPathName);
                    }
                }
            }

            // Change the name to the fullPathName. And check that they all share the same root node.
            string rootName = "";
            for(int index = 0; index < allMayaInfluenceNames.Count; index++)
            {
                string name = allMayaInfluenceNames[index];
                int indexFullPathName = boneFullPathNames.FindIndex(fullPathName => fullPathName.EndsWith(name));
                allMayaInfluenceNames[index] = boneFullPathNames[indexFullPathName];

                if (index == 0) {
                    rootName = allMayaInfluenceNames[index].Split('|')[1];
                    RaiseVerbose($"rootName: {rootName}", logRank + 1);
                }
                RaiseVerbose($"{index}: {name} => {allMayaInfluenceNames[index]}", logRank + 1);

                if (! allMayaInfluenceNames[index].StartsWith($"|{rootName}|") && ! allMayaInfluenceNames[index].Equals($"|{rootName}"))
                {
                    RaiseError($"Bones don't share the same root node. {rootName} != |{allMayaInfluenceNames[index].Split('|')[1]}|", logRank + 1);
                    isSkinExportSuccess = false;
                    return;
                }
            }
        }

        private int GetSkeletonIndex(MFnSkinCluster skin)
        {
            MObject rootNode = GetRootNode(skin);

            int index = skeletalRoots.FindIndex(root => {
                MFnDagNode rootDag = new MFnDagNode(root);
                MFnDagNode rootNodeDag = new MFnDagNode(rootNode);
                return rootDag.fullPathName.Equals(rootNodeDag.fullPathName);
            });

            if(index == -1)
            {
                skeletalRoots.Add(rootNode);
                skins.Add(skin);
                index = skeletalRoots.Count - 1;
            }

            return index;
        }
    }
}
