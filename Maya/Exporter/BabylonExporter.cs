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
        
    }
}
