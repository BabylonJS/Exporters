namespace Max2Babylon
{
    partial class ExporterForm
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
            this.components = new System.ComponentModel.Container();
            this.butExport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtModelName = new System.Windows.Forms.RichTextBox();
            this.butModelBrowse = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.treeView = new System.Windows.Forms.TreeView();
            this.butCancel = new System.Windows.Forms.Button();
            this.chkManifest = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkWriteTextures = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblBakeAnimation = new System.Windows.Forms.Label();
            this.cmbBakeAnimationOptions = new System.Windows.Forms.ComboBox();
            this.chkApplyPreprocessToScene = new System.Windows.Forms.CheckBox();
            this.chkMrgContainersAndXref = new System.Windows.Forms.CheckBox();
            this.chkUsePreExportProces = new System.Windows.Forms.CheckBox();
            this.chkFlatten = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtEnvironmentName = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkFullPBR = new System.Windows.Forms.CheckBox();
            this.btnEnvBrowse = new System.Windows.Forms.Button();
            this.chkNoAutoLight = new System.Windows.Forms.CheckBox();
            this.textureLabel = new System.Windows.Forms.Label();
            this.txtTextureName = new System.Windows.Forms.RichTextBox();
            this.btnTxtBrowse = new System.Windows.Forms.Button();
            this.chkExportMaterials = new System.Windows.Forms.CheckBox();
            this.chkKHRMaterialsUnlit = new System.Windows.Forms.CheckBox();
            this.chkKHRTextureTransform = new System.Windows.Forms.CheckBox();
            this.chkKHRLightsPunctual = new System.Windows.Forms.CheckBox();
            this.chkOverwriteTextures = new System.Windows.Forms.CheckBox();
            this.chkDoNotOptimizeAnimations = new System.Windows.Forms.CheckBox();
            this.chkAnimgroupExportNonAnimated = new System.Windows.Forms.CheckBox();
            this.chkDracoCompression = new System.Windows.Forms.CheckBox();
            this.chkMergeAOwithMR = new System.Windows.Forms.CheckBox();
            this.txtQuality = new System.Windows.Forms.TextBox();
            this.labelQuality = new System.Windows.Forms.Label();
            this.chkExportMorphNormals = new System.Windows.Forms.CheckBox();
            this.chkExportMorphTangents = new System.Windows.Forms.CheckBox();
            this.chkExportTangents = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtScaleFactor = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboOutputFormat = new System.Windows.Forms.ComboBox();
            this.chkOnlySelected = new System.Windows.Forms.CheckBox();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.chkHidden = new System.Windows.Forms.CheckBox();
            this.butExportAndRun = new System.Windows.Forms.Button();
            this.butClose = new System.Windows.Forms.Button();
            this.toolTipDracoCompression = new System.Windows.Forms.ToolTip(this.components);
            this.butMultiExport = new System.Windows.Forms.Button();
            this.saveOptionBtn = new System.Windows.Forms.Button();
            this.envFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // butExport
            // 
            this.butExport.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butExport.Enabled = false;
            this.butExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butExport.Location = new System.Drawing.Point(421, 495);
            this.butExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butExport.Name = "butExport";
            this.butExport.Size = new System.Drawing.Size(197, 27);
            this.butExport.TabIndex = 100;
            this.butExport.Text = "Export";
            this.butExport.UseVisualStyleBackColor = true;
            this.butExport.Click += new System.EventHandler(this.butExport_Click);
            this.butExport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Model name:";
            // 
            // txtModelName
            // 
            this.txtModelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModelName.Location = new System.Drawing.Point(86, 14);
            this.txtModelName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtModelName.Multiline = false;
            this.txtModelName.Name = "txtModelName";
            this.txtModelName.Size = new System.Drawing.Size(708, 20);
            this.txtModelName.TabIndex = 2;
            this.txtModelName.Text = "";
            this.txtModelName.TextChanged += new System.EventHandler(this.txtFilename_TextChanged);
            this.txtModelName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butModelBrowse
            // 
            this.butModelBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butModelBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butModelBrowse.Location = new System.Drawing.Point(800, 12);
            this.butModelBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butModelBrowse.Name = "butModelBrowse";
            this.butModelBrowse.Size = new System.Drawing.Size(28, 23);
            this.butModelBrowse.TabIndex = 3;
            this.butModelBrowse.Text = "...";
            this.butModelBrowse.UseVisualStyleBackColor = true;
            this.butModelBrowse.Click += new System.EventHandler(this.butModelBrowse_Click);
            this.butModelBrowse.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "babylon";
            this.saveFileDialog.Filter = "Babylon files|*.babylon";
            this.saveFileDialog.RestoreDirectory = true;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 861);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1080, 23);
            this.progressBar.TabIndex = 104;
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.Location = new System.Drawing.Point(12, 532);
            this.treeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(1252, 319);
            this.treeView.TabIndex = 103;
            this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Enabled = false;
            this.butCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butCancel.Location = new System.Drawing.Point(1098, 861);
            this.butCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 105;
            this.butCancel.Text = "Cancel";
            this.butCancel.UseVisualStyleBackColor = true;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            this.butCancel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkManifest
            // 
            this.chkManifest.AutoSize = true;
            this.chkManifest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkManifest.Location = new System.Drawing.Point(320, 171);
            this.chkManifest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkManifest.Name = "chkManifest";
            this.chkManifest.Size = new System.Drawing.Size(112, 17);
            this.chkManifest.TabIndex = 14;
            this.chkManifest.Text = "Generate .manifest";
            this.chkManifest.UseVisualStyleBackColor = true;
            this.chkManifest.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 106);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Options:";
            // 
            // chkWriteTextures
            // 
            this.chkWriteTextures.AutoSize = true;
            this.chkWriteTextures.Checked = true;
            this.chkWriteTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWriteTextures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkWriteTextures.Location = new System.Drawing.Point(18, 125);
            this.chkWriteTextures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkWriteTextures.Name = "chkWriteTextures";
            this.chkWriteTextures.Size = new System.Drawing.Size(92, 17);
            this.chkWriteTextures.TabIndex = 11;
            this.chkWriteTextures.Text = "Write Textures";
            this.chkWriteTextures.UseVisualStyleBackColor = true;
            this.chkWriteTextures.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblBakeAnimation);
            this.groupBox1.Controls.Add(this.cmbBakeAnimationOptions);
            this.groupBox1.Controls.Add(this.chkApplyPreprocessToScene);
            this.groupBox1.Controls.Add(this.chkMrgContainersAndXref);
            this.groupBox1.Controls.Add(this.chkUsePreExportProces);
            this.groupBox1.Controls.Add(this.chkFlatten);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtEnvironmentName);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.chkFullPBR);
            this.groupBox1.Controls.Add(this.btnEnvBrowse);
            this.groupBox1.Controls.Add(this.chkNoAutoLight);
            this.groupBox1.Controls.Add(this.textureLabel);
            this.groupBox1.Controls.Add(this.txtTextureName);
            this.groupBox1.Controls.Add(this.btnTxtBrowse);
            this.groupBox1.Controls.Add(this.chkExportMaterials);
            this.groupBox1.Controls.Add(this.chkKHRMaterialsUnlit);
            this.groupBox1.Controls.Add(this.chkKHRTextureTransform);
            this.groupBox1.Controls.Add(this.chkKHRLightsPunctual);
            this.groupBox1.Controls.Add(this.chkOverwriteTextures);
            this.groupBox1.Controls.Add(this.chkDoNotOptimizeAnimations);
            this.groupBox1.Controls.Add(this.chkAnimgroupExportNonAnimated);
            this.groupBox1.Controls.Add(this.chkDracoCompression);
            this.groupBox1.Controls.Add(this.chkMergeAOwithMR);
            this.groupBox1.Controls.Add(this.txtQuality);
            this.groupBox1.Controls.Add(this.labelQuality);
            this.groupBox1.Controls.Add(this.chkExportMorphNormals);
            this.groupBox1.Controls.Add(this.chkExportMorphTangents);
            this.groupBox1.Controls.Add(this.chkExportTangents);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtScaleFactor);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboOutputFormat);
            this.groupBox1.Controls.Add(this.chkOnlySelected);
            this.groupBox1.Controls.Add(this.chkAutoSave);
            this.groupBox1.Controls.Add(this.chkHidden);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkWriteTextures);
            this.groupBox1.Controls.Add(this.txtModelName);
            this.groupBox1.Controls.Add(this.chkManifest);
            this.groupBox1.Controls.Add(this.butModelBrowse);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 6);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(892, 479);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // lblBakeAnimation
            // 
            this.lblBakeAnimation.AutoSize = true;
            this.lblBakeAnimation.Enabled = false;
            this.lblBakeAnimation.Location = new System.Drawing.Point(195, 264);
            this.lblBakeAnimation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBakeAnimation.Name = "lblBakeAnimation";
            this.lblBakeAnimation.Size = new System.Drawing.Size(125, 13);
            this.lblBakeAnimation.TabIndex = 40;
            this.lblBakeAnimation.Text = "Bake animations options:";
            // 
            // cmbBakeAnimationOptions
            // 
            this.cmbBakeAnimationOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBakeAnimationOptions.Enabled = false;
            this.cmbBakeAnimationOptions.Items.AddRange(new object[] {
            "Do not bake animations",
            "Bake all animations",
            "Selective bake"});
            this.cmbBakeAnimationOptions.Location = new System.Drawing.Point(328, 261);
            this.cmbBakeAnimationOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBakeAnimationOptions.Name = "cmbBakeAnimationOptions";
            this.cmbBakeAnimationOptions.Size = new System.Drawing.Size(178, 21);
            this.cmbBakeAnimationOptions.TabIndex = 41;
            // 
            // chkApplyPreprocessToScene
            // 
            this.chkApplyPreprocessToScene.AutoSize = true;
            this.chkApplyPreprocessToScene.Enabled = false;
            this.chkApplyPreprocessToScene.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkApplyPreprocessToScene.Location = new System.Drawing.Point(18, 302);
            this.chkApplyPreprocessToScene.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkApplyPreprocessToScene.Name = "chkApplyPreprocessToScene";
            this.chkApplyPreprocessToScene.Size = new System.Drawing.Size(155, 17);
            this.chkApplyPreprocessToScene.TabIndex = 39;
            this.chkApplyPreprocessToScene.Text = "Apply Preprocess To Scene";
            this.chkApplyPreprocessToScene.UseVisualStyleBackColor = true;
            // 
            // chkMrgContainersAndXref
            // 
            this.chkMrgContainersAndXref.AutoSize = true;
            this.chkMrgContainersAndXref.Enabled = false;
            this.chkMrgContainersAndXref.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkMrgContainersAndXref.Location = new System.Drawing.Point(18, 282);
            this.chkMrgContainersAndXref.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkMrgContainersAndXref.Name = "chkMrgContainersAndXref";
            this.chkMrgContainersAndXref.Size = new System.Drawing.Size(155, 17);
            this.chkMrgContainersAndXref.TabIndex = 37;
            this.chkMrgContainersAndXref.Text = "Merge Containers And XRef";
            this.chkMrgContainersAndXref.UseVisualStyleBackColor = true;
            // 
            // chkUsePreExportProces
            // 
            this.chkUsePreExportProces.AutoSize = true;
            this.chkUsePreExportProces.Location = new System.Drawing.Point(9, 265);
            this.chkUsePreExportProces.Name = "chkUsePreExportProces";
            this.chkUsePreExportProces.Size = new System.Drawing.Size(138, 17);
            this.chkUsePreExportProces.TabIndex = 36;
            this.chkUsePreExportProces.Text = "Use PreExport Process:";
            this.chkUsePreExportProces.UseVisualStyleBackColor = true;
            this.chkUsePreExportProces.CheckedChanged += new System.EventHandler(this.chkUsePreExportProces_CheckedChanged);
            // 
            // chkFlatten
            // 
            this.chkFlatten.AutoSize = true;
            this.chkFlatten.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkFlatten.Location = new System.Drawing.Point(18, 240);
            this.chkFlatten.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFlatten.Name = "chkFlatten";
            this.chkFlatten.Size = new System.Drawing.Size(111, 17);
            this.chkFlatten.TabIndex = 35;
            this.chkFlatten.Text = "Flatten Hierarchies";
            this.chkFlatten.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 420);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Environment";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 340);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Morph Target Options:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 383);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "Babylon PBR Options:";
            // 
            // txtEnvironmentName
            // 
            this.txtEnvironmentName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEnvironmentName.Location = new System.Drawing.Point(91, 418);
            this.txtEnvironmentName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtEnvironmentName.Multiline = false;
            this.txtEnvironmentName.Name = "txtEnvironmentName";
            this.txtEnvironmentName.Size = new System.Drawing.Size(708, 20);
            this.txtEnvironmentName.TabIndex = 30;
            this.txtEnvironmentName.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 438);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "GLTF Options:";
            // 
            // chkFullPBR
            // 
            this.chkFullPBR.AutoSize = true;
            this.chkFullPBR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkFullPBR.Location = new System.Drawing.Point(177, 399);
            this.chkFullPBR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFullPBR.Name = "chkFullPBR";
            this.chkFullPBR.Size = new System.Drawing.Size(86, 17);
            this.chkFullPBR.TabIndex = 28;
            this.chkFullPBR.Text = "Use Full PBR";
            this.chkFullPBR.UseVisualStyleBackColor = true;
            // 
            // btnEnvBrowse
            // 
            this.btnEnvBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnvBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnvBrowse.Location = new System.Drawing.Point(805, 416);
            this.btnEnvBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnEnvBrowse.Name = "btnEnvBrowse";
            this.btnEnvBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnEnvBrowse.TabIndex = 31;
            this.btnEnvBrowse.Text = "...";
            this.btnEnvBrowse.UseVisualStyleBackColor = true;
            this.btnEnvBrowse.Click += new System.EventHandler(this.btnEnvBrowse_Click);
            // 
            // chkNoAutoLight
            // 
            this.chkNoAutoLight.AutoSize = true;
            this.chkNoAutoLight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkNoAutoLight.Location = new System.Drawing.Point(23, 399);
            this.chkNoAutoLight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkNoAutoLight.Name = "chkNoAutoLight";
            this.chkNoAutoLight.Size = new System.Drawing.Size(113, 17);
            this.chkNoAutoLight.TabIndex = 27;
            this.chkNoAutoLight.Text = "No Automatic Light";
            this.chkNoAutoLight.UseVisualStyleBackColor = true;
            // 
            // textureLabel
            // 
            this.textureLabel.AutoSize = true;
            this.textureLabel.Location = new System.Drawing.Point(6, 43);
            this.textureLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.textureLabel.Name = "textureLabel";
            this.textureLabel.Size = new System.Drawing.Size(75, 13);
            this.textureLabel.TabIndex = 24;
            this.textureLabel.Text = "Texture folder:";
            // 
            // txtTextureName
            // 
            this.txtTextureName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTextureName.Location = new System.Drawing.Point(86, 40);
            this.txtTextureName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTextureName.Multiline = false;
            this.txtTextureName.Name = "txtTextureName";
            this.txtTextureName.Size = new System.Drawing.Size(708, 20);
            this.txtTextureName.TabIndex = 25;
            this.txtTextureName.Text = "";
            // 
            // btnTxtBrowse
            // 
            this.btnTxtBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTxtBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTxtBrowse.Location = new System.Drawing.Point(800, 38);
            this.btnTxtBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTxtBrowse.Name = "btnTxtBrowse";
            this.btnTxtBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnTxtBrowse.TabIndex = 26;
            this.btnTxtBrowse.Text = "...";
            this.btnTxtBrowse.UseVisualStyleBackColor = true;
            this.btnTxtBrowse.Click += new System.EventHandler(this.btnTextureBrowse_Click);
            // 
            // chkExportMaterials
            // 
            this.chkExportMaterials.AutoSize = true;
            this.chkExportMaterials.Checked = true;
            this.chkExportMaterials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportMaterials.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMaterials.Location = new System.Drawing.Point(18, 195);
            this.chkExportMaterials.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkExportMaterials.Name = "chkExportMaterials";
            this.chkExportMaterials.Size = new System.Drawing.Size(98, 17);
            this.chkExportMaterials.TabIndex = 23;
            this.chkExportMaterials.Text = "Export Materials";
            this.chkExportMaterials.UseVisualStyleBackColor = true;
            // 
            // chkKHRMaterialsUnlit
            // 
            this.chkKHRMaterialsUnlit.AutoSize = true;
            this.chkKHRMaterialsUnlit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKHRMaterialsUnlit.Location = new System.Drawing.Point(323, 455);
            this.chkKHRMaterialsUnlit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKHRMaterialsUnlit.Name = "chkKHRMaterialsUnlit";
            this.chkKHRMaterialsUnlit.Size = new System.Drawing.Size(118, 17);
            this.chkKHRMaterialsUnlit.TabIndex = 22;
            this.chkKHRMaterialsUnlit.Text = "KHR_materials_unlit";
            this.chkKHRMaterialsUnlit.UseVisualStyleBackColor = true;
            // 
            // chkKHRTextureTransform
            // 
            this.chkKHRTextureTransform.AutoSize = true;
            this.chkKHRTextureTransform.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKHRTextureTransform.Location = new System.Drawing.Point(172, 455);
            this.chkKHRTextureTransform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKHRTextureTransform.Name = "chkKHRTextureTransform";
            this.chkKHRTextureTransform.Size = new System.Drawing.Size(133, 17);
            this.chkKHRTextureTransform.TabIndex = 21;
            this.chkKHRTextureTransform.Text = "KHR_texture_transform";
            this.chkKHRTextureTransform.UseVisualStyleBackColor = true;
            // 
            // chkKHRLightsPunctual
            // 
            this.chkKHRLightsPunctual.AutoSize = true;
            this.chkKHRLightsPunctual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKHRLightsPunctual.Location = new System.Drawing.Point(24, 455);
            this.chkKHRLightsPunctual.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKHRLightsPunctual.Name = "chkKHRLightsPunctual";
            this.chkKHRLightsPunctual.Size = new System.Drawing.Size(123, 17);
            this.chkKHRLightsPunctual.TabIndex = 20;
            this.chkKHRLightsPunctual.Text = "KHR_lights_punctual";
            this.chkKHRLightsPunctual.UseVisualStyleBackColor = true;
            // 
            // chkOverwriteTextures
            // 
            this.chkOverwriteTextures.AutoSize = true;
            this.chkOverwriteTextures.Checked = true;
            this.chkOverwriteTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOverwriteTextures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkOverwriteTextures.Location = new System.Drawing.Point(18, 149);
            this.chkOverwriteTextures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOverwriteTextures.Name = "chkOverwriteTextures";
            this.chkOverwriteTextures.Size = new System.Drawing.Size(112, 17);
            this.chkOverwriteTextures.TabIndex = 19;
            this.chkOverwriteTextures.Text = "Overwrite Textures";
            this.chkOverwriteTextures.UseVisualStyleBackColor = true;
            // 
            // chkDoNotOptimizeAnimations
            // 
            this.chkDoNotOptimizeAnimations.AutoSize = true;
            this.chkDoNotOptimizeAnimations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDoNotOptimizeAnimations.Location = new System.Drawing.Point(166, 195);
            this.chkDoNotOptimizeAnimations.Name = "chkDoNotOptimizeAnimations";
            this.chkDoNotOptimizeAnimations.Size = new System.Drawing.Size(154, 17);
            this.chkDoNotOptimizeAnimations.TabIndex = 18;
            this.chkDoNotOptimizeAnimations.Text = "Do Not Optimize Animations";
            this.chkDoNotOptimizeAnimations.UseVisualStyleBackColor = true;
            this.chkDoNotOptimizeAnimations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkAnimgroupExportNonAnimated
            // 
            this.chkAnimgroupExportNonAnimated.AutoSize = true;
            this.chkAnimgroupExportNonAnimated.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAnimgroupExportNonAnimated.Location = new System.Drawing.Point(18, 218);
            this.chkAnimgroupExportNonAnimated.Name = "chkAnimgroupExportNonAnimated";
            this.chkAnimgroupExportNonAnimated.Size = new System.Drawing.Size(249, 17);
            this.chkAnimgroupExportNonAnimated.TabIndex = 18;
            this.chkAnimgroupExportNonAnimated.Text = "(Animation Group) Export Non-Animated Objects";
            this.chkAnimgroupExportNonAnimated.UseVisualStyleBackColor = true;
            this.chkAnimgroupExportNonAnimated.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkDracoCompression
            // 
            this.chkDracoCompression.AutoSize = true;
            this.chkDracoCompression.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDracoCompression.Location = new System.Drawing.Point(166, 171);
            this.chkDracoCompression.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkDracoCompression.Name = "chkDracoCompression";
            this.chkDracoCompression.Size = new System.Drawing.Size(136, 17);
            this.chkDracoCompression.TabIndex = 18;
            this.chkDracoCompression.Text = "Use Draco compression";
            this.chkDracoCompression.UseVisualStyleBackColor = true;
            this.chkDracoCompression.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkMergeAOwithMR
            // 
            this.chkMergeAOwithMR.AutoSize = true;
            this.chkMergeAOwithMR.Checked = true;
            this.chkMergeAOwithMR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMergeAOwithMR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkMergeAOwithMR.Location = new System.Drawing.Point(18, 171);
            this.chkMergeAOwithMR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkMergeAOwithMR.Name = "chkMergeAOwithMR";
            this.chkMergeAOwithMR.Size = new System.Drawing.Size(94, 17);
            this.chkMergeAOwithMR.TabIndex = 17;
            this.chkMergeAOwithMR.Text = "Merge AO map";
            this.chkMergeAOwithMR.UseVisualStyleBackColor = true;
            this.chkMergeAOwithMR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // txtQuality
            // 
            this.txtQuality.Location = new System.Drawing.Point(403, 92);
            this.txtQuality.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtQuality.Name = "txtQuality";
            this.txtQuality.Size = new System.Drawing.Size(43, 20);
            this.txtQuality.TabIndex = 9;
            this.txtQuality.Text = "100";
            this.txtQuality.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtQuality.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // labelQuality
            // 
            this.labelQuality.AutoSize = true;
            this.labelQuality.Location = new System.Drawing.Point(319, 94);
            this.labelQuality.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelQuality.Name = "labelQuality";
            this.labelQuality.Size = new System.Drawing.Size(79, 13);
            this.labelQuality.TabIndex = 8;
            this.labelQuality.Text = "Texture quality:";
            // 
            // chkExportMorphNormals
            // 
            this.chkExportMorphNormals.AutoSize = true;
            this.chkExportMorphNormals.Checked = true;
            this.chkExportMorphNormals.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportMorphNormals.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMorphNormals.Location = new System.Drawing.Point(166, 356);
            this.chkExportMorphNormals.Name = "chkExportMorphNormals";
            this.chkExportMorphNormals.Size = new System.Drawing.Size(124, 17);
            this.chkExportMorphNormals.TabIndex = 16;
            this.chkExportMorphNormals.Text = "Export morph normals";
            this.chkExportMorphNormals.UseVisualStyleBackColor = true;
            this.chkExportMorphNormals.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkExportMorphTangents
            // 
            this.chkExportMorphTangents.AutoSize = true;
            this.chkExportMorphTangents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMorphTangents.Location = new System.Drawing.Point(18, 356);
            this.chkExportMorphTangents.Name = "chkExportMorphTangents";
            this.chkExportMorphTangents.Size = new System.Drawing.Size(129, 17);
            this.chkExportMorphTangents.TabIndex = 16;
            this.chkExportMorphTangents.Text = "Export morph tangents";
            this.chkExportMorphTangents.UseVisualStyleBackColor = true;
            this.chkExportMorphTangents.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkExportTangents
            // 
            this.chkExportTangents.AutoSize = true;
            this.chkExportTangents.Checked = true;
            this.chkExportTangents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportTangents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportTangents.Location = new System.Drawing.Point(320, 148);
            this.chkExportTangents.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkExportTangents.Name = "chkExportTangents";
            this.chkExportTangents.Size = new System.Drawing.Size(97, 17);
            this.chkExportTangents.TabIndex = 16;
            this.chkExportTangents.Text = "Export tangents";
            this.chkExportTangents.UseVisualStyleBackColor = true;
            this.chkExportTangents.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(331, 70);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Scale factor:";
            // 
            // txtScaleFactor
            // 
            this.txtScaleFactor.Location = new System.Drawing.Point(403, 68);
            this.txtScaleFactor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtScaleFactor.Name = "txtScaleFactor";
            this.txtScaleFactor.Size = new System.Drawing.Size(42, 20);
            this.txtScaleFactor.TabIndex = 7;
            this.txtScaleFactor.Text = "1";
            this.txtScaleFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScaleFactor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 66);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Output format:";
            // 
            // comboOutputFormat
            // 
            this.comboOutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOutputFormat.Items.AddRange(new object[] {
            "babylon",
            "binary babylon",
            "gltf",
            "glb"});
            this.comboOutputFormat.Location = new System.Drawing.Point(86, 64);
            this.comboOutputFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboOutputFormat.Name = "comboOutputFormat";
            this.comboOutputFormat.Size = new System.Drawing.Size(121, 21);
            this.comboOutputFormat.TabIndex = 5;
            this.comboOutputFormat.SelectedIndexChanged += new System.EventHandler(this.comboOutputFormat_SelectedIndexChanged);
            this.comboOutputFormat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkOnlySelected
            // 
            this.chkOnlySelected.AutoSize = true;
            this.chkOnlySelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkOnlySelected.Location = new System.Drawing.Point(320, 125);
            this.chkOnlySelected.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOnlySelected.Name = "chkOnlySelected";
            this.chkOnlySelected.Size = new System.Drawing.Size(118, 17);
            this.chkOnlySelected.TabIndex = 13;
            this.chkOnlySelected.Text = "Export only selected";
            this.chkOnlySelected.UseVisualStyleBackColor = true;
            this.chkOnlySelected.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkAutoSave
            // 
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAutoSave.Location = new System.Drawing.Point(166, 148);
            this.chkAutoSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(130, 17);
            this.chkAutoSave.TabIndex = 15;
            this.chkAutoSave.Text = "Auto save 3ds Max file";
            this.chkAutoSave.UseVisualStyleBackColor = true;
            this.chkAutoSave.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkHidden
            // 
            this.chkHidden.AutoSize = true;
            this.chkHidden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkHidden.Location = new System.Drawing.Point(166, 125);
            this.chkHidden.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHidden.Name = "chkHidden";
            this.chkHidden.Size = new System.Drawing.Size(125, 17);
            this.chkHidden.TabIndex = 12;
            this.chkHidden.Text = "Export hidden objects";
            this.chkHidden.UseVisualStyleBackColor = true;
            this.chkHidden.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butExportAndRun
            // 
            this.butExportAndRun.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butExportAndRun.Enabled = false;
            this.butExportAndRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butExportAndRun.Location = new System.Drawing.Point(624, 495);
            this.butExportAndRun.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butExportAndRun.Name = "butExportAndRun";
            this.butExportAndRun.Size = new System.Drawing.Size(197, 27);
            this.butExportAndRun.TabIndex = 102;
            this.butExportAndRun.Text = "Export && Run";
            this.butExportAndRun.UseVisualStyleBackColor = true;
            this.butExportAndRun.Click += new System.EventHandler(this.butExportAndRun_Click);
            this.butExportAndRun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butClose
            // 
            this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butClose.Location = new System.Drawing.Point(1184, 861);
            this.butClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(80, 23);
            this.butClose.TabIndex = 106;
            this.butClose.Text = "Close";
            this.butClose.UseVisualStyleBackColor = true;
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            this.butClose.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // toolTipDracoCompression
            // 
            this.toolTipDracoCompression.ShowAlways = true;
            // 
            // butMultiExport
            // 
            this.butMultiExport.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butMultiExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butMultiExport.Location = new System.Drawing.Point(827, 495);
            this.butMultiExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butMultiExport.Name = "butMultiExport";
            this.butMultiExport.Size = new System.Drawing.Size(199, 27);
            this.butMultiExport.TabIndex = 109;
            this.butMultiExport.Text = "Multi-File Export | Shift-click to edit";
            this.butMultiExport.UseVisualStyleBackColor = true;
            this.butMultiExport.Click += new System.EventHandler(this.butMultiExport_Click);
            // 
            // saveOptionBtn
            // 
            this.saveOptionBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.saveOptionBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveOptionBtn.Location = new System.Drawing.Point(218, 495);
            this.saveOptionBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.saveOptionBtn.Name = "saveOptionBtn";
            this.saveOptionBtn.Size = new System.Drawing.Size(197, 27);
            this.saveOptionBtn.TabIndex = 110;
            this.saveOptionBtn.Text = "Save";
            this.saveOptionBtn.UseVisualStyleBackColor = true;
            this.saveOptionBtn.Click += new System.EventHandler(this.saveOptionBtn_Click);
            // 
            // envFileDialog
            // 
            this.envFileDialog.DefaultExt = "dds";
            this.envFileDialog.Filter = "dds files|*.dds";
            this.envFileDialog.Title = "Select Environment";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = global::Max2Babylon.Properties.Resources.MaxExporter;
            this.pictureBox2.Location = new System.Drawing.Point(916, 11);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(348, 183);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // ExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 898);
            this.Controls.Add(this.saveOptionBtn);
            this.Controls.Add(this.butMultiExport);
            this.Controls.Add(this.butExportAndRun);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.butExport);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(846, 388);
            this.Name = "ExporterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Babylon.js - Export scene to babylon or glTF format";
            this.Activated += new System.EventHandler(this.ExporterForm_Activated);
            this.Deactivate += new System.EventHandler(this.ExporterForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExporterForm_FormClosed);
            this.Load += new System.EventHandler(this.ExporterForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox txtModelName;
        private System.Windows.Forms.Button butModelBrowse;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Button butCancel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.CheckBox chkManifest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkHidden;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.Button butExportAndRun;
        private System.Windows.Forms.CheckBox chkOnlySelected;
        private System.Windows.Forms.Button butClose;
        private System.Windows.Forms.ComboBox comboOutputFormat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScaleFactor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkExportTangents;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.TextBox txtQuality;
        private System.Windows.Forms.CheckBox chkMergeAOwithMR;
        private System.Windows.Forms.CheckBox chkDracoCompression;
        private System.Windows.Forms.ToolTip toolTipDracoCompression;
        private System.Windows.Forms.CheckBox chkOverwriteTextures;
        private System.Windows.Forms.Button butMultiExport;
        private System.Windows.Forms.CheckBox chkKHRLightsPunctual;
        private System.Windows.Forms.CheckBox chkKHRTextureTransform;
        private System.Windows.Forms.CheckBox chkKHRMaterialsUnlit;
        private System.Windows.Forms.CheckBox chkExportMaterials;
        private System.Windows.Forms.Button saveOptionBtn;
        private System.Windows.Forms.Label textureLabel;
        private System.Windows.Forms.RichTextBox txtTextureName;
        private System.Windows.Forms.Button btnTxtBrowse;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RichTextBox txtEnvironmentName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkFullPBR;
        private System.Windows.Forms.Button btnEnvBrowse;
        private System.Windows.Forms.CheckBox chkNoAutoLight;
        private System.Windows.Forms.CheckBox chkWriteTextures;
        private System.Windows.Forms.OpenFileDialog envFileDialog;
        private System.Windows.Forms.CheckBox chkAnimgroupExportNonAnimated;
        private System.Windows.Forms.CheckBox chkDoNotOptimizeAnimations;
        private System.Windows.Forms.CheckBox chkExportMorphTangents;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkExportMorphNormals;
        private System.Windows.Forms.CheckBox chkFlatten;
        private System.Windows.Forms.CheckBox chkUsePreExportProces;
        private System.Windows.Forms.CheckBox chkMrgContainersAndXref;
        private System.Windows.Forms.CheckBox chkApplyPreprocessToScene;
        private System.Windows.Forms.Label lblBakeAnimation;
        private System.Windows.Forms.ComboBox cmbBakeAnimationOptions;
    }
}
