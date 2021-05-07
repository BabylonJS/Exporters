using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using Color = System.Drawing.Color;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace Max2Babylon
{
    public partial class ExporterForm : Form
    {
        private readonly BabylonExportActionItem babylonExportAction;
        private BabylonExporter exporter;
        private bool gltfPipelineInstalled = true;  // true if the gltf-pipeline is installed and runnable.

        TreeNode currentNode;
        int currentRank;

        private ExportItem singleExportItem;


        private bool filePostOpenCallback = false;
        private GlobalDelegates.Delegate5 m_FilePostOpenDelegate;

        private void RegisterFilePostOpen()
        {
            if (!filePostOpenCallback)
            {
                m_FilePostOpenDelegate = new GlobalDelegates.Delegate5(OnFilePostOpen);
                GlobalInterface.Instance.RegisterNotification(this.m_FilePostOpenDelegate, null, SystemNotificationCode.FilePostOpen);

                filePostOpenCallback = true;
            }
        }

        private void OnFilePostOpen(IntPtr param0, IntPtr param1)
        {
            LoadOptions();
        }

        private void OnFilePostOpen(IntPtr param0, INotifyInfo param1)
        {
            LoadOptions();
        }

        public ExporterForm(BabylonExportActionItem babylonExportAction)
        {
            InitializeComponent();
            RegisterFilePostOpen();


            this.Text = $"Babylon.js - Export scene to babylon or glTF format v{BabylonExporter.exporterVersion}";

            this.babylonExportAction = babylonExportAction;

            // Check if the gltf-pipeline module is installed
            this.gltfPipelineInstalled = GLTFPipelineUtilities.IsGLTFPipelineInstalled();

            exportOptionsScrollPanel.MouseMove += exportOptionsScrollPanel_MouseMove;
        }

        private void LoadOptions()
        {
            string storedModelPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.ModelFilePathProperty, string.Empty);
            string absoluteModelPath = Tools.ResolveRelativePath(storedModelPath);
            txtModelPath.MaxPath(absoluteModelPath);

            string storedFolderPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.TextureFolderPathProperty, string.Empty);
            string absoluteTexturesFolderPath = Tools.ResolveRelativePath(storedFolderPath);
            txtTexturesPath.MaxPath(absoluteTexturesFolderPath);

            singleExportItem = new ExportItem(absoluteModelPath);

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
            Tools.PrepareCheckBox(chkMergeAO, Loader.Core.RootNode, "babylonjs_mergeAOwithMR", 1);
            Tools.PrepareCheckBox(chkDracoCompression, Loader.Core.RootNode, "babylonjs_dracoCompression", 0);
            Tools.PrepareCheckBox(chkKHRLightsPunctual, Loader.Core.RootNode, "babylonjs_khrLightsPunctual");
            Tools.PrepareCheckBox(chkKHRTextureTransform, Loader.Core.RootNode, "babylonjs_khrTextureTransform");
            Tools.PrepareCheckBox(chkAnimgroupExportNonAnimated, Loader.Core.RootNode, "babylonjs_animgroupexportnonanimated");
            Tools.PrepareCheckBox(chkDoNotOptimizeAnimations, Loader.Core.RootNode, "babylonjs_donotoptimizeanimations");
            Tools.PrepareCheckBox(chkKHRMaterialsUnlit, Loader.Core.RootNode, "babylonjs_khr_materials_unlit");
            Tools.PrepareCheckBox(chkExportMaterials, Loader.Core.RootNode, "babylonjs_export_materials", 1);
            Tools.PrepareCheckBox(chkExportAnimations, Loader.Core.RootNode, "babylonjs_export_animations", 1);
            Tools.PrepareCheckBox(chkExportAnimationsOnly, Loader.Core.RootNode, "babylonjs_export_animations_only");
            Tools.PrepareCheckBox(chkExportMorphTangents, Loader.Core.RootNode, "babylonjs_export_Morph_Tangents", 0);
            Tools.PrepareCheckBox(chkExportMorphNormals, Loader.Core.RootNode, "babylonjs_export_Morph_Normals", 1);
            Tools.PrepareCheckBox(chkExportTextures, Loader.Core.RootNode, "babylonjs_export_Textures", 1);
            Tools.PrepareComboBox(cmbBakeAnimationOptions, Loader.Core.RootNode, "babylonjs_bakeAnimationsType", (int)BakeAnimationType.DoNotBakeAnimation);
            Tools.PrepareCheckBox(chkApplyPreprocessToScene, Loader.Core.RootNode, "babylonjs_applyPreprocess", 0);

            if (comboOutputFormat.SelectedText == "babylon" || comboOutputFormat.SelectedText == "binary babylon" || !gltfPipelineInstalled)
            {
                chkDracoCompression.Checked = false;
                chkDracoCompression.Enabled = false;
            }


            Tools.PrepareCheckBox(chkFullPBR, Loader.Core.RootNode, ExportParameters.PBRFullPropertyName);
            Tools.PrepareCheckBox(chkNoAutoLight, Loader.Core.RootNode, ExportParameters.PBRNoLightPropertyName);
            string storedEnvironmentPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, string.Empty);
            string absoluteEnvironmentPath = Tools.ResolveRelativePath(storedEnvironmentPath);
            txtEnvironmentName.MaxPath(absoluteEnvironmentPath);

            Tools.PrepareCheckBox(chkUsePreExportProces, Loader.Core.RootNode, "babylonjs_preproces", 0);
            Tools.PrepareCheckBox(chkFlatten, Loader.Core.RootNode, "babylonjs_flattenScene", 0);
            Tools.PrepareCheckBox(chkMrgContainersAndXref, Loader.Core.RootNode, "babylonjs_mergecontainersandxref", 0);
            Tools.PrepareCheckBox(chkTryReuseTexture, Loader.Core.RootNode, "babylonjs_tryReuseTexture", 0);

            Tools.PrepareCheckBox(chkUseClone, Loader.Core.RootNode, "babylonjs_useClone", 0);


            #region prepare draco            
            LoadDracoOptions();
            dracoUserControl.UpdateValueLabels(); // force value label to be updated
            dracoGroupBox.Enabled = chkDracoCompression.Enabled && chkDracoCompression.Checked;
            #endregion
        }

        private void LoadDracoOptions()
        {
            Tools.PrepareNumericUpDown(dracoUserControl.CompressionLevelNumericUpDown, Loader.Core.RootNode, $"babylonjs_{DracoParameters.compressionLevel_param_name}", DracoParameters.compressionLevel_default);
            Tools.PrepareTrackBar(dracoUserControl.QPositionTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizePositionBits_param_name}", DracoParameters.quantizePositionBits_default);
            Tools.PrepareTrackBar(dracoUserControl.QNormalTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeNormalBits_param_name}", DracoParameters.quantizeNormalBits_default);
            Tools.PrepareTrackBar(dracoUserControl.QTexcoordTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeTexcoordBits_param_name}", DracoParameters.quantizeTexcoordBits_default);
            Tools.PrepareTrackBar(dracoUserControl.QColorTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeColorBits_param_name}", DracoParameters.quantizeColorBits_default);
            Tools.PrepareTrackBar(dracoUserControl.QGenericTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeGenericBits_param_name}", DracoParameters.quantizeGenericBits_default);
            Tools.PrepareCheckBox(dracoUserControl.UnifiedCheckBox, Loader.Core.RootNode, $"babylonjs_{DracoParameters.unifiedQuantization_param_name}", DracoParameters.unifiedQuantization_default?1:0);
        }

        private void ExporterForm_Load(object sender, EventArgs e)
        {
            LoadOptions();

            var maxVersion = Tools.GetMaxVersion();
            if (maxVersion.Major == 22 && maxVersion.Minor < 2)
            {
                CreateErrorMessage("You must update 3dsMax 2020 to version 2020.2 to use Max2Babylon. Unpatched versions of 3dsMax will crash during export.", 0);
            }
            else
            {
                CreateMessage(String.Format("Using Max2Babylon for 3dsMax version v{0}.{1}.{2}.{3}", maxVersion.Major, maxVersion.Minor, maxVersion.Revision, maxVersion.BuildNumber), Color.Black, 0, true);
            }
        }

        private void butModelBrowse_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtModelPath.Text))
            {
                string intialDirectory = Path.GetDirectoryName(txtModelPath.Text);

                if (!Directory.Exists(intialDirectory))
                {
                    intialDirectory = Loader.Core.GetDir((int)MaxDirectory.ProjectFolder);
                }

                if (!Directory.Exists(intialDirectory))
                {
                    intialDirectory = null;
                }

                saveFileDialog.InitialDirectory = intialDirectory;
            }

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtModelPath.MaxPath(saveFileDialog.FileName);
                if (string.IsNullOrWhiteSpace(txtTexturesPath.Text))
                {
                    string defaultTexturesDir = Path.GetDirectoryName(txtModelPath.Text);
                    txtTexturesPath.MaxPath(defaultTexturesDir);
                }

                if (!PathUtilities.IsBelowPath(txtTexturesPath.Text, txtModelPath.Text))
                {
                    CreateWarningMessage("WARNING: textures path should be below model file path, not all client renderers support this feature", 0);
                }
            }
        }

        private void btnTextureBrowse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModelPath.Text))
            {
                MessageBox.Show("Select model file path first");
                return;
            }

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            string intialDirectory = txtTexturesPath.Text;

            if (!Directory.Exists(intialDirectory))
            {
                intialDirectory = Path.GetDirectoryName(txtModelPath.Text);
            }

            if (!Directory.Exists(intialDirectory))
            {
                intialDirectory = Loader.Core.GetDir((int)MaxDirectory.ProjectFolder);
            }

            if (!Directory.Exists(intialDirectory))
            {
                intialDirectory = null;
            }

            dialog.InitialDirectory = intialDirectory;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedFolderPath = dialog.FileName;
                string absoluteModelPath = txtModelPath.Text;

                if (!PathUtilities.IsBelowPath(selectedFolderPath, absoluteModelPath))
                {
                    CreateWarningMessage("WARNING: textures path should be below model file path, not all client renderers support this feature", 0);
                }

                txtTexturesPath.MaxPath(selectedFolderPath);
            }
        }

        private void btnEnvBrowse_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEnvironmentName.Text))
            {
                string intialDirectory = Path.GetDirectoryName(txtEnvironmentName.Text);

                if (!Directory.Exists(intialDirectory))
                {
                    intialDirectory = Loader.Core.GetDir((int)MaxDirectory.ProjectFolder);
                }

                if (!Directory.Exists(intialDirectory))
                {
                    intialDirectory = null;
                }

                envFileDialog.InitialDirectory = intialDirectory;
            }

            if (envFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtEnvironmentName.MaxPath(envFileDialog.FileName);
            }
        }

        public async void butExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkUsePreExportProces.Checked)
                {
                    Loader.Core.FileHold();
                }

                await DoExport(singleExportItem);
            }
            catch { }
            finally
            {
                if (chkUsePreExportProces.Checked && !chkApplyPreprocessToScene.Checked)
                {
                    Loader.Core.SetQuietMode(true);
                    Loader.Core.FileFetch();
                    Loader.Core.SetQuietMode(false);
                }
            }
        }

        private async Task<bool> DoExport(ExportItemList exportItemList)
        {
            logTreeView.Nodes.Clear();

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
            Tools.UpdateCheckBox(chkMergeAO, Loader.Core.RootNode, "babylonjs_mergeAOwithMR");
            Tools.UpdateCheckBox(chkDracoCompression, Loader.Core.RootNode, "babylonjs_dracoCompression");
            Tools.UpdateCheckBox(chkKHRTextureTransform, Loader.Core.RootNode, "babylonjs_khrTextureTransform");
            Tools.UpdateCheckBox(chkKHRLightsPunctual, Loader.Core.RootNode, "babylonjs_khrLightsPunctual");
            Tools.UpdateCheckBox(chkKHRMaterialsUnlit, Loader.Core.RootNode, "babylonjs_khr_materials_unlit");
            Tools.UpdateCheckBox(chkExportMaterials, Loader.Core.RootNode, "babylonjs_export_materials");
            Tools.UpdateCheckBox(chkExportAnimations, Loader.Core.RootNode, "babylonjs_export_animations");
            Tools.UpdateCheckBox(chkExportAnimationsOnly, Loader.Core.RootNode, "babylonjs_export_animations_only");
            Tools.UpdateCheckBox(chkAnimgroupExportNonAnimated, Loader.Core.RootNode, "babylonjs_animgroupexportnonanimated");
            Tools.UpdateCheckBox(chkDoNotOptimizeAnimations, Loader.Core.RootNode, "babylonjs_donotoptimizeanimations");
            Tools.UpdateCheckBox(chkExportMorphTangents, Loader.Core.RootNode, "babylonjs_export_Morph_Tangents");
            Tools.UpdateCheckBox(chkExportMorphNormals, Loader.Core.RootNode, "babylonjs_export_Morph_Normals");
            Tools.UpdateCheckBox(chkExportTextures, Loader.Core.RootNode, "babylonjs_export_Textures");
            Tools.UpdateComboBoxByIndex(cmbBakeAnimationOptions, Loader.Core.RootNode, "babylonjs_bakeAnimationsType");
            Tools.UpdateCheckBox(chkApplyPreprocessToScene, Loader.Core.RootNode, "babylonjs_applyPreprocess");

            Loader.Core.RootNode.SetStringProperty(ExportParameters.ModelFilePathProperty, Tools.RelativePathStore(txtModelPath.Text));
            Loader.Core.RootNode.SetStringProperty(ExportParameters.TextureFolderPathProperty, Tools.RelativePathStore(txtTexturesPath.Text));

            Tools.UpdateCheckBox(chkFullPBR, Loader.Core.RootNode, ExportParameters.PBRFullPropertyName);
            Tools.UpdateCheckBox(chkNoAutoLight, Loader.Core.RootNode, ExportParameters.PBRNoLightPropertyName);
            Loader.Core.RootNode.SetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, Tools.RelativePathStore(txtEnvironmentName.Text));

            Tools.UpdateCheckBox(chkUsePreExportProces, Loader.Core.RootNode, "babylonjs_preproces");
            Tools.UpdateCheckBox(chkFlatten, Loader.Core.RootNode, "babylonjs_flattenScene");
            Tools.UpdateCheckBox(chkMrgContainersAndXref, Loader.Core.RootNode, "babylonjs_mergecontainersandxref");
            Tools.UpdateCheckBox(chkTryReuseTexture, Loader.Core.RootNode, "babylonjs_tryReuseTexture");

            SaveDracoOptions();
        }

        private void SaveDracoOptions()
        {
            Tools.UpdateNumericUpDown(dracoUserControl.CompressionLevelNumericUpDown, Loader.Core.RootNode, $"babylonjs_{DracoParameters.compressionLevel_param_name}");
            Tools.UpdateTrackBar(dracoUserControl.QPositionTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizePositionBits_param_name}");
            Tools.UpdateTrackBar(dracoUserControl.QNormalTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeNormalBits_param_name}");
            Tools.UpdateTrackBar(dracoUserControl.QTexcoordTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeTexcoordBits_param_name}");
            Tools.UpdateTrackBar(dracoUserControl.QColorTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeColorBits_param_name}");
            Tools.UpdateTrackBar(dracoUserControl.QGenericTrackBar, Loader.Core.RootNode, $"babylonjs_{DracoParameters.quantizeGenericBits_param_name}");
            Tools.UpdateCheckBox(dracoUserControl.UnifiedCheckBox, Loader.Core.RootNode, $"babylonjs_{DracoParameters.unifiedQuantization_param_name}");
        }


        private async Task<bool> DoExport(ExportItem exportItem, bool multiExport = false, bool clearLogs = true)
        {
            new BabylonAnimationActionItem().Close();
            SaveOptions();

            //store layer visibility status and force visibility on

            Dictionary<IILayer, bool> layerState = new Dictionary<IILayer, bool>();
            if (exportItem.Layers != null)
            {
                foreach (IILayer layer in exportItem.Layers)
                {
                    List<IILayer> treeLayers = layer.LayerTree().ToList();
                    treeLayers.Add(layer);

                    foreach (IILayer l in treeLayers)
                    {
#if MAX2015
                        layerState.Add( l,  l.IsHidden);
#else
                        layerState.Add(l, l.IsHidden(false));
#endif
                        l.Hide(false, false);
                    }

                }
            }

            exporter = new BabylonExporter();

            if (clearLogs)
                logTreeView.Nodes.Clear();

            exporter.OnExportProgressChanged += progress =>
            {
                progressBar.Value = progress;
                Application.DoEvents();
            };

            exporter.OnWarning += (warning, rank) => CreateWarningMessage(warning, rank);

            exporter.OnError += (error, rank) => CreateErrorMessage(error, rank);

            exporter.OnMessage += (message, color, rank, emphasis) => CreateMessage(message, color, rank, emphasis);

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
                    //do nothing
                }
                Application.DoEvents();
            };

            // disable UI controls during export.
            butExport.Enabled = false;
            butExportAndRun.Enabled = false;
            butMultiExport.Enabled = false;
            butCancel.Enabled = true;

            // switch to the log tab.
            exporterTabControl.SelectTab(logTabPage.Name);

            bool success = true;
            try
            {
                string modelAbsolutePath = multiExport ? exportItem.ExportFilePathAbsolute : txtModelPath.Text;
                string textureExportPath = multiExport ? exportItem.ExportTexturesesFolderAbsolute : txtTexturesPath.Text;

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

                MaxExportParameters exportParameters = new MaxExportParameters
                {
                    outputPath = modelAbsolutePath,
                    textureFolder = textureExportPath,
                    outputFormat = comboOutputFormat.SelectedItem.ToString(),
                    scaleFactor = scaleFactorParsed,
                    exportTextures = chkExportTextures.Checked,
                    writeTextures = chkWriteTextures.Checked,
                    overwriteTextures = chkOverwriteTextures.Checked,
                    exportHiddenObjects = chkHidden.Checked,
                    exportOnlySelected = chkOnlySelected.Checked,
                    generateManifest = chkManifest.Checked,
                    autoSaveSceneFile = chkAutoSave.Checked,
                    exportTangents = chkExportTangents.Checked,
                    exportMorphTangents = chkExportMorphTangents.Checked,
                    exportMorphNormals = chkExportMorphNormals.Checked,
                    txtQuality = textureQualityParsed,
                    mergeAO = chkMergeAO.Checked,
                    bakeAnimationType = (BakeAnimationType)cmbBakeAnimationOptions.SelectedIndex,
                    dracoCompression = chkDracoCompression.Enabled && chkDracoCompression.Checked,
                    enableKHRLightsPunctual = chkKHRLightsPunctual.Checked,
                    enableKHRTextureTransform = chkKHRTextureTransform.Checked,
                    enableKHRMaterialsUnlit = chkKHRMaterialsUnlit.Checked,
                    exportMaterials = chkExportMaterials.Checked,
                    exportAnimations = chkExportAnimations.Checked,
                    exportAnimationsOnly = chkExportAnimationsOnly.Checked,
                    optimizeAnimations = !chkDoNotOptimizeAnimations.Checked,
                    animgroupExportNonAnimated = chkAnimgroupExportNonAnimated.Checked,
                    exportNode = exportItem?.Node,
                    exportLayers = exportItem?.Layers,
                    pbrNoLight = chkNoAutoLight.Checked,
                    pbrFull = chkFullPBR.Checked,
                    pbrEnvironment = txtEnvironmentName.Text,
                    usePreExportProcess = chkUsePreExportProces.Checked,
                    flattenScene = chkFlatten.Checked,
                    mergeContainersAndXRef = chkMrgContainersAndXref.Checked,
                    useMultiExporter = multiExport,
                    tryToReuseOpaqueAndBlendTexture = chkTryReuseTexture.Checked,
                    useClone = chkUseClone.Checked
                };

                if (exportParameters.dracoCompression)
                {
                    exportParameters.dracoParams = new DracoParameters()
                    {
                        compressionLevel = (int)dracoUserControl.CompressionLevelNumericUpDown.Value,
                        quantizePositionBits = dracoUserControl.QPositionTrackBar.Value,
                        quantizeNormalBits = dracoUserControl.QNormalTrackBar.Value,
                        quantizeTexcoordBits = dracoUserControl.QTexcoordTrackBar.Value,
                        quantizeColorBits = dracoUserControl.QColorTrackBar.Value,
                        quantizeGenericBits = dracoUserControl.QGenericTrackBar.Value,
                        unifiedQuantization = dracoUserControl.UnifiedCheckBox.Checked
                    }; 
                }

                exporter.callerForm = this;

                exporter.Export(exportParameters);
            }
            catch (OperationCanceledException)
            {
                progressBar.Value = 0;
                success = false;
                ScriptsUtilities.ExecuteMaxScriptCommand(@"global BabylonExporterStatus = ""Available""");
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
                ScriptsUtilities.ExecuteMaxScriptCommand(@"global BabylonExporterStatus = Available");
            }

            butCancel.Enabled = false;
            butExport.Enabled = true;
            butMultiExport.Enabled = true;
            butExportAndRun.Enabled = WebServer.IsSupported;

            BringToFront();

            //re-store layer visibility status
            if (exportItem.Layers != null)
            {
                foreach (IILayer layer in exportItem.Layers)
                {
                    List<IILayer> treeLayers = layer.LayerTree().ToList();
                    treeLayers.Add(layer);
                    foreach (IILayer l in treeLayers)
                    {
                        bool exist;
                        layerState.TryGetValue(l, out exist);
                        if (exist)
                        {
                            l.Hide(layerState[l], false);
                        }
                    }
                }
            }

            return success;
        }

        void CreateWarningMessage(string warning, int rank)
        {
            try
            {
                currentNode = CreateTreeNode(rank, warning, Color.DarkOrange);
                currentNode.EnsureVisible();
            }
            catch
            {
                //do nothing
            }
            Application.DoEvents();
        }

        void CreateErrorMessage(string error, int rank)
        {
            try
            {
                currentNode = CreateTreeNode(rank, error, Color.Red);
                currentNode.EnsureVisible();
            }
            catch
            {
                //do nothing
            }
            Application.DoEvents();
        }

        void CreateMessage(string message, Color color, int rank, bool emphasis)
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
                //do nothing
            }
            Application.DoEvents();
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
                    logTreeView.Nodes.Add(new TreeNode("Invalid rank passed to CreateTreeNode (through RaiseMessage, RaiseWarning or RaiseError)!") { ForeColor = Color.DarkOrange });
                }
                if (rank == 0)
                {
                    logTreeView.Nodes.Add(newNode);
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
            butExport.Enabled = !string.IsNullOrEmpty(txtModelPath.Text.Trim());
            butExportAndRun.Enabled = butExport.Enabled && WebServer.IsSupported;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            exporter.IsCancelled = true;
        }

        private void butCopyToClipboard_Click(object sender, EventArgs e)
        {
            var textString = logTreeView.ToPrettyString();
            if (textString != string.Empty)
            {
                System.Windows.Forms.Clipboard.SetText(textString);
            }
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
                    WebServer.SceneFilename = Path.GetFileName(txtModelPath.Text);
                    WebServer.SceneFolder = Path.GetDirectoryName(txtModelPath.Text);

                    Process.Start(WebServer.url + WebServer.SceneFilename);

                    WindowState = FormWindowState.Minimized;
                }
            }
            catch { }
            finally
            {
                if (chkUsePreExportProces.Checked && !chkApplyPreprocessToScene.Checked)
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
                    txtTexturesPath.Text = string.Empty;
                    txtTexturesPath.Enabled = false;
                    textureLabel.Enabled = false;
                    btnTxtBrowse.Enabled = false;
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
                    txtTexturesPath.Enabled = true;
                    textureLabel.Enabled = true;
                    btnTxtBrowse.Enabled = true;
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
                    txtTexturesPath.Text = string.Empty;
                    txtTexturesPath.Enabled = false;
                    textureLabel.Enabled = false;
                    btnTxtBrowse.Enabled = false;
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

            dracoGroupBox.Enabled = chkDracoCompression.Enabled && chkDracoCompression.Checked;

            string newModelPath = Path.ChangeExtension(txtModelPath.Text, this.saveFileDialog.DefaultExt);
            this.txtModelPath.MaxPath(newModelPath);
        }

        /// <summary>
        /// Show a toolTip when the mouse is over the chkDracoCompression checkBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        bool IsShown = false;
        private void exportOptionsScrollPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Control ctrl = exportOptionsScrollPanel.GetChildAtPoint(e.Location);

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
                try
                {
                    if (chkUsePreExportProces.Checked)
                    {
                        Loader.Core.FileHold();
                    }
                    await DoExport(exportItemList);
                }
                catch { }
                finally
                {
                    if (chkUsePreExportProces.Checked && !chkApplyPreprocessToScene.Checked)
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
                chkMrgContainersAndXref.Enabled = false;
                cmbBakeAnimationOptions.Enabled = false;
                lblBakeAnimation.Enabled = false;
                chkApplyPreprocessToScene.Enabled = false;
            }
            else
            {
                chkMrgContainersAndXref.Enabled = true;
                cmbBakeAnimationOptions.Enabled = true;
                lblBakeAnimation.Enabled = true;
                chkApplyPreprocessToScene.Enabled = true;
            }
        }

        private void chkDracoCompression_CheckedChanged(object sender, EventArgs e)
        {
            dracoGroupBox.Enabled = chkDracoCompression.Checked;
        }
    }
}
