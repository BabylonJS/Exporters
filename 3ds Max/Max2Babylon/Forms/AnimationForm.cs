using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Max;

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
            Tools.InitializeGuidNodesMap();
            this.babylonAnimationAction = babylonAnimationAction;
        }

        private void AnimationForm_Load(object sender, EventArgs e)
        {
            animationGroups.LoadFromData();

            animationListBinding.DataSource = animationGroups;
            AnimationListBox.DataSource = animationListBinding;
            AnimationListBox.ClearSelected();

            animationGroupControl.SetAnimationGroupInfo(null);
            animationGroupControl.InfoChanged += animationGroupControl_InfoChanged;
            animationGroupControl.ConfirmPressed += animationGroupControl_ConfirmPressed;

            Tools.PrepareCheckBox(exportNonAnimatedNodesCheckBox, Loader.Core.RootNode, "babylonjs_animgroup_exportnonanimated");
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

            AnimationListBox.ClearSelected();
            AnimationListBox.SelectedItem = info;
        }

        private void deleteAnimationButton_Click(object sender, EventArgs e)
        {
           
            if (AnimationListBox.SelectedIndices.Count <= 0)
            {
                MessageBox.Show("Select at least one Animation Group");
                return;
            }

            //retrieve list of selected animation groups to delete 
            List<AnimationGroup> deletedAnimationGroups = new List<AnimationGroup>();

            foreach (int selectedIndex in AnimationListBox.SelectedIndices)
            {

                if (selectedIndex < 0)
                    return;

                deletedAnimationGroups.Add((AnimationGroup)AnimationListBox.Items[selectedIndex]);
                
            }

            foreach (AnimationGroup deletedItem in deletedAnimationGroups)
            {
                // delete item
                deletedItem.DeleteFromData();

                // update list
                animationGroups.Remove(deletedItem);
            }

            animationGroups.SaveToData();
            animationListBinding.ResetBindings(false);
            Loader.Global.SetSaveRequiredFlag(true, false);

            // get new selected item at the current index, if any
            if (AnimationListBox.Items.Count > 0)
            {
                int newIndex = Math.Min(AnimationListBox.SelectedIndices[0], AnimationListBox.Items.Count - 1);
                AnimationGroup newSelectedItem = newIndex < 0 ? null : (AnimationGroup)AnimationListBox.Items[newIndex];
                AnimationListBox.SelectedItem = newSelectedItem;
            }
        }

        private void animationList_SelectedValueChanged(object sender, EventArgs e)
        {
            animationGroupControl.SetAnimationGroupInfo((AnimationGroup)AnimationListBox.SelectedItem);
        }

        // Typically called when the user presses confirm, but can also happen when scene changes are detected.
        private void animationGroupControl_InfoChanged(AnimationGroup info)
        {
            info.SaveToData();
            animationListBinding.ResetBindings(false);
            Loader.Global.SetSaveRequiredFlag(true, false);
        }

        private void animationGroupControl_ConfirmPressed(AnimationGroup info)
        {
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

        private void exportNonAnimatedNodesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Tools.UpdateCheckBox(exportNonAnimatedNodesCheckBox, Loader.Core.RootNode, "babylonjs_animgroup_exportnonanimated");
            Loader.Global.SetSaveRequiredFlag(true, false);
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            if (AnimationListBox.SelectedIndices.Count <= 0)
            {
                MessageBox.Show("Select at least one Animation Group");
                return;
            }

            List<AnimationGroup> exportList = AnimationListBox.SelectedItems.Cast<AnimationGroup>().ToList();

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JSON File|*.json";
            saveFileDialog1.Title = "Save an Export Info File";

            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            animationGroups.SaveToJson(saveFileDialog1.FileName, exportList);
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            LoadDialog();
        }

        private void MergeBtn_Click(object sender, EventArgs e)
        {
            LoadDialog(true);
        }

        private void LoadDialog(bool merge = false)
        {
            string jsonPath = string.Empty;
            string jsonContent = string.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "json files (*.json)|*.json";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    jsonPath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        BindingSource bindingSource = (BindingSource)AnimationListBox.DataSource;
                        System.Collections.IList SourceList = bindingSource.List;
                        SourceList.Clear();
                        jsonContent = reader.ReadToEnd();
                        animationGroups.LoadFromJson(jsonContent, merge);
                        animationListBinding.ResetBindings(false);
                        Loader.Global.SetSaveRequiredFlag(true, false);
                    }
                }
            }
        }


        //remove aniamtion groups with no nodes
        private void cleanBtn_Click(object sender, EventArgs e)
        {
            foreach (AnimationGroup item in AnimationListBox.Items)
            {
                
                if (item.AnimationGroupNodes != null && item.AnimationGroupNodes.Count>0)
                {
                   continue;
                }
                item.DeleteFromData();
                animationGroups.Remove(item);
            }
            animationGroups.SaveToData();
            animationListBinding.ResetBindings(false);
            Loader.Global.SetSaveRequiredFlag(true, false);
        }

        public void HighlightAnimationGroupOfSelection()
        {
            AnimationListBox.ClearSelected();
            IINode node = Loader.Core.GetSelNode(0);
            if (node != null)
            {
                for (int i = 0; i < animationGroups.Count; i++)
                {
                    if (animationGroups[i].NodeGuids.Contains(node.GetGuid()))
                    {
                        AnimationListBox.SelectedItem = animationGroups[i];
                    }
                }
            }
        }
    }
}
