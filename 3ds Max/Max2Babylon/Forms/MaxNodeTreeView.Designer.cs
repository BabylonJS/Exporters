namespace Max2Babylon
{
    partial class MaxNodeTreeView
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.NodeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.IncludeNodeContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExcludeNodeContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NodeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // NodeContextMenu
            // 
            this.NodeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.IncludeNodeContextMenuItem,
            this.ExcludeNodeContextMenuItem});
            this.NodeContextMenu.Name = "contextMenuStrip1";
            this.NodeContextMenu.Size = new System.Drawing.Size(147, 48);
            this.NodeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.NodeContextMenu_Opening);
            this.NodeContextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.NodeContextMenu_ItemClicked);
            // 
            // IncludeNodeContextMenuItem
            // 
            this.IncludeNodeContextMenuItem.Name = "IncludeNodeContextMenuItem";
            this.IncludeNodeContextMenuItem.Size = new System.Drawing.Size(146, 22);
            this.IncludeNodeContextMenuItem.Tag = "Include";
            this.IncludeNodeContextMenuItem.Text = "Include Node";
            // 
            // ExcludeNodeContextMenuItem
            // 
            this.ExcludeNodeContextMenuItem.Name = "ExcludeNodeContextMenuItem";
            this.ExcludeNodeContextMenuItem.Size = new System.Drawing.Size(146, 22);
            this.ExcludeNodeContextMenuItem.Tag = "Exclude";
            this.ExcludeNodeContextMenuItem.Text = "Exclude Node";
            // 
            // MaxNodeTreeView
            // 
            this.AllowDrop = true;
            this.ContextMenuStrip = this.NodeContextMenu;
            this.LineColor = System.Drawing.Color.Black;
            this.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.MaxNodeTreeView_NodeMouseClick);
            this.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.MaxNodeTreeView_NodeMouseDoubleClick);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MaxNodeTreeView_KeyUp);
            this.NodeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip NodeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem IncludeNodeContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExcludeNodeContextMenuItem;
    }
}
