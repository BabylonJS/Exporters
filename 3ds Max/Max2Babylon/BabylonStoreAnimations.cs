using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Max;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
    class BabylonSaveAnimations:ActionItem
    {

        public override bool ExecuteAction()
        {
            var selectedContainers = Tools.GetContainerInSelection();

            if (selectedContainers.Count <= 0)
            {
                AnimationGroupList.SaveDataToAnimationHelper();
                return true;
            }

            foreach (IIContainerObject containerObject in selectedContainers)
            {
                AnimationGroupList.SaveDataToContainerHelper(containerObject);
            }

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
            get { return "Babylon Store AnimationGroups..."; }
        }

        public override string MenuText
        {
            get
            {
                var selectedContainers = Tools.GetContainerInSelection();
                if (selectedContainers?.Count > 0)
                {
                    return "&Babylon Store AnimationGroups to selected containers...";
                }
                else
                {
                    return "&(Xref/Merge) Babylon Store AnimationGroups";
                }
            }
        }

        public override string DescriptionText
        {
            get { return "Copy AnimationGroups into a BabylonAnimationHelper or a BabylonContainerHelper"; }
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
