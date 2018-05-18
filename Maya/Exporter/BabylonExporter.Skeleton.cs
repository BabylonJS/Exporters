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
        private Dictionary<string, Dictionary<string, int>> skinDictionary = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<MFnSkinCluster, MObject> rootBySkin = new Dictionary<MFnSkinCluster, MObject>();

        /// <summary>
        /// Create the BabylonSkeleton from the Maya MFnSkinCluster.
        /// And add it to the BabylonScene.
        /// </summary>
        /// <param name="skin">The maya skin cluster</param>
        /// <param name="babylonScene">The scene to export</param>
        /// <returns></returns>
        private void ExportSkin(MFnSkinCluster skin, BabylonScene babylonScene)
        {
            int logRank = 1;
            int skinIndex = skins.IndexOf(skin);
            string name = "skeleton #" + skinIndex;
            RaiseMessage(name, logRank);

            BabylonSkeleton babylonSkeleton = new BabylonSkeleton {
                id = skinIndex,
                name = name,
                bones = GetBones(skin),
                needInitialSkinMatrix = true
            };
            
            babylonScene.SkeletonsList.Add(babylonSkeleton);
        }

        /// <summary>
        /// Find a bone in the skin cluster connections.
        /// From this bone travel the Dag up to the root node.
        /// </summary>
        /// <param name="skin">The skin cluster</param>
        /// <returns>
        /// The root node of the skeleton.
        /// </returns>
        private MObject GetRootNode(MFnSkinCluster skin)
        {
            MObject rootJoint = null;

            if (rootBySkin.ContainsKey(skin))
            {
                return rootBySkin[skin];
            }

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
                //RaiseError(e.Message, logRank);
                //RaiseError(e.StackTrace, logRank + 1);
            }

            rootBySkin.Add(skin, rootJoint);
            return rootJoint;
        }

        private MStringArray GetBoneFullPathName(MFnSkinCluster skin, MFnTransform transform)
        {
            int logRank = 3;

            // Get the bone names that influence the mesh
            // We need to keep this order as we will use an other mel command to get the weight influence
            MStringArray mayaInfluenceNames = new MStringArray();
            MGlobal.executeCommand($"skinCluster -q -influence {transform.name}", mayaInfluenceNames);

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
            for(int index = 0; index < mayaInfluenceNames.Count; index++)
            {
                string name = mayaInfluenceNames[index];
                int indexFullPathName = boneFullPathNames.FindIndex(fullPathName => fullPathName.EndsWith(name));
                mayaInfluenceNames[index] = boneFullPathNames[indexFullPathName];

                if (index == 0) {
                    rootName = mayaInfluenceNames[index].Split('|')[1];
                    RaiseVerbose($"rootName: {rootName}", logRank + 1);
                }
                RaiseVerbose($"{index}: {name} => {mayaInfluenceNames[index]}", logRank + 1);

                if (! mayaInfluenceNames[index].StartsWith($"|{rootName}|") && ! mayaInfluenceNames[index].Equals($"|{rootName}"))
                {
                    RaiseError($"Bones don't share the same root node. {rootName} != {mayaInfluenceNames[index].Split('|')[1]}", logRank + 1);
                    return null;
                }
            }

            return mayaInfluenceNames;
        }

        private int GetSkeletonIndex(MFnSkinCluster skin)
        {
            // improvement? how can we distinguish skeleton?
            string rootNodeFullPathName = GetIndexByFullPathNameDictionary(skin).ElementAt(0).Key;
            int index = skins.FindIndex(skinToExport => GetIndexByFullPathNameDictionary(skinToExport).ElementAt(0).Key.Equals(rootNodeFullPathName));
            //int index = skins.FindIndex(skinToExport => skinToExport.name.Equals(skin.name));

            if (index == -1)
            {
                skins.Add(skin);
                index = skins.Count - 1;
            }

            return index;
        }

        private int GetMaxInfluence(MFnSkinCluster skin, MFnTransform transform, MFnMesh mesh)
        {
            int maxNumInfluences = 0;
            int numVertices = mesh.numVertices;

            // Get max influence on a vertex
            for (int index = 0; index < numVertices; index++)
            {
                MDoubleArray influenceWeights = new MDoubleArray();
                String command = $"skinPercent -query -value {skin.name} {transform.name}.vtx[{index}]";
                // Get the weight values of all the influences for this vertex
                MGlobal.executeCommand(command, influenceWeights);

                int numInfluences = influenceWeights.Count(weight => weight != 0);

                maxNumInfluences = Math.Max(maxNumInfluences, numInfluences);
            }

            return maxNumInfluences;
        }
        
        private Dictionary<string, int> GetIndexByFullPathNameDictionary(MFnSkinCluster skin)
        {
            if (skinDictionary.ContainsKey(skin.name))
            {
                return skinDictionary[skin.name];
            }

            Dictionary<string, int> indexByFullPathName = new Dictionary<string, int>();
            
            // get the root node
            MObject rootNode = GetRootNode(skin);
            // Travel the DAG
            MItDag dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(rootNode);
            int index = 0;
            while (!dagIterator.isDone)
            {
                // current node
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                MObject currentNode = mDagPath.node;

                try
                {
                    MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                    string currentFullPathName = currentNodeTransform.fullPathName;

                    indexByFullPathName.Add(currentFullPathName, index);
                    index++;
                }
                catch   // When it's not a kTransform or kJoint node. For exemple a kLocator, kNurbsCurve.
                {
                    if (currentNode.apiType == MFn.Type.kNurbsCurve)
                    {
                        RaiseError($"{currentNode.apiType} is not supported. It will not be exported: {mDagPath.fullPathName}", 3);
                        return null;
                    }
                }

                dagIterator.next();
            }

            skinDictionary.Add(skin.name, indexByFullPathName);

            return indexByFullPathName;
        }

        /// <summary>
        /// Convert Maya nodes to BabylonBone
        /// </summary>
        /// <param name="skin">The Maya skin cluster to export</param>
        /// <returns>
        /// An array of the babylon bones that form the BabylonSkeleton
        /// </returns>
        private BabylonBone[] GetBones(MFnSkinCluster skin)
        {
            int logRank = 1;
            int skinIndex = skins.IndexOf(skin);
            List<BabylonBone> bones = new List<BabylonBone>();
            Dictionary<string, int> indexByFullPathName = GetIndexByFullPathNameDictionary(skin);

            // Travel the DAG
            MItDag dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(GetRootNode(skin));                               // start from the root node of the skin
            for (; !dagIterator.isDone; dagIterator.next())
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
                    if (indexByFullPathName[currentFullPathName] > 0)
                    {
                        MFnDagNode mFnDagNode = new MFnDagNode(currentNode);
                        MFnTransform firstParentTransform = new MFnTransform(mFnDagNode.parent(0));
                        parentIndex = indexByFullPathName[firstParentTransform.fullPathName];
                    }

                    // create the bone
                    BabylonBone bone = new BabylonBone()
                    {
                        name = currentFullPathName,
                        index = indexByFullPathName[currentFullPathName],
                        parentBoneIndex = parentIndex,
                        matrix = ConvertMayaToBabylonMatrix(currentNodeTransform.transformationMatrix).m.ToArray(),
                        animation = GetAnimationsFrameByFrameMatrix(currentNodeTransform)
                    };
                    bones.Add(bone);

                    // The RaiseMessage is only here to prevent Maya from freezing...
                    RaiseMessage($"Bone: name={bone.name}, index={bone.index}, parentBoneIndex={bone.parentBoneIndex}, matrix={string.Join(" ", bone.matrix)}", (int)dagIterator.depth + logRank + 1);
                }
                catch(Exception e)
                {
                    //RaiseError(e.Message,logRank);
                    //RaiseError(e.StackTrace, logRank + 1);
                }
            }
            RaiseMessage($"{bones.Count} bone(s) exported", logRank + 1);

            return bones.ToArray();
        }

        /// <summary>
        /// Normalize the value in the dictionary.
        /// </summary>
        /// <param name="intByDouble"></param>
        /// <returns></returns>
        private void Normalize(ref Dictionary<int, double> intByDouble)
        {
            if (intByDouble.Count != 0)
            {
                double totalValue = intByDouble.Values.Sum();
                if (totalValue != 1)
                {
                    for (int index = 0; index < intByDouble.Count; index++)
                    {
                        int indexToNormalize = intByDouble.ElementAt(index).Key;

                        intByDouble[indexToNormalize] /= totalValue;
                    }
                }
            }
        }

        /// <summary>
        /// Order the dictionary by descending values.
        /// </summary>
        /// <param name="intByDouble">Dictionary</param>
        /// <returns></returns>
        private void OrderByDescending(ref Dictionary<int, double> intByDouble)
        {
            if (intByDouble.Count != 0)
            {
                Dictionary<int, double> sorted = new Dictionary<int, double>();
                sorted = intByDouble.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                intByDouble = sorted;
            }
        }

        /// <summary>
        /// Compare two Maya nodes.
        /// </summary>
        /// <param name="mObject1">First Maya node</param>
        /// <param name="mObject2">Seconde Maya node</param>
        /// <returns>
        /// True, if the two Maya nodes are equal.
        /// False, otherwise
        /// </returns>
        private bool Equals(MObject mObject1, MObject mObject2)
        {
            if (mObject1 == mObject2)
            {
                return true;
            }

            if (mObject1 == null || mObject2 == null)
            {
                return false;
            }

            MFnDagNode node1 = new MFnDagNode(mObject1);
            MFnDagNode node2 = new MFnDagNode(mObject2);

            return node1.fullPathName.Equals(node2.fullPathName);
        }

    }
}
