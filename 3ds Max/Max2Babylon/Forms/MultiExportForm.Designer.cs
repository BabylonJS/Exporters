namespace Max2Babylon
{
    partial class MultiExportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Panel panel_buttons;
            this.panel1 = new System.Windows.Forms.Panel();
            this.ExportItemGridView = new System.Windows.Forms.DataGridView();
            this.ColumnExport = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnNode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnFilePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_accept = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            panel_buttons = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExportItemGridView)).BeginInit();
            panel_buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ExportItemGridView);
            this.panel1.Controls.Add(panel_buttons);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(684, 311);
            this.panel1.TabIndex = 0;
            // 
            // ExportItemGridView
            // 
            this.ExportItemGridView.AllowUserToResizeColumns = false;
            this.ExportItemGridView.AllowUserToResizeRows = false;
            this.ExportItemGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ExportItemGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.ExportItemGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this.ExportItemGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.ExportItemGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ExportItemGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnExport,
            this.ColumnNode,
            this.ColumnFilePath});
            this.ExportItemGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportItemGridView.Location = new System.Drawing.Point(3, 3);
            this.ExportItemGridView.MultiSelect = false;
            this.ExportItemGridView.Name = "ExportItemGridView";
            this.ExportItemGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.ExportItemGridView.Size = new System.Drawing.Size(678, 272);
            this.ExportItemGridView.TabIndex = 8;
            this.ExportItemGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ExportItemGridView_CellDoubleClick);
            this.ExportItemGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.ExportItemGridView_CellValidating);
            this.ExportItemGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ExportItemGridView_CellValueChanged);
            // 
            // ColumnExport
            // 
            this.ColumnExport.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ColumnExport.HeaderText = "Export?";
            this.ColumnExport.Name = "ColumnExport";
            this.ColumnExport.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnExport.Width = 49;
            // 
            // ColumnNode
            // 
            this.ColumnNode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnNode.HeaderText = "Root node";
            this.ColumnNode.MaxInputLength = 1024;
            this.ColumnNode.Name = "ColumnNode";
            this.ColumnNode.ReadOnly = true;
            this.ColumnNode.Width = 82;
            // 
            // ColumnFilePath
            // 
            this.ColumnFilePath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnFilePath.HeaderText = "File path";
            this.ColumnFilePath.MaxInputLength = 1024;
            this.ColumnFilePath.Name = "ColumnFilePath";
            // 
            // panel_buttons
            // 
            panel_buttons.Controls.Add(this.btn_accept);
            panel_buttons.Controls.Add(this.btn_cancel);
            panel_buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel_buttons.Location = new System.Drawing.Point(3, 275);
            panel_buttons.MinimumSize = new System.Drawing.Size(406, 33);
            panel_buttons.Name = "panel_buttons";
            panel_buttons.Padding = new System.Windows.Forms.Padding(3);
            panel_buttons.Size = new System.Drawing.Size(678, 33);
            panel_buttons.TabIndex = 9;
            // 
            // btn_accept
            // 
            this.btn_accept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_accept.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_accept.Location = new System.Drawing.Point(435, 3);
            this.btn_accept.MaximumSize = new System.Drawing.Size(120, 27);
            this.btn_accept.MinimumSize = new System.Drawing.Size(120, 27);
            this.btn_accept.Name = "btn_accept";
            this.btn_accept.Size = new System.Drawing.Size(120, 27);
            this.btn_accept.TabIndex = 7;
            this.btn_accept.Text = "Accept";
            this.btn_accept.UseVisualStyleBackColor = true;
            this.btn_accept.Click += new System.EventHandler(this.btn_accept_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_cancel.Location = new System.Drawing.Point(555, 3);
            this.btn_cancel.MaximumSize = new System.Drawing.Size(120, 27);
            this.btn_cancel.MinimumSize = new System.Drawing.Size(120, 27);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(120, 27);
            this.btn_cancel.TabIndex = 6;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // MultiExportForm
            // 
            this.AcceptButton = this.btn_accept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(684, 311);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "MultiExportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multi-File Export";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ExportItemGridView)).EndInit();
            panel_buttons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView ExportItemGridView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnExport;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFilePath;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_accept;
    }
}