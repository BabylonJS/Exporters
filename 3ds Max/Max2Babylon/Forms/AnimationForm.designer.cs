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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.deleteAnimationButton = new System.Windows.Forms.Button();
            this.createAnimationButton = new System.Windows.Forms.Button();
            this.animationList = new System.Windows.Forms.ListBox();
            this.animationGroupControl = new Max2Babylon.AnimationGroupControl();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.deleteAnimationButton);
            this.groupBox1.Controls.Add(this.createAnimationButton);
            this.groupBox1.Controls.Add(this.animationList);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(168, 537);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animations";
            // 
            // deleteAnimationButton
            // 
            this.deleteAnimationButton.Location = new System.Drawing.Point(87, 19);
            this.deleteAnimationButton.Name = "deleteAnimationButton";
            this.deleteAnimationButton.Size = new System.Drawing.Size(75, 23);
            this.deleteAnimationButton.TabIndex = 2;
            this.deleteAnimationButton.Text = "Delete";
            this.deleteAnimationButton.UseVisualStyleBackColor = true;
            this.deleteAnimationButton.Click += new System.EventHandler(this.deleteAnimationButton_Click);
            // 
            // createAnimationButton
            // 
            this.createAnimationButton.Location = new System.Drawing.Point(6, 19);
            this.createAnimationButton.Name = "createAnimationButton";
            this.createAnimationButton.Size = new System.Drawing.Size(75, 23);
            this.createAnimationButton.TabIndex = 1;
            this.createAnimationButton.Text = "Create";
            this.createAnimationButton.UseVisualStyleBackColor = true;
            this.createAnimationButton.Click += new System.EventHandler(this.createAnimationButton_Click);
            // 
            // animationList
            // 
            this.animationList.FormattingEnabled = true;
            this.animationList.Location = new System.Drawing.Point(6, 48);
            this.animationList.MultiColumn = true;
            this.animationList.Name = "animationList";
            this.animationList.Size = new System.Drawing.Size(156, 186);
            this.animationList.Sorted = true;
            this.animationList.TabIndex = 0;
            this.animationList.SelectedIndexChanged += new System.EventHandler(this.animationList_SelectedIndexChanged);
            // 
            // animationGroupControl
            // 
            this.animationGroupControl.Location = new System.Drawing.Point(186, 12);
            this.animationGroupControl.MinimumSize = new System.Drawing.Size(100, 100);
            this.animationGroupControl.Name = "animationGroupControl";
            this.animationGroupControl.Size = new System.Drawing.Size(450, 403);
            this.animationGroupControl.TabIndex = 3;
            // 
            // AnimationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 561);
            this.Controls.Add(this.animationGroupControl);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(850, 400);
            this.Name = "AnimationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Babylon.js - Animation tool";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.AnimationForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox animationList;
        private System.Windows.Forms.Button createAnimationButton;
        private System.Windows.Forms.Button deleteAnimationButton;
        private Max2Babylon.AnimationGroupControl animationGroupControl;
    }
}