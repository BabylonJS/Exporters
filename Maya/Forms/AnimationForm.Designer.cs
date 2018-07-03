namespace Maya2Babylon.Forms
{
    partial class AnimationForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.createAnimationButton = new System.Windows.Forms.Button();
            this.deleteAnimationButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.animationListBox = new System.Windows.Forms.ListBox();
            this.ExportPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.exportNonAnimatedNodesCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.animationGroupControl = new Maya2Babylon.Forms.AnimationGroupControl();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.ExportPropertiesGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.createAnimationButton);
            this.panel1.Controls.Add(this.deleteAnimationButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(244, 29);
            this.panel1.TabIndex = 4;
            // 
            // createAnimationButton
            // 
            this.createAnimationButton.AutoSize = true;
            this.createAnimationButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.createAnimationButton.Location = new System.Drawing.Point(3, 3);
            this.createAnimationButton.Name = "createAnimationButton";
            this.createAnimationButton.Size = new System.Drawing.Size(48, 23);
            this.createAnimationButton.TabIndex = 1;
            this.createAnimationButton.Text = "Create";
            this.createAnimationButton.UseVisualStyleBackColor = true;
            this.createAnimationButton.Click += new System.EventHandler(this.createAnimationButton_Click);
            // 
            // deleteAnimationButton
            // 
            this.deleteAnimationButton.AutoSize = true;
            this.deleteAnimationButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.deleteAnimationButton.Location = new System.Drawing.Point(57, 3);
            this.deleteAnimationButton.Name = "deleteAnimationButton";
            this.deleteAnimationButton.Size = new System.Drawing.Size(48, 23);
            this.deleteAnimationButton.TabIndex = 2;
            this.deleteAnimationButton.Text = "Delete";
            this.deleteAnimationButton.UseVisualStyleBackColor = true;
            this.deleteAnimationButton.Click += new System.EventHandler(this.deleteAnimationButton_Click);
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.animationListBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 45);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(244, 208);
            this.panel2.TabIndex = 5;
            // 
            // animationListBox
            // 
            this.animationListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationListBox.HorizontalScrollbar = true;
            this.animationListBox.Location = new System.Drawing.Point(0, 0);
            this.animationListBox.Name = "animationListBox";
            this.animationListBox.Size = new System.Drawing.Size(244, 208);
            this.animationListBox.Sorted = true;
            this.animationListBox.TabIndex = 0;
            this.animationListBox.SelectedValueChanged += new System.EventHandler(this.animationList_SelectedValueChanged);
            // 
            // ExportPropertiesGroupBox
            // 
            this.ExportPropertiesGroupBox.Controls.Add(this.exportNonAnimatedNodesCheckBox);
            this.ExportPropertiesGroupBox.Location = new System.Drawing.Point(12, 12);
            this.ExportPropertiesGroupBox.Name = "ExportPropertiesGroupBox";
            this.ExportPropertiesGroupBox.Size = new System.Drawing.Size(476, 46);
            this.ExportPropertiesGroupBox.TabIndex = 7;
            this.ExportPropertiesGroupBox.TabStop = false;
            this.ExportPropertiesGroupBox.Text = "Animation Group Options";
            // 
            // exportNonAnimatedNodesCheckBox
            // 
            this.exportNonAnimatedNodesCheckBox.AutoSize = true;
            this.exportNonAnimatedNodesCheckBox.Location = new System.Drawing.Point(6, 19);
            this.exportNonAnimatedNodesCheckBox.Name = "exportNonAnimatedNodesCheckBox";
            this.exportNonAnimatedNodesCheckBox.Size = new System.Drawing.Size(185, 17);
            this.exportNonAnimatedNodesCheckBox.TabIndex = 6;
            this.exportNonAnimatedNodesCheckBox.Text = "Export non-animated node targets";
            this.exportNonAnimatedNodesCheckBox.UseVisualStyleBackColor = true;
            this.exportNonAnimatedNodesCheckBox.CheckedChanged += new System.EventHandler(this.exportNonAnimatedNodesCheckBox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.MinimumSize = new System.Drawing.Size(114, 256);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 256);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animations";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(12, 64);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1MinSize = 190;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.animationGroupControl);
            this.splitContainer1.Panel2MinSize = 190;
            this.splitContainer1.Size = new System.Drawing.Size(476, 256);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 5;
            // 
            // animationGroupControl
            // 
            this.animationGroupControl.AutoSize = true;
            this.animationGroupControl.BackColor = System.Drawing.SystemColors.Control;
            this.animationGroupControl.ChangedTextColor = System.Drawing.Color.Red;
            this.animationGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationGroupControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.animationGroupControl.Location = new System.Drawing.Point(0, 0);
            this.animationGroupControl.MinimumSize = new System.Drawing.Size(295, 256);
            this.animationGroupControl.Name = "animationGroupControl";
            this.animationGroupControl.Size = new System.Drawing.Size(295, 256);
            this.animationGroupControl.TabIndex = 3;
            // 
            // AnimationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(500, 332);
            this.Controls.Add(this.ExportPropertiesGroupBox);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(516, 371);
            this.Name = "AnimationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Babylon.js - Animation Groups";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AnimationForm_FormClosed);
            this.Load += new System.EventHandler(this.AnimationForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ExportPropertiesGroupBox.ResumeLayout(false);
            this.ExportPropertiesGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button createAnimationButton;
        private System.Windows.Forms.Button deleteAnimationButton;
        private AnimationGroupControl animationGroupControl;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox animationListBox;
        private System.Windows.Forms.CheckBox exportNonAnimatedNodesCheckBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox ExportPropertiesGroupBox;
    }
}