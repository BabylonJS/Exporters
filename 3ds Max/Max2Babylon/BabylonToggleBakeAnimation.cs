using System.Windows.Forms;
using Autodesk.Max;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
    class BabylonToggleBakeAnimation:ActionItem
    {

        public override bool ExecuteAction()
        {
            IINode sel = Loader.Core.GetSelNode(0);
            if (sel == null) return true;

            bool bakeAnimation = sel.IsMarkedAsObjectToBakeAnimation();
            sel.SetUserPropBool("babylonjs_BakeAnimation", !bakeAnimation);
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
            get { return "Babylon Toggle Bake Animation Status"; }
        }

        public override string MenuText
        {
            get
            {
                IINode sel = Loader.Core.GetSelNode(0);
                if (sel == null)
                {
                    return "&Bake Animation - Disabled";
                }

                if (!sel.IsMarkedAsObjectToBakeAnimation())
                {
                    return "&Bake Animation - Disabled";
                }

                return "&Bake Animation - Enabled";
            }
        }

        public override string DescriptionText
        {
            get { return "Toggle Bake Animation status"; }
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
