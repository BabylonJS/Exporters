using System.Windows.Forms;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
    class BabylonLoadAnimationFromContainers:ActionItem
    {

        public override bool ExecuteAction()
        {
            AnimationGroupList.LoadDataFromContainers();
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
            get { return "Babylon Load Animation From Containers"; }
        }

        public override string MenuText
        {
            get { return "&Babylon Load Animation From Containers..."; }
        }

        public override string DescriptionText
        {
            get { return "Load animation group from each containers"; }
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