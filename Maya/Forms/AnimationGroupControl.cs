using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon.Forms
{
    public partial class AnimationGroupControl : UserControl
    {
        public Color ChangedTextColor { get; set; } = Color.Red;

        private AnimationGroup currentInfo = null;

        // Typically called when the user presses confirm, but can also happen when scene changes are detected.
        public event Action<AnimationGroup> InfoChanged;
        public event Action<AnimationGroup> ConfirmPressed;

        public AnimationGroupControl()
        {
            InitializeComponent();
        }

        public void SetAnimationGroupInfo(AnimationGroup info)
        {
            currentInfo = info;

            SetFieldsFromInfo(currentInfo);
        }

        void SetFieldsFromInfo(AnimationGroup info)
        {
            if (info != null)
            {
                nameTextBox.Enabled = true;
                startTextBox.Enabled = true;
                endTextBox.Enabled = true;
                nameTextBox.Text = info.Name.ToString();
                startTextBox.Text = info.FrameStart.ToString();
                endTextBox.Text = info.FrameEnd.ToString();

                // a color can still be red after setting the string:
                // possible if we change a name, don't confirm and switch to another item with the same name
                ResetChangedTextBoxColors();

/*TODO                NodeTree.BeginUpdate();
                NodeTree.QueueSetNodes(info.NodeHandles, false);
                NodeTree.ApplyQueuedChanges(out List<uint> handles, false);
                NodeTree.EndUpdate();

                // if the nodes changed on max' side, even though the data has not changed, the list may be different (e.g. deleted nodes)
                // since we haven't loaded the list before, we can't compare it to the node tree
                // thus, we save it, and the property checks for actual differences (and set isdirty to true)
                info.NodeHandles = handles;
*/
                if (info.IsDirty)
                {
                    InfoChanged?.Invoke(info);
                }
            }
            else
            {
                nameTextBox.Enabled = false;
                startTextBox.Enabled = false;
                endTextBox.Enabled = false;
                nameTextBox.Text = "";
                startTextBox.Text = "";
                endTextBox.Text = "";

/*TODO                NodeTree.BeginUpdate();
                NodeTree.QueueSetNodes(null, false);
                NodeTree.ApplyQueuedChanges(out List<uint> handles, false);
                NodeTree.EndUpdate();
*/            }
        }

        void ResetChangedTextBoxColors()
        {
            nameTextBox.ForeColor = DefaultForeColor;
            startTextBox.ForeColor = DefaultForeColor;
            endTextBox.ForeColor = DefaultForeColor;
        }


        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            bool changed = currentInfo != null && nameTextBox.Text != currentInfo.Name;
            nameTextBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }

        private void startTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                startTextBox.ForeColor = DefaultForeColor;
                return;
            }

            if (!int.TryParse(startTextBox.Text, out int newFrameStart))
                newFrameStart = currentInfo.FrameStart;

            bool changed = newFrameStart != currentInfo.FrameStart;
            startTextBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }

        private void endTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                endTextBox.ForeColor = DefaultForeColor;
                return;
            }

            if (!int.TryParse(endTextBox.Text, out int newFrameEnd))
                newFrameEnd = currentInfo.FrameEnd;

            bool changed = newFrameEnd != currentInfo.FrameEnd;
            endTextBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }


        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            AnimationGroup confirmedInfo = currentInfo;

            string newName = nameTextBox.Text;

            if (!int.TryParse(startTextBox.Text, out int newFrameStart))
                newFrameStart = confirmedInfo.FrameStart;
            if (!int.TryParse(endTextBox.Text, out int newFrameEnd))
                newFrameEnd = confirmedInfo.FrameEnd;

//TODO            List<uint> newHandles;
//            bool nodesChanged = NodeTree.ApplyQueuedChanges(out newHandles);

//            bool changed = newName != confirmedInfo.Name || newFrameStart != confirmedInfo.FrameStart || newFrameEnd != confirmedInfo.FrameEnd || nodesChanged;

//            if (!changed)
//                return;

            confirmedInfo.Name = newName;
            confirmedInfo.FrameStart = newFrameStart;
            confirmedInfo.FrameEnd = newFrameEnd;

//            if (nodesChanged)
//                confirmedInfo.NodeHandles = newHandles;

            ResetChangedTextBoxColors();
            NodeTree.SelectedNode = null;

            InfoChanged?.Invoke(confirmedInfo);
            ConfirmPressed?.Invoke(confirmedInfo);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            SetFieldsFromInfo(currentInfo);
        }


        private void addSelectedButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            MDagPath node;
            MObject component = new MObject();
            MFnDagNode nodeFn = new MFnDagNode();
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            for (uint index = 0; index < selected.length; index++)
            {
                node = selected.getDagPath(index, component);
                nodeFn.setObject(node);
                MGlobal.displayInfo($"{nodeFn.name} - {nodeFn.fullPathName} - {component.apiType}");
            }

            //NodeTree.BeginUpdate();
            //for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
            //{
            //    IINode node = Loader.Core.GetSelNode(i);
            //    NodeTree.QueueAddNode(node);
            //}
            //NodeTree.EndUpdate();
        }

        private void removeNodeButton_Click(object sender, EventArgs e)
        {
/*TODO            if (currentInfo == null)
                return;

            NodeTree.BeginUpdate();
            for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
            {
                IINode node = Loader.Core.GetSelNode(i);
                NodeTree.QueueRemoveNode(node);
            }
            NodeTree.EndUpdate();
*/        }
    }
}
