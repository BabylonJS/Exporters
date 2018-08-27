namespace Maya2Babylon.Forms
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
            this.nameFieldPanel = new System.Windows.Forms.Panel();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.optionsButtonsPanel = new System.Windows.Forms.Panel();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.startEndPanel = new System.Windows.Forms.Panel();
            this.endTextBox = new System.Windows.Forms.TextBox();
            this.endLabel = new System.Windows.Forms.Label();
            this.startTextBox = new System.Windows.Forms.TextBox();
            this.startLabel = new System.Windows.Forms.Label();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.nameFieldPanel.SuspendLayout();
            this.optionsButtonsPanel.SuspendLayout();
            this.startEndPanel.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameFieldPanel
            // 
            this.nameFieldPanel.AutoSize = true;
            this.nameFieldPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.nameFieldPanel.Controls.Add(this.nameTextBox);
            this.nameFieldPanel.Controls.Add(this.nameLabel);
            this.nameFieldPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.nameFieldPanel.Location = new System.Drawing.Point(3, 16);
            this.nameFieldPanel.Name = "nameFieldPanel";
            this.nameFieldPanel.Size = new System.Drawing.Size(181, 26);
            this.nameFieldPanel.TabIndex = 0;
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
            this.optionsButtonsPanel.AutoSize = true;
            this.optionsButtonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.optionsButtonsPanel.Controls.Add(this.ConfirmButton);
            this.optionsButtonsPanel.Controls.Add(this.cancelButton);
            this.optionsButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsButtonsPanel.Location = new System.Drawing.Point(3, 68);
            this.optionsButtonsPanel.Name = "optionsButtonsPanel";
            this.optionsButtonsPanel.Size = new System.Drawing.Size(181, 29);
            this.optionsButtonsPanel.TabIndex = 0;
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
            // startEndPanel
            // 
            this.startEndPanel.AutoSize = true;
            this.startEndPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.startEndPanel.Controls.Add(this.endTextBox);
            this.startEndPanel.Controls.Add(this.endLabel);
            this.startEndPanel.Controls.Add(this.startTextBox);
            this.startEndPanel.Controls.Add(this.startLabel);
            this.startEndPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.startEndPanel.Location = new System.Drawing.Point(3, 42);
            this.startEndPanel.Name = "startEndPanel";
            this.startEndPanel.Size = new System.Drawing.Size(181, 26);
            this.startEndPanel.TabIndex = 0;
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
            // optionsGroupBox
            // 
            this.optionsGroupBox.AutoSize = true;
            this.optionsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.optionsGroupBox.Controls.Add(this.optionsButtonsPanel);
            this.optionsGroupBox.Controls.Add(this.startEndPanel);
            this.optionsGroupBox.Controls.Add(this.nameFieldPanel);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(187, 101);
            this.optionsGroupBox.TabIndex = 0;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Animation Options";
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
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Size = new System.Drawing.Size(187, 101);
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
            this.Size = new System.Drawing.Size(187, 101);
            this.nameFieldPanel.ResumeLayout(false);
            this.nameFieldPanel.PerformLayout();
            this.optionsButtonsPanel.ResumeLayout(false);
            this.startEndPanel.ResumeLayout(false);
            this.startEndPanel.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Panel nameFieldPanel;
        private System.Windows.Forms.Panel optionsButtonsPanel;
        private System.Windows.Forms.Panel startEndPanel;
    }
}
