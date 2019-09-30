using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Max;
using ActionItem = Autodesk.Max.Plugins.ActionItem;

namespace Max2Babylon
{
    class BabylonLoadAnimationFromContainers:ActionItem
    {

        public override bool ExecuteAction()
        {
            if (Loader.Core.SelNodeCount == 0)
            {
                MessageBox.Show("No Container selected");
                return false;
            }
#if MAX2020
            IINodeTab selection = Loader.Global.INodeTab.Create();
#else
            IINodeTab selection = Loader.Global.INodeTabNS.Create();
#endif
            Loader.Core.GetSelNodeTab(selection);
            List<IIContainerObject> selectedContainers = new List<IIContainerObject>();

            for (int i = 0; i < selection.Count; i++)
            {
#if MAX2015
                var selectedNode = selection[(IntPtr)i];
#else
                var selectedNode = selection[i];
#endif
                IIContainerObject containerObject  = Loader.Global.ContainerManagerInterface.IsContainerNode(selectedNode);
                if (containerObject != null)
                {
                    selectedContainers.Add(containerObject);
                }
            }

            if (selectedContainers.Count <= 0)
            {
                MessageBox.Show("No Container selected");
                return false;
            }

            foreach (IIContainerObject containerObject in selectedContainers)
            {
                AnimationGroupList.LoadDataFromContainer(containerObject);
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
            get { return "Babylon Load Animation From Selected Containers"; }
        }

        public override string MenuText
        {
            get { return "&Babylon Load Animation From Selected Containers..."; }
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