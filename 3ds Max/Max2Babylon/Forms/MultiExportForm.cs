using Autodesk.Max;
using System;
using System.IO;
using System.Windows.Forms;

namespace Max2Babylon
{
    public partial class MultiExportForm : Form
    {
        ExportItemList exportItemList;

        public MultiExportForm(ExportItemList exportItemList)
        {
            InitializeComponent();
            ExportItemGridView_Populate(exportItemList);
            ExportItemGridView.KeyDown += ExportItemGridView_KeyDown;
        }

        private void ExportItemGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                foreach(DataGridViewRow row in ExportItemGridView.SelectedRows)
                {
                    if (row.IsNewRow) continue;
                    ExportItem item = row.Tag as ExportItem;
                    if (item != null) exportItemList.Remove(item);
                    row.Tag = null;
                    ExportItemGridView.Rows.Remove(row);
                }
                foreach(DataGridViewCell cell in ExportItemGridView.SelectedCells)
                {
                    if (cell.OwningRow.IsNewRow) continue;
                    ExportItem item = cell.OwningRow.Tag as ExportItem;
                    if (item != null) exportItemList.Remove(item);
                    cell.OwningRow.Tag = null;
                    ExportItemGridView.Rows.Remove(cell.OwningRow);
                }
            }
        }

        private void ExportItemGridView_Populate(ExportItemList exportItemList)
        {
            ExportItemGridView.Rows.Clear();
            this.exportItemList = exportItemList;
            
            foreach (var item in exportItemList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewCheckBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                SetRowData(row, item);
                ExportItemGridView.Rows.Add(row);
            }
        }

        private void SetRowData(DataGridViewRow row, ExportItem item)
        {
            row.Tag = item;
            row.Cells[0].Value = item.Selected;
            row.Cells[1].Value = item.NodeName;
            row.Cells[2].Value = item.ExportFilePathAbsolute;
        }

        private void ExportItemGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.ColumnIndex == ColumnFilePath.Index)
            {
                var row = ExportItemGridView.Rows[e.RowIndex];
                string str = row.Cells[e.ColumnIndex].Value as string;
                if(!string.IsNullOrWhiteSpace(str) && !Uri.TryCreate(str, UriKind.Absolute, out Uri uri))
                {
                    e.Cancel = true;
                }
            }
        }

        private void ExportItemGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return; // not sure why but this happens when opening the form

            if (e.ColumnIndex == ColumnFilePath.Index)
            {
                var row = ExportItemGridView.Rows[e.RowIndex];
                ExportItem item = row.Tag as ExportItem;
                string str = row.Cells[e.ColumnIndex].Value as string;

                if (item == null)
                {
                    item = new ExportItem(exportItemList.OutputFileExtension);
                    item.SetExportFilePath(GetUniqueExportPath(str));
                    exportItemList.Add(item);
                }

                item.SetExportFilePath(str);
                SetRowData(row, item);
            }
        }
        
        private string GetUniqueExportPath(string initialPath)
        {
            string dir = Path.GetDirectoryName(initialPath);
            string filename = Path.GetFileNameWithoutExtension(initialPath);
            string filepathNoExt = Path.Combine(dir, filename);
            string ext = Path.GetExtension(initialPath);

            string path = initialPath;
            int fileCounter = 0;
            while (exportItemList.Find(exportItem => exportItem.ExportFilePathRelative == path) != null)
            {
                path = Path.ChangeExtension(path, null);
                path = Path.Combine(filepathNoExt + fileCounter.ToString());
                path = Path.ChangeExtension(path, ext);
                ++fileCounter;
            }

            return path;
        }

        private void ExportItemGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // double-clicked node column cell, select a node!
            if(e.ColumnIndex == ColumnNode.Index)
            {
                if (Loader.Core.DoHitByNameDialog(null) && Loader.Core.SelNodeCount > 0)
                {
                    var row = ExportItemGridView.Rows[e.RowIndex];
                    ExportItem item = row.Tag as ExportItem;
                    IINode node = Loader.Core.GetSelNode(0);
                    if (item == null)
                    {
                        item = new ExportItem(exportItemList.OutputFileExtension, node.Handle);
                        item.SetExportFilePath(GetUniqueExportPath(item.ExportFilePathRelative));
                        exportItemList.Add(item);
                    }
                    else
                    {
                        item.NodeHandle = node.Handle;
                    }
                    SetRowData(row, item);
                    ExportItemGridView.NotifyCurrentCellDirty(true);
                }
            }
        }

        private void btn_accept_Click(object sender, EventArgs e)
        { 
            exportItemList.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            // don't save
        }
    }
}
