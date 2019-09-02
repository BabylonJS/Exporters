using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Max;

namespace Max2Babylon.Forms
{
    public partial class LayerSelector : Form
    {
        public List<IILayer> selectedLayers = new List<IILayer>();

        public event EventHandler OnConfirmButtonClicked;

        public LayerSelector()
        {
            InitializeComponent();
            FillTreeView();
        }

        private void FillTreeView()
        {
            layerTreeView.Nodes.Clear();
            List<IILayer> rootLayers = LayerUtilities.RootLayers();
            for (int i = 0; i < rootLayers.Count; i++)
            {
                layerTreeView.Nodes.Add(rootLayers[i].Name);
                BuildLayerTreeRecusively(rootLayers[i],i,layerTreeView.Nodes);
            }
        }

        private void BuildLayerTreeRecusively(IILayer layer,int index,TreeNodeCollection treeNodeCollection)
        {
            for (int i = 0; i < layer.NumOfChildLayers; i++)
            {
                IILayer l = layer.GetChildLayer(i);
                treeNodeCollection[index].Nodes.Add(l.Name);
                BuildLayerTreeRecusively(l, i, treeNodeCollection[index].Nodes);
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
                    selectedLayers.Add(l);
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
