namespace Maya2Babylon.Forms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExporterForm));
            this.butExport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFilename = new System.Windows.Forms.TextBox();
            this.butBrowse = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.treeView = new System.Windows.Forms.TreeView();
            this.butCancel = new System.Windows.Forms.Button();
            this.chkManifest = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkCopyTextures = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkDefaultSkybox = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtEnvironmentName = new System.Windows.Forms.TextBox();
            this.butEnvironmentPath = new System.Windows.Forms.Button();
            this.chkFullPBR = new System.Windows.Forms.CheckBox();
            this.chkNoAutoLight = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkExportAnimationsOnly = new System.Windows.Forms.CheckBox();
            this.chkExportAnimations = new System.Windows.Forms.CheckBox();
            this.chkBakeAnimationFrames = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkExportKHRMaterialsUnlit = new System.Windows.Forms.CheckBox();
            this.chkExportKHRTextureTransform = new System.Windows.Forms.CheckBox();
            this.chkExportKHRLightsPunctual = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkExportMorphNormal = new System.Windows.Forms.CheckBox();
            this.chkExportMorphTangent = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkDracoCompression = new System.Windows.Forms.CheckBox();
            this.txtQuality = new System.Windows.Forms.TextBox();
            this.labelQuality = new System.Windows.Forms.Label();
            this.chkExportSkin = new System.Windows.Forms.CheckBox();
            this.chkExportMaterials = new System.Windows.Forms.CheckBox();
            this.chkExportTangents = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtScaleFactor = new System.Windows.Forms.TextBox();
            this.chkOptimizeVertices = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboOutputFormat = new System.Windows.Forms.ComboBox();
            this.chkOnlySelected = new System.Windows.Forms.CheckBox();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.chkHidden = new System.Windows.Forms.CheckBox();
            this.chkExportTextures = new System.Windows.Forms.CheckBox();
            this.butExportAndRun = new System.Windows.Forms.Button();
            this.butClose = new System.Windows.Forms.Button();
            this.toolTipDracoCompression = new System.Windows.Forms.ToolTip(this.components);
            this.envFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.butCopyToClipboard = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.comboTexturePolicy = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // butExport
            // 
            this.butExport.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butExport.Enabled = false;
            this.butExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butExport.Location = new System.Drawing.Point(284, 581);
            this.butExport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butExport.Name = "butExport";
            this.butExport.Size = new System.Drawing.Size(263, 33);
            this.butExport.TabIndex = 100;
            this.butExport.Text = "Export";
            this.butExport.UseVisualStyleBackColor = true;
            this.butExport.Click += new System.EventHandler(this.butExport_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "File name:";
            // 
            // txtFilename
            // 
            this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilename.Location = new System.Drawing.Point(24, 42);
            this.txtFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFilename.Name = "txtFilename";
            this.txtFilename.Size = new System.Drawing.Size(504, 22);
            this.txtFilename.TabIndex = 2;
            this.txtFilename.TextChanged += new System.EventHandler(this.txtFilename_TextChanged);
            // 
            // butBrowse
            // 
            this.butBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butBrowse.Location = new System.Drawing.Point(537, 39);
            this.butBrowse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butBrowse.Name = "butBrowse";
            this.butBrowse.Size = new System.Drawing.Size(57, 28);
            this.butBrowse.TabIndex = 3;
            this.butBrowse.Text = "...";
            this.butBrowse.UseVisualStyleBackColor = true;
            this.butBrowse.Click += new System.EventHandler(this.butBrowse_Click);
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
            this.progressBar.Location = new System.Drawing.Point(16, 846);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(691, 28);
            this.progressBar.TabIndex = 103;
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.Location = new System.Drawing.Point(16, 622);
            this.treeView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(1079, 215);
            this.treeView.TabIndex = 102;
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Enabled = false;
            this.butCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butCancel.Location = new System.Drawing.Point(875, 846);
            this.butCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(107, 28);
            this.butCancel.TabIndex = 104;
            this.butCancel.Text = "Cancel";
            this.butCancel.UseVisualStyleBackColor = true;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // chkManifest
            // 
            this.chkManifest.AutoSize = true;
            this.chkManifest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkManifest.Location = new System.Drawing.Point(22, 206);
            this.chkManifest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkManifest.Name = "chkManifest";
            this.chkManifest.Size = new System.Drawing.Size(137, 20);
            this.chkManifest.TabIndex = 14;
            this.chkManifest.Text = "Generate .manifest";
            this.chkManifest.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 155);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Options:";
            // 
            // chkCopyTextures
            // 
            this.chkCopyTextures.AutoSize = true;
            this.chkCopyTextures.Checked = true;
            this.chkCopyTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCopyTextures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkCopyTextures.Location = new System.Drawing.Point(219, 262);
            this.chkCopyTextures.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkCopyTextures.Name = "chkCopyTextures";
            this.chkCopyTextures.Size = new System.Drawing.Size(159, 20);
            this.chkCopyTextures.TabIndex = 11;
            this.chkCopyTextures.Text = "Copy textures to output";
            this.chkCopyTextures.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.chkDefaultSkybox);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtEnvironmentName);
            this.groupBox1.Controls.Add(this.comboTexturePolicy);
            this.groupBox1.Controls.Add(this.butEnvironmentPath);
            this.groupBox1.Controls.Add(this.chkFullPBR);
            this.groupBox1.Controls.Add(this.chkNoAutoLight);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.chkExportAnimationsOnly);
            this.groupBox1.Controls.Add(this.chkExportAnimations);
            this.groupBox1.Controls.Add(this.chkBakeAnimationFrames);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.chkExportKHRMaterialsUnlit);
            this.groupBox1.Controls.Add(this.chkExportKHRTextureTransform);
            this.groupBox1.Controls.Add(this.chkExportKHRLightsPunctual);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.chkExportMorphNormal);
            this.groupBox1.Controls.Add(this.chkExportMorphTangent);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.chkDracoCompression);
            this.groupBox1.Controls.Add(this.txtQuality);
            this.groupBox1.Controls.Add(this.labelQuality);
            this.groupBox1.Controls.Add(this.chkExportSkin);
            this.groupBox1.Controls.Add(this.chkExportMaterials);
            this.groupBox1.Controls.Add(this.chkExportTangents);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtScaleFactor);
            this.groupBox1.Controls.Add(this.chkOptimizeVertices);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboOutputFormat);
            this.groupBox1.Controls.Add(this.chkOnlySelected);
            this.groupBox1.Controls.Add(this.chkAutoSave);
            this.groupBox1.Controls.Add(this.chkHidden);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkExportTextures);
            this.groupBox1.Controls.Add(this.chkCopyTextures);
            this.groupBox1.Controls.Add(this.txtFilename);
            this.groupBox1.Controls.Add(this.chkManifest);
            this.groupBox1.Controls.Add(this.butBrowse);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(16, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(603, 555);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // chkDefaultSkybox
            // 
            this.chkDefaultSkybox.AutoSize = true;
            this.chkDefaultSkybox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDefaultSkybox.Location = new System.Drawing.Point(426, 455);
            this.chkDefaultSkybox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkDefaultSkybox.Name = "chkDefaultSkybox";
            this.chkDefaultSkybox.Size = new System.Drawing.Size(143, 20);
            this.chkDefaultSkybox.TabIndex = 34;
            this.chkDefaultSkybox.Text = "Add Default Skybox";
            this.chkDefaultSkybox.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 508);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 16);
            this.label9.TabIndex = 33;
            this.label9.Text = "Environment";
            // 
            // txtEnvironmentName
            // 
            this.txtEnvironmentName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEnvironmentName.Location = new System.Drawing.Point(119, 503);
            this.txtEnvironmentName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtEnvironmentName.Name = "txtEnvironmentName";
            this.txtEnvironmentName.Size = new System.Drawing.Size(413, 22);
            this.txtEnvironmentName.TabIndex = 31;
            // 
            // butEnvironmentPath
            // 
            this.butEnvironmentPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butEnvironmentPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butEnvironmentPath.Location = new System.Drawing.Point(540, 502);
            this.butEnvironmentPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butEnvironmentPath.Name = "butEnvironmentPath";
            this.butEnvironmentPath.Size = new System.Drawing.Size(57, 28);
            this.butEnvironmentPath.TabIndex = 32;
            this.butEnvironmentPath.Text = "...";
            this.butEnvironmentPath.UseVisualStyleBackColor = true;
            this.butEnvironmentPath.Click += new System.EventHandler(this.butEnvironmentPath_Click);
            // 
            // chkFullPBR
            // 
            this.chkFullPBR.AutoSize = true;
            this.chkFullPBR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkFullPBR.Location = new System.Drawing.Point(221, 455);
            this.chkFullPBR.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkFullPBR.Name = "chkFullPBR";
            this.chkFullPBR.Size = new System.Drawing.Size(105, 20);
            this.chkFullPBR.TabIndex = 30;
            this.chkFullPBR.Text = "Use Full PBR";
            this.chkFullPBR.UseVisualStyleBackColor = true;
            // 
            // chkNoAutoLight
            // 
            this.chkNoAutoLight.AutoSize = true;
            this.chkNoAutoLight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkNoAutoLight.Location = new System.Drawing.Point(23, 455);
            this.chkNoAutoLight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkNoAutoLight.Name = "chkNoAutoLight";
            this.chkNoAutoLight.Size = new System.Drawing.Size(136, 20);
            this.chkNoAutoLight.TabIndex = 29;
            this.chkNoAutoLight.Text = "No Automatic Light";
            this.chkNoAutoLight.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 435);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(137, 16);
            this.label8.TabIndex = 28;
            this.label8.Text = "Babylon PBR Options";
            // 
            // chkExportAnimationsOnly
            // 
            this.chkExportAnimationsOnly.AutoSize = true;
            this.chkExportAnimationsOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportAnimationsOnly.Location = new System.Drawing.Point(426, 412);
            this.chkExportAnimationsOnly.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportAnimationsOnly.Name = "chkExportAnimationsOnly";
            this.chkExportAnimationsOnly.Size = new System.Drawing.Size(162, 20);
            this.chkExportAnimationsOnly.TabIndex = 27;
            this.chkExportAnimationsOnly.Text = "Export Animations Only";
            this.chkExportAnimationsOnly.UseVisualStyleBackColor = true;
            // 
            // chkExportAnimations
            // 
            this.chkExportAnimations.AutoSize = true;
            this.chkExportAnimations.Checked = true;
            this.chkExportAnimations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportAnimations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportAnimations.Location = new System.Drawing.Point(221, 412);
            this.chkExportAnimations.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportAnimations.Name = "chkExportAnimations";
            this.chkExportAnimations.Size = new System.Drawing.Size(132, 20);
            this.chkExportAnimations.TabIndex = 27;
            this.chkExportAnimations.Text = "Export Animations";
            this.chkExportAnimations.UseVisualStyleBackColor = true;
            // 
            // chkBakeAnimationFrames
            // 
            this.chkBakeAnimationFrames.AutoSize = true;
            this.chkBakeAnimationFrames.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkBakeAnimationFrames.Location = new System.Drawing.Point(23, 412);
            this.chkBakeAnimationFrames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkBakeAnimationFrames.Name = "chkBakeAnimationFrames";
            this.chkBakeAnimationFrames.Size = new System.Drawing.Size(168, 20);
            this.chkBakeAnimationFrames.TabIndex = 27;
            this.chkBakeAnimationFrames.Text = "Bake Animation Frames";
            this.chkBakeAnimationFrames.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 391);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 16);
            this.label7.TabIndex = 26;
            this.label7.Text = "Animations";
            // 
            // chkExportKHRMaterialsUnlit
            // 
            this.chkExportKHRMaterialsUnlit.AutoSize = true;
            this.chkExportKHRMaterialsUnlit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportKHRMaterialsUnlit.Location = new System.Drawing.Point(426, 363);
            this.chkExportKHRMaterialsUnlit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportKHRMaterialsUnlit.Name = "chkExportKHRMaterialsUnlit";
            this.chkExportKHRMaterialsUnlit.Size = new System.Drawing.Size(145, 20);
            this.chkExportKHRMaterialsUnlit.TabIndex = 25;
            this.chkExportKHRMaterialsUnlit.Text = "KHR_materials_unlit";
            this.chkExportKHRMaterialsUnlit.UseVisualStyleBackColor = true;
            // 
            // chkExportKHRTextureTransform
            // 
            this.chkExportKHRTextureTransform.AutoSize = true;
            this.chkExportKHRTextureTransform.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportKHRTextureTransform.Location = new System.Drawing.Point(221, 363);
            this.chkExportKHRTextureTransform.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportKHRTextureTransform.Name = "chkExportKHRTextureTransform";
            this.chkExportKHRTextureTransform.Size = new System.Drawing.Size(161, 20);
            this.chkExportKHRTextureTransform.TabIndex = 25;
            this.chkExportKHRTextureTransform.Text = "KHR_texture_transform";
            this.chkExportKHRTextureTransform.UseVisualStyleBackColor = true;
            // 
            // chkExportKHRLightsPunctual
            // 
            this.chkExportKHRLightsPunctual.AutoSize = true;
            this.chkExportKHRLightsPunctual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportKHRLightsPunctual.Location = new System.Drawing.Point(23, 363);
            this.chkExportKHRLightsPunctual.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportKHRLightsPunctual.Name = "chkExportKHRLightsPunctual";
            this.chkExportKHRLightsPunctual.Size = new System.Drawing.Size(148, 20);
            this.chkExportKHRLightsPunctual.TabIndex = 24;
            this.chkExportKHRLightsPunctual.Text = "KHR_lights_punctual";
            this.chkExportKHRLightsPunctual.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 337);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 16);
            this.label6.TabIndex = 23;
            this.label6.Text = "glTF Extensions";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // chkExportMorphNormal
            // 
            this.chkExportMorphNormal.AutoSize = true;
            this.chkExportMorphNormal.Checked = true;
            this.chkExportMorphNormal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportMorphNormal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMorphNormal.Location = new System.Drawing.Point(23, 305);
            this.chkExportMorphNormal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportMorphNormal.Name = "chkExportMorphNormal";
            this.chkExportMorphNormal.Size = new System.Drawing.Size(107, 20);
            this.chkExportMorphNormal.TabIndex = 21;
            this.chkExportMorphNormal.Text = "Export normal";
            this.chkExportMorphNormal.UseVisualStyleBackColor = true;
            // 
            // chkExportMorphTangent
            // 
            this.chkExportMorphTangent.AutoSize = true;
            this.chkExportMorphTangent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMorphTangent.Location = new System.Drawing.Point(221, 305);
            this.chkExportMorphTangent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportMorphTangent.Name = "chkExportMorphTangent";
            this.chkExportMorphTangent.Size = new System.Drawing.Size(110, 20);
            this.chkExportMorphTangent.TabIndex = 22;
            this.chkExportMorphTangent.Text = "Export tangent";
            this.chkExportMorphTangent.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 285);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(135, 16);
            this.label5.TabIndex = 20;
            this.label5.Text = "MorphTarget options:";
            // 
            // chkDracoCompression
            // 
            this.chkDracoCompression.AutoSize = true;
            this.chkDracoCompression.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDracoCompression.Location = new System.Drawing.Point(219, 235);
            this.chkDracoCompression.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkDracoCompression.Name = "chkDracoCompression";
            this.chkDracoCompression.Size = new System.Drawing.Size(171, 20);
            this.chkDracoCompression.TabIndex = 18;
            this.chkDracoCompression.Text = "Use Draco compression";
            this.chkDracoCompression.UseVisualStyleBackColor = true;
            // 
            // txtQuality
            // 
            this.txtQuality.Location = new System.Drawing.Point(536, 114);
            this.txtQuality.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtQuality.Name = "txtQuality";
            this.txtQuality.Size = new System.Drawing.Size(55, 22);
            this.txtQuality.TabIndex = 9;
            this.txtQuality.Text = "100";
            this.txtQuality.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelQuality
            // 
            this.labelQuality.AutoSize = true;
            this.labelQuality.Location = new System.Drawing.Point(423, 121);
            this.labelQuality.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelQuality.Name = "labelQuality";
            this.labelQuality.Size = new System.Drawing.Size(97, 16);
            this.labelQuality.TabIndex = 8;
            this.labelQuality.Text = "Texture quality:";
            // 
            // chkExportSkin
            // 
            this.chkExportSkin.AutoSize = true;
            this.chkExportSkin.Checked = true;
            this.chkExportSkin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportSkin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportSkin.Location = new System.Drawing.Point(425, 235);
            this.chkExportSkin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportSkin.Name = "chkExportSkin";
            this.chkExportSkin.Size = new System.Drawing.Size(97, 20);
            this.chkExportSkin.TabIndex = 19;
            this.chkExportSkin.Text = "Export skins";
            this.chkExportSkin.UseVisualStyleBackColor = true;
            // 
            // chkExportMaterials
            // 
            this.chkExportMaterials.AutoSize = true;
            this.chkExportMaterials.Checked = true;
            this.chkExportMaterials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportMaterials.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMaterials.Location = new System.Drawing.Point(21, 262);
            this.chkExportMaterials.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportMaterials.Name = "chkExportMaterials";
            this.chkExportMaterials.Size = new System.Drawing.Size(121, 20);
            this.chkExportMaterials.TabIndex = 17;
            this.chkExportMaterials.Text = "Export materials";
            this.chkExportMaterials.UseVisualStyleBackColor = true;
            // 
            // chkExportTangents
            // 
            this.chkExportTangents.AutoSize = true;
            this.chkExportTangents.Checked = true;
            this.chkExportTangents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportTangents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportTangents.Location = new System.Drawing.Point(22, 235);
            this.chkExportTangents.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportTangents.Name = "chkExportTangents";
            this.chkExportTangents.Size = new System.Drawing.Size(117, 20);
            this.chkExportTangents.TabIndex = 17;
            this.chkExportTangents.Text = "Export tangents";
            this.chkExportTangents.UseVisualStyleBackColor = true;
            this.chkExportTangents.CheckedChanged += new System.EventHandler(this.chkExportTangents_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(439, 85);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Scale factor:";
            // 
            // txtScaleFactor
            // 
            this.txtScaleFactor.Location = new System.Drawing.Point(536, 81);
            this.txtScaleFactor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtScaleFactor.Name = "txtScaleFactor";
            this.txtScaleFactor.Size = new System.Drawing.Size(55, 22);
            this.txtScaleFactor.TabIndex = 7;
            this.txtScaleFactor.Text = "1";
            this.txtScaleFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkOptimizeVertices
            // 
            this.chkOptimizeVertices.AutoSize = true;
            this.chkOptimizeVertices.Checked = true;
            this.chkOptimizeVertices.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOptimizeVertices.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkOptimizeVertices.Location = new System.Drawing.Point(425, 206);
            this.chkOptimizeVertices.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkOptimizeVertices.Name = "chkOptimizeVertices";
            this.chkOptimizeVertices.Size = new System.Drawing.Size(127, 20);
            this.chkOptimizeVertices.TabIndex = 16;
            this.chkOptimizeVertices.Text = "Optimize vertices";
            this.chkOptimizeVertices.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 85);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 16);
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
            this.comboOutputFormat.Location = new System.Drawing.Point(115, 81);
            this.comboOutputFormat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboOutputFormat.Name = "comboOutputFormat";
            this.comboOutputFormat.Size = new System.Drawing.Size(160, 24);
            this.comboOutputFormat.TabIndex = 5;
            this.comboOutputFormat.SelectedIndexChanged += new System.EventHandler(this.comboOutputFormat_SelectedIndexChanged);
            // 
            // chkOnlySelected
            // 
            this.chkOnlySelected.AutoSize = true;
            this.chkOnlySelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkOnlySelected.Location = new System.Drawing.Point(425, 178);
            this.chkOnlySelected.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkOnlySelected.Name = "chkOnlySelected";
            this.chkOnlySelected.Size = new System.Drawing.Size(146, 20);
            this.chkOnlySelected.TabIndex = 13;
            this.chkOnlySelected.Text = "Export only selected";
            this.chkOnlySelected.UseVisualStyleBackColor = true;
            // 
            // chkAutoSave
            // 
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAutoSave.Location = new System.Drawing.Point(219, 206);
            this.chkAutoSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(142, 20);
            this.chkAutoSave.TabIndex = 15;
            this.chkAutoSave.Text = "Auto save Maya file";
            this.chkAutoSave.UseVisualStyleBackColor = true;
            // 
            // chkHidden
            // 
            this.chkHidden.AutoSize = true;
            this.chkHidden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkHidden.Location = new System.Drawing.Point(219, 178);
            this.chkHidden.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkHidden.Name = "chkHidden";
            this.chkHidden.Size = new System.Drawing.Size(154, 20);
            this.chkHidden.TabIndex = 12;
            this.chkHidden.Text = "Export hidden objects";
            this.chkHidden.UseVisualStyleBackColor = true;
            // 
            // chkExportTextures
            // 
            this.chkExportTextures.AutoSize = true;
            this.chkExportTextures.Checked = true;
            this.chkExportTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportTextures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportTextures.Location = new System.Drawing.Point(21, 179);
            this.chkExportTextures.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkExportTextures.Name = "chkExportTextures";
            this.chkExportTextures.Size = new System.Drawing.Size(112, 20);
            this.chkExportTextures.TabIndex = 11;
            this.chkExportTextures.Text = "Export textures";
            this.chkExportTextures.UseVisualStyleBackColor = true;
            // 
            // butExportAndRun
            // 
            this.butExportAndRun.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butExportAndRun.Enabled = false;
            this.butExportAndRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butExportAndRun.Location = new System.Drawing.Point(552, 581);
            this.butExportAndRun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butExportAndRun.Name = "butExportAndRun";
            this.butExportAndRun.Size = new System.Drawing.Size(263, 33);
            this.butExportAndRun.TabIndex = 101;
            this.butExportAndRun.Text = "Export && Run";
            this.butExportAndRun.UseVisualStyleBackColor = true;
            this.butExportAndRun.Click += new System.EventHandler(this.butExportAndRun_Click);
            // 
            // butClose
            // 
            this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butClose.Location = new System.Drawing.Point(989, 846);
            this.butClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(107, 28);
            this.butClose.TabIndex = 105;
            this.butClose.Text = "Close";
            this.butClose.UseVisualStyleBackColor = true;
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            // 
            // toolTipDracoCompression
            // 
            this.toolTipDracoCompression.ShowAlways = true;
            // 
            // envFileDialog
            // 
            this.envFileDialog.DefaultExt = "dds";
            this.envFileDialog.Filter = "dds files|*.dds";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = global::Maya2Babylon.Properties.Resources.MayaExporter;
            this.pictureBox2.Location = new System.Drawing.Point(624, 15);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(472, 207);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // butCopyToClipboard
            // 
            this.butCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCopyToClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butCopyToClipboard.Location = new System.Drawing.Point(715, 846);
            this.butCopyToClipboard.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.butCopyToClipboard.Name = "butCopyToClipboard";
            this.butCopyToClipboard.Size = new System.Drawing.Size(152, 28);
            this.butCopyToClipboard.TabIndex = 101;
            this.butCopyToClipboard.Text = "Copy To Clipboard";
            this.butCopyToClipboard.UseVisualStyleBackColor = true;
            this.butCopyToClipboard.Click += new System.EventHandler(this.butCopyToClipboard_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 120);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(174, 16);
            this.label10.TabIndex = 35;
            this.label10.Text = "Texture format export policy:";
            // 
            // comboTexturePolicy
            // 
            this.comboTexturePolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTexturePolicy.Items.AddRange(new object[] {
            "QUALITY",
            "CONSERVATIV",
            "SIZE"});
            this.comboTexturePolicy.Location = new System.Drawing.Point(199, 117);
            this.comboTexturePolicy.Margin = new System.Windows.Forms.Padding(4);
            this.comboTexturePolicy.Name = "comboTexturePolicy";
            this.comboTexturePolicy.Size = new System.Drawing.Size(160, 24);
            this.comboTexturePolicy.TabIndex = 36;
            // 
            // ExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1123, 884);
            this.Controls.Add(this.butCopyToClipboard);
            this.Controls.Add(this.butExportAndRun);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.butExport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1138, 836);
            this.Name = "ExporterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Babylon.js - Export scene to babylon or glTF format";
            this.Activated += new System.EventHandler(this.ExporterForm_Activated);
            this.Deactivate += new System.EventHandler(this.ExporterForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExporterForm_FormClosed);
            this.Load += new System.EventHandler(this.ExporterForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFilename;
        private System.Windows.Forms.Button butBrowse;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Button butCancel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.CheckBox chkManifest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkCopyTextures;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkHidden;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.Button butExportAndRun;
        private System.Windows.Forms.CheckBox chkOnlySelected;
        private System.Windows.Forms.Button butClose;
        private System.Windows.Forms.ComboBox comboOutputFormat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkOptimizeVertices;
        private System.Windows.Forms.TextBox txtScaleFactor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkExportTangents;
        private System.Windows.Forms.CheckBox chkExportSkin;
        private System.Windows.Forms.TextBox txtQuality;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.CheckBox chkDracoCompression;
        private System.Windows.Forms.ToolTip toolTipDracoCompression;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkExportMorphNormal;
        private System.Windows.Forms.CheckBox chkExportMorphTangent;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkExportKHRTextureTransform;
        private System.Windows.Forms.CheckBox chkExportKHRLightsPunctual;
        private System.Windows.Forms.CheckBox chkExportKHRMaterialsUnlit;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkBakeAnimationFrames;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtEnvironmentName;
        private System.Windows.Forms.Button butEnvironmentPath;
        private System.Windows.Forms.CheckBox chkFullPBR;
        private System.Windows.Forms.CheckBox chkNoAutoLight;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.OpenFileDialog envFileDialog;
        private System.Windows.Forms.CheckBox chkDefaultSkybox;
        private System.Windows.Forms.Button butCopyToClipboard;
        private System.Windows.Forms.CheckBox chkExportAnimations;
        private System.Windows.Forms.CheckBox chkExportAnimationsOnly;
        private System.Windows.Forms.CheckBox chkExportMaterials;
        private System.Windows.Forms.CheckBox chkExportTextures;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboTexturePolicy;
    }
}