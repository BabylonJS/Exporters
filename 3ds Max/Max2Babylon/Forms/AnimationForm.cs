using System.Windows.Forms;
using Autodesk.Max;
using System.Collections.Generic;

namespace Max2Babylon
{
    using AnimationGroupInfo = AnimationGroupControl.AnimationGroupInfo;

    public partial class AnimationForm : Form
    {
        private readonly BabylonAnimationActionItem babylonAnimationAction;
        private List<AnimationGroupInfo> animationInfos;

		public AnimationForm(BabylonAnimationActionItem babylonAnimationAction)
		{
			InitializeComponent();

			this.babylonAnimationAction = babylonAnimationAction;
        }

        private void AnimationForm_Load(object sender, System.EventArgs e)
        {
            // populate animation list
            // use some scene-wide properties?
            //var rootNodes = Tools.GetSceneRootNodes(Loader.Global.IGameInterface);
            //foreach(IIGameNode node in rootNodes)
            //{
            //    for(int i = 0; i < node.ChildCount; ++i)
            //    {
            //        IIGameNode child = node.GetNodeChild(i);
            //    }
            //}

            // for now, dummy data
            animationList.BeginUpdate();
            animationList.Items.Add(new AnimationGroupInfo { Name = "Shoot", frameStart = 0, frameEnd = 10, NodeIDs = new List<int>() { 0, 1, 2 } });
            animationList.Items.Add(new AnimationGroupInfo { Name = "Walk", frameStart = 0, frameEnd = 10, NodeIDs = new List<int>() { 3, 4, 5 } });
            animationList.Items.Add(new AnimationGroupInfo { Name = "Run", frameStart = 5, frameEnd = 15, NodeIDs = new List<int>() { 0, 1, 2 } });
            animationList.EndUpdate();
            
        }
        
        private void createAnimationButton_Click(object sender, System.EventArgs e)
        {
            // get a unique name
            AnimationGroupInfo info = new AnimationGroupInfo();
            string baseName = info.Name;
            int i = 0;
            while (true)
            {
                bool found = false;
                foreach (AnimationGroupInfo animGroupinfo in animationList.Items)
                {
                    if (animGroupinfo.Name.Equals(info.Name))
                    {
                        info.Name = baseName + i.ToString();
                        ++i;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    break;
            }
            
            animationList.BeginUpdate();
            int index = animationList.Items.Add(info);
            animationList.EndUpdate();

            animationList.SetSelected(index, true);
        }

        private void deleteAnimationButton_Click(object sender, System.EventArgs e)
        {
            if (animationList.SelectedIndex < 0)
                return;

            int selectedIndex = animationList.SelectedIndex;
            animationList.Items.RemoveAt(animationList.SelectedIndex);
            animationList.SelectedIndex = System.Math.Min(selectedIndex, animationList.Items.Count-1);
        }

        private void animationList_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            animationGroupControl.SetAnimationGroupInfo(animationList.SelectedItem as AnimationGroupInfo);
        }
    }
}
