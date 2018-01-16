using System.Windows.Forms;
using System;
using System.Collections.Generic;

namespace Max2Babylon
{
    public partial class AnimationForm : Form
    {
        const string s_AnimationListPropertyName = "babylonjs_AnimationList";

        private readonly BabylonAnimationActionItem babylonAnimationAction;

        AnimationGroupList animationGroups = new AnimationGroupList();
        BindingSource animationListBinding = new BindingSource();

        #region Initialization

        public AnimationForm(BabylonAnimationActionItem babylonAnimationAction)
        {
            InitializeComponent();

            this.babylonAnimationAction = babylonAnimationAction;
        }

        private void AnimationForm_Load(object sender, EventArgs e)
        {
            animationGroups.LoadFromData();

            animationListBinding.DataSource = animationGroups;
            AnimationListBox.DataSource = animationListBinding;
            AnimationListBox.ClearSelected();

            animationGroupControl.SetAnimationGroupInfo(null);
            animationGroupControl.InfoConfirmed += animationGroupControl_InfoConfirmed;
        }

        #endregion

        #region State change events

        private void createAnimationButton_Click(object sender, EventArgs e)
        {
            AnimationGroup info = new AnimationGroup();
            
            // get a unique name and guid
            string baseName = info.Name;
            int i = 0;
            bool hasConflict = true;
            while (hasConflict)
            {
                hasConflict = false;
                foreach (AnimationGroup animationGroup in animationGroups)
                {
                    if (info.Name.Equals(animationGroup.Name))
                    {
                        info.Name = baseName + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                    if (info.SerializedId.Equals(animationGroup.SerializedId))
                    {
                        info.SerializedId = Guid.NewGuid();
                        hasConflict = true;
                        break;
                    }
                }
            }

            // save info and animation list entry
            animationGroups.Add(info);
            animationGroups.SaveToData();
            animationListBinding.ResetBindings(false);
            Loader.Global.SetSaveRequiredFlag(true, false);

            AnimationListBox.SelectedItem = info;
        }

        private void deleteAnimationButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = AnimationListBox.SelectedIndex;

            if (selectedIndex < 0)
                return;

            AnimationGroup selectedItem = (AnimationGroup)AnimationListBox.SelectedItem;
            
            // delete item
            selectedItem.DeleteFromData();

            // update list
            animationGroups.Remove(selectedItem);
            animationGroups.SaveToData();
            animationListBinding.ResetBindings(false);
            Loader.Global.SetSaveRequiredFlag(true, false);
            
            AnimationListBox.SelectedIndex = Math.Min(selectedIndex, AnimationListBox.Items.Count - 1);
        }

        private void animationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            animationGroupControl.SetAnimationGroupInfo((AnimationGroup)AnimationListBox.SelectedItem);
        }

        private void animationGroupControl_InfoConfirmed(AnimationGroup info)
        {
            info.SaveToData();
            animationListBinding.ResetBindings(false);
            Loader.Global.SetSaveRequiredFlag(true, false);
            
            AnimationListBox.SelectedItem = info;
        }

        #endregion

        #region Windows.Form Events

        private void AnimationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            babylonAnimationAction.Close();
        }

        private void AnimationForm_Activated(object sender, EventArgs e)
        {
            Loader.Global.DisableAccelerators();
        }

        private void AnimationForm_Deactivate(object sender, EventArgs e)
        {
            Loader.Global.EnableAccelerators();
        }

        #endregion
    }
}
