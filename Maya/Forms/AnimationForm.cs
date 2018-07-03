using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Maya2Babylon.Forms
{
    public partial class AnimationForm : Form
    {
        public event Action On_animationFormClosed;
        const string exportNonAnimatedNodesCheckBoxProperty = "babylonjs_animgroup_exportnonanimated";

        AnimationGroupList animationGroups = new AnimationGroupList();
        BindingSource animationListBinding = new BindingSource();

        #region Initialization

        public AnimationForm()
        {
            InitializeComponent();
        }

        private void AnimationForm_Load(object sender, EventArgs e)
        {
            animationGroups.LoadFromData();

            animationListBinding.DataSource = animationGroups;
            animationListBox.DataSource = animationListBinding;
            animationListBox.ClearSelected();

            animationGroupControl.SetAnimationGroupInfo(null);
            animationGroupControl.InfoChanged += animationGroupControl_InfoChanged;
            animationGroupControl.ConfirmPressed += animationGroupControl_ConfirmPressed;

            exportNonAnimatedNodesCheckBox.Checked = Loader.GetBoolProperty(exportNonAnimatedNodesCheckBoxProperty);
        }

        #endregion

        #region State change events

        /// <summary>
        /// Click on the "Create" button. It create a new animation group with default value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createAnimationButton_Click(object sender, EventArgs e)
        {
            AnimationGroup newAnimationGroup = new AnimationGroup();

            //get a unique name and guid
            string baseName = newAnimationGroup.Name;
            int i = 0;
            bool hasConflict = true;
            while (hasConflict)
            {
                hasConflict = false;
                foreach (AnimationGroup animationGroup in animationGroups)
                {
                    if (newAnimationGroup.Name.Equals(animationGroup.Name))
                    {
                        newAnimationGroup.Name = baseName + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                    if (newAnimationGroup.SerializedId.Equals(animationGroup.SerializedId))
                    {
                        newAnimationGroup.SerializedId = Guid.NewGuid();
                        hasConflict = true;
                        break;
                    }
                }
            }

            // save info and animation list entry
            animationGroups.Add(newAnimationGroup);
            animationGroups.SaveToData();
            animationListBinding.ResetBindings(false);

            // Select the new animation group
            animationListBox.SelectedItem = newAnimationGroup;

        }

        /// <summary>
        /// Click on the "Delete" button. It delete the selected animation group.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteAnimationButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = animationListBox.SelectedIndex;

            if (selectedIndex < 0)
                return;

            AnimationGroup selectedItem = (AnimationGroup)animationListBox.SelectedItem;

            // delete item
            selectedItem.DeleteFromData();

            // update list
            animationGroups.Remove(selectedItem);
            animationGroups.SaveToData();
            animationListBinding.ResetBindings(false);

            // get new selected item at the current index, if any
            selectedIndex = Math.Min(selectedIndex, animationListBox.Items.Count - 1);
            selectedItem = selectedIndex < 0 ? null : (AnimationGroup)animationListBox.Items[selectedIndex];
            animationListBox.SelectedItem = selectedItem;
        }

        private void animationList_SelectedValueChanged(object sender, EventArgs e)
        {
            animationGroupControl.SetAnimationGroupInfo((AnimationGroup)animationListBox.SelectedItem);
        }

        // Typically called when the user presses confirm, but can also happen when scene changes are detected.
        private void animationGroupControl_InfoChanged(AnimationGroup info)
        {
            info.SaveToData();
            animationListBinding.ResetBindings(false);
        }

        private void animationGroupControl_ConfirmPressed(AnimationGroup info)
        {
            animationListBox.SelectedItem = info;
        }

        #endregion

        #region Windows.Form Events

        private void AnimationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            On_animationFormClosed();
        }

        #endregion

        private void exportNonAnimatedNodesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Loader.SetBoolProperty(exportNonAnimatedNodesCheckBoxProperty, exportNonAnimatedNodesCheckBox.Checked);
        }
    }
}
