using System.Windows.Forms;
using System;
using System.Collections.Generic;

namespace Max2Babylon
{
    public partial class AnimationForm : Form
    {
        const string s_AnimationListPropertyName = "babylonjs_AnimationList";

        private readonly BabylonAnimationActionItem babylonAnimationAction;

        #region Initialization

        public AnimationForm(BabylonAnimationActionItem babylonAnimationAction)
        {
            InitializeComponent();

            this.babylonAnimationAction = babylonAnimationAction;
        }

        private void AnimationForm_Load(object sender, EventArgs e)
        {
            string[] animationPropertyNames = Loader.Core.RootNode.GetStringArrayProperty(s_AnimationListPropertyName);
            
            animationList.BeginUpdate();
            foreach (string propertyNameStr in animationPropertyNames)
            {
                AnimationGroup info = new AnimationGroup();
                info.LoadFromData(propertyNameStr);
                animationList.Items.Add(info);
            }
            animationList.EndUpdate();

            animationGroupControl.InfoConfirmed += animationGroupControl_InfoSaved;
            animationGroupControl.SetAnimationGroupInfo(null);
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
                foreach (AnimationGroup animGroupinfo in animationList.Items)
                {
                    if (info.Name.Equals(animGroupinfo.Name))
                    {
                        info.Name = baseName + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                    if (info.SerializedId.Equals(animGroupinfo.SerializedId))
                    {
                        info.SerializedId = Guid.NewGuid();
                        hasConflict = true;
                        break;
                    }
                }
            }

            // save info and animation list entry
            info.SaveToData();
            List<string> animationGuidList = new List<string>(Loader.Core.RootNode.GetStringArrayProperty(s_AnimationListPropertyName));
            animationGuidList.Add(info.GetPropertyName());
            Loader.Core.RootNode.SetStringArrayProperty(s_AnimationListPropertyName, animationGuidList);
            Loader.Global.SetSaveRequiredFlag(true, false);

            animationList.BeginUpdate();
            int index = animationList.Items.Add(info);
            animationList.EndUpdate();

            animationList.SetSelected(index, true);
        }

        private void deleteAnimationButton_Click(object sender, EventArgs e)
        {
            if (animationList.SelectedIndex < 0)
                return;

            AnimationGroup selectedItem = animationList.SelectedItem as AnimationGroup;
            if (selectedItem != null)
            {
                // delete animation list entry
                List<string> animationGuidList = new List<string>(Loader.Core.RootNode.GetStringArrayProperty(s_AnimationListPropertyName));
                animationGuidList.Remove(selectedItem.GetPropertyName());
                Loader.Core.RootNode.SetStringArrayProperty(s_AnimationListPropertyName, animationGuidList);

                // delete item
                selectedItem.DeleteFromData();
                
                Loader.Global.SetSaveRequiredFlag(true, false);
            }
            int selectedIndex = animationList.SelectedIndex;
            animationList.Items.RemoveAt(animationList.SelectedIndex);
            animationList.SelectedIndex = Math.Min(selectedIndex, animationList.Items.Count - 1);
        }

        private void animationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            animationGroupControl.SetAnimationGroupInfo(animationList.SelectedItem as AnimationGroup);
        }

        private void animationGroupControl_InfoSaved(AnimationGroup info)
        {
            info.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);

            // hacky but effective way to refresh the list item without requiring data bindings
            animationList.Items[animationList.SelectedIndex] = animationList.Items[animationList.SelectedIndex];
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
