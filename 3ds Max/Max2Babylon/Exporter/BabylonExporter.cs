using Autodesk.Max;
using BabylonExport.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Max2Babylon
{
    internal partial class BabylonExporter
    {
        public event Action<int> OnImportProgressChanged;
        public event Action<string, int> OnWarning;
        public event Action<string, Color, int, bool> OnMessage;
        public event Action<string, int> OnError;

        public Form callerForm;

        public ExportParameters exportParameters;
        public bool IsCancelled { get; set; }

        public string MaxSceneFileName { get; set; }

        public bool ExportQuaternionsInsteadOfEulers { get; set; }

        private bool isBabylonExported, isGltfExported;

        private string exporterVersion = "1.2.18";

        void ReportProgressChanged(int progress)
        {
            if (OnImportProgressChanged != null)
            {
                OnImportProgressChanged(progress);
            }
        }

        void RaiseError(string error, int rank = 0)
        {
            if (OnError != null)
            {
                OnError(error, rank);
            }
        }

        void RaiseWarning(string warning, int rank = 0)
        {
            if (OnWarning != null)
            {
                OnWarning(warning, rank);
            }
        }

        void RaiseMessage(string message, int rank = 0, bool emphasis = false)
        {
            RaiseMessage(message, Color.Black, rank, emphasis);
        }

        void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false)
        {
            if (OnMessage != null)
            {
                OnMessage(message, color, rank, emphasis);
            }
        }

        // For debug purpose
        void RaiseVerbose(string message, int rank = 0, bool emphasis = false)
        {
            //RaiseMessage(message, Color.DarkGray, rank, emphasis);
        }

        void CheckCancelled()
        {
            Application.DoEvents();
            if (IsCancelled)
            {
                throw new OperationCanceledException();
            }
        }
        
        public void Export(ExportParameters exportParameters)
        {
            // Check input text is valid
            var scaleFactorFloat = 1.0f;
            string scaleFactor = exportParameters.scaleFactor;
            try
            {
                scaleFactor = scaleFactor.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                scaleFactor = scaleFactor.Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                scaleFactorFloat = float.Parse(scaleFactor);
            }
            catch
            {
                RaiseError("Scale factor is not a valid number.");
                return;
            }

            long quality = 0L;
            string txtQuality = exportParameters.txtQuality;
            try
            {
                quality = long.Parse(txtQuality);

                if(quality < 0 || quality > 100)
                {
                    throw new Exception();
                }
            }
            catch
            {
                RaiseError("Quality is not a valid number. It should be an integer between 0 and 100.");
                RaiseError("This parameter set the quality of jpg compression.");
                return;
            }

            this.exportParameters = exportParameters;

            var gameConversionManger = Loader.Global.ConversionManager;
            gameConversionManger.CoordSystem = Autodesk.Max.IGameConversionManager.CoordSystem.D3d;

            var gameScene = Loader.Global.IGameInterface;
            gameScene.InitialiseIGame(false);
            gameScene.SetStaticFrame(0);

            MaxSceneFileName = gameScene.SceneFileName;

            IsCancelled = false;
            RaiseMessage("Exportation started", Color.Blue);
            ReportProgressChanged(0);

            string outputDirectory = Path.GetDirectoryName(exportParameters.outputPath);
            string outputFileName = Path.GetFileName(exportParameters.outputPath);

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

            string outputFormat = exportParameters.outputFormat;
            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";
            isGltfExported = outputFormat == "gltf" || outputFormat == "glb";

            // Save scene
            if (exportParameters.autoSave3dsMaxFile)
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
                exporter_version = exporterVersion,
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
                if (babylonTexture != null)
                {
                    babylonScene.environmentTexture = babylonTexture.name;

                    // Skybox
                    babylonScene.createDefaultSkybox = rawScene.GetBoolProperty("babylonjs_createDefaultSkybox");
                    babylonScene.skyboxBlurLevel = rawScene.GetFloatProperty("babylonjs_skyboxBlurLevel");
                }
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

            // In 3DS Max the default camera look down (in the -z direction for the 3DS Max reference (+y for babylon))
            // In Babylon the default camera look to the horizon (in the +z direction for the babylon reference)
            if(isBabylonExported && babylonScene.CamerasList.Count > 0)
            {
                RaiseMessage("Fix camera children rotation and position", 1);
                for (int index = 0; index < babylonScene.CamerasList.Count; index++)
                {
                    BabylonCamera camera = babylonScene.CamerasList[index];
                    IList<BabylonMesh> meshes = babylonScene.MeshesList.FindAll(mesh => mesh.parentId == null ? false : mesh.parentId.Equals(camera.id));

                    RaiseMessage($"{camera.name}", 2);

                    if (camera.target == null)
                    {

                        // fix the vue
                        // Rotation around the axis X of PI / 2 in the indirect direction
                        double angle = Math.PI / 2;
                        if (camera.rotation != null)
                        {
                            camera.rotation[0] += (float)angle;
                        }
                        if (camera.rotationQuaternion != null)
                        {
                            BabylonQuaternion rotationQuaternion = FixQuaternion(camera, angle);

                            RaiseWarning($"{camera.name}: {string.Join(" ", camera.rotationQuaternion)}");
                            camera.rotationQuaternion = rotationQuaternion.ToArray();
                            RaiseWarning($"{camera.name}: {string.Join(" ", camera.rotationQuaternion)}");
                            camera.rotation = rotationQuaternion.toEulerAngles().ToArray();
                        }

                        // animation
                        List<BabylonAnimation> animations = new List<BabylonAnimation>(camera.animations);
                        BabylonAnimation animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                        if(animationRotationQuaternion != null)
                        {
                            foreach(BabylonAnimationKey key in animationRotationQuaternion.keys)
                            {
                                key.values = FixQuaternion(key.values, angle);
                            }
                        }

                        // fix direct children
                        // Rotation around the axis X of -PI / 2 in the direct direction
                        angle = -Math.PI / 2;
                        foreach (var mesh in meshes)
                        {
                            RaiseMessage($"{mesh.name}", 3);
                            mesh.position = new float[] { mesh.position[0], mesh.position[2], -mesh.position[1] };

                            // Add a rotation of PI/2 axis X in direct direction
                            if (mesh.rotationQuaternion != null)
                            {
                                // Rotation around the axis X of PI / 2 in the direct direction
                                BabylonQuaternion quaternion = FixQuaternion(mesh, angle);

                                mesh.rotationQuaternion = quaternion.ToArray();
                            }
                            if (mesh.rotation != null)
                            {
                                mesh.rotation[0] += (float)angle;
                            }


                            // Animations
                            animations = new List<BabylonAnimation>(mesh.animations);
                            // Position
                            BabylonAnimation animationPosition = animations.Find(animation => animation.property.Equals("position"));
                            if(animationPosition != null)
                            {
                                foreach(BabylonAnimationKey key in animationPosition.keys)
                                {
                                    key.values = new float[] { key.values[0], key.values[2], -key.values[1]};
                                }
                            }

                            // Rotation
                            animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                            if (animationRotationQuaternion != null)
                            {
                                foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                                {
                                    key.values = FixQuaternion(key.values, angle);
                                }
                            }
                        }
                    }

                }
            }

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

            if (scaleFactorFloat != 1.0f)
            {
                RaiseMessage("A root node is added for scaling", 1);

                // Create root node for scaling
                BabylonMesh rootNode = new BabylonMesh { name = "root", id = Guid.NewGuid().ToString() };
                rootNode.isDummy = true;
                float rootNodeScale = 1.0f / scaleFactorFloat;
                rootNode.scaling = new float[3] { rootNodeScale, rootNodeScale, rootNodeScale };

                // Update all top nodes
                var babylonNodes = new List<BabylonNode>();
                babylonNodes.AddRange(babylonScene.MeshesList);
                babylonNodes.AddRange(babylonScene.CamerasList);
                babylonNodes.AddRange(babylonScene.LightsList);
                foreach (BabylonNode babylonNode in babylonNodes)
                {
                    if (babylonNode.parentId == null)
                    {
                        babylonNode.parentId = rootNode.id;
                    }
                }

                // Store root node
                babylonScene.MeshesList.Add(rootNode);
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
                
                using (var jsonWriter = new JsonTextWriterOptimized(sw))
                {
                    jsonWriter.Formatting = Formatting.None;
                    jsonSerializer.Serialize(jsonWriter, babylonScene);
                }
                File.WriteAllText(outputFile, sb.ToString());

                if (exportParameters.generateManifest)
                {
                    File.WriteAllText(outputFile + ".manifest",
                        "{\r\n\"version\" : 1,\r\n\"enableSceneOffline\" : true,\r\n\"enableTexturesOffline\" : true\r\n}");
                }

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
            if (isGltfExported)
            {
                bool generateBinary = outputFormat == "glb";
                ExportGltf(babylonScene, outputDirectory, outputFileName, generateBinary);
            }

            watch.Stop();
            RaiseMessage(string.Format("Exportation done in {0:0.00}s", watch.ElapsedMilliseconds / 1000.0), Color.Blue);
        }

        private void exportNodeRec(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene)
        {
            BabylonNode babylonNode = null;
            switch (maxGameNode.IGameObject.IGameType)
            {
                case Autodesk.Max.IGameObject.ObjectTypes.Mesh:
                    babylonNode = ExportMesh(maxGameScene, maxGameNode, babylonScene);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Camera:
                    babylonNode = ExportCamera(maxGameScene, maxGameNode, babylonScene);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Light:
                    babylonNode = ExportLight(maxGameScene, maxGameNode, babylonScene);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Unknown:
                    // Create a dummy (empty mesh) when type is unknown
                    // An example of unknown type object is the target of target light or camera
                    babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);
                    break;
                default:
                    // The type of node is not exportable (helper, spline, xref...)
                    break;
            }
            CheckCancelled();

            // If node is not exported successfully but is significant
            if (babylonNode == null &&
                isNodeRelevantToExport(maxGameNode))
            {
                // Create a dummy (empty mesh)
                babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);
            };
            
            if (babylonNode != null)
            {
                babylonNode.tag = maxGameNode.MaxNode.GetStringProperty("babylonjs_tag", "");
                
                // Export its children
                for (int i = 0; i < maxGameNode.ChildCount; i++)
                {
                    var descendant = maxGameNode.GetNodeChild(i);
                    exportNodeRec(descendant, babylonScene, maxGameScene);
                }
            }


        }

        /// <summary>
        /// Return true if node descendant hierarchy has any exportable Mesh, Camera or Light
        /// </summary>
        private bool isNodeRelevantToExport(IIGameNode maxGameNode)
        {
            bool isRelevantToExport;
            switch (maxGameNode.IGameObject.IGameType)
            {
                case Autodesk.Max.IGameObject.ObjectTypes.Mesh:
                    isRelevantToExport = IsMeshExportable(maxGameNode);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Camera:
                    isRelevantToExport = IsCameraExportable(maxGameNode);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Light:
                    isRelevantToExport = IsLightExportable(maxGameNode);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Helper:
                    isRelevantToExport = IsNodeExportable(maxGameNode);
                    break;
                default:
                    isRelevantToExport = false;
                    break;
            }

            if (isRelevantToExport)
            {
                return true;
            }

            // Descandant recursivity
            List<IIGameNode> maxDescendants = getDescendants(maxGameNode);
            int indexDescendant = 0;
            while (indexDescendant < maxDescendants.Count) // while instead of for to stop as soon as a relevant node has been found
            {
                if (isNodeRelevantToExport(maxDescendants[indexDescendant]))
                {
                    return true;
                }
                indexDescendant++;
            }

            // No relevant node found in hierarchy
            return false;
        }

        private List<IIGameNode> getDescendants(IIGameNode maxGameNode)
        {
            var maxDescendants = new List<IIGameNode>();
            for (int i = 0; i < maxGameNode.ChildCount; i++)
            {
                maxDescendants.Add(maxGameNode.GetNodeChild(i));
            }
            return maxDescendants;
        }

        private HashSet<IIGameNode> getRootNodes(IIGameScene maxGameScene)
        {
            HashSet<IIGameNode> maxGameNodes = new HashSet<IIGameNode>();

            Func<IIGameNode, IIGameNode> getMaxRootNode = delegate (IIGameNode maxGameNode)
            {
                while (maxGameNode.NodeParent != null)
                {
                    maxGameNode = maxGameNode.NodeParent;
                }
                return maxGameNode;
            };

            Action<Autodesk.Max.IGameObject.ObjectTypes> addMaxRootNodes = delegate (Autodesk.Max.IGameObject.ObjectTypes type)
            {
                ITab<IIGameNode> maxGameNodesOfType = maxGameScene.GetIGameNodeByType(type);
                if (maxGameNodesOfType != null)
                {
                    TabToList(maxGameNodesOfType).ForEach(maxGameNode =>
                    {
                        var maxRootNode = getMaxRootNode(maxGameNode);
                        maxGameNodes.Add(maxRootNode);
                    });
                }
            };

            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Mesh);
            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Light);
            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Camera);
            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Helper);

            return maxGameNodes;
        }

        private static List<T> TabToList<T>(ITab<T> tab)
        {
            if (tab == null)
            {
                return null;
            }
            else
            {
                List<T> list = new List<T>();
                for (int i = 0; i < tab.Count; i++)
                {
#if MAX2017 || MAX2018
                    var item = tab[i];
#else
                    var item = tab[new IntPtr(i)];
#endif
                    list.Add(item);
                }
                return list;
            }
        }

        private bool IsNodeExportable(IIGameNode gameNode)
        {
            if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            {
                return false;
            }

            if (exportParameters.exportOnlySelected && !gameNode.MaxNode.Selected)
            {
                return false;
            }

            if (!exportParameters.exportHiddenObjects && gameNode.MaxNode.IsHidden(NodeHideFlags.None, false))
            {
                return false;
            }

            return true;
        }

        private IMatrix3 GetInvertWorldTM(IIGameNode gameNode, int key)
        {
            var worldMatrix = gameNode.GetWorldTM(key);
            var invertedWorldMatrix = worldMatrix.ExtractMatrix3();
            invertedWorldMatrix.Invert();
            return invertedWorldMatrix;
        }



        private BabylonQuaternion FixQuaternion(BabylonNode node, double angle)
        {
            BabylonQuaternion qFix = new BabylonQuaternion((float)Math.Sin(angle/2), 0, 0, (float)Math.Cos(angle/2));
            BabylonQuaternion quaternion = new BabylonQuaternion(node.rotationQuaternion[0], node.rotationQuaternion[1], node.rotationQuaternion[2], node.rotationQuaternion[3]);
            BabylonQuaternion rotationQuaternion = quaternion.MultiplyWith(qFix);

            return rotationQuaternion;
        }

        private float[] FixQuaternion(float[] q, double angle)
        {
            BabylonQuaternion qFix = new BabylonQuaternion((float)Math.Sin(angle / 2), 0, 0, (float)Math.Cos(angle / 2));
            BabylonQuaternion quaternion = new BabylonQuaternion(q[0], q[1], q[2], q[3]);
            BabylonQuaternion rotationQuaternion = quaternion.MultiplyWith(qFix);

            return rotationQuaternion.ToArray();
        }
    }
}
