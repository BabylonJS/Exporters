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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel nameFieldPanel;
            System.Windows.Forms.Panel optionsButtonsPanel;
            System.Windows.Forms.Panel nodeButtonsPanel;
            System.Windows.Forms.Label warningLabel;
            System.Windows.Forms.Panel startEndPanel;
            System.Windows.Forms.Panel warningLabelPanel;
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.calculateTimeRangeBtn = new System.Windows.Forms.Button();
            this.removeNodeButton = new System.Windows.Forms.Button();
            this.addSelectedButton = new System.Windows.Forms.Button();
            this.endTextBox = new System.Windows.Forms.TextBox();
            this.endLabel = new System.Windows.Forms.Label();
            this.startTextBox = new System.Windows.Forms.TextBox();
            this.startLabel = new System.Windows.Forms.Label();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.nodesGroupBox = new System.Windows.Forms.GroupBox();
            this.MaxNodeTree = new Max2Babylon.MaxNodeTreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            nameFieldPanel = new System.Windows.Forms.Panel();
            optionsButtonsPanel = new System.Windows.Forms.Panel();
            nodeButtonsPanel = new System.Windows.Forms.Panel();
            warningLabel = new System.Windows.Forms.Label();
            startEndPanel = new System.Windows.Forms.Panel();
            warningLabelPanel = new System.Windows.Forms.Panel();
            nameFieldPanel.SuspendLayout();
            optionsButtonsPanel.SuspendLayout();
            nodeButtonsPanel.SuspendLayout();
            startEndPanel.SuspendLayout();
            warningLabelPanel.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
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
            nameFieldPanel.Size = new System.Drawing.Size(184, 26);
            nameFieldPanel.TabIndex = 0;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(39, 3);
            this.nameTextBox.MinimumSize = new System.Drawing.Size(20, 20);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(134, 20);
            this.nameTextBox.TabIndex = 0;
            this.nameTextBox.WordWrap = false;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(4, 6);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name";
            // 
            // optionsButtonsPanel
            // 
            optionsButtonsPanel.AutoSize = true;
            optionsButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            optionsButtonsPanel.Controls.Add(this.ConfirmButton);
            optionsButtonsPanel.Controls.Add(this.cancelButton);
            optionsButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            optionsButtonsPanel.Location = new System.Drawing.Point(3, 143);
            optionsButtonsPanel.Name = "optionsButtonsPanel";
            optionsButtonsPanel.Size = new System.Drawing.Size(184, 29);
            optionsButtonsPanel.TabIndex = 0;
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Location = new System.Drawing.Point(3, 3);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(60, 23);
            this.ConfirmButton.TabIndex = 5;
            this.ConfirmButton.Text = "Confirm";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(69, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // nodeButtonsPanel
            // 
            nodeButtonsPanel.AutoSize = true;
            nodeButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            nodeButtonsPanel.Controls.Add(this.calculateTimeRangeBtn);
            nodeButtonsPanel.Controls.Add(this.removeNodeButton);
            nodeButtonsPanel.Controls.Add(this.addSelectedButton);
            nodeButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            nodeButtonsPanel.Location = new System.Drawing.Point(3, 68);
            nodeButtonsPanel.Name = "nodeButtonsPanel";
            nodeButtonsPanel.Padding = new System.Windows.Forms.Padding(3);
            nodeButtonsPanel.Size = new System.Drawing.Size(184, 75);
            nodeButtonsPanel.TabIndex = 0;
            // 
            // calculateTimeRangeBtn
            // 
            this.calculateTimeRangeBtn.AutoSize = true;
            this.calculateTimeRangeBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.calculateTimeRangeBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.calculateTimeRangeBtn.Location = new System.Drawing.Point(3, 49);
            this.calculateTimeRangeBtn.Name = "calculateTimeRangeBtn";
            this.calculateTimeRangeBtn.Size = new System.Drawing.Size(178, 23);
            this.calculateTimeRangeBtn.TabIndex = 5;
            this.calculateTimeRangeBtn.Text = "Calculate Time Range";
            this.calculateTimeRangeBtn.UseVisualStyleBackColor = true;
            this.calculateTimeRangeBtn.Click += new System.EventHandler(this.calculateTimeRangeBtn_Click);
            // 
            // removeNodeButton
            // 
            this.removeNodeButton.AutoSize = true;
            this.removeNodeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.removeNodeButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.removeNodeButton.Location = new System.Drawing.Point(3, 26);
            this.removeNodeButton.Name = "removeNodeButton";
            this.removeNodeButton.Size = new System.Drawing.Size(178, 23);
            this.removeNodeButton.TabIndex = 4;
            this.removeNodeButton.Text = "Remove Viewport Selection";
            this.removeNodeButton.UseVisualStyleBackColor = true;
            this.removeNodeButton.Click += new System.EventHandler(this.removeNodeButton_Click);
            // 
            // addSelectedButton
            // 
            this.addSelectedButton.AutoSize = true;
            this.addSelectedButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addSelectedButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.addSelectedButton.Location = new System.Drawing.Point(3, 3);
            this.addSelectedButton.Name = "addSelectedButton";
            this.addSelectedButton.Size = new System.Drawing.Size(178, 23);
            this.addSelectedButton.TabIndex = 3;
            this.addSelectedButton.Text = "Add Viewport Selection";
            this.addSelectedButton.UseVisualStyleBackColor = true;
            this.addSelectedButton.Click += new System.EventHandler(this.addSelectedButton_Click);
            // 
            // warningLabel
            // 
            warningLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            warningLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            warningLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            warningLabel.Location = new System.Drawing.Point(3, 3);
            warningLabel.Name = "warningLabel";
            warningLabel.Size = new System.Drawing.Size(178, 97);
            warningLabel.TabIndex = 0;
            warningLabel.Text = "\r\n*NOTE*\r\n\r\nChanging the 3dsMax scene node hierarchy with this window open may le" +
    "ad to undefined behavior.";
            warningLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // startEndPanel
            // 
            startEndPanel.AutoSize = true;
            startEndPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            startEndPanel.Controls.Add(this.endTextBox);
            startEndPanel.Controls.Add(this.endLabel);
            startEndPanel.Controls.Add(this.startTextBox);
            startEndPanel.Controls.Add(this.startLabel);
            startEndPanel.Dock = System.Windows.Forms.DockStyle.Top;
            startEndPanel.Location = new System.Drawing.Point(3, 42);
            startEndPanel.Name = "startEndPanel";
            startEndPanel.Size = new System.Drawing.Size(184, 26);
            startEndPanel.TabIndex = 0;
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
            this.endLabel.TabIndex = 0;
            this.endLabel.Text = "End";
            // 
            // startTextBox
            // 
            this.startTextBox.Location = new System.Drawing.Point(39, 3);
            this.startTextBox.Name = "startTextBox";
            this.startTextBox.Size = new System.Drawing.Size(40, 20);
            this.startTextBox.TabIndex = 1;
            this.startTextBox.TextChanged += new System.EventHandler(this.startTextBox_TextChanged);
            // 
            // startLabel
            // 
            this.startLabel.AutoSize = true;
            this.startLabel.Location = new System.Drawing.Point(4, 6);
            this.startLabel.Name = "startLabel";
            this.startLabel.Size = new System.Drawing.Size(29, 13);
            this.startLabel.TabIndex = 0;
            this.startLabel.Text = "Start";
            // 
            // warningLabelPanel
            // 
            warningLabelPanel.Controls.Add(warningLabel);
            warningLabelPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            warningLabelPanel.Location = new System.Drawing.Point(3, 376);
            warningLabelPanel.Name = "warningLabelPanel";
            warningLabelPanel.Padding = new System.Windows.Forms.Padding(3);
            warningLabelPanel.Size = new System.Drawing.Size(184, 103);
            warningLabelPanel.TabIndex = 0;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.AutoSize = true;
            this.optionsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.optionsGroupBox.Controls.Add(warningLabelPanel);
            this.optionsGroupBox.Controls.Add(optionsButtonsPanel);
            this.optionsGroupBox.Controls.Add(nodeButtonsPanel);
            this.optionsGroupBox.Controls.Add(startEndPanel);
            this.optionsGroupBox.Controls.Add(nameFieldPanel);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(190, 482);
            this.optionsGroupBox.TabIndex = 0;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Animation Options";
            // 
            // nodesGroupBox
            // 
            this.nodesGroupBox.AutoSize = true;
            this.nodesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.nodesGroupBox.Controls.Add(this.MaxNodeTree);
            this.nodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nodesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nodesGroupBox.Location = new System.Drawing.Point(0, 0);
            this.nodesGroupBox.Name = "nodesGroupBox";
            this.nodesGroupBox.Size = new System.Drawing.Size(481, 482);
            this.nodesGroupBox.TabIndex = 0;
            this.nodesGroupBox.TabStop = false;
            this.nodesGroupBox.Text = "Animation Nodes";
            // 
            // MaxNodeTree
            // 
            this.MaxNodeTree.AllowDrop = true;
            this.MaxNodeTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MaxNodeTree.DummyAddedBackColor = System.Drawing.Color.PaleGreen;
            this.MaxNodeTree.DummyAddedForeColor = System.Drawing.SystemColors.GrayText;
            this.MaxNodeTree.DummyDefaultBackColor = System.Drawing.SystemColors.Control;
            this.MaxNodeTree.DummyDefaultForeColor = System.Drawing.SystemColors.GrayText;
            this.MaxNodeTree.DummyRemovedBackColor = System.Drawing.Color.IndianRed;
            this.MaxNodeTree.DummyRemovedForeColor = System.Drawing.SystemColors.ControlText;
            this.MaxNodeTree.DummyUpgradedBackColor = System.Drawing.Color.PaleGreen;
            this.MaxNodeTree.DummyUpgradedForeColor = System.Drawing.SystemColors.ControlText;
            this.MaxNodeTree.Location = new System.Drawing.Point(3, 16);
            this.MaxNodeTree.Name = "MaxNodeTree";
            this.MaxNodeTree.NodeAddedBackColor = System.Drawing.Color.PaleGreen;
            this.MaxNodeTree.NodeAddedForeColor = System.Drawing.SystemColors.ControlText;
            this.MaxNodeTree.NodeDefaultBackColor = System.Drawing.SystemColors.Window;
            this.MaxNodeTree.NodeDefaultForeColor = System.Drawing.SystemColors.ControlText;
            this.MaxNodeTree.NodeDowngradedBackColor = System.Drawing.Color.PaleGreen;
            this.MaxNodeTree.NodeDowngradedForeColor = System.Drawing.SystemColors.GrayText;
            this.MaxNodeTree.NodeRemovedBackColor = System.Drawing.Color.IndianRed;
            this.MaxNodeTree.NodeRemovedForeColor = System.Drawing.SystemColors.ControlText;
            this.MaxNodeTree.Size = new System.Drawing.Size(475, 463);
            this.MaxNodeTree.TabIndex = 7;
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
            this.splitContainer1.SplitterDistance = 190;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.TabStop = false;
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
            startEndPanel.ResumeLayout(false);
            startEndPanel.PerformLayout();
            warningLabelPanel.ResumeLayout(false);
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.nodesGroupBox.ResumeLayout(false);
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
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label endLabel;
        private System.Windows.Forms.TextBox endTextBox;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.TextBox startTextBox;
        private System.Windows.Forms.GroupBox nodesGroupBox;
        private Max2Babylon.MaxNodeTreeView MaxNodeTree;
        private System.Windows.Forms.Button addSelectedButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button removeNodeButton;
        private System.Windows.Forms.Button calculateTimeRangeBtn;
    }
}
