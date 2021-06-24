using System.Windows.Forms;
using Autodesk.Max;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
    class BabylonSkipFlattenToggle:ActionItem
    {

        public override bool ExecuteAction()
        {
            IINode sel = Loader.Core.GetSelNode(0);
            if (sel == null) return true;

            bool doNotFlatten = sel.IsMarkedAsNotFlattenable();
            sel.SetUserPropBool("babylonjs_DoNotFlatten", !doNotFlatten);
            return true;
        }

        public void Close()
        {
            return;
        }

        public override int Id_ => 6;

        public override string ButtonText
        {
            get { return "Babylon Toggle Skip Flatten Status"; }
        }

        public override string MenuText
        {
            get
            {
                IINode sel = Loader.Core.GetSelNode(0);
                if (sel == null)
                {
                    return "&Node Flattening - Disabled";
                }

                if (!sel.IsMarkedAsNotFlattenable())
                {
                    return "&Node Flattening - Disabled";
                }

                return "&Node Flattening - Enabled";
            }
        }

        public override string DescriptionText
        {
            get { return "Babylon - Toggle skip flatten status"; }
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
