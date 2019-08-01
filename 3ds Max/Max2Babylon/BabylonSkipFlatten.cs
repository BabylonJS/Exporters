using System.Windows.Forms;
using Autodesk.Max;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
    class BabylonInvertSkipFlatten:ActionItem
    {

        public override bool ExecuteAction()
        {
            IINode sel = Loader.Core.GetSelNode(0);
            if (sel == null) return true;

            bool skip = sel.IsMarkedAsNotCollapsable();
            sel.SetUserPropBool("babylonjs_SkipFlatten", !skip);
            return true;
        }

        public void Close()
        {
            return;
        }

        public override int Id_
        {
            get { return 1; }
        }

        public override string ButtonText
        {
            get { return "Babylon Invert Skip Flatten Status"; }
        }

        public override string MenuText
        {
            get
            {
                IINode sel = Loader.Core.GetSelNode(0);
                if (sel == null)
                {
                    return "&Babylon Skip Flatten";
                }

                if (!sel.IsMarkedAsNotCollapsable())
                {
                    return "&Babylon Skip Flatten";
                }

                return "&Babylon Reset Flatten";
            }
        }

        public override string DescriptionText
        {
            get { return "Invert skip flatten status"; }
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
