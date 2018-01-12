using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Text;
using System.Linq;
using Autodesk.Max;

namespace Max2Babylon
{
    public partial class AnimationGroupControl : UserControl
    {
        public class AnimationGroupInfo
        {
            public Guid SerializedId = Guid.NewGuid();
            public string Name = "Animation";
            public int FrameStart = 0;
            public int FrameEnd = 100;
            public List<uint> NodeHandles = new List<uint>();

            public AnimationGroupInfo() { }
            public AnimationGroupInfo(AnimationGroupInfo other)
            {
                DeepCopyFrom(other);
            }
            public void DeepCopyFrom(AnimationGroupInfo other)
            {
                SerializedId = other.SerializedId;
                Name = other.Name;
                FrameStart = other.FrameStart;
                FrameEnd = other.FrameEnd;
                NodeHandles.Clear();
                NodeHandles.AddRange(other.NodeHandles);
            }

            const string s_DisplayNameFormat = "{0} ({1:d}, {2:d})";
            public override string ToString()
            {
                return string.Format(s_DisplayNameFormat, Name, FrameStart, FrameEnd);
            }

            #region Serialization

            const char s_PropertySeparator = ';';
            const string s_PropertyFormat = "{0};{1};{2};{3}";
            public string GetPropertyName() { return SerializedId.ToString(); }

            public void LoadFromData(string propertyName)
            {
                if (!Guid.TryParse(propertyName, out SerializedId))
                    throw new Exception("Invalid ID, can't deserialize.");

                string propertiesString = string.Empty;
                if (!Loader.Core.RootNode.GetUserPropString(propertyName, ref propertiesString))
                    return;

                string[] properties = propertiesString.Split(s_PropertySeparator);

                if (properties.Length < 4)
                    throw new Exception("Invalid number of properties, can't deserialize.");

                Name = properties[0];
                if (!int.TryParse(properties[1], out FrameStart))
                    throw new Exception("Failed to parse FrameStart property.");
                if (!int.TryParse(properties[2], out FrameEnd))
                    throw new Exception("Failed to parse FrameEnd property.");

                if (string.IsNullOrEmpty(properties[3]))
                    return;

                int numNodeIDs = properties.Length - 3;
                if (NodeHandles.Capacity < numNodeIDs) NodeHandles.Capacity = numNodeIDs;
                int numFailed = 0;
                for(int i = 0; i < numNodeIDs; ++i)
                {
                    if (!uint.TryParse(properties[3 + i], out uint id))
                        ++numFailed;
                }

                if (numFailed > 0)
                    throw new Exception(string.Format("Failed to parse {0} node ids.", numFailed));
            }

            public void SaveToData()
            {
                // ' ' and '=' are not allowed by max, ';' is our data separator
                if (Name.Contains(" ") || Name.Contains("=") || Name.Contains(s_PropertySeparator))
                    throw new FormatException("Invalid character(s) in animation Name: " + Name + ". Spaces, equal signs and the separator '" + s_PropertySeparator + "' are not allowed.");

                string nodes = string.Join(s_PropertySeparator.ToString(), NodeHandles);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(s_PropertyFormat, Name, FrameStart, FrameEnd, nodes);

                Loader.Core.RootNode.SetStringProperty(GetPropertyName(), stringBuilder.ToString());
            }

            public void DeleteFromData()
            {
                Loader.Core.RootNode.DeleteProperty(GetPropertyName());
            }
            
            #endregion
        }

        public Color DefaultTextColor { get; set; } = Color.Black;
        public Color ChangedTextColor { get; set; } = Color.Red;

        AnimationGroupInfo currentInfo = null;
        Dictionary<uint, TreeNode> nodeMap = new Dictionary<uint, TreeNode>();

        public event Action<AnimationGroupInfo> InfoSaved;

        public AnimationGroupControl()
        {
            InitializeComponent();
        }

        public void SetAnimationGroupInfo(AnimationGroupInfo info)
        {
            if (info == null)
                currentInfo = null;
            else
                currentInfo = info;
            
            SetFieldsFromInfo(currentInfo);
        }

        void SetFieldsFromInfo(AnimationGroupInfo info)
        {
            if (info != null)
            {
                nameTextBox.Enabled = true;
                startTextBox.Enabled = true;
                endTextBox.Enabled = true;
                nameTextBox.Text = info.Name.ToString();
                startTextBox.Text = info.FrameStart.ToString();
                endTextBox.Text = info.FrameEnd.ToString();
            }
            else
            {
                nameTextBox.Enabled = false;
                startTextBox.Enabled = false;
                endTextBox.Enabled = false;
                nameTextBox.Text = "";
                startTextBox.Text = "";
                endTextBox.Text = "";
            }

            SetTreeView(info);
        }

        void SetTreeView(AnimationGroupInfo info)
        {
            if (info == null)
            {
                nodeTreeView.Nodes.Clear();
                nodeMap.Clear();
                return;
            }

            for(int i = 0; i < info.NodeHandles.Count; ++i)
            {
                uint nodeHandle = info.NodeHandles[i];
                TreeNode treeNode;
                if (nodeMap.TryGetValue(nodeHandle, out treeNode))
                {
                    continue;
                }

                foreach(IINode node in Loader.Core.RootNode.Nodes())
                {
                    if(nodeHandle.Equals(node.Handle))
                    {
                        AddNodeToTreeRecursive(node);
                        break;
                    }
                }
            }
        }

        TreeNode AddNodeToTreeRecursive(IINode node)
        {
            // if this node was already added, we don't have to do anything
            TreeNode treeNode;
            if (nodeMap.TryGetValue(node.Handle, out treeNode))
                return treeNode;

            // node wasn't added yet, first add parent if we have any
            TreeNode parentTreeNode = null;
            if (node.HasParent() && !nodeMap.TryGetValue(node.ParentNode.Handle, out parentTreeNode))
                parentTreeNode = AddNodeToTreeRecursive(node.ParentNode);

            // create and link to parent
            treeNode = new TreeNode(node.Name);
            treeNode.Tag = node;
            if (parentTreeNode != null)
                parentTreeNode.Nodes.Add(treeNode);
            else
                nodeTreeView.Nodes.Add(treeNode);
            nodeMap.Add(node.Handle, treeNode);
            return treeNode;
        }
        
        void ResetColors()
        {
            nameTextBox.ForeColor = DefaultTextColor;
            startTextBox.ForeColor = DefaultTextColor;
            endTextBox.ForeColor = DefaultTextColor;
        }


        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            bool changed = currentInfo != null && nameTextBox.Text != currentInfo.Name;
            nameTextBox.ForeColor = changed ? ChangedTextColor : DefaultTextColor;
        }

        private void startTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                startTextBox.ForeColor = DefaultTextColor;
                return;
            }

            if (!int.TryParse(startTextBox.Text, out int newFrameStart))
                newFrameStart = currentInfo.FrameStart;

            bool changed = newFrameStart != currentInfo.FrameStart;
            startTextBox.ForeColor = changed ? ChangedTextColor : DefaultTextColor;
        }

        private void endTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                endTextBox.ForeColor = DefaultTextColor;
                return;
            }

            if (!int.TryParse(endTextBox.Text, out int newFrameEnd))
                newFrameEnd = currentInfo.FrameEnd;

            bool changed = newFrameEnd != currentInfo.FrameEnd;
            endTextBox.ForeColor = changed ? ChangedTextColor : DefaultTextColor;
        }
        

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            string newName = nameTextBox.Text;

            if (!int.TryParse(startTextBox.Text, out int newFrameStart))
                newFrameStart = currentInfo.FrameStart;
            if (!int.TryParse(endTextBox.Text, out int newFrameEnd))
                newFrameEnd = currentInfo.FrameEnd;

            bool changed = newName != currentInfo.Name || newFrameStart != currentInfo.FrameStart || newFrameEnd != currentInfo.FrameEnd;

            if (!changed)
                return;

            currentInfo.Name = newName;
            currentInfo.FrameStart = newFrameStart;
            currentInfo.FrameEnd = newFrameEnd;

            ResetColors();

            InfoSaved?.Invoke(currentInfo);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            SetFieldsFromInfo(currentInfo);
        }

        private void addSelectedButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
            {
                AddNodeToTreeRecursive(Loader.Core.GetSelNode(i));
            }
        }
    }
}
