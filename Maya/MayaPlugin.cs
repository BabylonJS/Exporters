using Autodesk.Maya.OpenMaya;
using Maya2Babylon.Forms;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: MPxCommandClass(typeof(Maya2Babylon.toBabylon), "toBabylon")]
[assembly: ExtensionPlugin(typeof(Maya2Babylon.MayaPlugin), "Any")]

namespace Maya2Babylon
{
    public class MayaPlugin : IExtensionPlugin
    {
        bool IExtensionPlugin.InitializePlugin()
        {
            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
            return true;
        }

        string IExtensionPlugin.GetMayaDotNetSdkBuildVersion()
        {
            String version = "20171207";
            return version;
        }
    }

    public class toBabylon : MPxCommand, IMPxCommand
    {
        /// <summary>
        /// Entry point of the plug in
        /// Write "toBabylon" in the Maya console to start it
        /// </summary>
        /// <param name="argl"></param>
        public override void doIt(MArgList argl)
        {
            MGlobal.displayInfo("Start Maya Plugin\n");
            ExporterForm BabylonExport = new ExporterForm();
            BabylonExport.Show();
            BabylonExport.BringToFront();
            BabylonExport.WindowState = FormWindowState.Normal;
            // DoExport();
        }

        private bool DoExport()
        {
            BabylonExporter babylonExporter = new BabylonExporter();

            // Display logs to console
            // TODO - Display logs to custom frame with ranks
            babylonExporter.OnWarning += (warning, rank) =>
            {
                MGlobal.displayInfo("[WARNING] " + warning);
            };

            babylonExporter.OnError += (error, rank) =>
            {
                MGlobal.displayInfo("[ERROR] " + error);
            };

            babylonExporter.OnMessage += (message, color, rank, emphasis) =>
            {
                MGlobal.displayInfo("[MESSAGE] " + message);
            };

            // For debug purpose
            babylonExporter.OnVerbose += (message, color, rank, emphasis) =>
            {
                MGlobal.displayInfo("[VERBOSE] " + message);
            };

            babylonExporter.OnExportProgressChanged += (progress) =>
            {
                // TODO - Add progress bar
                //progressBar.Value = progress;
                //Application.DoEvents();
                MGlobal.displayInfo("[PROGRESS] " + progress + "%");
            };

            // TODO - Retreive export parameters
            string outputDirectory = @"C:\";
            string outputFileName = "MyFirstExportToBabylon";
            string outputFormat = "babylon";
            bool generateManifest = false;
            bool onlySelected = false;
            bool autoSaveMayaFile = false;
            bool exportHiddenObjects = false;
            bool copyTexturesToOutput = true;
            
            bool success = true;
            try
            {
                babylonExporter.Export(outputDirectory, outputFileName, outputFormat, generateManifest, onlySelected, autoSaveMayaFile, exportHiddenObjects, copyTexturesToOutput);
            }
            catch (OperationCanceledException)
            {
                success = false;
            }
            catch (Exception ex)
            {
                MGlobal.displayInfo("Exportation cancelled: " + ex.Message);
                success = false;
            }
            return success;
        }
    }
}
