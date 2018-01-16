namespace Max2Babylon
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
            System.Windows.Forms.Panel panel1;
            System.Windows.Forms.Panel panel2;
            this.createAnimationButton = new System.Windows.Forms.Button();
            this.deleteAnimationButton = new System.Windows.Forms.Button();
            this.AnimationListBox = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.animationGroupControl = new Max2Babylon.AnimationGroupControl();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(this.createAnimationButton);
            panel1.Controls.Add(this.deleteAnimationButton);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(3, 16);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(171, 29);
            panel1.TabIndex = 4;
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
            panel2.AutoSize = true;
            panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel2.Controls.Add(this.AnimationListBox);
            panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            panel2.Location = new System.Drawing.Point(3, 45);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(171, 389);
            panel2.TabIndex = 5;
            // 
            // AnimationListBox
            // 
            this.AnimationListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AnimationListBox.HorizontalScrollbar = true;
            this.AnimationListBox.Location = new System.Drawing.Point(0, 0);
            this.AnimationListBox.Name = "AnimationListBox";
            this.AnimationListBox.Size = new System.Drawing.Size(171, 389);
            this.AnimationListBox.Sorted = true;
            this.AnimationListBox.TabIndex = 0;
            this.AnimationListBox.SelectedValueChanged += new System.EventHandler(this.animationList_SelectedValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(panel2);
            this.groupBox1.Controls.Add(panel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(177, 437);
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
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.animationGroupControl);
            this.splitContainer1.Size = new System.Drawing.Size(660, 437);
            this.splitContainer1.SplitterDistance = 177;
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
            this.animationGroupControl.MinimumSize = new System.Drawing.Size(100, 100);
            this.animationGroupControl.Name = "animationGroupControl";
            this.animationGroupControl.Size = new System.Drawing.Size(479, 437);
            this.animationGroupControl.TabIndex = 3;
            // 
            // AnimationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(684, 461);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "AnimationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Babylon.js - Animation Groups";
            this.Activated += new System.EventHandler(this.AnimationForm_Activated);
            this.Deactivate += new System.EventHandler(this.AnimationForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AnimationForm_FormClosed);
            this.Load += new System.EventHandler(this.AnimationForm_Load);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
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
        private Max2Babylon.AnimationGroupControl animationGroupControl;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox AnimationListBox;
    }
}