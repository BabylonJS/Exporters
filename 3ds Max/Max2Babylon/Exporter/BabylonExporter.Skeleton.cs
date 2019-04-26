using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        readonly List<IIGameSkin> skins = new List<IIGameSkin>();

        private bool IsSkinEqualTo(IIGameSkin skin1, IIGameSkin skin2)
        {
            // Check nb of bones
            if (skin1.TotalSkinBoneCount != skin2.TotalSkinBoneCount)
            {
                return false;
            }

            // Check all bones are identical
            var bones1 = GetBones(skin1);
            var bones2 = GetBones(skin2);
            foreach (var bone1 in bones1)
            {
                if (!bones2.Contains(bone1))
                {
                    return false;
                }
            }
            return true;
        }

        private List<IIGameNode> GetBones(IIGameSkin skin)
        {
            var bones = new List<IIGameNode>();
            for (int i = 0; i < skin.TotalSkinBoneCount; i++)
            {
                var bone = skin.GetIGameBone(i, false);
                bones.Add(bone);
            }
            return bones;
        }

        /// <summary>
        /// Find all nodes needed for the skeleton (revelant nodes)
        /// Find the root node of the skeleton. If there is more than one, it's a problem.
        /// Sort the revelant nodes
        /// </summary>
        /// <param name="skin">The skin to export</param>
        /// <returns>
        /// All nodes needed for the skeleton hierarchy
        /// </returns>
        private Dictionary<IIGameSkin, List<IIGameNode>> revelantNodesBySkin = new Dictionary<IIGameSkin, List<IIGameNode>>();
        private List<IIGameNode> GetRevelantNodes(IIGameSkin skin)
        {
            int logRank = 2;
            
            // For optimization
            if (revelantNodesBySkin.ContainsKey(skin))
            {
                return revelantNodesBySkin[skin];
            }

            List<IIGameNode> bones = GetBones(skin);

            if(bones.Contains(null))
            {
                RaiseError("Skin has bones that are outside of the exported hierarchy.", logRank);
                RaiseError("The skin cannot be exported", logRank);
                return new List<IIGameNode>();
            }

            //return a list of all bones that are root
            List<IIGameNode> rootNodes = GetSkinRoots(bones,skin);
            if (rootNodes.Count > 1)
            {
                string rootNames = "";
                foreach(IIGameNode root in rootNodes)
                {
                    rootNames += $" {root.Name}";
                }
                RaiseError($"More than one root node for the skin: {rootNames}", logRank);
                RaiseError($"The skin cannot be exported", logRank);

                return new List<IIGameNode>();
            }
            if(rootNodes.Count <= 0)
            {
                RaiseWarning("Skin has no bones.", logRank);
                return new List<IIGameNode>();
            }

            // starting from the root, sort the nodes by depth first (add the children before the siblings)
            List<IIGameNode> sorted = new List<IIGameNode>();
            Stack<IIGameNode> siblings = new Stack<IIGameNode>();   // keep the siblings in a LIFO list to add them after the children
            siblings.Push(rootNodes[0]);
            while (siblings.Count > 0)
            {
                IIGameNode currentNode = siblings.Pop();

                if (bones.Contains(currentNode))    // The node is part of the skeleton
                {
                    // Add the current node to the sorted list
                    sorted.Add(currentNode);

                    // Add its children to the stack (in reverse order because it's a LIFO)
                    int childCount = currentNode.ChildCount;
                    for (int index = 0; index < childCount; index++)
                    {
                        siblings.Push(currentNode.GetNodeChild(childCount - 1 - index));
                    }
                }
            }

            revelantNodesBySkin.Add(skin, sorted);   // Stock the result for optimization

            return sorted;
        }

        private List<IIGameNode> GetSkinRoots(List<IIGameNode> relevantNodes, IIGameSkin skin)
        {
            List<IIGameNode> relevantRootNodes = new List<IIGameNode>();
            List<IIGameNode> skinnedNodes = GetBones(skin);
            foreach (IIGameNode relevantNode in relevantNodes)
            {
                IIGameNode parent = relevantNode.NodeParent;
                if (!skinnedNodes.Contains(parent))
                {
                    //found a skin bone root
                    relevantRootNodes.Add(relevantNode);
                }
            }
            return relevantRootNodes;
        }

        /// <summary>
        /// Create and return a list. This list contains the 3DS Max bone id.
        /// The index of an element in the list is equivalent to the babylon id of the bone.
        /// </summary>
        /// <param name="skin"></param>
        /// <returns>
        /// The list that will convert à 3DS Max bone ID (value of the list) in Babylon bone ID (index of the list)
        /// </returns>
        private Dictionary<IIGameSkin, List<int>> nodeIndexBySkin = new Dictionary<IIGameSkin, List<int>>();
        private List<int> GetNodeIndices(IIGameSkin skin)
        {
            // For optimization
            if (nodeIndexBySkin.ContainsKey(skin))
            {
                return nodeIndexBySkin[skin];
            }

            List<int> nodeIndex = new List<int>();
            List<IIGameNode> revelantNodes = GetRevelantNodes(skin);

            for (int index = 0; index < revelantNodes.Count; index++)
            {
                nodeIndex.Add(revelantNodes[index].NodeID);
            }

            nodeIndexBySkin.Add(skin, nodeIndex);   // Stock the result for optimization

            return nodeIndex;
        }

        /// <summary>
        /// Export the skeleton
        /// </summary>
        /// <param name="skin">The skin to export</param>
        /// <param name="babylonScene">The exported scene that will contain the skeleton</param>
        private void ExportSkin(IIGameSkin skin, BabylonScene babylonScene)
        {
            int logRank = 1;
            int skinIndex = skins.IndexOf(skin);
            string name = "skeleton #" + skinIndex;
            RaiseMessage(name, logRank);

            BabylonSkeleton babylonSkeleton = new BabylonSkeleton
            {
                id = skinIndex,
                name = name,
                bones = ExportBones(skin),
                needInitialSkinMatrix = true
            };

            babylonScene.SkeletonsList.Add(babylonSkeleton);
        }

        /// <summary>
        /// Export the bones and their animation for the given skin
        /// </summary>
        /// <param name="skin">The skin to export</param>
        /// <returns></returns>
        private BabylonBone[] ExportBones(IIGameSkin skin)
        {
            List<BabylonBone> bones = new List<BabylonBone>();
            List<int> nodeIndices = GetNodeIndices(skin);
            List<IIGameNode> revelantNodes = GetRevelantNodes(skin);

            foreach (IIGameNode node in revelantNodes)
            {
                int parentIndex = (node.NodeParent == null) ? -1 : nodeIndices.IndexOf(node.NodeParent.NodeID);

                // create the bone
                BabylonBone bone = new BabylonBone()
                {
                    id = isBabylonExported ? node.MaxNode.GetGuid().ToString()+"-bone" : node.MaxNode.GetGuid().ToString(), // the suffix "-bone" is added in babylon export format to assure the uniqueness of IDs
                    name = node.Name,
                    index = nodeIndices.IndexOf(node.NodeID),
                    parentBoneIndex = parentIndex,
                    matrix = node.GetLocalTM(0).ToArray()
                };

                // export its animation
                var babylonAnimation = ExportMatrixAnimation("_matrix", key =>
                {
                    var objectTM = node.GetObjectTM(key);
                    var parentNode = node.NodeParent;
                    IGMatrix mat;
                    if (parentNode == null || bone.parentBoneIndex == -1)
                    {
                        mat = objectTM;
                    }
                    else
                    {
                        mat = node.GetLocalTM(key);
                    }
                    return mat.ToArray();
                },
                false); // Do not remove linear animation keys for bones

                if (babylonAnimation != null)
                {
                    babylonAnimation.name = node.Name + "Animation"; // override default animation name
                    bone.animation = babylonAnimation;
                }

                bones.Add(bone);
            }

            return bones.ToArray();
        }
    }
}
