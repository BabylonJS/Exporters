using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Drawing;
using Autodesk.Max;

namespace Max2Babylon
{
    public partial class AnimationGroupControl : UserControl
    {
        public Color ChangedTextColor { get; set; } = Color.Red;

        AnimationGroup currentInfo = null;

        // Typically called when the user presses confirm, but can also happen when scene changes are detected.
        public event Action<AnimationGroup> InfoChanged;
        public event Action<AnimationGroup> ConfirmPressed;

        public AnimationGroupControl()
        {
            InitializeComponent();
        }

        public void SetAnimationGroupInfo(AnimationGroup info)
        {
            if (info == null)
                currentInfo = null;
            else
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

                MaxNodeTree.BeginUpdate();
                MaxNodeTree.QueueSetNodes(info.NodeHandles, false);
                List<uint> handles;
                MaxNodeTree.ApplyQueuedChanges(out handles, false);
                MaxNodeTree.EndUpdate();

                // if the nodes changed on max' side, even though the data has not changed, the list may be different (e.g. deleted nodes)
                // since we haven't loaded the list before, we can't compare it to the node tree
                // thus, we save it, and the property checks for actual differences (and set isdirty to true)
                info.NodeHandles = handles;

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

                MaxNodeTree.BeginUpdate();
                MaxNodeTree.QueueSetNodes(null, false);
                List<uint> handles;
                MaxNodeTree.ApplyQueuedChanges(out handles, false);
                MaxNodeTree.EndUpdate();
            }
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

            int newFrameStart;

            if (!int.TryParse(startTextBox.Text, out newFrameStart))
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

            int newFrameEnd;

            if (!int.TryParse(endTextBox.Text, out newFrameEnd))
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

            int newFrameStart;

            if (!int.TryParse(startTextBox.Text, out newFrameStart))
                newFrameStart = confirmedInfo.FrameStart;
            int newFrameEnd;
            if (!int.TryParse(endTextBox.Text, out newFrameEnd))
                newFrameEnd = confirmedInfo.FrameEnd;

            List<uint> newHandles;
            bool nodesChanged = MaxNodeTree.ApplyQueuedChanges(out newHandles);

            bool changed = newName != confirmedInfo.Name || newFrameStart != confirmedInfo.FrameStart || newFrameEnd != confirmedInfo.FrameEnd || nodesChanged;
            
            if (!changed)
                return;

            confirmedInfo.Name = newName;
            confirmedInfo.FrameStart = newFrameStart;
            confirmedInfo.FrameEnd = newFrameEnd;

            if (nodesChanged)
            {
                confirmedInfo.NodeHandles = newHandles;
                confirmedInfo = CalculateEndFrameFromAnimationGroupNodes(confirmedInfo);
            }

            ResetChangedTextBoxColors();
            MaxNodeTree.SelectedNode = null;

            InfoChanged?.Invoke(confirmedInfo);
            ConfirmPressed?.Invoke(confirmedInfo);
        }

        private AnimationGroup CalculateEndFrameFromAnimationGroupNodes(AnimationGroup animationGroup)
        {
            int endFrame = 0;
            foreach (uint nodeHandle in currentInfo.NodeHandles)
            {
                IINode node = Loader.Core.GetINodeByHandle(nodeHandle);
                if (node.IsAnimated && node.TMController!=null)
                {
                    int lastKey = 0;
                    if (node.TMController.PositionController != null)
                    {
                        int posKeys = node.TMController.PositionController.NumKeys;
                        lastKey = Math.Max(lastKey, node.TMController.PositionController.GetKeyTime(posKeys - 1));
                    }

                    if (node.TMController.RotationController!=null)
                    {
                        int rotKeys = node.TMController.RotationController.NumKeys;
                        lastKey = Math.Max(lastKey, node.TMController.RotationController.GetKeyTime(rotKeys - 1));
                    }

                    if (node.TMController.ScaleController!=null)
                    {
                        int scaleKeys = node.TMController.ScaleController.NumKeys;
                        lastKey = Math.Max(lastKey, node.TMController.ScaleController.GetKeyTime(scaleKeys - 1));
                    }
                    decimal keyTime = Decimal.Ceiling(lastKey / 160);
                    endFrame = Math.Max(endFrame, Decimal.ToInt32(keyTime));
                }
            }
            animationGroup.FrameEnd = endFrame;
            return animationGroup;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            SetFieldsFromInfo(currentInfo);
        }


        private void addSelectedButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            MaxNodeTree.BeginUpdate();
            for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
            {
                IINode node = Loader.Core.GetSelNode(i);
                MaxNodeTree.QueueAddNode(node);
            }
            MaxNodeTree.EndUpdate();
        }

        private void removeNodeButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            MaxNodeTree.BeginUpdate();
            for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
            {
                IINode node = Loader.Core.GetSelNode(i);
                MaxNodeTree.QueueRemoveNode(node);
            }
            MaxNodeTree.EndUpdate();
        }
    }
}
