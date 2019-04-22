using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Max2Babylon
{
    public partial class MultiExportForm : Form
    {
        const bool Default_ExportItemSelected = true;

        ExportItemList exportItemList;

        public MultiExportForm(ExportItemList exportItemList)
        {
            InitializeComponent();
            ExportItemGridView_Populate(exportItemList);
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

        private ExportItem AddExportItem(DataGridViewRow row, string exportPath)
        {
            ExportItem item = new ExportItem(exportItemList.OutputFileExtension);
            item.SetExportFilePath(GetUniqueExportPath(exportPath));
            item.Selected = row.Cells[0].Value == null ? Default_ExportItemSelected : (bool)row.Cells[0].Value;
            exportItemList.Add(item);
            return item;
        }

        //! Add a new export item. Returns null if the given handle already exists in the export item list.
        private ExportItem TryAddExportItem(DataGridViewRow row, uint nodeHandle, string exportPath = null)
        {
            foreach(ExportItem existingItem in exportItemList)
            {
                if (existingItem.NodeHandle == nodeHandle)
                    return null;
            }

            ExportItem item = new ExportItem(exportItemList.OutputFileExtension, nodeHandle);
            item.SetExportFilePath(GetUniqueExportPath(exportPath != null ? exportPath : item.ExportFilePathRelative));
            item.Selected = row.Cells[0].Value == null ? Default_ExportItemSelected : (bool)row.Cells[0].Value;
            exportItemList.Add(item);
            return item;
        }

        #region  ExportItemGridView Events

        private void ExportItemGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (DataGridViewRow row in ExportItemGridView.SelectedRows)
                {
                    if (row.IsNewRow) continue;
                    ExportItem item = row.Tag as ExportItem;
                    if (item != null) exportItemList.Remove(item);
                    row.Tag = null;
                    ExportItemGridView.Rows.Remove(row);
                }
                foreach (DataGridViewCell cell in ExportItemGridView.SelectedCells)
                {
                    if (cell.OwningRow.IsNewRow) continue;
                    ExportItem item = cell.OwningRow.Tag as ExportItem;
                    if (item != null) exportItemList.Remove(item);
                    cell.OwningRow.Tag = null;
                    ExportItemGridView.Rows.Remove(cell.OwningRow);
                }
            }
        }

        private void ExportItemGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.ColumnIndex == ColumnFilePath.Index)
            {
                var row = ExportItemGridView.Rows[e.RowIndex];
                string str = row.Cells[e.ColumnIndex].Value as string;
                Uri uri;
                if (!string.IsNullOrWhiteSpace(str) && !Uri.TryCreate(str, UriKind.Absolute, out uri))
                {
                    e.Cancel = true;
                }
            }
        }

        private void ExportItemGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var row = ExportItemGridView.Rows[e.RowIndex];
            ExportItem item = row.Tag as ExportItem;

            if (e.ColumnIndex == ColumnExportCheckbox.Index)
            {
                if(item != null)
                    item.Selected = row.Cells[0].Value == null ? Default_ExportItemSelected : (bool)row.Cells[0].Value;
            }
            if (e.ColumnIndex == ColumnFilePath.Index)
            {
                string str = row.Cells[e.ColumnIndex].Value as string;

                if (item == null) item = AddExportItem(row, str);
                else item.SetExportFilePath(str);

                SetRowData(row, item);
            }
        }

        private void ExportItemGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // double-clicked node column cell, select a node!
            if(e.ColumnIndex == ColumnNode.Index)
            {
                if (Loader.Core.DoHitByNameDialog(null) && Loader.Core.SelNodeCount > 0)
                {
                    int highestRowIndexEdited = e.RowIndex;
                    var selectedRow = ExportItemGridView.Rows[e.RowIndex];
                    ExportItem existingItem = selectedRow.Tag as ExportItem;
                    IINode node = Loader.Core.GetSelNode(0);

                    if (existingItem == null)
                        existingItem = TryAddExportItem(selectedRow, node.Handle);
                    else existingItem.NodeHandle = node.Handle;
                    
                    // may be null after trying to add a node that already exists in another row
                    if(existingItem != null) SetRowData(selectedRow, existingItem);

                    // add remaining selected nodes as new rows
                    for (int i = 1; i < Loader.Core.SelNodeCount; ++i)
                    {
                        int rowIndex = ExportItemGridView.Rows.Add();
                        var newRow = ExportItemGridView.Rows[rowIndex];

                        ExportItem newItem = TryAddExportItem(newRow, Loader.Core.GetSelNode(i).Handle);

                        // may be null after trying to add a node that already exists in another row
                        if (newItem == null)
                            continue;

                        SetRowData(newRow, newItem);
                        highestRowIndexEdited = rowIndex;
                    }

                    // have to explicitly set it dirty for an edge case:
                    // when a new row is added "automatically-programmatically", through notify cell dirty and endedit(),
                    //   if the user then clicks on the checkbox of the newly added row,
                    //     it doesn't add a new row "automatically", whereas otherwise it will.
                    ExportItemGridView.CurrentCell = ExportItemGridView[e.ColumnIndex, highestRowIndexEdited];
                    ExportItemGridView.NotifyCurrentCellDirty(true);
                    ExportItemGridView.EndEdit();
                }
            }
        }

        private void ExportItemGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // necessary for checkboxes, problem with datagridview
            if (e.ColumnIndex == ColumnExportCheckbox.Index)
            {
                ExportItemGridView.EndEdit();
            }
        }

        #endregion

        private void btn_change_path_Click(object sender, EventArgs e)
        {
            if (ExportItemGridView.SelectedCells.Count <= 0) return;
            
            List<DataGridViewCell> pathCells = new List<DataGridViewCell>(ExportItemGridView.SelectedCells.Count);
            
            foreach (DataGridViewCell selectedCell in ExportItemGridView.SelectedCells)
            {
                DataGridViewCell matchingPathCell = selectedCell.OwningRow.Cells[ColumnFilePath.Index];
                if(pathCells.Contains(matchingPathCell)) continue;
                pathCells.Add(matchingPathCell);
            }

            if (pathCells.Count == 0) return;

            string firstSelectedCellPath = pathCells[0].Value as string;
            
            if (string.IsNullOrWhiteSpace(firstSelectedCellPath))
            {
                SetPathFileDialog.InitialDirectory = null;
                SetPathFileDialog.FileName = "FileName";
            }
            else
            {
                SetPathFileDialog.InitialDirectory = Path.GetDirectoryName(firstSelectedCellPath);
                SetPathFileDialog.FileName = Path.GetFileName(firstSelectedCellPath);
            }

            SetPathFileDialog.FileName = pathCells.Count <= 1 ? SetPathFileDialog.FileName : "FileName not used for multiple file path changes";

            if (SetPathFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string dir = Path.GetDirectoryName(SetPathFileDialog.FileName);
                string filename = Path.GetFileNameWithoutExtension(SetPathFileDialog.FileName);

                Action<DataGridViewCell, string> funcUpdatePath = (DataGridViewCell selectedCell, string forcedFileName) => 
                {
                    string oldFileName = Path.GetFileNameWithoutExtension(selectedCell.Value as string);
                    string oldExtension = Path.GetExtension(selectedCell.Value as string);
                        
                    if (forcedFileName != null && string.IsNullOrWhiteSpace(oldFileName))
                        return;

                    string newPath = Path.Combine(dir, forcedFileName ?? oldFileName);
                    newPath = Path.ChangeExtension(newPath, oldExtension);

                    // change cell value, which triggers value changed event to update the export item
                    selectedCell.Value = newPath;
                };

                if (pathCells.Count > 1)
                    foreach (DataGridViewCell selectedCell in pathCells)
                        funcUpdatePath(selectedCell, null);
                else funcUpdatePath(pathCells[0], SetPathFileDialog.FileName);
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
