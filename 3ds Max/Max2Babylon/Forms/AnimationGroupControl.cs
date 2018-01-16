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

        public event Action<AnimationGroup> InfoConfirmed;

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

                MaxNodeTree.QueueSetNodes(info.NodeHandles);
                MaxNodeTree.ApplyQueuedChanges(out List<uint> handles);
            }
            else
            {
                nameTextBox.Enabled = false;
                startTextBox.Enabled = false;
                endTextBox.Enabled = false;
                nameTextBox.Text = "";
                startTextBox.Text = "";
                endTextBox.Text = "";
                MaxNodeTree.QueueSetNodes(null);
                MaxNodeTree.ApplyQueuedChanges(out List<uint> handles);
            }
        }
        
        void ResetChanged()
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

            string newName = nameTextBox.Text;

            if (!int.TryParse(startTextBox.Text, out int newFrameStart))
                newFrameStart = currentInfo.FrameStart;
            if (!int.TryParse(endTextBox.Text, out int newFrameEnd))
                newFrameEnd = currentInfo.FrameEnd;

            List<uint> newHandles;
            bool nodesChanged = MaxNodeTree.ApplyQueuedChanges(out newHandles);

            bool changed = newName != currentInfo.Name || newFrameStart != currentInfo.FrameStart || newFrameEnd != currentInfo.FrameEnd || nodesChanged;
            
            if (!changed)
                return;

            currentInfo.Name = newName;
            currentInfo.FrameStart = newFrameStart;
            currentInfo.FrameEnd = newFrameEnd;

            if(nodesChanged)
                currentInfo.NodeHandles = newHandles;

            ResetChanged();

            InfoConfirmed?.Invoke(currentInfo);
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

            for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
            {
                IINode node = Loader.Core.GetSelNode(i);
                MaxNodeTree.QueueAddNode(node);
            }
        }

        private void removeNodeButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            MaxNodeTree.QueueRemoveNode(MaxNodeTree.SelectedNode);
        }
    }
}
