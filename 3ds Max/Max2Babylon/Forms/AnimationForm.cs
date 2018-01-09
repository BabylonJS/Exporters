using System.Windows.Forms;

namespace Max2Babylon
{
	public partial class AnimationForm : Form
	{
        private readonly BabylonAnimationActionItem babylonAnimationAction;

		public AnimationForm(BabylonAnimationActionItem babylonAnimationAction)
		{
			InitializeComponent();

			this.babylonAnimationAction = babylonAnimationAction;
		}
	}
}
