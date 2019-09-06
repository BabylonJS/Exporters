using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Max;

namespace Max2Babylon.Forms
{
    public partial class LayerSelector : Form
    {
        public List<IILayer> SelectedLayers { get; private set; }

        public event EventHandler OnConfirmButtonClicked;

        public LayerSelector()
        {
            InitializeComponent();
            SelectedLayers = new List<IILayer>();
        }

        public void FillLayerSelector(List<IILayer> previoslySelected)
        {
            layerTreeView.Nodes.Clear();
            List<IILayer> rootLayers = LayerUtilities.RootLayers();
            for (int i = 0; i < rootLayers.Count; i++)
            {
                IILayer layer = rootLayers[i];
                TreeNode layerNode = layerTreeView.Nodes.Add(layer.Name);

                if (previoslySelected!=null && previoslySelected.Contains(layer))
                {
                    layerNode.Checked = true;
                }

                BuildLayerTreeRecusively(layer,i,layerTreeView.Nodes);
            }
        }

        private void BuildLayerTreeRecusively(IILayer layer,int index,TreeNodeCollection treeNodeCollection)
        {
            for (int i = 0; i < layer.NumOfChildLayers; i++)
            {
                IILayer childLayer = layer.GetChildLayer(i);
                TreeNode layerNode = treeNodeCollection[index].Nodes.Add(childLayer.Name);

                if (SelectedLayers.Contains(layer))
                {
                    layerNode.Checked = true;
                }
                BuildLayerTreeRecusively(childLayer, i, treeNodeCollection[index].Nodes);
            }
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <  layerTreeView.Nodes.Count; i++)
            {
                TreeNode nodeLayer = layerTreeView.Nodes[i];
                if (nodeLayer.Checked)
                {
                    IILayer l = Loader.Core.LayerManager.GetLayer(nodeLayer.Text);
                    SelectedLayers.Add(l);
                }
            }

            OnConfirmButtonClicked?.Invoke(this,EventArgs.Empty);
            Dispose();
            
        }

        private void cancelBtnClick_Click(object sender, EventArgs e)
        {
            Dispose();
        }

    }
}
