using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utilities;
using Color = System.Drawing.Color;

namespace Max2Babylon
{
    internal partial class BabylonExporter : ILoggingProvider
    {
        public Form callerForm;

        public MaxExportParameters exportParameters;
        public bool IsCancelled { get; set; }

        public string MaxSceneFileName { get; set; }

        public bool ExportQuaternionsInsteadOfEulers { get; set; }

        private bool isBabylonExported, isGltfExported;

        public static string exporterVersion = "Custom.Build.Version";
        public float scaleFactorToMeters = 1.0f;

        public const int MaxSceneTicksPerSecond = 4800; //https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2016/ENU/MAXScript-Help/files/GUID-141213A1-B5A8-457B-8838-E602022C8798-htm.html


        public void CheckCancelled()
        {
            Application.DoEvents();
            if (IsCancelled)
            {
                throw new OperationCanceledException();
            }
        }

        private bool IsMeshFlattenable(IINode node, AnimationGroupList animationGroupList,ref List<IINode> flattenableNodes)
        {
            //a node can't be flatten if:
            //- is marked as not flattenable
            //- is hidden
            //- is not selected when exportOnlyselected is checked
            //- is part of animation group
            //- is skinned
            //- is linked to animated node

            if (node.IsMarkedAsNotFlattenable()) return false;

            if (node.IsRootNode)
            {
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n,animationGroupList,ref flattenableNodes);
                }
                return false;
            }

            if (!exportParameters.exportHiddenObjects && node.IsNodeHidden(false)) return false;

            if (exportParameters.exportOnlySelected && !node.IsNodeSelected()) return false;

            if (node.IsSkinned())
            {
                string message = $"{node.Name} can't be flatten, because is skinned";
                RaiseMessage(message, 0);
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n,animationGroupList,ref flattenableNodes);
                }
                return false;
            }

            if (node.IsNodeTreeAnimated())
            {
                string message = $"{node.Name} can't be flatten, his hierachy contains animated node";
                RaiseMessage(message, 0);
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n,animationGroupList,ref flattenableNodes);
                }
                return false;
            }

            if (node.IsInAnimationGroups(animationGroupList))
            {
                string message = $"{node.Name} can't be flatten, because is part of an AnimationGroup";
                RaiseMessage(message, 0);
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n, animationGroupList, ref flattenableNodes);
                }
                return false;
            }

            flattenableNodes.Add(node);
            return true;
        }

        public void FlattenItem(ref IINode itemNode)
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData(this,Loader.Core.RootNode);

            if (itemNode == null)
            {
                string message = "Flattening nodes of scene not supported...";
                RaiseMessage(message, 0);
            }
            else
            {
                string message = $"Flattening child nodes of {itemNode.Name}...";
                RaiseMessage(message, 0);
                List<IINode> flattenableNodes = new List<IINode>();
                if(IsMeshFlattenable(itemNode, animationGroupList,ref flattenableNodes))
                {
                    itemNode = itemNode.FlattenHierarchy();
                }
            }
        }

        public void BakeAnimationsFrame(IINode node,BakeAnimationType bakeAnimationType)
        {
            if (bakeAnimationType == BakeAnimationType.DoNotBakeAnimation) return;

            IINode hierachyRoot = (node != null) ? node : Loader.Core.RootNode;

#if MAX2020 || MAX2021
            var tobake = Loader.Global.INodeTab.Create();
#else
            var tobake = Loader.Global.NodeTab.Create();
#endif
            if (bakeAnimationType == BakeAnimationType.BakeSelective)
            {
                foreach (IINode iNode in hierachyRoot.NodeTree())
                {
                    if (iNode.IsMarkedAsObjectToBakeAnimation())
                    {
                        tobake.AppendNode(iNode,false,Loader.Core.Time);
                    }
                }
            }

            
            if (!hierachyRoot.IsRootNode) tobake.AppendNode(hierachyRoot,false,Loader.Core.Time);

            Loader.Core.SelectNodeTab(tobake,true,false);

            if (bakeAnimationType == BakeAnimationType.BakeAllAnimations)
            {
                foreach (IINode n in Tools.ITabToIEnumerable(tobake))
                {
                    n.SetUserPropBool("babylonjs_BakeAnimation", true);
                }
            }

            ScriptsUtilities.ExecuteMaxScriptCommand(@"
                for obj in selection do 
                (
                    tag = getUserProp obj ""babylonjs_BakeAnimation""
                    if tag!=true then continue

                    tmp = Point()
                    --store anim to a point
                    for t = animationRange.start to animationRange.end do (
                       with animate on at time t tmp.transform = obj.transform
                       )

                    --remove constraint on original object
                    obj.pos.controller = Position_XYZ ()
                    obj.rotation.controller = Euler_XYZ ()
                    obj.scale.controller = bezier_scale ()
                    obj.transform = tmp.transform

                    --copy back anim from point
                    for t = animationRange.start to animationRange.end do (
                       with animate on at time t obj.transform = tmp.transform
                       )
                    delete tmp
                )
             ");
        }

        public void ExportClosedContainers()
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (IIContainerObject containerObject in sceneContainers)
            {
                if (!containerObject.IsInherited)continue;
                ScriptsUtilities.ExecuteMaxScriptCommand($@"(getNodeByName(""{containerObject.ContainerNode.Name}"")).LoadContainer()");
                ScriptsUtilities.ExecuteMaxScriptCommand($@"(getNodeByName(""{containerObject.ContainerNode.Name}"")).UpdateContainer()");
                bool makeUnique = containerObject.MakeUnique;
                RaiseMessage($"Update and merge container {containerObject.ContainerNode.Name}...");
            }
            AnimationGroupList.LoadDataFromAllContainers();
        }

        public void MergeAllXrefRecords()
        {
            if (Loader.IIObjXRefManager.RecordCount <= 0) return;
            while (Loader.IIObjXRefManager.RecordCount>0)
            {
                var record = Loader.IIObjXRefManager.GetRecord(0);
                RaiseMessage($"Merge XRef record {record.SrcFile.FileName}...");
                Loader.IIObjXRefManager.MergeRecordIntoScene(record);
                //todo: load data from animation helper of xref scene merged
                //to prevent to load animations from helper created without intenction
            }
            AnimationGroupList.LoadDataFromAnimationHelpers();
        }

        public void Export(MaxExportParameters exportParameters)
        {
            ScriptsUtilities.ExecuteMaxScriptCommand(@"global BabylonExporterStatus = ""Unavailable""");
            var watch = new Stopwatch();
            watch.Start();

            this.exportParameters = exportParameters;
            IINode exportNode = null;
            double flattenTime = 0;
            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                exportNode = maxExporterParameters.exportNode;

                if (maxExporterParameters.usePreExportProcess)
                {
                    if (maxExporterParameters.mergeContainersAndXRef)
                    {
                        string message = "Merging containers and Xref...";
                        RaiseMessage(message, 0);
                        ExportClosedContainers();
                        MergeAllXrefRecords();
#if DEBUG
                        var containersXrefMergeTime = watch.ElapsedMilliseconds / 1000.0;
                        RaiseMessage(string.Format("Containers and Xref  merged in {0:0.00}s", containersXrefMergeTime ), Color.Blue);
#endif
                    }
                    BakeAnimationsFrame(exportNode,maxExporterParameters.bakeAnimationType);
                }

                if (maxExporterParameters.flattenScene && maxExporterParameters.useMultiExporter)
                {
                    FlattenItem(ref exportNode);
#if DEBUG
                    flattenTime = watch.ElapsedMilliseconds / 1000.0;
                    RaiseMessage(string.Format("Nodes flattened in {0:0.00}s", flattenTime ), Color.Blue);
#endif
                }
            }

            Tools.InitializeGuidNodesMap();

            string fileExportString = exportNode != null? $"{exportNode.NodeName} | {exportParameters.outputPath}": exportParameters.outputPath;
            RaiseMessage($"Exportation started: {fileExportString}", Color.Blue);


            scaleFactorToMeters = Tools.GetScaleFactorToMeters();
            RaiseVerbose($"scaleFactorToMeters: {scaleFactorToMeters}");

            long quality = exportParameters.txtQuality;
            try
            {
                if (quality < 0 || quality > 100)
                {
                    throw new Exception();
                }
            }
            catch
            {
                RaiseError("Quality is not a valid number. It should be an integer between 0 and 100.");
                RaiseError("This parameter sets the quality of jpg compression.");
                ScriptsUtilities.ExecuteMaxScriptCommand(@"global BabylonExporterStatus = ""Available""");
                return;
            }

            var gameConversionManger = Loader.Global.ConversionManager;
            gameConversionManger.CoordSystem = Autodesk.Max.IGameConversionManager.CoordSystem.D3d;

            var gameScene = Loader.Global.IGameInterface;
            if (exportNode == null || exportNode.IsRootNode)
            {
                gameScene.InitialiseIGame(false);
            }
            else
            {
                gameScene.InitialiseIGame(exportNode, true);
            }
            
            gameScene.SetStaticFrame(0);

            MaxSceneFileName = gameScene.SceneFileName;

            IsCancelled = false;

            
            ReportProgressChanged(0);

            string tempOutputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string outputDirectory = Path.GetDirectoryName(exportParameters.outputPath);
            string folderOuputDirectory = exportParameters.textureFolder;
            string outputFileName = Path.GetFileName(exportParameters.outputPath);

            // Check directory exists
            if (!Directory.Exists(outputDirectory))
            {
                RaiseError("Exportation stopped: Output folder does not exist");
                ReportProgressChanged(100);
                ScriptsUtilities.ExecuteMaxScriptCommand(@"global BabylonExporterStatus = ""Available""");
                return;
            }
            Directory.CreateDirectory(tempOutputDirectory);
            
            var outputBabylonDirectory = tempOutputDirectory;

            // Force output file extension to be babylon
            outputFileName = Path.ChangeExtension(outputFileName, "babylon");

            var babylonScene = new BabylonScene(outputBabylonDirectory);

            var rawScene = Loader.Core.RootNode;

             string outputFormat = exportParameters.outputFormat;
            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";
            isGltfExported = outputFormat == "gltf" || outputFormat == "glb";

            // Save scene
            if (exportParameters.autoSaveSceneFile)
            {
                RaiseMessage("Saving 3ds max file");
                var forceSave = Loader.Core.FileSave;

                callerForm?.BringToFront();
            }

            // Producer
            babylonScene.producer = new BabylonProducer
            {
                name = "3dsmax",
#if MAX2021
                version = "2021",
#elif MAX2020
                version = "2020",
#elif MAX2019
                version = "2019",
#elif MAX2018
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
            babylonScene.TimelineStartFrame = Loader.Core.AnimRange.Start / Loader.Global.TicksPerFrame;
            babylonScene.TimelineEndFrame = Loader.Core.AnimRange.End / Loader.Global.TicksPerFrame;
            babylonScene.TimelineFramesPerSecond = MaxSceneTicksPerSecond / Loader.Global.TicksPerFrame;

            if (exportParameters.exportAnimationsOnly == false)
            {
                ExportQuaternionsInsteadOfEulers = rawScene.GetBoolProperty("babylonjs_exportquaternions", 1);

                babylonScene.autoClear = true;
                babylonScene.clearColor = Loader.Core.GetBackGround(0, Tools.Forever).ToArray();
                babylonScene.ambientColor = Loader.Core.GetAmbient(0, Tools.Forever).ToArray();

                babylonScene.gravity = rawScene.GetVector3Property("babylonjs_gravity");
                if (string.IsNullOrEmpty(exportParameters.pbrEnvironment) && Loader.Core.UseEnvironmentMap && Loader.Core.EnvironmentMap != null)
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
                else if (!string.IsNullOrEmpty(exportParameters.pbrEnvironment))
                {
                    babylonScene.createDefaultSkybox = rawScene.GetBoolProperty("babylonjs_createDefaultSkybox");
                    babylonScene.skyboxBlurLevel = rawScene.GetFloatProperty("babylonjs_skyboxBlurLevel");
                }
            }

            // Instantiate custom material exporters
            materialExporters = new Dictionary<ClassIDWrapper, IMaxMaterialExporter>();
            foreach (Type type in Tools.GetAllLoadableTypes())
            {
                if (type.IsAbstract || type.IsInterface )
                    continue;

                if (typeof(IBabylonExtensionExporter).IsAssignableFrom(type))
                {
                    IBabylonExtensionExporter exporter = Activator.CreateInstance(type) as IBabylonExtensionExporter;

                    if (exporter == null)
                        RaiseWarning("Creating exporter instance failed: " + type.Name, 1);

                    Type t = exporter.GetGLTFExtendedType();
                    babylonScene.BabylonToGLTFExtensions.Add(exporter,t);
                }

                if (typeof(IMaxMaterialExporter).IsAssignableFrom(type))
                {
                    IMaxMaterialExporter exporter = Activator.CreateInstance(type) as IMaxMaterialExporter;

                    if (exporter == null)
                        RaiseWarning("Creating exporter instance failed: " + type.Name, 1);

                    materialExporters.Add(exporter.MaterialClassID, exporter);
                }
            }

            // Sounds
            if (exportParameters.exportAnimationsOnly == false)
            {
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
                BabylonNode node = exportNodeRec(maxRootNode, babylonScene, gameScene);
                // if we're exporting from a specific node, reset the pivot to {0,0,0}
                if (node != null && exportNode != null && !exportNode.IsRootNode)
                {
                    if (!exportParameters.exportKeepNodePosition)
                    {
                        SetNodePosition(ref node, ref babylonScene, new float[] { 0, 0, 0 });
                    }
                }

                progression += progressionStep;
                ReportProgressChanged((int)progression);
                CheckCancelled();
            };
            RaiseMessage(string.Format("Total meshes: {0}", babylonScene.MeshesList.Count), Color.Gray, 1);

            // In 3DS Max the default camera look down (in the -z direction for the 3DS Max reference (+y for babylon))
            // In Babylon the default camera look to the horizon (in the +z direction for the babylon reference)
            // In glTF the default camera look to the horizon (in the +Z direction for glTF reference)
            RaiseMessage("Update camera rotation and position", 1);
            for (int index = 0; index < babylonScene.CamerasList.Count; index++)
            {
                BabylonCamera camera = babylonScene.CamerasList[index];
                FixCamera(ref camera, ref babylonScene);
            }

            // Light for glTF
            if (isGltfExported)
            {
                RaiseMessage("Update light rotation for glTF export", 1);
                for (int index = 0; index < babylonScene.LightsList.Count; index++)
                {
                    BabylonNode light = babylonScene.LightsList[index];
                    FixNodeRotation(ref light, ref babylonScene, Math.PI / 2);
                }
            }

            // Convert fixed cameras and lights to dummies (meshes without geometry)
            if (exportParameters.exportAnimationsOnly)
            {
                babylonScene.CamerasList.ForEach(camera =>
                {
                    ExportDummy(camera, babylonScene);
                });
                babylonScene.CamerasList.Clear();

                babylonScene.LightsList.ForEach(light =>
                {
                    ExportDummy(light, babylonScene);
                });
                babylonScene.LightsList.Clear();
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
            if (exportParameters.exportAnimationsOnly == false)
            {
                if (!exportParameters.pbrNoLight && babylonScene.LightsList.Count == 0 && rawScene.GetBoolProperty("babylonjs_addDefaultLight"))
                {
                    RaiseWarning("No light defined", 1);
                    RaiseWarning("A default hemispheric light was added for your convenience", 1);
                    ExportDefaultLight(babylonScene);
                }
                else
                {
                    RaiseMessage(string.Format("Total lights: {0}", babylonScene.LightsList.Count), Color.Gray, 1);
                }
            }

            if (exportParameters.scaleFactor != 1.0f)
            {
                RaiseMessage(String.Format("A root node is added to globally scale the scene by {0}", exportParameters.scaleFactor), 1);

                // Create root node for scaling
                BabylonMesh rootNode = new BabylonMesh { name = "root", id = Guid.NewGuid().ToString() };
                rootNode.isDummy = true;
                float rootNodeScale = exportParameters.scaleFactor;
                rootNode.scaling = new float[3] { rootNodeScale, rootNodeScale, rootNodeScale };

                if (ExportQuaternionsInsteadOfEulers)
                {
                    rootNode.rotationQuaternion = new float[] { 0, 0, 0, 1 };
                }
                else
                {
                    rootNode.rotation = new float[] { 0, 0, 0 };
                }

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
#if DEBUG
            var nodesExportTime = watch.ElapsedMilliseconds / 1000.0 - flattenTime;
            RaiseMessage($"Nodes exported in {nodesExportTime:0.00}s", Color.Blue);
#endif
            // Materials
            if (exportParameters.exportMaterials && exportParameters.exportAnimationsOnly == false)
            {
                RaiseMessage("Exporting materials");
                var matsToExport = referencedMaterials.ToArray(); // Snapshot because multimaterials can export new materials
                foreach (var mat in matsToExport)
                {
                    ExportMaterial(mat, babylonScene);
                    CheckCancelled();
                }
                RaiseMessage(string.Format("Total: {0}", babylonScene.MaterialsList.Count + babylonScene.MultiMaterialsList.Count), Color.Gray, 1);
            }
            else
            {
                RaiseMessage("Skipping material export.");
            }
#if DEBUG
            var materialsExportTime = watch.ElapsedMilliseconds / 1000.0 - nodesExportTime;
            RaiseMessage($"Materials exported in {materialsExportTime:0.00}s", Color.Blue);
#endif
            // Fog
            if (exportParameters.exportAnimationsOnly == false)
            {
                for (var index = 0; index < Loader.Core.NumAtmospheric; index++)
                {
                    var atmospheric = Loader.Core.GetAtmospheric(index);

                    if (atmospheric != null && atmospheric.Active(0) && atmospheric.ClassName == "Fog")
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
#if DEBUG
            var skeletonsExportTime = watch.ElapsedMilliseconds / 1000.0 - materialsExportTime;
            RaiseMessage($"Skeletons exported in {skeletonsExportTime:0.00}s", Color.Blue);
#endif
            // ----------------------------
            // ----- Animation groups -----
            // ----------------------------
            if (exportParameters.exportAnimations)
            {
                RaiseMessage("Export animation groups");
                // add animation groups to the scene
                babylonScene.animationGroups = ExportAnimationGroups(babylonScene);
#if DEBUG
            var animationGroupExportTime = watch.ElapsedMilliseconds / 1000.0 -nodesExportTime;
            RaiseMessage(string.Format("Animation groups exported in {0:0.00}s", animationGroupExportTime), Color.Blue);
#endif
            }

            if (isBabylonExported)
            {
                // if we are exporting to .Babylon then remove then remove animations from nodes if there are animation groups.
                if (babylonScene.animationGroups?.Count > 0)
                {
                    foreach (BabylonNode node in babylonScene.MeshesList)
                    {
                        node.animations = null;
                    }
                    foreach (BabylonNode node in babylonScene.LightsList)
                    {
                        node.animations = null;
                    }
                    foreach (BabylonNode node in babylonScene.CamerasList)
                    {
                        node.animations = null;
                    }
                    foreach (BabylonSkeleton skel in babylonScene.SkeletonsList)
                    {
                        foreach (BabylonBone bone in skel.bones)
                        {
                            bone.animation = null;
                        }
                    }
                }

                if (exportParameters.exportAnimationsOnly == false)
                {
                    // setup a default skybox for the scene for .Babylon export.
                    var sourcePath = exportParameters.pbrEnvironment;
                    if (!string.IsNullOrEmpty(sourcePath))
                    {
                        var fileName = Path.GetFileName(sourcePath);

                        // Allow only dds file format
                        if (!fileName.EndsWith(".dds"))
                        {
                            RaiseWarning("Failed to export defauenvironment texture: only .dds format is supported.");
                        }
                        else
                        {
                            RaiseMessage($"texture id = Max_Babylon_Default_Environment");
                            babylonScene.environmentTexture = fileName;

                            if (exportParameters.writeTextures)
                            {
                                try
                                {
                                    var destPath = Path.Combine(babylonScene.OutputPath, fileName);
                                    if (File.Exists(sourcePath) && sourcePath != destPath)
                                    {
                                        File.Copy(sourcePath, destPath, true);
                                    }
                                }
                                catch
                                {
                                    // silently fails
                                    RaiseMessage($"Fail to export the default env texture", 3);
                                }
                            }
                        }
                    }
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
#if DEBUG
                    jsonWriter.Formatting = Formatting.Indented;
#else
                    jsonWriter.Formatting = Formatting.None;
#endif
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

                GLTFExporter gltfExporter = new GLTFExporter();
                exportParameters.customGLTFMaterialExporter = new MaxGLTFMaterialExporter(exportParameters, gltfExporter, this);
                gltfExporter.ExportGltf(this.exportParameters, babylonScene, tempOutputDirectory, outputFileName, generateBinary, this);
            }
            // Move files to output directory
            var filePaths = Directory.GetFiles(tempOutputDirectory);
            if (outputFormat == "binary babylon")
            {
                var tempBinaryOutputDirectory = Path.Combine(tempOutputDirectory, "Binary");
                var binaryFilePaths = Directory.GetFiles(tempBinaryOutputDirectory);
                foreach(var filePath in binaryFilePaths)
                {
                    if (filePath.EndsWith(".binary.babylon"))
                    {
                        var file = Path.GetFileName(filePath);
                        var tempFilePath = Path.Combine(tempBinaryOutputDirectory, file);
                        var outputFile = Path.Combine(outputDirectory, file);

                        IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputFile);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                        moveFileToOutputDirectory(tempFilePath, outputFile, exportParameters);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);
                    }
                    else if (filePath.EndsWith(".babylonbinarymeshdata"))
                    {
                        var file = Path.GetFileName(filePath);
                        var tempFilePath = Path.Combine(tempBinaryOutputDirectory, file);
                        var outputFile = Path.Combine(outputDirectory, file);

                        IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputFile);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                        moveFileToOutputDirectory(tempFilePath, outputFile, exportParameters);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);
                    }
                }
            }
            if (outputFormat == "glb")
            {
                foreach (var file_path in filePaths)
                {
                    if (Path.GetExtension(file_path) == ".glb")
                    {
                        var file = Path.GetFileName(file_path);
                        var tempFilePath = Path.Combine(tempOutputDirectory, file);
                        var outputFile = Path.Combine(outputDirectory, file);


                        IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputFile);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                        moveFileToOutputDirectory(tempFilePath, outputFile, exportParameters);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);

                        break;
                    }   
                }
            }
            else
            { 
                foreach (var filePath in filePaths)
                {
                    var file = Path.GetFileName(filePath);
                    string ext = Path.GetExtension(file);
                    var tempFilePath = Path.Combine(tempOutputDirectory, file);
                    var outputPath = Path.Combine(outputDirectory, file);
                    if (!string.IsNullOrWhiteSpace(exportParameters.textureFolder) && TextureUtilities.ExtensionIsValidGLTFTexture(ext))
                    {
                        outputPath = Path.Combine(exportParameters.textureFolder, file);
                    }

                    IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputPath);
                    Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                    moveFileToOutputDirectory(tempFilePath, outputPath, exportParameters);
                    Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);
                }
            }
            Directory.Delete(tempOutputDirectory, true);
            watch.Stop();

            RaiseMessage(string.Format("Exportation done in {0:0.00}s: {1}", watch.ElapsedMilliseconds / 1000.0, fileExportString), Color.Blue);
            IUTF8Str max_notification = Autodesk.Max.GlobalInterface.Instance.UTF8Str.Create("BabylonExportComplete");
            Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, max_notification);

            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                if (maxExporterParameters.flattenScene)
                {
                    Tools.RemoveFlattenModification();
                }
            }
            ScriptsUtilities.ExecuteMaxScriptCommand(@"global BabylonExporterStatus = ""Available""");
        }

        private void moveFileToOutputDirectory(string sourceFilePath, string targetFilePath, ExportParameters exportParameters)
        {
            var fileExtension = Path.GetExtension(sourceFilePath).Substring(1).ToLower();
            if (validFormats.Contains(fileExtension))
            {
                if (exportParameters.writeTextures)
                {
                    if (File.Exists(targetFilePath))
                    {
                        if (exportParameters.overwriteTextures)
                        {
                            File.Delete(targetFilePath);
                            File.Move(sourceFilePath, targetFilePath);
                            RaiseMessage(sourceFilePath + " -> " + targetFilePath);
                        }
                    }
                    else
                    {
                        File.Move(sourceFilePath, targetFilePath);
                        RaiseMessage(sourceFilePath + " -> " + targetFilePath);
                    }
                }
            }
            else
            {
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }
                File.Move(sourceFilePath, targetFilePath);
                RaiseMessage(sourceFilePath + " -> " + targetFilePath);
            }
        }

        private BabylonNode exportNodeRec(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene)
        {
            BabylonNode babylonNode = null;
            try
            {
                switch (maxGameNode.IGameObject.IGameType)
                {
                    case Autodesk.Max.IGameObject.ObjectTypes.Mesh:
                        if (exportParameters.exportAnimationsOnly == false)
                        {
                            babylonNode = ExportMesh(maxGameScene, maxGameNode, babylonScene);
                        }
                        else
                        {
                            babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);
                        }
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
            }
            catch (Exception e)
            {
                this.RaiseWarning(String.Format("Exception raised during export. Node will be exported as dummy node. \r\nMessage: \r\n{0} \r\n{1}", e.Message, e.InnerException), 1);
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
                string tag = maxGameNode.MaxNode.GetStringProperty("babylonjs_tag", "");
                if (tag != "")
                {
                    babylonNode.tags = tag;
                }

                // Export its children
                for (int i = 0; i < maxGameNode.ChildCount; i++)
                {
                    var descendant = maxGameNode.GetNodeChild(i);
                    exportNodeRec(descendant, babylonScene, maxGameScene);
                }
                babylonScene.NodeMap[babylonNode.id] = babylonNode;
            }

            return babylonNode;
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
                    if (exportParameters.exportAnimationsOnly && maxGameNode.IGameControl != null && !isAnimated(maxGameNode))
                    {
                        isRelevantToExport = false;
                    }
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
#if MAX2017 || MAX2018 || MAX2019 || MAX2020 || MAX2021
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
            if (gameNode.MaxNode.GetBoolProperty("babylonjs_flatteningTemp"))
            {
                return true;
            }

            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                if (maxExporterParameters.exportLayers!=null && maxExporterParameters.exportLayers.Count>0)
                {
                    if (!maxExporterParameters.exportLayers.HaveNode(gameNode.MaxNode))
                    {
                        return false;
                    }
                }
            }

            if (gameNode.MaxNode.GetBoolProperty("babylonjs_flattened"))
            {
                return false;
            }


            if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            {
                return false;
            }

            if (gameNode.MaxNode.IsBabylonContainerHelper() || gameNode.MaxNode.IsBabylonAnimationHelper())
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

        private IMatrix3 GetOffsetTM(IIGameNode gameNode, int key)
        {
            IPoint3 objOffsetPos = gameNode.MaxNode.ObjOffsetPos;
            IQuat objOffsetQuat = gameNode.MaxNode.ObjOffsetRot;
            IPoint3 objOffsetScale = gameNode.MaxNode.ObjOffsetScale.S;

            // conversion: LH vs RH coordinate system (swap Y and Z)
            var tmpSwap = objOffsetPos.Y;
            objOffsetPos.Y = objOffsetPos.Z;
            objOffsetPos.Z = tmpSwap;

            tmpSwap = objOffsetQuat.Y;
            objOffsetQuat.Y = objOffsetQuat.Z;
            objOffsetQuat.Z = tmpSwap;
            var objOffsetRotMat = Tools.Identity;
            objOffsetQuat.MakeMatrix(objOffsetRotMat, true);

            tmpSwap = objOffsetScale.Y;
            objOffsetScale.Y = objOffsetScale.Z;
            objOffsetScale.Z = tmpSwap;

            // build the offset transform; equivalent in maxscript: 
            // offsetTM = (scaleMatrix $.objectOffsetScale) * ($.objectOffsetRot as matrix3) * (transMatrix $.objectOffsetPos)
            IMatrix3 offsetTM = Tools.Identity;
            offsetTM.Scale(objOffsetScale, false);
            offsetTM.MultiplyBy(objOffsetRotMat);
            offsetTM.Translate(objOffsetPos); 

            return offsetTM;
        }

        /// <summary>
        /// In 3DS Max default element can look in different direction than the same default element in Babylon or in glTF.
        /// This function correct the node rotation.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="babylonScene"></param>
        /// <param name="angle"></param>
        private void FixNodeRotation(ref BabylonNode node, ref BabylonScene babylonScene, double angle)
        {
            string id = node.id;
            IList<BabylonMesh> meshes = babylonScene.MeshesList.FindAll(mesh => mesh.parentId == null ? false : mesh.parentId.Equals(id));

            RaiseMessage($"{node.name}", 2);

            // fix the vue
            // Rotation around the axis X of PI / 2 in the indirect direction for camera
            // double angle = Math.PI / 2; // for camera
            // double angle = -Math.PI / 2; // for light

            if (node.rotation != null)
            {
                node.rotation[0] += (float)angle;
            }
            if (node.rotationQuaternion != null)
            {
                BabylonQuaternion rotationQuaternion = FixCameraQuaternion(node, angle);

                node.rotationQuaternion = rotationQuaternion.ToArray();
                node.rotation = rotationQuaternion.toEulerAngles().ToArray();
            }

            BabylonAnimation animationRotationQuaternion;

            // animation
            if (node.animations != null)
            {
                List<BabylonAnimation> animations = new List<BabylonAnimation>(node.animations);
                animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                if (animationRotationQuaternion != null)
                {
                    foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                    {
                        key.values = FixCameraQuaternion(key.values, angle);
                    }
                }
            }
            // if the camera has a lockedTargetId, it is the extraAnimations that stores the rotation animation
            if (node.extraAnimations != null)
            {
                List<BabylonAnimation> extraAnimations = new List<BabylonAnimation>(node.extraAnimations);
                animationRotationQuaternion = extraAnimations.Find(animation => animation.property.Equals("rotationQuaternion"));
                if (animationRotationQuaternion != null)
                {
                    foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                    {
                        key.values = FixCameraQuaternion(key.values, angle);
                    }
                }
            }

            // fix direct children
            // Rotation around the axis X of -PI / 2 in the direct direction for camera children
            angle = -angle;
            foreach (var mesh in meshes)
            {
                RaiseVerbose($"{mesh.name}", 3);
                mesh.position = new float[] { mesh.position[0], mesh.position[2], -mesh.position[1] };

                // Add a rotation of PI/2 axis X in direct direction
                if (mesh.rotationQuaternion != null)
                {
                    // Rotation around the axis X of -PI / 2 in the direct direction
                    BabylonQuaternion quaternion = FixChildQuaternion(mesh, angle);

                    mesh.rotationQuaternion = quaternion.ToArray();
                }
                if (mesh.rotation != null)
                {
                    mesh.rotation[0] += (float)angle;
                }


                // Animations
                if (mesh.animations != null)
                {
                    List<BabylonAnimation> animations = new List<BabylonAnimation>(mesh.animations);
                    // Position
                    BabylonAnimation animationPosition = animations.Find(animation => animation.property.Equals("position"));
                    if (animationPosition != null)
                    {
                        foreach (BabylonAnimationKey key in animationPosition.keys)
                        {
                            key.values = new float[] { key.values[0], key.values[2], -key.values[1] };
                        }
                    }

                    // Rotation
                    animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                    if (animationRotationQuaternion != null)
                    {
                        foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                        {
                            key.values = FixChildQuaternion(key.values, angle);
                        }
                    }
                }
            }

        }

        private void SetNodePosition(ref BabylonNode node, ref BabylonScene babylonScene, float[] newPosition)
        {
            float[] offset = new float[] { newPosition[0] - node.position[0], newPosition[1] - node.position[1], newPosition[2] - node.position[2] };
            node.position = newPosition;

            if (node.animations != null)
            {
                List<BabylonAnimation> animations = new List<BabylonAnimation>(node.animations);
                BabylonAnimation animationPosition = animations.Find(animation => animation.property.Equals("position"));
                if (animationPosition != null)
                {
                    foreach (BabylonAnimationKey key in animationPosition.keys)
                    {
                        key.values = new float[] {
                        key.values[0] + offset[0],
                        key.values[1] + offset[1],
                        key.values[2] + offset[2] };
                    }
                }
            }
        }
    }
}