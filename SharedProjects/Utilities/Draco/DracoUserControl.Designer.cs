
namespace Utilities
{
    partial class DracoUserControl
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
            this.CompressionLevelLabel = new System.Windows.Forms.Label();
            this.CompressionLevelNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.QuantizationGroup = new System.Windows.Forms.GroupBox();
            this.QTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.QNormalValueLabel = new System.Windows.Forms.Label();
            this.QNormalLabel = new System.Windows.Forms.Label();
            this.QPositionValueLabel = new System.Windows.Forms.Label();
            this.QPositionLabel = new System.Windows.Forms.Label();
            this.QPositionTrackBar = new System.Windows.Forms.TrackBar();
            this.QNormalTrackBar = new System.Windows.Forms.TrackBar();
            this.QTexcoordLabel = new System.Windows.Forms.Label();
            this.MainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.HeaderTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.QTexcoordValueLabel = new System.Windows.Forms.Label();
            this.QColorLabel = new System.Windows.Forms.Label();
            this.QColorValueLabel = new System.Windows.Forms.Label();
            this.QGenericLabel = new System.Windows.Forms.Label();
            this.QGenericValueLabel = new System.Windows.Forms.Label();
            this.QTexcoordTrackBar = new System.Windows.Forms.TrackBar();
            this.QColorTrackBar = new System.Windows.Forms.TrackBar();
            this.QGenericTrackBar = new System.Windows.Forms.TrackBar();
            this.UnifiedCheckBox = new System.Windows.Forms.CheckBox();
            this.ResetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.CompressionLevelNumericUpDown)).BeginInit();
            this.QuantizationGroup.SuspendLayout();
            this.QTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.QPositionTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.QNormalTrackBar)).BeginInit();
            this.MainTableLayoutPanel.SuspendLayout();
            this.HeaderTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.QTexcoordTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.QColorTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.QGenericTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // CompressionLevelLabel
            // 
            this.CompressionLevelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CompressionLevelLabel.AutoSize = true;
            this.CompressionLevelLabel.Location = new System.Drawing.Point(2, 18);
            this.CompressionLevelLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CompressionLevelLabel.Name = "CompressionLevelLabel";
            this.CompressionLevelLabel.Size = new System.Drawing.Size(96, 13);
            this.CompressionLevelLabel.TabIndex = 0;
            this.CompressionLevelLabel.Text = "Compression Level";
            // 
            // CompressionLevelNumericUpDown
            // 
            this.CompressionLevelNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CompressionLevelNumericUpDown.Location = new System.Drawing.Point(107, 9);
            this.CompressionLevelNumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.CompressionLevelNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.CompressionLevelNumericUpDown.Name = "CompressionLevelNumericUpDown";
            this.CompressionLevelNumericUpDown.Size = new System.Drawing.Size(71, 20);
            this.CompressionLevelNumericUpDown.TabIndex = 1;
            this.CompressionLevelNumericUpDown.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // QuantizationGroup
            // 
            this.QuantizationGroup.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.QuantizationGroup.Controls.Add(this.QTableLayoutPanel);
            this.QuantizationGroup.Location = new System.Drawing.Point(5, 48);
            this.QuantizationGroup.Margin = new System.Windows.Forms.Padding(5);
            this.QuantizationGroup.Name = "QuantizationGroup";
            this.QuantizationGroup.Padding = new System.Windows.Forms.Padding(2);
            this.QuantizationGroup.Size = new System.Drawing.Size(353, 287);
            this.QuantizationGroup.TabIndex = 2;
            this.QuantizationGroup.TabStop = false;
            this.QuantizationGroup.Text = "Quantize bits";
            // 
            // QTableLayoutPanel
            // 
            this.QTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QTableLayoutPanel.ColumnCount = 3;
            this.QTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.01835F));
            this.QTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.13761F));
            this.QTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.53823F));
            this.QTableLayoutPanel.Controls.Add(this.UnifiedCheckBox, 0, 5);
            this.QTableLayoutPanel.Controls.Add(this.QGenericTrackBar, 1, 4);
            this.QTableLayoutPanel.Controls.Add(this.QColorTrackBar, 1, 3);
            this.QTableLayoutPanel.Controls.Add(this.QTexcoordTrackBar, 1, 2);
            this.QTableLayoutPanel.Controls.Add(this.QNormalValueLabel, 2, 1);
            this.QTableLayoutPanel.Controls.Add(this.QNormalLabel, 0, 1);
            this.QTableLayoutPanel.Controls.Add(this.QPositionValueLabel, 2, 0);
            this.QTableLayoutPanel.Controls.Add(this.QPositionLabel, 0, 0);
            this.QTableLayoutPanel.Controls.Add(this.QPositionTrackBar, 1, 0);
            this.QTableLayoutPanel.Controls.Add(this.QNormalTrackBar, 1, 1);
            this.QTableLayoutPanel.Controls.Add(this.QTexcoordLabel, 0, 2);
            this.QTableLayoutPanel.Controls.Add(this.QTexcoordValueLabel, 2, 2);
            this.QTableLayoutPanel.Controls.Add(this.QColorLabel, 0, 3);
            this.QTableLayoutPanel.Controls.Add(this.QColorValueLabel, 2, 3);
            this.QTableLayoutPanel.Controls.Add(this.QGenericLabel, 0, 4);
            this.QTableLayoutPanel.Controls.Add(this.QGenericValueLabel, 2, 4);
            this.QTableLayoutPanel.Controls.Add(this.ResetButton, 1, 5);
            this.QTableLayoutPanel.Location = new System.Drawing.Point(19, 34);
            this.QTableLayoutPanel.Margin = new System.Windows.Forms.Padding(5);
            this.QTableLayoutPanel.Name = "QTableLayoutPanel";
            this.QTableLayoutPanel.RowCount = 6;
            this.QTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.QTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.QTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.QTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.QTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.QTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.QTableLayoutPanel.Size = new System.Drawing.Size(327, 245);
            this.QTableLayoutPanel.TabIndex = 3;
            // 
            // QNormalValueLabel
            // 
            this.QNormalValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QNormalValueLabel.AutoSize = true;
            this.QNormalValueLabel.Location = new System.Drawing.Point(287, 40);
            this.QNormalValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QNormalValueLabel.Name = "QNormalValueLabel";
            this.QNormalValueLabel.Size = new System.Drawing.Size(38, 40);
            this.QNormalValueLabel.TabIndex = 6;
            this.QNormalValueLabel.Text = "value";
            this.QNormalValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // QNormalLabel
            // 
            this.QNormalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QNormalLabel.AutoSize = true;
            this.QNormalLabel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.QNormalLabel.Location = new System.Drawing.Point(2, 40);
            this.QNormalLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QNormalLabel.Name = "QNormalLabel";
            this.QNormalLabel.Size = new System.Drawing.Size(68, 40);
            this.QNormalLabel.TabIndex = 5;
            this.QNormalLabel.Text = "Normal";
            // 
            // QPositionValueLabel
            // 
            this.QPositionValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QPositionValueLabel.AutoSize = true;
            this.QPositionValueLabel.Location = new System.Drawing.Point(287, 0);
            this.QPositionValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QPositionValueLabel.Name = "QPositionValueLabel";
            this.QPositionValueLabel.Size = new System.Drawing.Size(38, 40);
            this.QPositionValueLabel.TabIndex = 3;
            this.QPositionValueLabel.Text = "value";
            this.QPositionValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.QPositionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QPositionLabel.AutoSize = true;
            this.QPositionLabel.Location = new System.Drawing.Point(2, 0);
            this.QPositionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QPositionLabel.Name = "QPositionLabel";
            this.QPositionLabel.Size = new System.Drawing.Size(68, 40);
            this.QPositionLabel.TabIndex = 2;
            this.QPositionLabel.Text = "Position";
            // 
            // QPositionTrackBar
            // 
            this.QPositionTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QPositionTrackBar.Location = new System.Drawing.Point(74, 2);
            this.QPositionTrackBar.Margin = new System.Windows.Forms.Padding(2);
            this.QPositionTrackBar.Maximum = 32;
            this.QPositionTrackBar.Name = "QPositionTrackBar";
            this.QPositionTrackBar.Size = new System.Drawing.Size(209, 36);
            this.QPositionTrackBar.TabIndex = 1;
            this.QPositionTrackBar.Value = 14;
            this.QPositionTrackBar.Scroll += new System.EventHandler(this.QPositionTrackBar_Scroll);
            // 
            // QNormalTrackBar
            // 
            this.QNormalTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QNormalTrackBar.Location = new System.Drawing.Point(74, 42);
            this.QNormalTrackBar.Margin = new System.Windows.Forms.Padding(2);
            this.QNormalTrackBar.Maximum = 32;
            this.QNormalTrackBar.Name = "QNormalTrackBar";
            this.QNormalTrackBar.Size = new System.Drawing.Size(209, 36);
            this.QNormalTrackBar.TabIndex = 4;
            this.QNormalTrackBar.Value = 10;
            this.QNormalTrackBar.Scroll += new System.EventHandler(this.QNormalTrackBar_Scroll);
            // 
            // QTexcoordLabel
            // 
            this.QTexcoordLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QTexcoordLabel.AutoSize = true;
            this.QTexcoordLabel.Location = new System.Drawing.Point(2, 80);
            this.QTexcoordLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QTexcoordLabel.Name = "QTexcoordLabel";
            this.QTexcoordLabel.Size = new System.Drawing.Size(68, 40);
            this.QTexcoordLabel.TabIndex = 7;
            this.QTexcoordLabel.Text = "Textcoord";
            // 
            // MainTableLayoutPanel
            // 
            this.MainTableLayoutPanel.ColumnCount = 1;
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainTableLayoutPanel.Controls.Add(this.QuantizationGroup, 0, 1);
            this.MainTableLayoutPanel.Controls.Add(this.HeaderTableLayoutPanel, 0, 0);
            this.MainTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
            this.MainTableLayoutPanel.RowCount = 2;
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.67708F));
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.32291F));
            this.MainTableLayoutPanel.Size = new System.Drawing.Size(363, 347);
            this.MainTableLayoutPanel.TabIndex = 3;
            // 
            // HeaderTableLayoutPanel
            // 
            this.HeaderTableLayoutPanel.ColumnCount = 3;
            this.HeaderTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.HeaderTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.HeaderTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 177F));
            this.HeaderTableLayoutPanel.Controls.Add(this.CompressionLevelLabel, 0, 0);
            this.HeaderTableLayoutPanel.Controls.Add(this.CompressionLevelNumericUpDown, 1, 0);
            this.HeaderTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeaderTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.HeaderTableLayoutPanel.Name = "HeaderTableLayoutPanel";
            this.HeaderTableLayoutPanel.RowCount = 1;
            this.HeaderTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.HeaderTableLayoutPanel.Size = new System.Drawing.Size(357, 31);
            this.HeaderTableLayoutPanel.TabIndex = 3;
            // 
            // QTexcoordValueLabel
            // 
            this.QTexcoordValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QTexcoordValueLabel.AutoSize = true;
            this.QTexcoordValueLabel.Location = new System.Drawing.Point(287, 80);
            this.QTexcoordValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QTexcoordValueLabel.Name = "QTexcoordValueLabel";
            this.QTexcoordValueLabel.Size = new System.Drawing.Size(38, 40);
            this.QTexcoordValueLabel.TabIndex = 8;
            this.QTexcoordValueLabel.Text = "value";
            this.QTexcoordValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // QColorLabel
            // 
            this.QColorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QColorLabel.AutoSize = true;
            this.QColorLabel.Location = new System.Drawing.Point(2, 120);
            this.QColorLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QColorLabel.Name = "QColorLabel";
            this.QColorLabel.Size = new System.Drawing.Size(68, 40);
            this.QColorLabel.TabIndex = 9;
            this.QColorLabel.Text = "Color";
            // 
            // QColorValueLabel
            // 
            this.QColorValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QColorValueLabel.AutoSize = true;
            this.QColorValueLabel.Location = new System.Drawing.Point(287, 120);
            this.QColorValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QColorValueLabel.Name = "QColorValueLabel";
            this.QColorValueLabel.Size = new System.Drawing.Size(38, 40);
            this.QColorValueLabel.TabIndex = 10;
            this.QColorValueLabel.Text = "value";
            this.QColorValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // QGenericLabel
            // 
            this.QGenericLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QGenericLabel.AutoSize = true;
            this.QGenericLabel.Location = new System.Drawing.Point(2, 160);
            this.QGenericLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QGenericLabel.Name = "QGenericLabel";
            this.QGenericLabel.Size = new System.Drawing.Size(68, 40);
            this.QGenericLabel.TabIndex = 11;
            this.QGenericLabel.Text = "Generic";
            // 
            // QGenericValueLabel
            // 
            this.QGenericValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QGenericValueLabel.AutoSize = true;
            this.QGenericValueLabel.Location = new System.Drawing.Point(287, 160);
            this.QGenericValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QGenericValueLabel.Name = "QGenericValueLabel";
            this.QGenericValueLabel.Size = new System.Drawing.Size(38, 40);
            this.QGenericValueLabel.TabIndex = 12;
            this.QGenericValueLabel.Text = "value";
            this.QGenericValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // QTexcoordTrackBar
            // 
            this.QTexcoordTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QTexcoordTrackBar.Location = new System.Drawing.Point(74, 82);
            this.QTexcoordTrackBar.Margin = new System.Windows.Forms.Padding(2);
            this.QTexcoordTrackBar.Maximum = 32;
            this.QTexcoordTrackBar.Name = "QTexcoordTrackBar";
            this.QTexcoordTrackBar.Size = new System.Drawing.Size(209, 36);
            this.QTexcoordTrackBar.TabIndex = 13;
            this.QTexcoordTrackBar.Value = 12;
            this.QTexcoordTrackBar.Scroll += new System.EventHandler(this.TexcoordTrackBar_Scroll);
            // 
            // QColorTrackBar
            // 
            this.QColorTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QColorTrackBar.Location = new System.Drawing.Point(74, 122);
            this.QColorTrackBar.Margin = new System.Windows.Forms.Padding(2);
            this.QColorTrackBar.Maximum = 32;
            this.QColorTrackBar.Name = "QColorTrackBar";
            this.QColorTrackBar.Size = new System.Drawing.Size(209, 36);
            this.QColorTrackBar.TabIndex = 8;
            this.QColorTrackBar.Value = 10;
            this.QColorTrackBar.Scroll += new System.EventHandler(this.QColorTrackBar_Scroll);
            // 
            // QGenericTrackBar
            // 
            this.QGenericTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.QGenericTrackBar.Location = new System.Drawing.Point(74, 162);
            this.QGenericTrackBar.Margin = new System.Windows.Forms.Padding(2);
            this.QGenericTrackBar.Maximum = 32;
            this.QGenericTrackBar.Name = "QGenericTrackBar";
            this.QGenericTrackBar.Size = new System.Drawing.Size(209, 36);
            this.QGenericTrackBar.TabIndex = 15;
            this.QGenericTrackBar.Value = 12;
            this.QGenericTrackBar.Scroll += new System.EventHandler(this.QGenericTrackBar_Scroll);
            // 
            // UnifiedCheckBox
            // 
            this.UnifiedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UnifiedCheckBox.AutoSize = true;
            this.UnifiedCheckBox.Location = new System.Drawing.Point(2, 202);
            this.UnifiedCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.UnifiedCheckBox.Name = "UnifiedCheckBox";
            this.UnifiedCheckBox.Size = new System.Drawing.Size(68, 22);
            this.UnifiedCheckBox.TabIndex = 17;
            this.UnifiedCheckBox.Text = "Unified";
            this.UnifiedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ResetButton.Location = new System.Drawing.Point(213, 202);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(69, 22);
            this.ResetButton.TabIndex = 18;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // DracoUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Controls.Add(this.MainTableLayoutPanel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DracoUserControl";
            this.Size = new System.Drawing.Size(369, 355);
            ((System.ComponentModel.ISupportInitialize)(this.CompressionLevelNumericUpDown)).EndInit();
            this.QuantizationGroup.ResumeLayout(false);
            this.QTableLayoutPanel.ResumeLayout(false);
            this.QTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.QPositionTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.QNormalTrackBar)).EndInit();
            this.MainTableLayoutPanel.ResumeLayout(false);
            this.HeaderTableLayoutPanel.ResumeLayout(false);
            this.HeaderTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.QTexcoordTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.QColorTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.QGenericTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.NumericUpDown CompressionLevelNumericUpDown;
        public System.Windows.Forms.TrackBar QPositionTrackBar;
        public System.Windows.Forms.TrackBar QNormalTrackBar;
        public System.Windows.Forms.TrackBar QTexcoordTrackBar;
        public System.Windows.Forms.TrackBar QColorTrackBar;
        public System.Windows.Forms.TrackBar QGenericTrackBar;
        public System.Windows.Forms.CheckBox UnifiedCheckBox;

        private System.Windows.Forms.Label CompressionLevelLabel;
        private System.Windows.Forms.GroupBox QuantizationGroup;
        private System.Windows.Forms.TableLayoutPanel QTableLayoutPanel;
        private System.Windows.Forms.Label QPositionValueLabel;
        private System.Windows.Forms.Label QPositionLabel;
        private System.Windows.Forms.Label QNormalValueLabel;
        private System.Windows.Forms.Label QNormalLabel;
        private System.Windows.Forms.TableLayoutPanel MainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel HeaderTableLayoutPanel;
        private System.Windows.Forms.Label QTexcoordLabel;
        private System.Windows.Forms.Label QTexcoordValueLabel;
        private System.Windows.Forms.Label QColorLabel;
        private System.Windows.Forms.Label QColorValueLabel;
        private System.Windows.Forms.Label QGenericLabel;
        private System.Windows.Forms.Label QGenericValueLabel;
        private System.Windows.Forms.Button ResetButton;
    }
}
