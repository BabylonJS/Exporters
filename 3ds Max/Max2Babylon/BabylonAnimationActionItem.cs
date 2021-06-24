using System.Windows.Forms;
using System;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
	public class BabylonAnimationActionItem : ActionItem
	{
        NativeWindow parentWindow;
        public static AnimationForm form;

		public override bool ExecuteAction()
		{
			if (form == null || form.IsDisposed)
				form = new AnimationForm(this);

            if (parentWindow == null)
                parentWindow = new NativeWindow();

            if (parentWindow.Handle == IntPtr.Zero)
                parentWindow.AssignHandle(Loader.Core.MAXHWnd);

            if (!form.Visible)
                form.Show(parentWindow);

            form.WindowState = FormWindowState.Normal;
            form.BringToFront();

            form.HighlightAnimationGroupOfSelection();

            return true;
		}

        public override void Dispose()
        {
            Cleanup();
            base.Dispose();
        }

        public void Close()
		{
            Cleanup();
        }

        private void Cleanup()
        {
            if (form != null)
            {
                if (!form.IsDisposed)
                {
                    form.Dispose();
                }
                form = null;
            }
            if (parentWindow != null)
            {
                parentWindow.ReleaseHandle();
                parentWindow = null;
            }
        }

		public override int Id_ => 3;

		public override string ButtonText
		{
			get { return "Babylon Animation Groups"; }
		}

		public override string MenuText
		{
			get { return "&Babylon Animation Groups"; }
		}

		public override string DescriptionText
		{
			get { return "Babylon - manage animation groups for this scene"; }
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
