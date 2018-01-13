namespace Max2Babylon
{
    partial class AnimationGroupControl
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
            System.Windows.Forms.Panel nameFieldPanel;
            System.Windows.Forms.Panel optionsButtonsPanel;
            System.Windows.Forms.Panel nodeButtonsPanel;
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.addSelectedButton = new System.Windows.Forms.Button();
            this.nodeTreeView = new System.Windows.Forms.TreeView();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.endTextBox = new System.Windows.Forms.TextBox();
            this.endLabel = new System.Windows.Forms.Label();
            this.startTextBox = new System.Windows.Forms.TextBox();
            this.startLabel = new System.Windows.Forms.Label();
            this.nodesGroupBox = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.removeNodeButton = new System.Windows.Forms.Button();
            nameFieldPanel = new System.Windows.Forms.Panel();
            optionsButtonsPanel = new System.Windows.Forms.Panel();
            nodeButtonsPanel = new System.Windows.Forms.Panel();
            nameFieldPanel.SuspendLayout();
            optionsButtonsPanel.SuspendLayout();
            nodeButtonsPanel.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.panel1.SuspendLayout();
            this.nodesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameFieldPanel
            // 
            nameFieldPanel.AutoSize = true;
            nameFieldPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            nameFieldPanel.Controls.Add(this.nameTextBox);
            nameFieldPanel.Controls.Add(this.nameLabel);
            nameFieldPanel.Dock = System.Windows.Forms.DockStyle.Top;
            nameFieldPanel.Location = new System.Drawing.Point(3, 16);
            nameFieldPanel.Name = "nameFieldPanel";
            nameFieldPanel.Size = new System.Drawing.Size(176, 26);
            nameFieldPanel.TabIndex = 0;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(39, 3);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(134, 20);
            this.nameTextBox.TabIndex = 4;
            this.nameTextBox.WordWrap = false;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(4, 6);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 3;
            this.nameLabel.Text = "Name";
            // 
            // optionsButtonsPanel
            // 
            optionsButtonsPanel.AutoSize = true;
            optionsButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            optionsButtonsPanel.Controls.Add(this.saveButton);
            optionsButtonsPanel.Controls.Add(this.cancelButton);
            optionsButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            optionsButtonsPanel.Location = new System.Drawing.Point(3, 68);
            optionsButtonsPanel.Name = "optionsButtonsPanel";
            optionsButtonsPanel.Size = new System.Drawing.Size(176, 29);
            optionsButtonsPanel.TabIndex = 5;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(3, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(50, 23);
            this.saveButton.TabIndex = 5;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(59, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(50, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // nodeButtonsPanel
            // 
            nodeButtonsPanel.AutoSize = true;
            nodeButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            nodeButtonsPanel.Controls.Add(this.removeNodeButton);
            nodeButtonsPanel.Controls.Add(this.addSelectedButton);
            nodeButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            nodeButtonsPanel.Location = new System.Drawing.Point(3, 16);
            nodeButtonsPanel.Name = "nodeButtonsPanel";
            nodeButtonsPanel.Size = new System.Drawing.Size(483, 27);
            nodeButtonsPanel.TabIndex = 6;
            // 
            // addSelectedButton
            // 
            this.addSelectedButton.AutoSize = true;
            this.addSelectedButton.Location = new System.Drawing.Point(3, 1);
            this.addSelectedButton.Name = "addSelectedButton";
            this.addSelectedButton.Size = new System.Drawing.Size(104, 23);
            this.addSelectedButton.TabIndex = 7;
            this.addSelectedButton.Text = "Add Scene Nodes";
            this.addSelectedButton.UseVisualStyleBackColor = true;
            this.addSelectedButton.Click += new System.EventHandler(this.addSelectedButton_Click);
            // 
            // nodeTreeView
            // 
            this.nodeTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nodeTreeView.Location = new System.Drawing.Point(3, 43);
            this.nodeTreeView.Name = "nodeTreeView";
            this.nodeTreeView.Size = new System.Drawing.Size(483, 436);
            this.nodeTreeView.TabIndex = 0;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.AutoSize = true;
            this.optionsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.optionsGroupBox.Controls.Add(optionsButtonsPanel);
            this.optionsGroupBox.Controls.Add(this.panel1);
            this.optionsGroupBox.Controls.Add(nameFieldPanel);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(182, 100);
            this.optionsGroupBox.TabIndex = 4;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Animation Options";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.endTextBox);
            this.panel1.Controls.Add(this.endLabel);
            this.panel1.Controls.Add(this.startTextBox);
            this.panel1.Controls.Add(this.startLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 42);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(176, 26);
            this.panel1.TabIndex = 6;
            // 
            // endTextBox
            // 
            this.endTextBox.Location = new System.Drawing.Point(124, 3);
            this.endTextBox.Name = "endTextBox";
            this.endTextBox.Size = new System.Drawing.Size(40, 20);
            this.endTextBox.TabIndex = 2;
            this.endTextBox.TextChanged += new System.EventHandler(this.endTextBox_TextChanged);
            // 
            // endLabel
            // 
            this.endLabel.AutoSize = true;
            this.endLabel.Location = new System.Drawing.Point(92, 6);
            this.endLabel.Name = "endLabel";
            this.endLabel.Size = new System.Drawing.Size(26, 13);
            this.endLabel.TabIndex = 3;
            this.endLabel.Text = "End";
            // 
            // startTextBox
            // 
            this.startTextBox.Location = new System.Drawing.Point(39, 3);
            this.startTextBox.Name = "startTextBox";
            this.startTextBox.Size = new System.Drawing.Size(40, 20);
            this.startTextBox.TabIndex = 0;
            this.startTextBox.TextChanged += new System.EventHandler(this.startTextBox_TextChanged);
            // 
            // startLabel
            // 
            this.startLabel.AutoSize = true;
            this.startLabel.Location = new System.Drawing.Point(4, 6);
            this.startLabel.Name = "startLabel";
            this.startLabel.Size = new System.Drawing.Size(29, 13);
            this.startLabel.TabIndex = 1;
            this.startLabel.Text = "Start";
            // 
            // nodesGroupBox
            // 
            this.nodesGroupBox.AutoSize = true;
            this.nodesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.nodesGroupBox.Controls.Add(this.nodeTreeView);
            this.nodesGroupBox.Controls.Add(nodeButtonsPanel);
            this.nodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nodesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nodesGroupBox.Location = new System.Drawing.Point(0, 0);
            this.nodesGroupBox.Name = "nodesGroupBox";
            this.nodesGroupBox.Size = new System.Drawing.Size(489, 482);
            this.nodesGroupBox.TabIndex = 5;
            this.nodesGroupBox.TabStop = false;
            this.nodesGroupBox.Text = "Animation Nodes";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.optionsGroupBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.nodesGroupBox);
            this.splitContainer1.Size = new System.Drawing.Size(675, 482);
            this.splitContainer1.SplitterDistance = 182;
            this.splitContainer1.TabIndex = 7;
            // 
            // removeNodeButton
            // 
            this.removeNodeButton.AutoSize = true;
            this.removeNodeButton.Location = new System.Drawing.Point(113, 1);
            this.removeNodeButton.Name = "removeNodeButton";
            this.removeNodeButton.Size = new System.Drawing.Size(102, 23);
            this.removeNodeButton.TabIndex = 8;
            this.removeNodeButton.Text = "Remove Selected";
            this.removeNodeButton.UseVisualStyleBackColor = true;
            this.removeNodeButton.Click += new System.EventHandler(this.removeNodeButton_Click);
            // 
            // AnimationGroupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.splitContainer1);
            this.Name = "AnimationGroupControl";
            this.Size = new System.Drawing.Size(675, 482);
            nameFieldPanel.ResumeLayout(false);
            nameFieldPanel.PerformLayout();
            optionsButtonsPanel.ResumeLayout(false);
            nodeButtonsPanel.ResumeLayout(false);
            nodeButtonsPanel.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.nodesGroupBox.ResumeLayout(false);
            this.nodesGroupBox.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label endLabel;
        private System.Windows.Forms.TextBox endTextBox;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.TextBox startTextBox;
        private System.Windows.Forms.GroupBox nodesGroupBox;
        private System.Windows.Forms.TreeView nodeTreeView;
        private System.Windows.Forms.Button addSelectedButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button removeNodeButton;
    }
}
