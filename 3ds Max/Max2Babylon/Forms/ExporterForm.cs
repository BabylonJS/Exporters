using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using Color = System.Drawing.Color;

namespace Max2Babylon
{
    public partial class ExporterForm : Form
    {
        private const string ModelFilePathProperty = "modelFilePathProperty";
        private const string TextureFolderPathProperty = "textureFolderPathProperty";

        private readonly BabylonExportActionItem babylonExportAction;
        private BabylonExporter exporter;
        private bool gltfPipelineInstalled = true;  // true if the gltf-pipeline is installed and runnable.

        TreeNode currentNode;
        int currentRank;

        private ExportItem singleExportItem;

        public ExporterForm(BabylonExportActionItem babylonExportAction)
        {
            InitializeComponent();
            this.Text = $"Babylon.js - Export scene to babylon or glTF format v{BabylonExporter.exporterVersion}";

            this.babylonExportAction = babylonExportAction;
            
            // Check if the gltf-pipeline module is installed
            try
            {
                Process gltfPipeline = new Process();
                gltfPipeline.StartInfo.FileName = "gltf-pipeline.cmd";

                // Hide the cmd window that show the gltf-pipeline result
                gltfPipeline.StartInfo.UseShellExecute = false;
                gltfPipeline.StartInfo.CreateNoWindow = true;

                gltfPipeline.Start();
                gltfPipeline.WaitForExit();
            }
            catch
            {
                gltfPipelineInstalled = false;
            }

            groupBox1.MouseMove += groupBox1_MouseMove;
        }

        private void ExporterForm_Load(object sender, EventArgs e)
        {
            string storedModelPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.ModelFilePathProperty, string.Empty);
            string userRelativePath = Tools.ResolveRelativePath(storedModelPath);
            txtModelName.Text = userRelativePath;
            string absoluteModelPath = PathUtilities.UnformatPath(txtModelName.Text);
            singleExportItem = new ExportItem(absoluteModelPath);

            string storedFolderPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.TextureFolderPathProperty, string.Empty);
            string formatedFolderPath = Tools.ResolveRelativePath(storedFolderPath);
            txtTextureName.Text = formatedFolderPath;

            Tools.PrepareCheckBox(chkManifest, Loader.Core.RootNode, "babylonjs_generatemanifest");
            Tools.PrepareCheckBox(chkWriteTextures, Loader.Core.RootNode, "babylonjs_writetextures", 1);
            Tools.PrepareCheckBox(chkOverwriteTextures, Loader.Core.RootNode, "babylonjs_overwritetextures", 1);
            Tools.PrepareCheckBox(chkHidden, Loader.Core.RootNode, "babylonjs_exporthidden");
            Tools.PrepareCheckBox(chkAutoSave, Loader.Core.RootNode, "babylonjs_autosave", 1);
            Tools.PrepareCheckBox(chkOnlySelected, Loader.Core.RootNode, "babylonjs_onlySelected");
            Tools.PrepareCheckBox(chkExportTangents, Loader.Core.RootNode, "babylonjs_exporttangents");
            Tools.PrepareComboBox(comboOutputFormat, Loader.Core.RootNode, "babylonjs_outputFormat", "babylon");
            Tools.PrepareTextBox(txtScaleFactor, Loader.Core.RootNode, "babylonjs_txtScaleFactor", "1");
            Tools.PrepareTextBox(txtQuality, Loader.Core.RootNode, "babylonjs_txtCompression", "100");
            Tools.PrepareCheckBox(chkMergeAOwithMR, Loader.Core.RootNode, "babylonjs_mergeAOwithMR", 1);
            Tools.PrepareCheckBox(chkDracoCompression, Loader.Core.RootNode, "babylonjs_dracoCompression", 0);
            Tools.PrepareCheckBox(chkKHRLightsPunctual, Loader.Core.RootNode, "babylonjs_khrLightsPunctual");
            Tools.PrepareCheckBox(chkKHRTextureTransform, Loader.Core.RootNode, "babylonjs_khrTextureTransform");
            Tools.PrepareCheckBox(chkAnimgroupExportNonAnimated, Loader.Core.RootNode, "babylonjs_animgroupexportnonanimated");
            Tools.PrepareCheckBox(chkDoNotOptimizeAnimations, Loader.Core.RootNode, "babylonjs_donotoptimizeanimations");
            Tools.PrepareCheckBox(chkKHRMaterialsUnlit, Loader.Core.RootNode, "babylonjs_khr_materials_unlit");
            Tools.PrepareCheckBox(chkExportMaterials, Loader.Core.RootNode, "babylonjs_export_materials", 1);
            Tools.PrepareCheckBox(chkExportMorphTangents, Loader.Core.RootNode, "babylonjs_export_Morph_Tangents", 0);
            Tools.PrepareCheckBox(chkExportMorphNormals, Loader.Core.RootNode, "babylonjs_export_Morph_Normals", 1);

            if (comboOutputFormat.SelectedText == "babylon" || comboOutputFormat.SelectedText == "binary babylon" || !gltfPipelineInstalled)
            {
                chkDracoCompression.Checked = false;
                chkDracoCompression.Enabled = false;
            }

            Tools.PrepareCheckBox(chkFullPBR, Loader.Core.RootNode, ExportParameters.PBRFullPropertyName);
            Tools.PrepareCheckBox(chkNoAutoLight, Loader.Core.RootNode, ExportParameters.PBRNoLightPropertyName);
            string storedEnvironmentPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, string.Empty);
            string formatedEnvironmentPath = Tools.ResolveRelativePath(storedEnvironmentPath);
            txtEnvironmentName.Text = formatedEnvironmentPath;

            Tools.PrepareCheckBox(chkUsePreExportProces, Loader.Core.RootNode, "babylonjs_preproces", 0);
            Tools.PrepareCheckBox(chkFlatten, Loader.Core.RootNode, "babylonjs_flattenScene", 0);
            Tools.PrepareCheckBox(chkMrgInheritedContainers, Loader.Core.RootNode, "babylonjs_mergeinheritedcontainers",0);
        }

        private void butModelBrowse_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtModelName.Text = Tools.FormatPath(saveFileDialog.FileName);
            }
        }

        private void btnTextureBrowse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                MessageBox.Show("Select model file path first");
                return;
            }

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog1.SelectedPath;
                string absoluteModelPath = PathUtilities.UnformatPath(txtModelName.Text);
                if (!PathUtilities.IsBelowPath(selectedFolderPath, absoluteModelPath))
                {
                    MessageBox.Show("WARNING: folderPath should be below model file path");
                }

                txtTextureName.Text = Tools.FormatPath(folderBrowserDialog1.SelectedPath);
            }
        }

        private void btnEnvBrowse_Click(object sender, EventArgs e)
        {
            if (envFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtEnvironmentName.Text = Tools.FormatPath(envFileDialog.FileName);
            }
        }

        private async void butExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkUsePreExportProces.Checked)
                {
                    Loader.Core.FileHold();
                }

                await DoExport(singleExportItem);
            }
            catch{}
            finally
            {
                if (chkUsePreExportProces.Checked)
                {
                    Loader.Core.SetQuietMode(true);
                    Loader.Core.FileFetch();
                    Loader.Core.SetQuietMode(false);
                }
            }
        }

        private async Task<bool> DoExport(ExportItemList exportItemList)
        {
            treeView.Nodes.Clear();

            bool allSucceeded = true;
            foreach (ExportItem item in exportItemList)
            {
                if (!item.Selected) continue;

                allSucceeded = allSucceeded && await DoExport(item, true, false);

                if (exporter.IsCancelled)
                    break;
            }

            return allSucceeded;
        }

        private void saveOptionBtn_Click(object sender, EventArgs e)
        {
            SaveOptions();
        }

        private void SaveOptions()
        {
            Tools.UpdateCheckBox(chkManifest, Loader.Core.RootNode, "babylonjs_generatemanifest");
            Tools.UpdateCheckBox(chkWriteTextures, Loader.Core.RootNode, "babylonjs_writetextures");
            Tools.UpdateCheckBox(chkOverwriteTextures, Loader.Core.RootNode, "babylonjs_overwritetextures");
            Tools.UpdateCheckBox(chkHidden, Loader.Core.RootNode, "babylonjs_exporthidden");
            Tools.UpdateCheckBox(chkAutoSave, Loader.Core.RootNode, "babylonjs_autosave");
            Tools.UpdateCheckBox(chkOnlySelected, Loader.Core.RootNode, "babylonjs_onlySelected");
            Tools.UpdateCheckBox(chkExportTangents, Loader.Core.RootNode, "babylonjs_exporttangents");
            Tools.UpdateComboBox(comboOutputFormat, Loader.Core.RootNode, "babylonjs_outputFormat");
            Tools.UpdateTextBox(txtScaleFactor, Loader.Core.RootNode, "babylonjs_txtScaleFactor");
            Tools.UpdateTextBox(txtQuality, Loader.Core.RootNode, "babylonjs_txtCompression");
            Tools.UpdateCheckBox(chkMergeAOwithMR, Loader.Core.RootNode, "babylonjs_mergeAOwithMR");
            Tools.UpdateCheckBox(chkDracoCompression, Loader.Core.RootNode, "babylonjs_dracoCompression");
            Tools.UpdateCheckBox(chkKHRTextureTransform, Loader.Core.RootNode, "babylonjs_khrTextureTransform");
            Tools.UpdateCheckBox(chkKHRLightsPunctual, Loader.Core.RootNode, "babylonjs_khrLightsPunctual");
            Tools.UpdateCheckBox(chkKHRMaterialsUnlit, Loader.Core.RootNode, "babylonjs_khr_materials_unlit");
            Tools.UpdateCheckBox(chkExportMaterials, Loader.Core.RootNode, "babylonjs_export_materials");
            Tools.UpdateCheckBox(chkAnimgroupExportNonAnimated, Loader.Core.RootNode, "babylonjs_animgroupexportnonanimated");
            Tools.UpdateCheckBox(chkDoNotOptimizeAnimations, Loader.Core.RootNode, "babylonjs_donotoptimizeanimations");
            Tools.UpdateCheckBox(chkExportMorphTangents, Loader.Core.RootNode, "babylonjs_export_Morph_Tangents");
            Tools.UpdateCheckBox(chkExportMorphNormals, Loader.Core.RootNode, "babylonjs_export_Morph_Normals");

            string unformattedPath = PathUtilities.UnformatPath(txtModelName.Text);
            Loader.Core.RootNode.SetStringProperty(ExportParameters.ModelFilePathProperty, Tools.RelativePathStore(unformattedPath));

            string unformattedTextureFolderPath = PathUtilities.UnformatPath(txtTextureName.Text);
            Loader.Core.RootNode.SetStringProperty(ExportParameters.TextureFolderPathProperty, Tools.RelativePathStore(unformattedTextureFolderPath));

            Tools.UpdateCheckBox(chkFullPBR, Loader.Core.RootNode, ExportParameters.PBRFullPropertyName);
            Tools.UpdateCheckBox(chkNoAutoLight, Loader.Core.RootNode, ExportParameters.PBRNoLightPropertyName);
            string unformattedEnvironmentPath = PathUtilities.UnformatPath(txtEnvironmentName.Text);
            Loader.Core.RootNode.SetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, Tools.RelativePathStore(unformattedEnvironmentPath));

            Tools.UpdateCheckBox(chkUsePreExportProces, Loader.Core.RootNode, "babylonjs_preproces");
            Tools.UpdateCheckBox(chkFlatten, Loader.Core.RootNode, "babylonjs_flattenScene");
            Tools.UpdateCheckBox(chkMrgInheritedContainers, Loader.Core.RootNode, "babylonjs_mergeinheritedcontainers");
        }

        private async Task<bool> DoExport(ExportItem exportItem, bool multiExport = false, bool clearLogs = true)
        {
            SaveOptions();

            exporter = new BabylonExporter();
            var textureExportPath = "";
            if (!string.IsNullOrWhiteSpace(txtTextureName.Text))
            {
                textureExportPath = PathUtilities.GetRelativePath(PathUtilities.UnformatPath(txtTextureName.Text), PathUtilities.UnformatPath(txtModelName.Text));
            }

            if (clearLogs)
                treeView.Nodes.Clear();

            exporter.OnImportProgressChanged += progress =>
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

            butExport.Enabled = false;
            butExportAndRun.Enabled = false;
            butMultiExport.Enabled = false;
            butCancel.Enabled = true;

            bool success = true;
            try
            {
                string modelAbsolutePath = multiExport ? exportItem.ExportFilePathAbsolute : PathUtilities.UnformatPath(txtModelName.Text);
                MaxExportParameters exportParameters = new MaxExportParameters
                {
                    outputPath = modelAbsolutePath,
                    outputTexturePath = textureExportPath,
                    textureFolder = PathUtilities.UnformatPath(txtTextureName.Text),
                    outputFormat = comboOutputFormat.SelectedItem.ToString(),
                    scaleFactor = float.Parse(txtScaleFactor.Text),
                    writeTextures = chkWriteTextures.Checked,
                    overwriteTextures = chkOverwriteTextures.Checked,
                    exportHiddenObjects = chkHidden.Checked,
                    exportOnlySelected = chkOnlySelected.Checked,
                    generateManifest = chkManifest.Checked,
                    autoSaveSceneFile = chkAutoSave.Checked,
                    exportTangents = chkExportTangents.Checked,
                    exportMorphTangents = chkExportMorphTangents.Checked,
                    exportMorphNormals = chkExportMorphNormals.Checked,
                    txtQuality = long.Parse(txtQuality.Text),
                    mergeAOwithMR = chkMergeAOwithMR.Checked,
                    dracoCompression = chkDracoCompression.Checked,
                    enableKHRLightsPunctual = chkKHRLightsPunctual.Checked,
                    enableKHRTextureTransform = chkKHRTextureTransform.Checked,
                    enableKHRMaterialsUnlit = chkKHRMaterialsUnlit.Checked,
                    exportMaterials = chkExportMaterials.Checked,
                    optimizeAnimations = !chkDoNotOptimizeAnimations.Checked,
                    animgroupExportNonAnimated = chkAnimgroupExportNonAnimated.Checked,
                    exportNode = exportItem != null ? exportItem.Node : null,
                    pbrNoLight = chkNoAutoLight.Checked,
                    pbrFull = chkFullPBR.Checked,
                    pbrEnvironment = txtEnvironmentName.Text,
                    flattenScene = chkFlatten.Checked,
                    mergeInheritedContainers = chkMrgInheritedContainers.Checked
                };

                exporter.callerForm = this;

                if (!multiExport && chkUsePreExportProces.Checked)
                {
                    Loader.Core.FileHold();
                }

                exporter.Export(exportParameters);
            }
            catch (OperationCanceledException)
            {
                progressBar.Value = 0;
                success = false;
            }
            catch (Exception ex)
            {
                IUTF8Str operationStatus = GlobalInterface.Instance.UTF8Str.Create("BabylonExportAborted");
                Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, operationStatus);

                currentNode = CreateTreeNode(0, "Export cancelled: " + ex.Message, Color.Red);
                currentNode = CreateTreeNode(1, ex.ToString(), Color.Red);
                currentNode.EnsureVisible();

                progressBar.Value = 0;
                success = false;
            }
            finally
            {
                if (!multiExport && chkUsePreExportProces.Checked)
                {
                    Loader.Core.SetQuietMode(true);
                    Loader.Core.FileFetch();
                    Loader.Core.SetQuietMode(false);
                }
            }

            butCancel.Enabled = false;
            butExport.Enabled = true;
            butMultiExport.Enabled = true;
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
                if (rank < 0 || rank > currentRank + 1)
                {
                    rank = 0;
                    treeView.Nodes.Add(new TreeNode("Invalid rank passed to CreateTreeNode (through RaiseMessage, RaiseWarning or RaiseError)!") { ForeColor = Color.DarkOrange });
                }
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

        private void ExporterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (exporter != null)
            {
                exporter.IsCancelled = true;                
            }
            babylonExportAction.Close();
        }

        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            butExport.Enabled = !string.IsNullOrEmpty(txtModelName.Text.Trim());
            butExportAndRun.Enabled = butExport.Enabled && WebServer.IsSupported;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            exporter.IsCancelled = true;
        }

        private void ExporterForm_Activated(object sender, EventArgs e)
        {
            Loader.Global.DisableAccelerators();
        }

        private void ExporterForm_Deactivate(object sender, EventArgs e)
        {
            Loader.Global.EnableAccelerators();
        }

        private async void butExportAndRun_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkUsePreExportProces.Checked)
                {
                    Loader.Core.FileHold();
                }

            if (await DoExport(singleExportItem))
            {
                WebServer.SceneFilename = Path.GetFileName(PathUtilities.UnformatPath(txtModelName.Text));
                WebServer.SceneFolder = Path.GetDirectoryName(PathUtilities.UnformatPath(txtModelName.Text));

                Process.Start(WebServer.url + WebServer.SceneFilename);

                WindowState = FormWindowState.Minimized;
            }
        }
            catch{}
            finally
            {
                if (chkUsePreExportProces.Checked)
                {
                    Loader.Core.SetQuietMode(true);
                    Loader.Core.FileFetch();
                    Loader.Core.SetQuietMode(false);
                }
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
                    chkWriteTextures.Enabled = true;
                    chkOverwriteTextures.Enabled = true;
                    txtTextureName.Text = string.Empty;
                    txtTextureName.Enabled = false;
                    textureLabel.Enabled = false;
                    btnTxtBrowse.Enabled = false;
                    chkNoAutoLight.Enabled = true;
                    chkFullPBR.Enabled = true;
                    btnEnvBrowse.Enabled = true;
                    txtEnvironmentName.Enabled = true;
                    chkKHRMaterialsUnlit.Enabled = false;
                    chkKHRLightsPunctual.Enabled = false;
                    chkKHRTextureTransform.Enabled = false;
                    break;
                case "gltf":
                    this.saveFileDialog.DefaultExt = "gltf";
                    this.saveFileDialog.Filter = "glTF files|*.gltf";
                    chkDracoCompression.Enabled = gltfPipelineInstalled;
                    chkWriteTextures.Enabled = true;
                    chkOverwriteTextures.Enabled = true;
                    txtTextureName.Enabled = true;
                    textureLabel.Enabled = true;
                    btnTxtBrowse.Enabled = true;
                    chkNoAutoLight.Enabled = false;
                    chkNoAutoLight.Checked = false;
                    chkFullPBR.Enabled = false;
                    chkFullPBR.Checked = false;
                    btnEnvBrowse.Enabled = false;
                    txtEnvironmentName.Enabled = false;
                    txtEnvironmentName.Text = string.Empty;
                    chkKHRMaterialsUnlit.Enabled = true;
                    chkKHRLightsPunctual.Enabled = true;
                    chkKHRTextureTransform.Enabled = true;
                    break;
                case "glb":
                    this.saveFileDialog.DefaultExt = "glb";
                    this.saveFileDialog.Filter = "glb files|*.glb";
                    chkDracoCompression.Enabled = gltfPipelineInstalled;
                    chkWriteTextures.Checked = true;
                    chkWriteTextures.Enabled = false;
                    chkOverwriteTextures.Checked = true;
                    chkOverwriteTextures.Enabled = false;
                    txtTextureName.Text = string.Empty;
                    txtTextureName.Enabled = false;
                    textureLabel.Enabled = false;
                    btnTxtBrowse.Enabled = false;
                    chkNoAutoLight.Enabled = false;
                    chkNoAutoLight.Checked = false;
                    chkFullPBR.Enabled = false;
                    chkFullPBR.Checked = false;
                    btnEnvBrowse.Enabled = false;
                    txtEnvironmentName.Enabled = false;
                    txtEnvironmentName.Text = string.Empty;
                    chkKHRMaterialsUnlit.Enabled = true;
                    chkKHRLightsPunctual.Enabled = true;
                    chkKHRTextureTransform.Enabled = true;
                    break;
            }
            this.txtModelName.Text = Path.ChangeExtension(txtModelName.Text, this.saveFileDialog.DefaultExt);
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
                    string tip = "For glTF and glb export only.\nNode.js and gltf-pipeline modules are required.";
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

        /// <summary>
        /// Handle the tab navigation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExporterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                if (e.Modifiers == Keys.Shift)
                    ProcessTabKey(false);
                else
                    ProcessTabKey(true);
            }
        }

        private async void butMultiExport_Click(object sender, EventArgs e)
        {
            string outputFileExt;
            outputFileExt = comboOutputFormat.SelectedItem.ToString();
            if (outputFileExt.Contains("binary babylon"))
                outputFileExt = "babylon";

            ExportItemList exportItemList = new ExportItemList(outputFileExt);

            exportItemList.LoadFromData();

            int numLoadedItems = exportItemList.Count;

            if (ModifierKeys == Keys.Shift)
            {
                MultiExportForm form = new MultiExportForm(exportItemList);
                form.ShowDialog(this);
            }
            else if (numLoadedItems > 0)
            {
                if (chkWriteTextures.Checked || chkOverwriteTextures.Checked)
                {
                    MessageBox.Show("Cannot write textures with Multi-File Export");
                    return;
                }
                try
                {
                    if (chkUsePreExportProces.Checked)
                    {
                        Loader.Core.FileHold();
                    }

                await DoExport(exportItemList);
            }
                catch{}
                finally
                {
                    if (chkUsePreExportProces.Checked)
                    {
                        Loader.Core.SetQuietMode(true);
                        Loader.Core.FileFetch();
                        Loader.Core.SetQuietMode(false);
                    }
                }
            }
        }

        private void chkUsePreExportProces_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkUsePreExportProces.Checked)
            {
                chkMrgInheritedContainers.Enabled = false;
                chkFlatten.Enabled = false;
            }
            else
            {
                chkMrgInheritedContainers.Enabled = true;
                chkFlatten.Enabled = true;
            }
        }
    }
}
