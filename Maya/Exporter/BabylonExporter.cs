using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        // Export options
        private bool _onlySelected;
        private bool _exportHiddenObjects;

        public event Action<int> OnImportProgressChanged;
        public bool AutoSaveMayaFile { get; set; }
        public bool ExportHiddenObjects { get; set; }
        public bool IsCancelled { get; set; }

        public bool CopyTexturesToOutput { get; set; }
        public object ExportQuaternionsInsteadOfEulers { get; private set; }

        private bool isBabylonExported;

        // Custom properties
        private bool _exportQuaternionsInsteadOfEulers;

        public void Export(string outputDirectory, string outputFileName, string outputFormat, bool generateManifest, bool onlySelected, bool autoSaveMayaFile, bool exportHiddenObjects, bool copyTexturesToOutput)
        {


            Debug.Print("toto");
            Debug.Print("outputDirectory=" + outputDirectory);
            Debug.Print("outputFileName=" + outputFileName);
            Debug.Print("outputFormat=" + outputFormat);
            Debug.Print("generateManifest=" + generateManifest);
            Debug.Print("onlySelected=" + onlySelected);
            Debug.Print("autoSaveMayaFile=" + autoSaveMayaFile);
            Debug.Print("exportHiddenObjects=" + exportHiddenObjects);
            Debug.Print("copyTexturesToOutput=" + copyTexturesToOutput);


            RaiseMessage("toto");
            RaiseMessage("toto", 1);
            RaiseMessage("toto", 0, true);
            RaiseMessage("toto", Color.Khaki);
            RaiseMessage("outputDirectory=" + outputDirectory);
            RaiseMessage("outputFileName=" + outputFileName);
            RaiseMessage("outputFormat=" + outputFormat);
            RaiseMessage("generateManifest=" + generateManifest);
            RaiseMessage("onlySelected=" + onlySelected);
            RaiseMessage("autoSaveMayaFile=" + autoSaveMayaFile);
            RaiseMessage("exportHiddenObjects=" + exportHiddenObjects);
            RaiseMessage("copyTexturesToOutput=" + copyTexturesToOutput);
            


            RaiseMessage("Exportation started", Color.Blue);
            var progression = 0.0f;
            ReportProgressChanged(progression);

            // Store export options
            _onlySelected = onlySelected;
            _exportHiddenObjects = exportHiddenObjects;
            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";

            // Check directory exists
            if (!Directory.Exists(outputDirectory))
            {
                RaiseError("Exportation stopped: Output folder does not exist");
                ReportProgressChanged(100);
                return;
            }

            var watch = new Stopwatch();
            watch.Start();

            var outputBabylonDirectory = outputDirectory;
            var babylonScene = new BabylonScene(outputBabylonDirectory);

            // Save scene
            if (autoSaveMayaFile)
            {
                RaiseMessage("Saving Maya file");
                // TODO
                //MFileIO.save();
            }

            // Force output file extension to be babylon
            outputFileName = Path.ChangeExtension(outputFileName, "babylon");

            // Producer
            babylonScene.producer = new BabylonProducer
            {
                name = "Maya",
                version = "2018",
                exporter_version = "1.0",
                file = outputFileName
            };

            // Global
            babylonScene.autoClear = true;
            // TODO - Retreive colors from Maya
            //babylonScene.clearColor = Loader.Core.GetBackGround(0, Tools.Forever).ToArray();
            //babylonScene.ambientColor = Loader.Core.GetAmbient(0, Tools.Forever).ToArray();

            // TODO - Add custom properties
            _exportQuaternionsInsteadOfEulers = true;

            // --------------------
            // ------ Meshes ------
            // --------------------
            RaiseMessage("Exporting meshes");
            // Get all meshes
            var dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst, MFn.Type.kMesh);
            List<MObject> mObjects = new List<MObject>();
            while (!dagIterator.isDone)
            {
                var mObject = dagIterator.currentItem();
                mObjects.Add(mObject);
                dagIterator.next();
            }
            // Export all meshes
            var progressionStep = 100.0f / mObjects.Count;
            foreach (MObject mObject in mObjects)
            {
                ExportMesh(mObject, babylonScene);

                // Update progress bar
                progression += progressionStep;
                ReportProgressChanged(progression);
            }
            RaiseMessage(string.Format("Total meshes: {0}", babylonScene.MeshesList.Count), Color.Gray, 1);

            dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst, MFn.Type.kMesh);

            // Switch from right to left handed coordinate system
            MUuid mUuid = new MUuid();
            mUuid.generate();
            var rootNode = new BabylonMesh
            {
                name = "root",
                id = mUuid.asString(),
                rotation = new float[] { 0, (float) -Math.PI, 0 },
                scaling = new float[] { 1, 1, -1 }
            };
            foreach(var babylonMesh in babylonScene.MeshesList)
            {
                // Add root meshes as child to root node
                if (babylonMesh.parentId == null)
                {
                    babylonMesh.parentId = rootNode.id;
                }
            }
            babylonScene.MeshesList.Add(rootNode);

            // --------------------
            // ----- Materials ----
            // --------------------
            // TODO - Materials

            // Output
            babylonScene.Prepare(false, false);
            if (isBabylonExported)
            {
                Write(babylonScene, outputBabylonDirectory, outputFileName, outputFormat, generateManifest);
            }

            // TODO - Export glTF
            
            watch.Stop();
            RaiseMessage(string.Format("Exportation done in {0:0.00}s", watch.ElapsedMilliseconds / 1000.0), Color.Blue);
        }


        private bool IsNodeExportable(MFnDagNode mFnDagNode)
        {
            // TODO - Add custom property
            //if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            //{
            //    return false;
            //}

            // TODO - Fix fatal error: Attempting to save in C:/Users/Fabrice/AppData/Local/Temp/Fabrice.20171205.1613.ma
            //if (_onlySelected && !MGlobal.isSelected(mDagPath.node))
            //{
            //    return false;
            //}

            // TODO - Fix fatal error: Attempting to save in C:/Users/Fabrice/AppData/Local/Temp/Fabrice.20171205.1613.ma
            //MDagPath mDagPath = mFnDagNode.dagPath;
            //if (!_exportHiddenObjects && !mDagPath.isVisible)
            //{
            //    return false;
            //}

            return true;
        }

        public async Task ExportAsync(string outputDirectory, string outputFileName, string outputFormat, bool generateManifest, bool onlySelected, Form callerForm)
        {

            /*var gameConversionManger = Loader.Global.ConversionManager;
            gameConversionManger.CoordSystem = Autodesk.Max.IGameConversionManager.CoordSystem.D3d;

            var gameScene = Loader.Global.IGameInterface;
            gameScene.InitialiseIGame(false);
            this._onlySelected = onlySelected;
            gameScene.SetStaticFrame(0);

            MaxSceneFileName = gameScene.SceneFileName;

            IsCancelled = false;
            RaiseMessage("Exportation started", Color.Blue);
            ReportProgressChanged(0);

            // Check directory exists
            if (!Directory.Exists(outputDirectory))
            {
                RaiseError("Exportation stopped: Output folder does not exist");
                ReportProgressChanged(100);
                return;
            }

            var outputBabylonDirectory = outputDirectory;

            // Force output file extension to be babylon
            outputFileName = Path.ChangeExtension(outputFileName, "babylon");

            var babylonScene = new BabylonScene(outputBabylonDirectory);

            var rawScene = Loader.Core.RootNode;

            var watch = new Stopwatch();
            watch.Start();

            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";

            // Save scene
            if (AutoSave3dsMaxFile)
            {
                RaiseMessage("Saving 3ds max file");
                var forceSave = Loader.Core.FileSave;

                callerForm?.BringToFront();
            }

            // Producer
            babylonScene.producer = new BabylonProducer
            {
                name = "3dsmax",
    #if MAX2018
                version = "2018",
    #elif MAX2017
                version = "2017",
    #else
                version = Loader.Core.ProductVersion.ToString(),
    #endif
                exporter_version = "0.4.5",
                file = outputFileName
            };

            // Global
            babylonScene.autoClear = true;
            babylonScene.clearColor = Loader.Core.GetBackGround(0, Tools.Forever).ToArray();
            babylonScene.ambientColor = Loader.Core.GetAmbient(0, Tools.Forever).ToArray();

            babylonScene.gravity = rawScene.GetVector3Property("babylonjs_gravity");
            ExportQuaternionsInsteadOfEulers = rawScene.GetBoolProperty("babylonjs_exportquaternions", 1);

            if (Loader.Core.UseEnvironmentMap && Loader.Core.EnvironmentMap != null)
            {
                // Environment texture
                var environmentMap = Loader.Core.EnvironmentMap;
                // Copy image file to output if necessary
                var babylonTexture = ExportEnvironmnentTexture(environmentMap, babylonScene);
                babylonScene.environmentTexture = babylonTexture.name;

                // Skybox
                babylonScene.createDefaultSkybox = rawScene.GetBoolProperty("babylonjs_createDefaultSkybox");
                babylonScene.skyboxBlurLevel = rawScene.GetFloatProperty("babylonjs_skyboxBlurLevel");
            }

            // Sounds
            var soundName = rawScene.GetStringProperty("babylonjs_sound_filename", "");

            if (!string.IsNullOrEmpty(soundName))
            {
                var filename = Path.GetFileName(soundName);

                var globalSound = new BabylonSound
                {
                    autoplay = rawScene.GetBoolProperty("babylonjs_sound_autoplay", 1),
                    loop = rawScene.GetBoolProperty("babylonjs_sound_loop", 1),
                    name = filename
                };

                babylonScene.SoundsList.Add(globalSound);

                if (isBabylonExported)
                {
                    try
                    {
                        File.Copy(soundName, Path.Combine(babylonScene.OutputPath, filename), true);
                    }
                    catch
                    {
                    }
                }
            }

            // Root nodes
            RaiseMessage("Exporting nodes");
            HashSet<IIGameNode> maxRootNodes = getRootNodes(gameScene);
            var progressionStep = 80.0f / maxRootNodes.Count;
            var progression = 10.0f;
            ReportProgressChanged((int)progression);
            referencedMaterials.Clear();
            // Reseting is optionnal. It makes each morph target manager export starts from id = 0.
            BabylonMorphTargetManager.Reset();
            foreach (var maxRootNode in maxRootNodes)
            {
                exportNodeRec(maxRootNode, babylonScene, gameScene);
                progression += progressionStep;
                ReportProgressChanged((int)progression);
                CheckCancelled();
            };
            RaiseMessage(string.Format("Total meshes: {0}", babylonScene.MeshesList.Count), Color.Gray, 1);

            // Main camera
            BabylonCamera babylonMainCamera = null;
            ICameraObject maxMainCameraObject = null;
            if (babylonMainCamera == null && babylonScene.CamerasList.Count > 0)
            {
                // Set first camera as main one
                babylonMainCamera = babylonScene.CamerasList[0];
                babylonScene.activeCameraID = babylonMainCamera.id;
                RaiseMessage("Active camera set to " + babylonMainCamera.name, Color.Green, 1, true);

                // Retreive camera node with same GUID
                var maxCameraNodesAsTab = gameScene.GetIGameNodeByType(Autodesk.Max.IGameObject.ObjectTypes.Camera);
                var maxCameraNodes = TabToList(maxCameraNodesAsTab);
                var maxMainCameraNode = maxCameraNodes.Find(_camera => _camera.MaxNode.GetGuid().ToString() == babylonMainCamera.id);
                maxMainCameraObject = (maxMainCameraNode.MaxNode.ObjectRef as ICameraObject);
            }

            if (babylonMainCamera == null)
            {
                RaiseWarning("No camera defined", 1);
            }
            else
            {
                RaiseMessage(string.Format("Total cameras: {0}", babylonScene.CamerasList.Count), Color.Gray, 1);
            }

            // Default light
            if (babylonScene.LightsList.Count == 0)
            {
                RaiseWarning("No light defined", 1);
                RaiseWarning("A default hemispheric light was added for your convenience", 1);
                ExportDefaultLight(babylonScene);
            }
            else
            {
                RaiseMessage(string.Format("Total lights: {0}", babylonScene.LightsList.Count), Color.Gray, 1);
            }

            // Materials
            RaiseMessage("Exporting materials");
            var matsToExport = referencedMaterials.ToArray(); // Snapshot because multimaterials can export new materials
            foreach (var mat in matsToExport)
            {
                ExportMaterial(mat, babylonScene);
                CheckCancelled();
            }
            RaiseMessage(string.Format("Total: {0}", babylonScene.MaterialsList.Count + babylonScene.MultiMaterialsList.Count), Color.Gray, 1);

            // Fog
            for (var index = 0; index < Loader.Core.NumAtmospheric; index++)
            {
                var atmospheric = Loader.Core.GetAtmospheric(index);

                if (atmospheric.Active(0) && atmospheric.ClassName == "Fog")
                {
                    var fog = atmospheric as IStdFog;

                    RaiseMessage("Exporting fog");

                    if (fog != null)
                    {
                        babylonScene.fogColor = fog.GetColor(0).ToArray();
                        babylonScene.fogMode = 3;
                    }
                    if (babylonMainCamera != null)
                    {
                        babylonScene.fogStart = maxMainCameraObject.GetEnvRange(0, 0, Tools.Forever);
                        babylonScene.fogEnd = maxMainCameraObject.GetEnvRange(0, 1, Tools.Forever);
                    }
                }
            }

            // Skeletons
            if (skins.Count > 0)
            {
                RaiseMessage("Exporting skeletons");
                foreach (var skin in skins)
                {
                    ExportSkin(skin, babylonScene);
                }
            }

            // Output
            babylonScene.Prepare(false, false);
            if (isBabylonExported)
            {
                RaiseMessage("Saving to output file");

                var outputFile = Path.Combine(outputBabylonDirectory, outputFileName);

                var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings());
                var sb = new StringBuilder();
                var sw = new StringWriter(sb, CultureInfo.InvariantCulture);

                await Task.Run(() =>
                {
                    using (var jsonWriter = new JsonTextWriterOptimized(sw))
                    {
                        jsonWriter.Formatting = Formatting.None;
                        jsonSerializer.Serialize(jsonWriter, babylonScene);
                    }
                    File.WriteAllText(outputFile, sb.ToString());

                    if (generateManifest)
                    {
                        File.WriteAllText(outputFile + ".manifest",
                            "{\r\n\"version\" : 1,\r\n\"enableSceneOffline\" : true,\r\n\"enableTexturesOffline\" : true\r\n}");
                    }
                });

                // Binary
                if (outputFormat == "binary babylon")
                {
                    RaiseMessage("Generating binary files");
                    BabylonFileConverter.BinaryConverter.Convert(outputFile, outputBabylonDirectory + "\\Binary",
                        message => RaiseMessage(message, 1),
                        error => RaiseError(error, 1));
                }
            }

            ReportProgressChanged(100);

            // Export glTF
            if (outputFormat == "gltf" || outputFormat == "glb")
            {
                bool generateBinary = outputFormat == "glb";
                ExportGltf(babylonScene, outputDirectory, outputFileName, generateBinary);
            }

            watch.Stop();
            RaiseMessage(string.Format("Exportation done in {0:0.00}s", watch.ElapsedMilliseconds / 1000.0), Color.Blue);*/
        }

    }
}
