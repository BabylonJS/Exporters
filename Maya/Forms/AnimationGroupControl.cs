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

            confirmedInfo.Name = newName;
            confirmedInfo.FrameStart = newFrameStart;
            confirmedInfo.FrameEnd = newFrameEnd;

            ResetChangedTextBoxColors();

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
        }

        private void removeNodeButton_Click(object sender, EventArgs e)
        {
        }
    }
}
