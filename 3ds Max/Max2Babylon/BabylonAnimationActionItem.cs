using System.Windows.Forms;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
	public class BabylonAnimationActionItem : ActionItem
	{
		private AnimationForm form;

		public override bool ExecuteAction()
		{
			if (form == null || form.IsDisposed)
				form = new AnimationForm(this);
			form.Show();
			form.BringToFront();
			form.WindowState = FormWindowState.Normal;

			return true;
		}

		public void Close()
		{
			if (form == null)
			{
				return;
			}
			form.Dispose();
			form = null;
		}

		public override int Id_
		{
			get { return 2; }
		}

		public override string ButtonText
		{
			get { return "Babylon Animation Groups"; }
		}

		public override string MenuText
		{
			get { return "&Babylon Animation Groups..."; }
		}

		public override string DescriptionText
		{
			get { return "Manage babylon animation groups"; }
		}

		public override string CategoryText
		{
			get { return "Babylon"; }
		}

		public override bool IsChecked_
		{
			get { return false; }
		}

		public override bool IsItemVisible
		{
			get { return true; }
		}

		public override bool IsEnabled_
		{
			get { return true; }
		}
	}
}
