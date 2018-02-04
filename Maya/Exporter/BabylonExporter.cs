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
        private bool ExportHiddenObjects { get; set; }
        private bool CopyTexturesToOutput { get; set; }
        private bool ExportQuaternionsInsteadOfEulers { get; set; }
        private bool isBabylonExported;

        public bool IsCancelled { get; set; }

        // Custom properties
        private bool _exportQuaternionsInsteadOfEulers;

        // TODO - Check if it's ok for other languages
        /// <summary>
        /// Names of the default cameras used as viewports in Maya in English language
        /// Those cameras are always ignored when exporting (ie even when exporting hidden objects)
        /// </summary>
        private static List<string> defaultCameraNames = new List<string>(new string[] { "persp", "top", "front", "side" });

        public void Export(string outputDirectory, string outputFileName, string outputFormat, bool generateManifest, bool onlySelected, bool autoSaveMayaFile, bool exportHiddenObjects, bool copyTexturesToOutput)
        {
            RaiseMessage("Exportation started", Color.Blue);
            var progression = 0.0f;
            ReportProgressChanged(progression);

            // Store export options
            _onlySelected = onlySelected;
            _exportHiddenObjects = exportHiddenObjects;
            CopyTexturesToOutput = copyTexturesToOutput;
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
                mItSelectionList.getDagPath(mDagPath);
                selectedNodeFullPaths.Add(mDagPath.fullPathName);

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

            PrintDAG(true);
            PrintDAG(false);

            // --------------------
            // ------ Nodes -------
            // --------------------
            RaiseMessage("Exporting nodes");

            // Get all nodes
            var dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst, MFn.Type.kTransform);
            List<MDagPath> nodes = new List<MDagPath>();
            while (!dagIterator.isDone)
            {
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);
                
                // Check if one of its descendant (direct or not) is a mesh/camera/light
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
                RaiseWarning("A default hemispheric light was added for your convenience", 1);
                ExportDefaultLight(babylonScene);
            }
            else
            {
                RaiseMessage(string.Format("Total lights: {0}", babylonScene.LightsList.Count), Color.Gray, 1);
            }

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

            ReportProgressChanged(100);

            // Export glTF
            if (outputFormat == "gltf" || outputFormat == "glb")
            {
                bool generateBinary = outputFormat == "glb";
                ExportGltf(babylonScene, outputDirectory, outputFileName, generateBinary);
            }

            watch.Stop();
            RaiseMessage(string.Format("Exportation done in {0:0.00}s", watch.ElapsedMilliseconds / 1000.0), Color.Blue);
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
        /// Return true if node descendant hierarchy has any exportable Mesh, Camera or Light
        /// </summary>
        private bool isNodeRelevantToExportRec(MDagPath mDagPathRoot)
        {
            var mIteratorType = new MIteratorType();
            MIntArray listOfFilters = new MIntArray();
            listOfFilters.Add((int)MFn.Type.kMesh);
            listOfFilters.Add((int)MFn.Type.kCamera);
            listOfFilters.Add((int)MFn.Type.kLight);
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
            }

            return MFn.Type.kUnknown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isFull">If true all nodes are printed, otherwise only relevant ones</param>
        private void PrintDAG(bool isFull)
        {
            var dagIterator = new MItDag(MItDag.TraversalType.kDepthFirst);
            RaiseMessage("DAG: " + (isFull ? "full" : "relevant"));
            while (!dagIterator.isDone)
            {
                MDagPath mDagPath = new MDagPath();
                dagIterator.getPath(mDagPath);

                if (isFull || isNodeRelevantToExportRec(mDagPath) || mDagPath.apiType == MFn.Type.kMesh || mDagPath.apiType == MFn.Type.kCamera || mDagPath.hasFn(MFn.Type.kLight))
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
