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
        private Dictionary<IIGameSkin, List<IIGameNode>> relevantNodesBySkin = new Dictionary<IIGameSkin, List<IIGameNode>>();
        private List<IIGameNode> GetRelevantNodes(IIGameSkin skin)
        {
            int logRank = 2;
            
            // For optimization
            if (relevantNodesBySkin.ContainsKey(skin))
            {
                return relevantNodesBySkin[skin];
            }

            List<IIGameNode> bones = GetBones(skin);

            if (bones.Count == 0)
            {
                RaiseWarning("Skin has no bones.", logRank);
                return new List<IIGameNode>();
            }

            if (bones.Contains(null))
            {
                RaiseError("Skin has bones that are outside of the exported hierarchy.", logRank);
                RaiseError("The skin cannot be exported", logRank);
                return new List<IIGameNode>();
            }

            List<IIGameNode> allHierarchyNodes = null;
            IIGameNode lowestCommonAncestor = GetLowestCommonAncestor(bones, ref allHierarchyNodes);

            if (lowestCommonAncestor == null)
            {
                RaiseError($"More than one root node for the skin. The skeleton bones need to be part of the same hierarchy.", logRank);
                RaiseError($"The skin cannot be exported", logRank);

                return new List<IIGameNode>();
            }

            // starting from the root, sort the nodes by depth first (add the children before the siblings)
            List<IIGameNode> sorted = new List<IIGameNode>();
            Stack<IIGameNode> siblings = new Stack<IIGameNode>();   // keep the siblings in a LIFO list to add them after the children
            siblings.Push(lowestCommonAncestor);

            // add the skeletonroot:
            // - as a fallback for vertices without any joint weights (although invalid joints could also be "ok"?)
            // - to have easy access to the root node for the gltf's [skin.skeleton] property (skeleton root node)
            // [##onlyBones] commented for now because uncertain if it will work with babylon bone exports
            //sorted.Add(lowestCommonAncestor);

            while (siblings.Count > 0)
            {
                IIGameNode currentNode = siblings.Pop();

                if (allHierarchyNodes.Contains(currentNode))    // The node is part of the skeleton hierarchy
                {
                    // only add if the node is an actual bone (to keep the joint list small)
                    // [##onlyBones] commented for now because uncertain if it will work with babylon bone exports
                    //if (bones.Contains(currentNode))
                    sorted.Add(currentNode);

                    // Add its children to the stack (in reverse order because it's a LIFO)
                    int childCount = currentNode.ChildCount;
                    for (int index = 0; index < childCount; index++)
                    {
                        siblings.Push(currentNode.GetNodeChild(childCount - 1 - index));
                    }
                }
            }

            relevantNodesBySkin.Add(skin, sorted);   // Stock the result for optimization

            return sorted;
        }

        private IIGameNode GetLowestCommonAncestor(List<IIGameNode> nodes, ref List<IIGameNode> allHierarchyNodes)
        {
            IIGameNode commonAncestor = null;
            allHierarchyNodes = new List<IIGameNode>();
            List<IIGameNode> nodeHierarchyA = new List<IIGameNode>();
            List<IIGameNode> nodeHierarchyB = new List<IIGameNode>();
            foreach (IIGameNode node in nodes)
            {
                commonAncestor = GetLowestCommonAncestor(commonAncestor, node, nodeHierarchyA, nodeHierarchyB);
                if (commonAncestor == null)
                {
                    allHierarchyNodes.Clear();
                    return null;
                }
                
                foreach (IIGameNode nodeA in nodeHierarchyA)
                {
                    if (!allHierarchyNodes.Contains(nodeA))
                        allHierarchyNodes.Add(nodeA);
                }
                foreach (IIGameNode nodeB in nodeHierarchyB)
                {
                    if (!allHierarchyNodes.Contains(nodeB))
                        allHierarchyNodes.Add(nodeB);
                }
            }

            return commonAncestor;
        }

        private IIGameNode GetLowestCommonAncestor(IIGameNode nodeA, IIGameNode nodeB, List<IIGameNode> nodeHierarchyA = null, List<IIGameNode> nodeHierarchyB = null)
        {
            if (nodeA == nodeB || nodeB == null) return nodeA;
            if (nodeA == null) return nodeB;

            if (nodeHierarchyA == null) nodeHierarchyA = new List<IIGameNode>();
            else nodeHierarchyA.Clear();
            if (nodeHierarchyB == null) nodeHierarchyB = new List<IIGameNode>();
            else nodeHierarchyB.Clear();

            nodeHierarchyA.Add(nodeA);
            nodeHierarchyB.Add(nodeB);

            int hierarchyAIndex = 0;
            int hierarchyBIndex = 0;

            // build hierarchies up to the root node
            while (true)
            {
                if (nodeHierarchyA[hierarchyAIndex].NodeParent != null)
                {
                    nodeHierarchyA.Add(nodeHierarchyA[hierarchyAIndex].NodeParent);
                    ++hierarchyAIndex;
                }

                if (nodeHierarchyB[hierarchyBIndex].NodeParent != null)
                {
                    nodeHierarchyB.Add(nodeHierarchyB[hierarchyBIndex].NodeParent);
                    ++hierarchyBIndex;
                }

                if (nodeHierarchyA[hierarchyAIndex].NodeParent == null && nodeHierarchyB[hierarchyBIndex].NodeParent == null)
                {
                    break;
                }
            }

            // check whether the nodes exist in the same hierarchy
            IIGameNode topA = nodeHierarchyA[hierarchyAIndex];
            IIGameNode topB = nodeHierarchyB[hierarchyBIndex];
            if (topA.MaxNode.Handle != topB.MaxNode.Handle)
            {
                return null;
            }

            // traverse down the two hierarchies until they diverge
            IIGameNode lowestCommonAncestor = null;
            while (true)
            {
                --hierarchyAIndex;
                --hierarchyBIndex;
                
                // in the case that one is a child of the other, other will be invalid when one still has more nodes to go
                if (hierarchyAIndex == -1)
                {
                    lowestCommonAncestor = nodeHierarchyA[0];
                    break;
                }
                if (hierarchyBIndex == -1)
                {
                    lowestCommonAncestor = nodeHierarchyB[0];
                    break;
                }

                // if the current nodes differ, the parent was the last shared node
                if (nodeHierarchyA[hierarchyAIndex].MaxNode.Handle != nodeHierarchyB[hierarchyBIndex].MaxNode.Handle)
                {
                    lowestCommonAncestor = nodeHierarchyA[hierarchyAIndex + 1];
                    break;
                }
            }

            // fix up hierarchies to only contain nodes between the nodes we got (which have index 0) and the common ancestor we found
            int ancestorIndexA = hierarchyAIndex + 1;
            int ancestorIndexB = hierarchyBIndex + 1;
            int numNodesToKeepA = ancestorIndexA + 1;
            int numNodesToKeepB = ancestorIndexB + 1;
            if (nodeHierarchyA.Count > numNodesToKeepA)
                nodeHierarchyA.RemoveRange(numNodesToKeepA, nodeHierarchyA.Count - numNodesToKeepA);
            if (nodeHierarchyB.Count > numNodesToKeepB)
                nodeHierarchyB.RemoveRange(numNodesToKeepB, nodeHierarchyB.Count - numNodesToKeepB);

            return lowestCommonAncestor;
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
            List<IIGameNode> revelantNodes = GetRelevantNodes(skin);

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
            List<IIGameNode> revelantNodes = GetRelevantNodes(skin);

            foreach (IIGameNode node in revelantNodes)
            {
                int parentIndex = (node.NodeParent == null) ? -1 : nodeIndices.IndexOf(node.NodeParent.NodeID);

                // create the bone
                BabylonBone bone = new BabylonBone()
                {
                    id = node.MaxNode.GetGuid().ToString() + "-bone",// the suffix "-bone" is added in babylon export format to assure the uniqueness of IDs
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
