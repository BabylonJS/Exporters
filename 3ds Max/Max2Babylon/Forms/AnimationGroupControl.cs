using System.Windows.Forms;
using System.Collections.Generic;

namespace Max2Babylon
{
    public partial class AnimationGroupControl : UserControl
    {
        public class AnimationGroupInfo
        {
            public string Name = "Animation";
            public int frameStart = 0;
            public int frameEnd = 100;
            public List<int> NodeIDs = new List<int>();

            public AnimationGroupInfo() { }
            public AnimationGroupInfo(AnimationGroupInfo other)
            {
                DeepCopyFrom(other);
            }

            public void DeepCopyFrom(AnimationGroupInfo other)
            {
                Name = other.Name;
                frameStart = other.frameStart;
                frameEnd = other.frameEnd;
                NodeIDs.Clear();
                NodeIDs.AddRange(other.NodeIDs);
            }

            static string s_DisplayNameFormat = "{0} ({1:d}, {2:d})";
            public override string ToString()
            {
                return string.Format(s_DisplayNameFormat, Name, frameStart, frameEnd);
            }
        }

        AnimationGroupInfo currentInfo;

        public AnimationGroupControl()
        {
            InitializeComponent();
        }

        public void SetAnimationGroupInfo(AnimationGroupInfo info)
        {
            this.currentInfo = info;
            SetFields();
        }

        void SetFields()
        {
            if (currentInfo == null)
            {
                nameTextBox.Enabled = false;
                nameTextBox.Text = "";

                startTextBox.Enabled = false;
                startTextBox.Text = "";

                endTextBox.Enabled = false;
                endTextBox.Text = "";
            }
            else
            {
                nameTextBox.Enabled = true;
                nameTextBox.Text = currentInfo.Name.ToString();

                startTextBox.Enabled = true;
                startTextBox.Text = currentInfo.frameStart.ToString();

                endTextBox.Enabled = true;
                endTextBox.Text = currentInfo.frameEnd.ToString();
            }
        }

        private void nameTextBox_TextChanged(object sender, System.EventArgs e)
        {
        }

        private void nameTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
