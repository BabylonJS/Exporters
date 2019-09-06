namespace Max2Babylon.Forms
{
    partial class LayerSelector
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
            this.confirmButton = new System.Windows.Forms.Button();
            this.cancelBtnClick = new System.Windows.Forms.Button();
            this.layerTreeView = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // confirmButton
            // 
            this.confirmButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.confirmButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.confirmButton.Location = new System.Drawing.Point(0, 0);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(150, 38);
            this.confirmButton.TabIndex = 1;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = true;
            this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // cancelBtnClick
            // 
            this.cancelBtnClick.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtnClick.Dock = System.Windows.Forms.DockStyle.Right;
            this.cancelBtnClick.Location = new System.Drawing.Point(179, 0);
            this.cancelBtnClick.Name = "cancelBtnClick";
            this.cancelBtnClick.Size = new System.Drawing.Size(150, 38);
            this.cancelBtnClick.TabIndex = 2;
            this.cancelBtnClick.Text = "Cancel";
            this.cancelBtnClick.UseVisualStyleBackColor = true;
            this.cancelBtnClick.Click += new System.EventHandler(this.cancelBtnClick_Click);
            // 
            // layerTreeView
            // 
            this.layerTreeView.CheckBoxes = true;
            this.layerTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerTreeView.Location = new System.Drawing.Point(0, 0);
            this.layerTreeView.Name = "layerTreeView";
            this.layerTreeView.Size = new System.Drawing.Size(329, 634);
            this.layerTreeView.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.confirmButton);
            this.panel1.Controls.Add(this.cancelBtnClick);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 634);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(329, 38);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.layerTreeView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(329, 634);
            this.panel2.TabIndex = 5;
            // 
            // LayerSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 672);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "LayerSelector";
            this.Text = "Layer to Export";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.Button cancelBtnClick;
        private System.Windows.Forms.TreeView layerTreeView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}