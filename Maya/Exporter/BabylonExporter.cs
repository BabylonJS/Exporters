using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        // Export options
        private bool _onlySelected;
        private bool _exportHiddenObjects;
        private bool _optimizeVertices;
        private bool _exportTangents;
        private bool ExportHiddenObjects { get; set; }
        private bool CopyTexturesToOutput { get; set; }
        private bool ExportQuaternionsInsteadOfEulers { get; set; } = true;
        private bool isBabylonExported;
        private bool _exportSkin;
        private long _quality;
        private bool _dracoCompression;
        private bool _exportMorphNormal;
        private bool _exportMorphTangent;
        private bool _exportKHRLightsPunctual;
        private bool _exportKHRTextureTransform;

        public bool IsCancelled { get; set; }

        // Custom properties
        private bool _exportQuaternionsInsteadOfEulers;

        // TODO - Check if it's ok for other languages
        /// <summary>
        /// Names of the default cameras used as viewports in Maya in English language
        /// Those cameras are always ignored when exporting (ie even when exporting hidden objects)
        /// </summary>
        private static List<string> defaultCameraNames = new List<string>(new string[] { "persp", "top", "front", "side" });

        private string exporterVersion = "1.2.25";

        /// <summary>
        /// Export to file
        /// </summary>
        /// <param name="outputDirectory">The directory to store the generated files</param>
        /// <param name="outputFileName">The filename to use for the generated files</param>
        /// <param name="outputFormat">The format to use for the generated files</param>
        /// <param name="generateManifest">Specifies if a manifest file should be generated</param>
        /// <param name="onlySelected">Specifies if only the selected objects should be exported</param>
        /// <param name="autoSaveMayaFile">Specifies if the Maya scene should be auto-saved</param>
        /// <param name="exportHiddenObjects">Specifies if hidden objects should be exported</param>
        /// <param name="copyTexturesToOutput">Specifies if textures should be copied to the output directory</param>
        /// <param name="optimizeVertices">Specifies if vertices should be optimized on export</param>
        /// <param name="exportTangents">Specifies if tangents should be exported</param>
        /// <param name="scaleFactor">Scales the scene by this factor</param>
        /// <param name="exportSkin">Specifies if skins should be exported</param>
        /// <param name="quality">The texture quality</param>
        /// <param name="dracoCompression">Specifies if draco compression should be used</param>
        /// <param name="exportMorphNormal">Specifies if normals should be exported for morph targets</param>
        /// <param name="exportMorphTangent">Specifies if tangents should be exported for morph targets</param>
        /// <param name="exportKHRLightsPunctual">Specifies if the KHR_lights_punctual extension should be enabled</param>
        /// <param name="exportKHRTextureTransform">Specifies if the KHR_texture_transform extension should be enabled</param>
        public void Export(string outputDirectory, string outputFileName, string outputFormat, bool generateManifest,
                            bool onlySelected, bool autoSaveMayaFile, bool exportHiddenObjects, bool copyTexturesToOutput,
                            bool optimizeVertices, bool exportTangents, string scaleFactor, bool exportSkin, string quality, bool dracoCompression,
                            bool exportMorphNormal, bool exportMorphTangent, bool exportKHRLightsPunctual, bool exportKHRTextureTransform)
        {
            // Check if the animation is running
            MGlobal.executeCommand("play -q - state", out int isPlayed);
            if(isPlayed == 1)
            {
                RaiseError("Stop the animation before exporting.");
                return;
            }

            // Check input text is valid
            var scaleFactorFloat = 1.0f;
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

            try
            {
                _quality = long.Parse(quality);

                if (_quality < 0 || _quality > 100)
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

            RaiseMessage("Export started", Color.Blue);
            var progression = 0.0f;
            ReportProgressChanged(progression);

            // Store export options
            _onlySelected = onlySelected;
            _exportHiddenObjects = exportHiddenObjects;
            _optimizeVertices = optimizeVertices;
            _exportTangents = exportTangents;
            CopyTexturesToOutput = copyTexturesToOutput;
            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";
            _exportSkin = exportSkin;
            _dracoCompression = dracoCompression;
            _exportMorphNormal = exportMorphNormal;
            _exportMorphTangent = exportMorphTangent;
            _exportKHRLightsPunctual = exportKHRLightsPunctual;
            _exportKHRTextureTransform = exportKHRTextureTransform;
            

            // Check directory exists
            if (!Directory.Exists(outputDirectory))
            {
                RaiseError("Export stopped: Output folder does not exist");
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

                // Query expand file name
                string fileName = MGlobal.executeCommandStringResult($@"file -q -exn;");

                // If scene has already been saved previously
                if (fileName.EndsWith(".ma") || fileName.EndsWith(".mb"))
                {
                    // Name is already specified and this line will not fail
                    MFileIO.save();
                }
                else
                {
                    // Open SaveAs dialog window
                    MGlobal.executeCommand($@"fileDialog2;");
                }
            }

            // Force output file extension to be babylon
            outputFileName = Path.ChangeExtension(outputFileName, "babylon");

            // Store selected nodes
            MSelectionList selectedNodes = new MSelectionList();
            MGlobal.getActiveSelectionList(selectedNodes);
            selectedNodeFullPaths = new List<string>();
            MItSelectionList mItSelectionList = new MItSelectionList(selectedNodes);
            while (!mItSelectionList.isDone)
            {
                MDagPath mDagPath = new MDagPath();
                try
                {
                    mItSelectionList.getDagPath(mDagPath);
                    selectedNodeFullPaths.Add(mDagPath.fullPathName);
                } catch
                {
                    // selected object is not a DAG object
                    // fail silently
                }

                mItSelectionList.next();
            }
            if (selectedNodeFullPaths.Count > 0)
            {
                RaiseMessage("Selected nodes full path");
                foreach(string selectedNodeFullPath in selectedNodeFullPaths)
                {
                    RaiseMessage(selectedNodeFullPath, 1);
                }
            }

            // Producer
            babylonScene.producer = new BabylonProducer
            {
                name = "Maya",
                version = "2018",
                exporter_version = exporterVersion,
                file = outputFileName
            };

            // Global
            babylonScene.autoClear = true;
            // TODO - Retreive colors from Maya
            //babylonScene.clearColor = Loader.Core.GetBackGround(0, Tools.Forever).ToArray();
            //babylonScene.ambientColor = Loader.Core.GetAmbient(0, Tools.Forever).ToArray();

            // TODO - Add custom properties
            _exportQuaternionsInsteadOfEulers = true;
            
            PrintDAG(true);
            PrintDAG(false);

            // Store the current frame. It can be change to find a proper one for the node/bone export
            double currentTime = Loader.GetCurrentTime();

            // --------------------
            // ------ Nodes -------
            // --------------------
            RaiseMessage("Exporting nodes");

            // It makes each morph target manager export starts from id = 0.
            BabylonMorphTargetManager.Reset();

            // Clear materials
            referencedMaterials.Clear();
            multiMaterials.Clear();

            // Get all nodes
            var dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst, MFn.Type.kTransform);
            List<MDagPath> nodes = new List<MDagPath>();
            while (!dagIterator.isDone)
            {
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                
                // Check if one of its descendant (direct or not) is a mesh/camera/light/locator
                if (isNodeRelevantToExportRec(mDagPath)
                    // Ensure it's not one of the default cameras used as viewports in Maya
                    && defaultCameraNames.Contains(mDagPath.partialPathName) == false)
                {
                    nodes.Add(mDagPath);
                }
                else
                {
                    // Skip descendants
                    dagIterator.prune();
                }

                dagIterator.next();
            }
            // Export all nodes
            var progressionStep = 100.0f / nodes.Count;
            foreach (MDagPath mDagPath in nodes)
            {
                BabylonNode babylonNode = null;
                
                switch (getApiTypeOfDirectDescendants(mDagPath))
                {
                    case MFn.Type.kMesh:
                        babylonNode = ExportMesh(mDagPath, babylonScene);
                        break;
                    case MFn.Type.kCamera:
                        babylonNode = ExportCamera(mDagPath, babylonScene);
                        break;
                    case MFn.Type.kLight: // Lights api type are actually kPointLight, kSpotLight...
                        babylonNode = ExportLight(mDagPath, babylonScene);
                        break;
                    case MFn.Type.kLocator: // Camera target
                        babylonNode = ExportDummy(mDagPath, babylonScene);
                        break;
                }

                // If node is not exported successfully
                if (babylonNode == null)
                {
                    // Create a dummy (empty mesh)
                    babylonNode = ExportDummy(mDagPath, babylonScene);
                };
                
                // Update progress bar
                progression += progressionStep;
                ReportProgressChanged(progression);

                CheckCancelled();
            }
            RaiseMessage(string.Format("Total meshes: {0}", babylonScene.MeshesList.Count), Color.Gray, 1);


            // if nothing is enlightened, exclude all meshes
            foreach (BabylonLight light in babylonScene.LightsList)
            {
                if(light.includedOnlyMeshesIds.Length == 0)
                {
                    light.excludedMeshesIds = babylonScene.MeshesList.Select(m => m.id).ToArray();
                }
            }

            /*
             * Switch coordinate system at global level
             * 
             * Add a root node with negative scaling
             * Pros - It's safer to use a root node
             * Cons - It's cleaner to switch at object level (as it is done now)
             * Use root node method when you want to be 100% sure of the output
             * Don't forget to also inverse winding order of mesh indices
             */
            //// Switch from right to left handed coordinate system
            //MUuid mUuid = new MUuid();
            //mUuid.generate();
            //var rootNode = new BabylonMesh
            //{
            //    name = "root",
            //    id = mUuid.asString(),
            //    scaling = new float[] { 1, 1, -1 }
            //};
            //foreach(var babylonMesh in babylonScene.MeshesList)
            //{
            //    // Add root meshes as child to root node
            //    if (babylonMesh.parentId == null)
            //    {
            //        babylonMesh.parentId = rootNode.id;
            //    }
            //}
            //babylonScene.MeshesList.Add(rootNode);
            
            // Main camera
            BabylonCamera babylonMainCamera = null;
            if (babylonScene.CamerasList.Count > 0)
            {
                // Set first camera as main one
                babylonMainCamera = babylonScene.CamerasList[0];
                babylonScene.activeCameraID = babylonMainCamera.id;
                RaiseMessage("Active camera set to " + babylonMainCamera.name, Color.Green, 1, true);
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
                RaiseWarning("A default ambient light was added for your convenience", 1);
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
                BabylonMesh rootNode = new BabylonMesh { name = "root", id = Tools.GenerateUUID() };
                rootNode.isDummy = true;
                float rootNodeScale = scaleFactorFloat;
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

            // --------------------
            // ----- Materials ----
            // --------------------
            RaiseMessage("Exporting materials");
            GenerateMaterialDuplicationDatas(babylonScene);
            foreach (var mat in referencedMaterials)
            {
                ExportMaterial(mat, babylonScene);
                CheckCancelled();
            }
            foreach (var mat in multiMaterials)
            {
                ExportMultiMaterial(mat.Key, mat.Value, babylonScene);
                CheckCancelled();
            }
            UpdateMeshesMaterialId(babylonScene);
            RaiseMessage(string.Format("Total: {0}", babylonScene.MaterialsList.Count + babylonScene.MultiMaterialsList.Count), Color.Gray, 1);


            // Export skeletons
            if (_exportSkin && skins.Count > 0)
            {
                progressSkin = 0;
                progressSkinStep = 100 / skins.Count;
                ReportProgressChanged(progressSkin);
                RaiseMessage("Exporting skeletons");
                foreach (var skin in skins)
                {
                    ExportSkin(skin, babylonScene);
                }
            }

            // set back the frame
            Loader.SetCurrentTime(currentTime);

            // Animation group
            if (isBabylonExported)
            {
                RaiseMessage("Export animation groups");
                // add animation groups to the scene
                babylonScene.animationGroups = ExportAnimationGroups(babylonScene);

                // if there is animationGroup, then remove animations from nodes
                if (babylonScene.animationGroups.Count > 0)
                {
                    // add animations of each nodes in the animGroup
                    List<BabylonNode> babylonNodes = new List<BabylonNode>();
                    babylonNodes.AddRange(babylonScene.MeshesList);
                    babylonNodes.AddRange(babylonScene.CamerasList);
                    babylonNodes.AddRange(babylonScene.LightsList);

                    foreach (BabylonNode node in babylonNodes)
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
            }


            // Output
            babylonScene.Prepare(false, false);
            if (isBabylonExported)
            {
                Write(babylonScene, outputBabylonDirectory, outputFileName, outputFormat, generateManifest);
            }

            ReportProgressChanged(100);

            // Export glTF
            if (outputFormat == "gltf" || outputFormat == "glb")
            {
                bool generateBinary = outputFormat == "glb";
                ExportGltf(babylonScene, outputDirectory, outputFileName, generateBinary);
            }

            watch.Stop();
            RaiseMessage(string.Format("Export done in {0:0.00}s", watch.ElapsedMilliseconds / 1000.0), Color.Blue);
        }

        void CheckCancelled()
        {
            Application.DoEvents();
            if (IsCancelled)
            {
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Return true if node descendant hierarchy has any exportable Mesh, Camera, Light or Locator
        /// </summary>
        private bool isNodeRelevantToExportRec(MDagPath mDagPathRoot)
        {
            var mIteratorType = new MIteratorType();
            MIntArray listOfFilters = new MIntArray();
            listOfFilters.Add((int)MFn.Type.kMesh);
            listOfFilters.Add((int)MFn.Type.kCamera);
            listOfFilters.Add((int)MFn.Type.kLight);
            listOfFilters.Add((int)MFn.Type.kLocator);
            mIteratorType.setFilterList(listOfFilters);
            var dagIterator = new MItDag(mIteratorType, MItDag.TraversalType.kDepthFirst);
            dagIterator.reset(mDagPathRoot);

            while (!dagIterator.isDone)
            {
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);

                // Check direct descendants
                if (getApiTypeOfDirectDescendants(mDagPath) != MFn.Type.kUnknown)
                {
                    return true;
                }

                dagIterator.next();
            }

            // No relevant node found among descendants
            return false;
        }

        /// <summary>
        /// Return the type of the direct descendants of the node
        /// </summary>
        /// <param name="mDagPath"></param>
        /// <returns> Return Mesh, Camera, Light or Unknown</returns>
        private MFn.Type getApiTypeOfDirectDescendants(MDagPath mDagPath)
        {
            for (uint i = 0; i < mDagPath.childCount; i++)
            {
                MObject childObject = mDagPath.child(i);
                MFnDagNode nodeObject = new MFnDagNode(childObject);

                switch (childObject.apiType)
                {
                    case MFn.Type.kMesh:
                        if (IsMeshExportable(nodeObject, mDagPath))
                        {
                            return MFn.Type.kMesh;
                        }
                        break;
                    case MFn.Type.kCamera:
                        if (IsCameraExportable(nodeObject, mDagPath))
                        {
                            return MFn.Type.kCamera;
                        }
                        break;
                }

                // Lights api type are kPointLight, kSpotLight...
                // Easier to check if has generic light function set rather than check all cases
                if (mDagPath.hasFn(MFn.Type.kLight) && IsLightExportable(nodeObject, mDagPath))
                {
                    // Return generic kLight api type
                    return MFn.Type.kLight;
                }

                // Locators
                if (mDagPath.hasFn(MFn.Type.kLocator) && IsNodeExportable(nodeObject, mDagPath))
                {
                    return MFn.Type.kLocator;
                }
            }

            return MFn.Type.kUnknown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isFull">If true all nodes are printed, otherwise only relevant ones</param>
        private void PrintDAG(bool isFull, MObject root = null)
        {
            var dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            if (root != null)
            {
                dagIterator.reset(root);
            }
            RaiseMessage("DAG: " + (isFull ? "full" : "relevant"));
            while (!dagIterator.isDone)
            {
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);

                if (isFull || isNodeRelevantToExportRec(mDagPath) || mDagPath.apiType == MFn.Type.kMesh || mDagPath.apiType == MFn.Type.kCamera || mDagPath.hasFn(MFn.Type.kLight) || mDagPath.apiType == MFn.Type.kLocator)
                {
                    RaiseMessage("name=" + mDagPath.partialPathName + "\t type=" + mDagPath.apiType, (int)dagIterator.depth + 1);
                }
                else
                {
                    dagIterator.prune();
                }
                dagIterator.next();
            }
        }
    }
}
