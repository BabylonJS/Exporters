using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Max;
using System.Drawing;
using System.ComponentModel;

namespace Max2Babylon
{
    // to clarify on what a treenode represents
    using VisualNode = TreeNode;
    using VisualNodeCollection = TreeNodeCollection;

    // an overview of what functions to use with this control
    public interface IMaxNodeTreeView
    {
        void QueueSetNodes(IEnumerable<uint> nodeHandles, bool doBeginUpdate = true);

        void QueueAddNode(uint nodeHandle);
        void QueueAddNode(IINode node);

        void QueueRemoveNode(uint nodeHandle);
        void QueueRemoveNode(IINode node);
        void QueueRemoveNode(VisualNode visualNode);

        bool ApplyQueuedChanges(out List<uint> nodeHandles, bool doBeginUpdate = true);
    }

    public partial class MaxNodeTreeView : TreeView, IMaxNodeTreeView
    {
        class VisualNodeInfo
        {
            public enum EState
            {
                Added,
                Removed,
                Downgraded,
                Upgraded,
                Saved
            }

            public IINode MaxNode { get; private set; }
            public EState State { get; set; }
            public bool IsDummy { get; set; }

            public VisualNodeInfo(IINode maxNode, bool isDummy = true, EState state = EState.Added)
            {
                MaxNode = maxNode;
                State = state;
                IsDummy = isDummy;
            }
        }

        #region Public Properties

        [Category("Node State Colors")]
        public Color NodeDefaultForeColor { get; set; } = SystemColors.ControlText;
        [Category("Node State Colors")]
        public Color NodeDefaultBackColor { get; set; } = SystemColors.Window;

        [Category("Node State Colors")]
        public Color NodeAddedForeColor { get; set; } = SystemColors.ControlText;
        [Category("Node State Colors")]
        public Color NodeAddedBackColor { get; set; } = Color.PaleGreen;

        [Category("Node State Colors")]
        public Color NodeDowngradedForeColor { get; set; } = SystemColors.GrayText;
        [Category("Node State Colors")]
        public Color NodeDowngradedBackColor { get; set; } = Color.PaleGreen;

        [Category("Node State Colors")]
        public Color NodeRemovedForeColor { get; set; } = SystemColors.ControlText;
        [Category("Node State Colors")]
        public Color NodeRemovedBackColor { get; set; } = Color.IndianRed;

        [Category("Node State Colors")]
        public Color DummyDefaultForeColor { get; set; } = SystemColors.GrayText;
        [Category("Node State Colors")]
        public Color DummyDefaultBackColor { get; set; } = SystemColors.Control;

        [Category("Node State Colors")]
        public Color DummyAddedForeColor { get; set; } = SystemColors.ControlText;
        [Category("Node State Colors")]
        public Color DummyAddedBackColor { get; set; } = Color.PaleGreen;

        [Category("Node State Colors")]
        public Color DummyRemovedForeColor { get; set; } = SystemColors.ControlText;
        [Category("Node State Colors")]
        public Color DummyRemovedBackColor { get; set; } = Color.IndianRed;

        [Category("Node State Colors")]
        public Color DummyUpgradedForeColor { get; set; } = SystemColors.ControlText;
        [Category("Node State Colors")]
        public Color DummyUpgradedBackColor { get; set; } = Color.PaleGreen;

        #endregion
        
        #region Fields & Constructor

        // Stores the handles that were in the previously saved tree view, together with if they were dummies or not.
        Dictionary<uint, bool> previousAppliedHandles = new Dictionary<uint, bool>();

        // Stores the current handles with references to the visual nodes they belong to.
        Dictionary<uint, VisualNode> visualNodeMap = new Dictionary<uint, VisualNode>();

        bool changed;

        public MaxNodeTreeView()
        {
            InitializeComponent();
        }

        #endregion

        #region Interface Implementation: IMaxNodeTreeView

        public void QueueSetNodes(IEnumerable<uint> nodeHandles, bool doBeginUpdate = true)
        {
            changed = true;

            Nodes.Clear();
            visualNodeMap.Clear();

            if (nodeHandles == null)
            {
                return;
            }

            if (doBeginUpdate)
                BeginUpdate();

            foreach (uint nodeHandle in nodeHandles)
            {
                // get visual node from tree or create it (with isDummy == false)
                if (!visualNodeMap.TryGetValue(nodeHandle, out VisualNode visualNode))
                {
                    IINode node = Loader.Core.RootNode.FindChildNode(nodeHandle);

                    // node does not exist (anymore)
                    if (node == null)
                    {
                        continue;
                    }

                    visualNode = QueueAddNodeRecursively(node, true);
                }

                visualNode.EnsureVisible();
            }

            if (doBeginUpdate)
                EndUpdate();
        }

        public void QueueAddNode(uint nodeHandle)
        {
            IINode node = Loader.Core.RootNode.FindChildNode(nodeHandle);
            QueueAddNode(node);
        }
        public void QueueAddNode(IINode node)
        {
            BeginUpdate();
            QueueAddNodeRecursively(node, true);
            EndUpdate();
        }

        public void QueueRemoveNode(IINode node)
        {
            QueueRemoveNode(node.Handle);
        }
        public void QueueRemoveNode(uint nodeHandle)
        {
            if (!visualNodeMap.TryGetValue(nodeHandle, out VisualNode node))
                return;

            BeginUpdate();
            QueueRemoveNodeRecursively(node);
            EndUpdate();
        }
        public void QueueRemoveNode(VisualNode visualNode)
        {
            BeginUpdate();
            QueueRemoveNodeRecursively(visualNode);
            EndUpdate();
        }

        public bool ApplyQueuedChanges(out List<uint> nodeHandles, bool doBeginUpdate = true)
        {
            nodeHandles = null;

            if (!changed)
                return false;

            changed = false;

            if (doBeginUpdate)
                BeginUpdate();
            ApplyQueuedChangesRecursively(Nodes);
            if (doBeginUpdate)
                EndUpdate();

            // - get all non-dummy handles to return
            // - save all handles from the 'applied changes'
            previousAppliedHandles.Clear();
            nodeHandles = new List<uint>();
            foreach(KeyValuePair<uint,VisualNode> pair in visualNodeMap)
            {
                VisualNodeInfo info = pair.Value.Tag as VisualNodeInfo;
                previousAppliedHandles.Add(pair.Key, info.IsDummy);

                if (!info.IsDummy)
                    nodeHandles.Add(pair.Key);
            }
            
            return true;
        }

        #endregion

        #region Recursive Node Manipulation

        VisualNode QueueAddNodeRecursively(IINode node, bool setNonDummy = false)
        {
            changed = true;

            // recurse up the chain to make sure the hierarchy is in a good state
            VisualNode parentNode = null;
            if (node.HasParent())
                parentNode = QueueAddNodeRecursively(node.ParentNode);

            VisualNode visualNode;

            // if this node was not already added, create it
            if (!visualNodeMap.TryGetValue(node.Handle, out visualNode))
            {
                visualNode = new VisualNode(node.Name);
                visualNode.Tag = new VisualNodeInfo(node, !setNonDummy);

                // add to hierarchy
                if (parentNode != null) parentNode.Nodes.Add(visualNode);
                else Nodes.Add(visualNode);

                // save reference
                visualNodeMap.Add(node.Handle, visualNode);
            }

            // update node state, compares to previous node and state
            // only upgrade dummy state !
            SetNodeState_ToAdd(visualNode, setNonDummy);

            return visualNode;
        }

        void QueueRemoveNodeRecursively(VisualNode visualNode)
        {
            if (visualNode == null)
                return;

            changed = true;
            // see if we have non-dummy children
            // a little redundant during recursion as we go up the hierarchy
            bool hasNonDummyChildren = false;
            foreach (VisualNode node in visualNode.NodeTree())
            {
                VisualNodeInfo nodeInfo = (VisualNodeInfo)node.Tag;
                if (!nodeInfo.IsDummy && nodeInfo.State != VisualNodeInfo.EState.Removed)
                {
                    hasNonDummyChildren = true;
                    break;
                }
            }

            // set state so we don't see this node as a valid non-dummy node during recursion
            SetNodeState_ToRemove(visualNode, hasNonDummyChildren);

            // queue remove parent if it's a dummy
            if (visualNode.Parent != null)
            {
                VisualNodeInfo parentStateInfo = (VisualNodeInfo)visualNode.Parent.Tag;
                if (parentStateInfo.IsDummy)
                    QueueRemoveNodeRecursively(visualNode.Parent);
            }
        }

        void ApplyQueuedChangesRecursively(VisualNodeCollection visualNodeCollection)
        {
            for(int i = visualNodeCollection.Count-1; i >= 0; --i)
            {
                VisualNode visualNode = visualNodeCollection[i];

                // depth-first
                if(visualNode.Nodes.Count > 0)
                    ApplyQueuedChangesRecursively(visualNode.Nodes);

                VisualNodeInfo stateInfo = (VisualNodeInfo)visualNode.Tag;
                if(stateInfo.State == VisualNodeInfo.EState.Removed)
                {
                    visualNodeCollection.RemoveAt(i);
                    visualNodeMap.Remove(stateInfo.MaxNode.Handle);
                    continue;
                }

                stateInfo.State = VisualNodeInfo.EState.Saved;
                visualNode.ForeColor = GetNodeForeColor(stateInfo);
                visualNode.BackColor = GetNodeBackColor(stateInfo);
            }
        }

        #endregion

        #region VisualNode Helper Functions

        void SetNodeState_ToAdd(VisualNode visualNode, bool setNonDummy = true)
        {
            VisualNodeInfo nodeInfo = (VisualNodeInfo)visualNode.Tag;

            if(setNonDummy)
                nodeInfo.IsDummy = false;

            if (previousAppliedHandles.TryGetValue(nodeInfo.MaxNode.Handle, out bool wasDummy))
            {
                if (wasDummy)
                    nodeInfo.State = nodeInfo.IsDummy ? VisualNodeInfo.EState.Saved : VisualNodeInfo.EState.Upgraded;
                else
                    nodeInfo.State = nodeInfo.IsDummy ? VisualNodeInfo.EState.Downgraded : VisualNodeInfo.EState.Saved;
            }
            else
                nodeInfo.State = VisualNodeInfo.EState.Added;

            visualNode.ForeColor = GetNodeForeColor(nodeInfo);
            visualNode.BackColor = GetNodeBackColor(nodeInfo);
        }

        void SetNodeState_ToRemove(VisualNode visualNode, bool keepAsDummy)
        {
            VisualNodeInfo nodeInfo = (VisualNodeInfo)visualNode.Tag;

            if (keepAsDummy)
            {
                nodeInfo.IsDummy = true;
                if (previousAppliedHandles.TryGetValue(nodeInfo.MaxNode.Handle, out bool wasDummy))
                    nodeInfo.State = wasDummy ? VisualNodeInfo.EState.Saved : VisualNodeInfo.EState.Downgraded;
                else
                    nodeInfo.State = VisualNodeInfo.EState.Added;
            }
            else
            {
                nodeInfo.State = VisualNodeInfo.EState.Removed;
            }

            visualNode.ForeColor = GetNodeForeColor(nodeInfo);
            visualNode.BackColor = GetNodeBackColor(nodeInfo);
        }

        Color GetNodeForeColor(VisualNodeInfo nodeInfo)
        {
            switch (nodeInfo.State)
            {
                case VisualNodeInfo.EState.Added:
                return nodeInfo.IsDummy ? DummyAddedForeColor : NodeAddedForeColor;
                case VisualNodeInfo.EState.Removed:
                return nodeInfo.IsDummy ? DummyRemovedForeColor : NodeRemovedForeColor;
                case VisualNodeInfo.EState.Downgraded:
                return NodeDowngradedForeColor;
                case VisualNodeInfo.EState.Upgraded:
                return DummyUpgradedForeColor;
                case VisualNodeInfo.EState.Saved:
                return nodeInfo.IsDummy ? DummyDefaultForeColor : NodeDefaultForeColor;
            }
            return NodeDefaultForeColor;
        }

        Color GetNodeBackColor(VisualNodeInfo nodeInfo)
        {
            switch (nodeInfo.State)
            {
                case VisualNodeInfo.EState.Added:
                return nodeInfo.IsDummy ? DummyAddedBackColor : NodeAddedBackColor;
                case VisualNodeInfo.EState.Removed:
                return nodeInfo.IsDummy ? DummyRemovedBackColor : NodeRemovedBackColor;
                case VisualNodeInfo.EState.Downgraded:
                return NodeDowngradedBackColor;
                case VisualNodeInfo.EState.Upgraded:
                return DummyUpgradedBackColor;
                case VisualNodeInfo.EState.Saved:
                return nodeInfo.IsDummy ? DummyDefaultBackColor : NodeDefaultBackColor;
            }
            return NodeDefaultBackColor;
        }

        #endregion
    }
}
