﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BabylonExport.Entities;
using Utilities;

namespace Maya2Babylon.Forms
{
    public partial class ExporterForm : Form
    {
        private readonly BabylonExportActionItem babylonExportAction;
        private BabylonExporter exporter;
        private bool gltfPipelineInstalled = true;  // true if the gltf-pipeline is installed and runnable.

        const string chkCopyTexturesProperty = "babylonjs_copyTextures";
        const string chkHiddenProperty = "babylonjs_exportHidden";
        const string chkOnlySelectedProperty = "babylonjs_exportOnlySelected";
        const string chkManifestProperty = "babylonjs_generateManifest";
        const string chkAutoSaveProperty = "babylonjs_autoSave";
        const string chkOptimizeVerticesProperty = "babylonjs_optimizeVertices";
        const string chkExportTangentsProperty = "babylonjs_exportTangents";
        const string chkDracoCompressionProperty = "babylonjs_dracoCompression";
        const string chkExportSkinProperty = "babylonjs_exportSkin";
        const string chkExportMorphNormalProperty = "babylonjs_exportMorphNormal";
        const string chkExportMorphTangentProperty = "babylonjs_exportMorphTangent";
        const string chkExportKHRTextureTransformProperty = "babylonjs_exportKHRTextureTransform";
        const string chkExportKHRLightsPunctualProperty = "babylonjs_exportKHRLightsPunctual";
        const string chkExportKHRMaterialsUnlitProperty = "babylonjs_exportKHRMaterialsUnlit";
        const string chkBakeAnimationFramesProperty = "babylonjs_bakeAnimationFrames";
        const string chkExportAnimationsProperty = "babylonjs_exportAnimations";
        const string chkExportAnimationsOnlyProperty = "babylonjs_exportAnimationsOnly";

        const string PBRFullPropertyName = "babylonjs_pbr_full";
        const string PBRNoLightPropertyName = "babylonjs_pbr_nolight";
        const string PBREnvironmentPathPropertyName = "babylonjs_pbr_environmentPathProperty";
        const string PBRDefaultSkyboxName = "babylonjs_pbr_defaultSkybox";

        TreeNode currentNode;
        int currentRank;

        public ExporterForm()
        {
            InitializeComponent();
            this.Text = $"Babylon.js - Export scene to babylon or glTF format v{BabylonExporter.exporterVersion}";

            // Check if the gltf-pipeline module is installed
            gltfPipelineInstalled = GLTFPipelineUtilities.IsGLTFPipelineInstalled();

            groupBox1.MouseMove += groupBox1_MouseMove;
        }

        private void ExporterForm_Load(object sender, EventArgs e)
        {
            comboOutputFormat.SelectedIndex = 0;

            chkCopyTextures.Checked = Loader.GetBoolProperty(chkCopyTexturesProperty, true);
            chkHidden.Checked = Loader.GetBoolProperty(chkHiddenProperty, false);
            chkOnlySelected.Checked = Loader.GetBoolProperty(chkOnlySelectedProperty, false);
            chkManifest.Checked = Loader.GetBoolProperty(chkManifestProperty, false);
            chkAutoSave.Checked = Loader.GetBoolProperty(chkAutoSaveProperty, false);
            chkOptimizeVertices.Checked = Loader.GetBoolProperty(chkOptimizeVerticesProperty, true);
            chkExportTangents.Checked = Loader.GetBoolProperty(chkExportTangentsProperty, true);
            chkDracoCompression.Checked = Loader.GetBoolProperty(chkDracoCompressionProperty, false);
            chkExportSkin.Checked = Loader.GetBoolProperty(chkExportSkinProperty, true);
            chkExportMorphNormal.Checked = Loader.GetBoolProperty(chkExportMorphNormalProperty, true);
            chkExportMorphTangent.Checked = Loader.GetBoolProperty(chkExportMorphTangentProperty, false);
            chkExportKHRLightsPunctual.Checked = Loader.GetBoolProperty(chkExportKHRTextureTransformProperty, false);
            chkExportKHRTextureTransform.Checked = Loader.GetBoolProperty(chkExportKHRLightsPunctualProperty, false);
            chkExportKHRMaterialsUnlit.Checked = Loader.GetBoolProperty(chkExportKHRMaterialsUnlitProperty, false);
            chkBakeAnimationFrames.Checked = Loader.GetBoolProperty(chkBakeAnimationFramesProperty, false);
            chkExportAnimations.Checked = Loader.GetBoolProperty(chkExportAnimationsProperty, true);
            chkExportAnimationsOnly.Checked = Loader.GetBoolProperty(chkExportAnimationsOnlyProperty, false);
            /* txtFilename.Text = Loader.Core.RootNode.GetLocalData();
            Tools.PrepareComboBox(comboOutputFormat, Loader.Core.RootNode, "babylonjs_outputFormat", "babylon");*/

            chkFullPBR.Checked = Loader.GetBoolProperty(PBRFullPropertyName, false);
            chkNoAutoLight.Checked = Loader.GetBoolProperty(PBRNoLightPropertyName, false);
            chkDefaultSkybox.Checked = Loader.GetBoolProperty(PBRDefaultSkyboxName, false);
        }

        private void butBrowse_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtFilename.Text = saveFileDialog.FileName;
            }
        }

        private async void butExport_Click(object sender, EventArgs e)
        {
            await DoExport();
        }

        private async Task<bool> DoExport()
        {
            Loader.SetBoolProperty(chkCopyTexturesProperty, chkCopyTextures.Checked);
            Loader.SetBoolProperty(chkHiddenProperty, chkHidden.Checked);
            Loader.SetBoolProperty(chkOnlySelectedProperty, chkOnlySelected.Checked);
            Loader.SetBoolProperty(chkManifestProperty, chkManifest.Checked);
            Loader.SetBoolProperty(chkAutoSaveProperty, chkAutoSave.Checked);
            Loader.SetBoolProperty(chkOptimizeVerticesProperty, chkOptimizeVertices.Checked);
            Loader.SetBoolProperty(chkExportTangentsProperty, chkExportTangents.Checked);
            Loader.SetBoolProperty(chkDracoCompressionProperty, chkDracoCompression.Checked);
            Loader.SetBoolProperty(chkExportSkinProperty, chkExportSkin.Checked);
            Loader.SetBoolProperty(chkExportMorphNormalProperty, chkExportMorphNormal.Checked);
            Loader.SetBoolProperty(chkExportMorphTangentProperty, chkExportMorphTangent.Checked);
            Loader.SetBoolProperty(chkExportKHRLightsPunctualProperty, chkExportKHRLightsPunctual.Checked);
            Loader.SetBoolProperty(chkExportKHRTextureTransformProperty, chkExportKHRTextureTransform.Checked);
            Loader.SetBoolProperty(chkExportKHRMaterialsUnlitProperty, chkExportKHRMaterialsUnlit.Checked);
            Loader.SetBoolProperty(chkBakeAnimationFramesProperty, chkBakeAnimationFrames.Checked);
            Loader.SetBoolProperty(chkExportAnimationsProperty, chkExportAnimations.Checked);
            Loader.SetBoolProperty(chkExportAnimationsOnlyProperty, chkExportAnimationsOnly.Checked);

            Loader.SetBoolProperty(PBRFullPropertyName, chkFullPBR.Checked);
            Loader.SetBoolProperty(PBRNoLightPropertyName, chkNoAutoLight.Checked);
            Loader.SetBoolProperty(PBRDefaultSkyboxName, chkDefaultSkybox.Checked);

            /*Tools.UpdateComboBox(comboOutputFormat, Loader.Core.RootNode, "babylonjs_outputFormat");

            Loader.Core.RootNode.SetLocalData(txtFilename.Text);*/

            exporter = new BabylonExporter();

            treeView.Nodes.Clear();

            exporter.OnExportProgressChanged += progress =>
            {
                progressBar.Value = progress;
                Application.DoEvents();
            };

            exporter.OnWarning += (warning, rank) =>
            {
                try
                {
                    currentNode = CreateTreeNode(rank, warning, Color.DarkOrange);
                    currentNode.EnsureVisible();
                }
                catch
                {
                }
                Application.DoEvents();
            };

            exporter.OnError += (error, rank) =>
            {
                try
                {
                    currentNode = CreateTreeNode(rank, error, Color.Red);
                    currentNode.EnsureVisible();
                }
                catch
                {
                }
                Application.DoEvents();
            };

            exporter.OnMessage += (message, color, rank, emphasis) =>
            {
                try
                {
                    currentNode = CreateTreeNode(rank, message, color);

                    if (emphasis)
                    {
                        currentNode.EnsureVisible();
                    }
                }
                catch
                {
                }
                Application.DoEvents();
            };

            exporter.OnVerbose += (message, color, rank, emphasis) =>
            {
                try
                {
                    currentNode = CreateTreeNode(rank, message, color);

                    if (emphasis)
                    {
                        currentNode.EnsureVisible();
                    }
                }
                catch
                {
                }
                Application.DoEvents();
            };

            butExport.Enabled = false;
            butExportAndRun.Enabled = false;
            butCancel.Enabled = true;
            
            bool success = true;
            try
            {
                var scaleFactorParsed = 1.0f;
                var textureQualityParsed = 100L;
                try
                {
                    scaleFactorParsed = float.Parse(txtScaleFactor.Text);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException(String.Format("Invalid Scale Factor value: {0}", txtScaleFactor.Text));
                }
                try
                {
                    textureQualityParsed = long.Parse(txtQuality.Text);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException(String.Format("Invalid Texture Quality value: {0}", txtScaleFactor.Text));
                }
    
                var exportParameters = new ExportParameters
                {
                    outputPath = txtFilename.Text,
                    outputFormat = comboOutputFormat.SelectedItem.ToString(),
                    generateManifest = chkManifest.Checked,
                    exportOnlySelected = chkOnlySelected.Checked,
                    autoSaveSceneFile = chkAutoSave.Checked,
                    exportHiddenObjects = chkHidden.Checked,
                    writeTextures = chkCopyTextures.Checked,
                    overwriteTextures = chkCopyTextures.Checked,
                    optimizeVertices = chkOptimizeVertices.Checked,
                    exportTangents = chkExportTangents.Checked,
                    scaleFactor = scaleFactorParsed,
                    exportSkins = chkExportSkin.Checked,
                    txtQuality = textureQualityParsed,
                    dracoCompression = chkDracoCompression.Checked,
                    exportMorphNormals = chkExportMorphNormal.Checked,
                    exportMorphTangents = chkExportMorphTangent.Checked,
                    enableKHRLightsPunctual = chkExportKHRLightsPunctual.Checked,
                    enableKHRTextureTransform = chkExportKHRTextureTransform.Checked,
                    enableKHRMaterialsUnlit = chkExportKHRMaterialsUnlit.Checked,
                    bakeAnimationFrames = chkBakeAnimationFrames.Checked,
                    exportAnimations = chkExportAnimations.Checked,
                    exportAnimationsOnly = chkExportAnimationsOnly.Checked,
                    pbrFull = chkFullPBR.Checked,
                    pbrNoLight = chkNoAutoLight.Checked,
                    createDefaultSkybox = chkDefaultSkybox.Checked,
                    pbrEnvironment = txtEnvironmentName.Text
                };
                exporter.Export(exportParameters);
            }
            catch (OperationCanceledException)
            {
                progressBar.Value = 0;
                success = false;
            }
            catch (Exception ex)
            {
                currentNode = CreateTreeNode(0, "Export cancelled: " + ex.Message, Color.Red);
                currentNode = CreateTreeNode(1, ex.ToString(), Color.Red);
                currentNode.EnsureVisible();

                progressBar.Value = 0;
                success = false;
            }

            butCancel.Enabled = false;
            butExport.Enabled = true;
            butExportAndRun.Enabled = WebServer.IsSupported;

            BringToFront();
            
            return success;
        }

        private TreeNode CreateTreeNode(int rank, string text, Color color)
        {
            TreeNode newNode = null;
            
            Invoke(new Action(() =>
            {
                newNode = new TreeNode(text) { ForeColor = color };
                if (rank == 0)
                {
                    treeView.Nodes.Add(newNode);
                }
                else if (rank == currentRank + 1)
                {
                    currentNode.Nodes.Add(newNode);
                }
                else
                {
                    var parentNode = currentNode;
                    while (currentRank != rank - 1)
                    {
                        parentNode = parentNode.Parent;
                        currentRank--;
                    }
                    parentNode.Nodes.Add(newNode);
                }

                currentRank = rank;
            }));

            return newNode;
        }

        internal Func<bool> closingByUser;
        internal Func<bool> closingByShutDown;
        //internal Func<bool> closingByCrash;
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // closing the form with (x)
            e.Cancel = true;

            // if windows is shutting down
            if (e.CloseReason == CloseReason.WindowsShutDown) this.closingByShutDown();

            // if user is closing
            if (e.CloseReason == CloseReason.UserClosing) this.closingByUser();
        }

        private void ExporterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (exporter != null)
            {
                exporter.IsCancelled = true;
            }
            // babylonExportAction.Close();
            Close();
        }

        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            butExport.Enabled = !string.IsNullOrEmpty(txtFilename.Text.Trim());
            butExportAndRun.Enabled = butExport.Enabled && WebServer.IsSupported;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            exporter.IsCancelled = true;
        }

        private void butCopyToClipboard_Click(object sender, EventArgs e)
        {
            var textString = treeView.ToPrettyString();
            if (textString != string.Empty)
            {
                System.Windows.Forms.Clipboard.SetText(textString);
            }
        }

        private void ExporterForm_Activated(object sender, EventArgs e)
        {
            /*Loader.Global.DisableAccelerators();*/
        }

        private void ExporterForm_Deactivate(object sender, EventArgs e)
        {
            /*Loader.Global.EnableAccelerators();*/
        }

       private async void butExportAndRun_Click(object sender, EventArgs e)
        {
            if (await DoExport())
            {
                WebServer.SceneFilename = Path.GetFileName(txtFilename.Text);
                WebServer.SceneFolder = Path.GetDirectoryName(txtFilename.Text);

                Process.Start(WebServer.url + WebServer.SceneFilename);

                WindowState = FormWindowState.Minimized;
            }
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboOutputFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            var outputFormat = comboOutputFormat.SelectedItem.ToString();
            switch (outputFormat)
            {
                case "babylon":
                case "binary babylon":
                    this.saveFileDialog.DefaultExt = "babylon";
                    this.saveFileDialog.Filter = "Babylon files|*.babylon";
                    chkDracoCompression.Checked = false;
                    chkDracoCompression.Enabled = false;
                    chkNoAutoLight.Enabled = true;
                    chkFullPBR.Enabled = true;
                    chkDefaultSkybox.Enabled = true;
                    butEnvironmentPath.Enabled = true;
                    txtEnvironmentName.Enabled = true;
                    break;
                case "gltf":
                    this.saveFileDialog.DefaultExt = "gltf";
                    this.saveFileDialog.Filter = "glTF files|*.gltf";
                    chkDracoCompression.Enabled = gltfPipelineInstalled;
                    chkNoAutoLight.Enabled = false;
                    chkNoAutoLight.Checked = false;
                    chkFullPBR.Enabled = false;
                    chkFullPBR.Checked = false;
                    chkDefaultSkybox.Enabled = false;
                    chkDefaultSkybox.Checked = false;
                    butEnvironmentPath.Enabled = false;
                    txtEnvironmentName.Enabled = false;
                    txtEnvironmentName.Text = string.Empty;
                    break;
                case "glb":
                    this.saveFileDialog.DefaultExt = "glb";
                    this.saveFileDialog.Filter = "glb files|*.glb";
                    chkDracoCompression.Enabled = gltfPipelineInstalled;
                    chkNoAutoLight.Enabled = false;
                    chkNoAutoLight.Checked = false;
                    chkFullPBR.Enabled = false;
                    chkFullPBR.Checked = false;
                    chkDefaultSkybox.Enabled = false;
                    chkDefaultSkybox.Checked = false;
                    butEnvironmentPath.Enabled = false;
                    txtEnvironmentName.Enabled = false;
                    txtEnvironmentName.Text = string.Empty;
                    break;
            }
            this.txtFilename.Text = Path.ChangeExtension(this.txtFilename.Text, this.saveFileDialog.DefaultExt);
        }

        /// <summary>
        /// Show a toolTip when the mouse is over the chkDracoCompression checkBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        bool IsShown = false;
        private void groupBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Control ctrl = groupBox1.GetChildAtPoint(e.Location);

            if (ctrl != null)
            {
                if (ctrl == chkDracoCompression && !ctrl.Enabled && !IsShown)
                {
                    string tip = "For gltf and glb export only.\nNode.js and gltf-pipeline module are required.";
                    toolTipDracoCompression.Show(tip, chkDracoCompression, chkDracoCompression.Width / 2, chkDracoCompression.Height / 2);
                    IsShown = true;
                }
            }
            else
            {
                toolTipDracoCompression.Hide(chkDracoCompression);
                IsShown = false;
            }
        }

        private void chkExportTangents_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkExportTangents.Checked)
            {
                chkExportMorphTangent.Enabled = false;
                chkExportMorphTangent.Checked = false;
            }
            else
            {
                chkExportMorphTangent.Enabled = true;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void butEnvironmentPath_Click(object sender, EventArgs e)
        {
            if (envFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtEnvironmentName.Text = envFileDialog.FileName;
            }
        }
    }
}
