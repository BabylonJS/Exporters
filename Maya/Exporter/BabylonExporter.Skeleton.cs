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
        private Dictionary<MFnSkinCluster, List<MObject>> influentNodesBySkin = new Dictionary<MFnSkinCluster, List<MObject>>();
        private Dictionary<MFnSkinCluster, List<MObject>> revelantNodesBySkin = new Dictionary<MFnSkinCluster, List<MObject>>();
        private IDictionary<int, double> frameBySkeletonID = new Dictionary<int, double>(); // store the id of the skeleton and the frame to used for the bone export

        // For the progress bar
        private float progressSkin;
        private float progressSkinStep;
        private float progressBoneStep;

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
            int skinIndex = GetSkeletonIndex(skin);
            string name = "skeleton #" + skinIndex;
            RaiseMessage(name, logRank);

            BabylonSkeleton babylonSkeleton = new BabylonSkeleton {
                id = skinIndex,
                name = name,
                bones = ExportBones(skin),
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

            // Get a joint of the skin
            rootJoint = GetInfluentNodes(skin)[0];

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

            rootBySkin.Add(skin, rootJoint);
            return rootJoint;
        }

        /// <summary>
        /// The MEL command only return name and not fullPathName. Unfortunatly you need the full path name to differentiate two nodes with the same names.
        /// 
        /// </summary>
        /// <param name="skin">The skin cluster</param>
        /// <param name="transform">The transform above the mesh</param>
        /// <returns>
        /// The array with the node full path names.
        /// </returns>
        private MStringArray GetBoneFullPathName(MFnSkinCluster skin, MFnTransform transform)
        {
            int logRank = 3;

            // Get the bone names that influence the mesh
            // We need to keep this order as we will use an other mel command to get the weight influence
            MStringArray mayaInfluenceNames = new MStringArray();
            MGlobal.executeCommand($"skinCluster -q -influence {transform.name}", mayaInfluenceNames);

            List<string> boneFullPathNames = new List<string>();
            MPlugArray connections = new MPlugArray();

            // Get the bone full path names of the skin cluster
            foreach(MObject node in GetInfluentNodes(skin))
            {
                boneFullPathNames.Add((new MFnDagNode(node)).fullPathName);
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
                    RaiseError($"Bones don't share the same root node. {rootName} != {mayaInfluenceNames[index].Split('|')[1]}", logRank);
                    return null;
                }
            }

            return mayaInfluenceNames;
        }

        /// <summary>
        /// If the skin is not in the list of those that will be export, it add the skin into the list .
        /// Then it returns its index.
        /// </summary>
        /// <param name="skin">the skin to export</param>
        /// <returns>The index of the skin in the list of those that will be exported</returns>
        private int GetSkeletonIndex(MFnSkinCluster skin)
        {
            int index = -1;
            List<MObject> revelantNodes = GetRevelantNodes(skin);

            // Compare the revelant nodes with the list of those exported
            int currentIndex = 0;

            while(currentIndex < skins.Count && index == -1)
            {
                List<MObject> currentRevelantNodes = GetRevelantNodes(skins[currentIndex]);

                if (revelantNodes.Count == currentRevelantNodes.Count
                    && revelantNodes.All(node1 => currentRevelantNodes.Count(node2 => Equals(node1,node2)) == 1))
                {
                    index = currentIndex;
                }

                currentIndex++;
            }

            if (index == -1)
            {
                skins.Add(skin);
                index = skins.Count - 1;
            }

            return index;
        }

        /// <summary>
        /// Return the max influence of a skin cluster on a mesh.
        /// </summary>
        /// <param name="skin">the skin</param>
        /// <param name="transform">the transform above the mesh</param>
        /// <param name="mesh">the mesh</param>
        /// <returns>The max influence</returns>
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

        /// <summary>
        /// Init the list influentNodesBySkin of a skin cluster.
        /// By find the kjoint that are connected to the skin.
        /// </summary>
        /// <param name="skin">the skin cluster</param>
        /// <returns>
        /// The list of skin kjoint
        /// </returns>
        private List<MObject> GetInfluentNodes(MFnSkinCluster skin)
        {
            if (influentNodesBySkin.ContainsKey(skin))
            {
                return influentNodesBySkin[skin];
            }

            List<MObject> influentNodes = new List<MObject>();

            // Get all influenting nodes
            MPlugArray connections = new MPlugArray();
            skin.getConnections(connections);
            foreach (MPlug connection in connections)
            {
                MObject source = connection.source.node;
                if (source != null && source.hasFn(MFn.Type.kJoint))
                {
                    if (influentNodes.Count(node => Equals(node, source)) == 0)
                    {
                        influentNodes.Add(source);
                    }
                }
            }
            influentNodesBySkin.Add(skin, influentNodes);

            return influentNodes;
        }

        /// <summary>
        /// Init the two list influentNodesBySkin and revelantNodesBySkin of a skin cluster.
        /// By getting the parents of the influent nodes.
        /// </summary>
        /// <param name="skin">the skin cluster</param>
        /// <returns>
        /// The list of nodes that form the skeleton
        /// </returns>
        private List<MObject> GetRevelantNodes(MFnSkinCluster skin)
        {
            if (revelantNodesBySkin.ContainsKey(skin))
            {
                return revelantNodesBySkin[skin];
            }

            List<MObject> influentNodes = GetInfluentNodes(skin);
            List<MObject> revelantNodes = new List<MObject>();


            // Add parents until it's a kWorld
            foreach(MObject node in influentNodes)
            {
                MObject currentNode = node;
                //MObject parent = findValidParent(node);   // A node can have several parents. Which one is the right one ? It seems that the first one is most likely a transform

                while (!currentNode.apiType.Equals(MFn.Type.kWorld))
                {
                    MFnDagNode dagNode = new MFnDagNode(currentNode);

                    if (revelantNodes.Count(revelantNode => Equals(revelantNode,currentNode)) == 0)
                    {
                        revelantNodes.Add(currentNode);
                    }

                    // iter
                    MObject firstParent = dagNode.parent(0);
                    currentNode = firstParent;
                }
            }
            revelantNodesBySkin.Add(skin, revelantNodes);

            return revelantNodes;
        }

        /// <summary>
        /// Init the dictionary of the skin. This dictionary represents the skeleton. It contains the node names and their index.
        /// And add it to skinDictionary
        /// </summary>
        /// <param name="skin">the skin cluster</param>
        /// <returns>
        /// The dictionary that represents the skin skeleton
        /// </returns>
        private Dictionary<string, int> GetIndexByFullPathNameDictionary(MFnSkinCluster skin)
        {
            if (skinDictionary.ContainsKey(skin.name))
            {
                return skinDictionary[skin.name];
            }
            Dictionary<string, int> indexByFullPathName = new Dictionary<string, int>();
            List<MObject> revelantNodes = GetRevelantNodes(skin);

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
                    if (revelantNodes.Count(node => Equals(node, currentNode)) > 0)
                    {
                        MFnTransform currentNodeTransform = new MFnTransform(currentNode);
                        string currentFullPathName = currentNodeTransform.fullPathName;

                        indexByFullPathName.Add(currentFullPathName, index);
                        index++;
                    }
                }
                catch   // When it's not a kTransform or kJoint node. For exemple a kLocator, kNurbsCurve.
                {
                    RaiseError($"{currentNode.apiType} is not supported. It will not be exported: {mDagPath.fullPathName}", 3);
                    return null;
                }

                dagIterator.next();
            }

            skinDictionary.Add(skin.name, indexByFullPathName);

            return indexByFullPathName;
        }

        /// <summary>
        /// Return the bones to export.
        /// </summary>
        /// <param name="skin">the skin to export</param>
        /// <returns>Array of BabylonBone to export</returns>
        private BabylonBone[] ExportBones(MFnSkinCluster skin)
        {
            int logRank = 1;
            int skinIndex = GetSkeletonIndex(skin);
            List<BabylonBone> bones = new List<BabylonBone>();
            Dictionary<string, int> indexByFullPathName = GetIndexByFullPathNameDictionary(skin);
            List<MObject> revelantNodes = GetRevelantNodes(skin);

            progressBoneStep = progressSkinStep / revelantNodes.Count;

            foreach (MObject node in revelantNodes)
            {
                MFnDagNode dagNode = new MFnDagNode(node);
                MFnTransform currentNodeTransform = new MFnTransform(node);
                string currentFullPathName = dagNode.fullPathName;
                int index = indexByFullPathName[currentFullPathName];
                int parentIndex = -1;

                // find the parent node to get its index
                if (! dagNode.parent(0).hasFn(MFn.Type.kWorld))
                {
                    MFnTransform firstParentTransform = new MFnTransform(dagNode.parent(0));
                    parentIndex = indexByFullPathName[firstParentTransform.fullPathName];
                }

                // create the bone
                BabylonBone bone = new BabylonBone()
                {
                    id = currentNodeTransform.uuid().asString(),
                    name = dagNode.name,
                    index = indexByFullPathName[currentFullPathName],
                    parentBoneIndex = parentIndex,
                    matrix = GetBabylonMatrix(currentNodeTransform, frameBySkeletonID[skinIndex]).m,
                    animation = GetAnimationsFrameByFrameMatrix(currentNodeTransform)
                };

                bones.Add(bone);
                RaiseVerbose($"Bone: name={bone.name}, index={bone.index}, parentBoneIndex={bone.parentBoneIndex}, matrix={string.Join(" ", bone.matrix)}", logRank + 1);

                // Progress bar
                progressSkin += progressBoneStep;
                ReportProgressChanged(progressSkin);
                CheckCancelled();
            }

            // sort
            List<BabylonBone> sorted = new List<BabylonBone>();
            sorted = bones.OrderBy(bone => bone.index).ToList();
            bones = sorted;

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

        /// <summary>
        /// Check if the nodes have a scale near to zero.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="currentFrame"></param>
        /// <returns>
        /// True if all nodes have a scale higher than zero + epsilon
        /// Flase otherwise
        /// </returns>
        private bool HasNonZeroScale(List<MObject> nodes, double currentFrame)
        {
            bool isValid = true;
            for (int index = 0; index < nodes.Count && isValid; index++)
            {
                MObject node = nodes[index];
                MFnTransform transform = new MFnTransform(node);

                // get scale at this frame
                MDoubleArray mDoubleScale = new MDoubleArray();
                MGlobal.executeCommand($"getAttr -t {currentFrame.ToString(System.Globalization.CultureInfo.InvariantCulture)} {transform.fullPathName}.scale", mDoubleScale);
                mDoubleScale.get(out float[] scale);

                isValid = !scale.IsEqualTo(new float[] { 0, 0, 0 }, 0.01f);
            }

            return isValid;
        }

        /// <summary>
        /// Using the HasNonZeroScale function, it search for a frame where all bones have a scale higher than zero + epsilon.
        /// </summary>
        /// <param name="skin"></param>
        /// <returns>
        /// A list containing only the first valid frame. Otherwise it returns an empty list.
        /// </returns>
        private IList<double> GetValidFrames(MFnSkinCluster skin)
        {
            List<MObject> revelantNodes = GetRevelantNodes(skin);
            IList<double> validFrames = new List<double>();
            int start = Loader.GetMinTime();
            int end = Loader.GetMaxTime();

            // For each frame:
            //  if bone scale near 0, move to the next frame
            //  else add the frame to the list and return the list
            bool isValid = false;
            double currentFrame = start;
            while (!isValid && currentFrame <= end)
            {
                isValid = HasNonZeroScale(revelantNodes, currentFrame);
                

                if(!isValid)
                {
                    currentFrame++;
                }
                else
                {
                    validFrames.Add(currentFrame);
                }
            }

            return validFrames;
        }
    }
}
