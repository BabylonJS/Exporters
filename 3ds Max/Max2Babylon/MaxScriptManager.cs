using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Max2Babylon
{
    public class MaxScriptManager
    {
        public static void Export()
        {
            string outputPath = Tools.ResolveRelativePath(Loader.Core.RootNode.GetLocalData());
            Export(InitParameters(outputPath));
        }

        public static void Export(string outputPath)
        {
            Export(InitParameters(outputPath));
        }

        public static void Export(ExportParameters exportParameters)
        {
            if (Loader.Class_ID == null)
            {
                Loader.AssemblyMain();
            }
            // Check output format is valid
            List<string> validFormats = new List<string>(new string[] { "babylon", "binary babylon", "gltf", "glb" });
            if (!validFormats.Contains(exportParameters.outputFormat))
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf("ERROR - Valid output formats are: "+ validFormats.ToArray().ToString(true) + "\n");
                return;
            }

            BabylonExporter exporter = new BabylonExporter();

            // Init log system
            exporter.OnWarning += (warning, rank) =>
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(warning+"\n");
            };
            exporter.OnError += (error, rank) =>
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(error + "\n");
            };
            exporter.OnMessage += (message, color, rank, emphasis) =>
            {
                // TODO - Add a log level parameter (Error, Warning, Message, Verbose)
                if (rank < 1)
                {
                    Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(message + "\n");
                }
            };
            
            // Start export
            exporter.Export(exportParameters);
        }


        //leave the possibility to do get the outputh path from the babylon exporter with all the settings as presaved
        public static ExportParameters InitParameters(string outputPath)
        {
            ExportParameters exportParameters = new ExportParameters();
            exportParameters.outputPath = outputPath;
            exportParameters.outputFormat = Path.GetExtension(outputPath)?.Substring(1);
            exportParameters.generateManifest = Loader.Core.RootNode.GetBoolProperty("babylonjs_generatemanifest");
            exportParameters.writeTextures = Loader.Core.RootNode.GetBoolProperty("babylonjs_writetextures");
            exportParameters.overwriteTextures = Loader.Core.RootNode.GetBoolProperty("babylonjs_overwritetextures");
            exportParameters.exportHiddenObjects = Loader.Core.RootNode.GetBoolProperty("babylonjs_exporthidden");
            exportParameters.autoSave3dsMaxFile = Loader.Core.RootNode.GetBoolProperty("babylonjs_autosave");
            exportParameters.exportOnlySelected = Loader.Core.RootNode.GetBoolProperty("babylonjs_onlySelected");
            exportParameters.exportTangents = Loader.Core.RootNode.GetBoolProperty("babylonjs_exporttangents");
            exportParameters.scaleFactor = Loader.Core.RootNode.GetStringProperty("babylonjs_txtScaleFactor", "1");
            exportParameters.txtQuality = Loader.Core.RootNode.GetStringProperty("babylonjs_txtCompression", "100");
            exportParameters.mergeAOwithMR = Loader.Core.RootNode.GetBoolProperty("babylonjs_mergeAOwithMR");
            exportParameters.dracoCompression = Loader.Core.RootNode.GetBoolProperty("babylonjs_dracoCompression");
            exportParameters.enableKHRLightsPunctual = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrLightsPunctual");
            exportParameters.enableKHRTextureTransform = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrTextureTransform");
            exportParameters.enableKHRMaterialsUnlit = Loader.Core.RootNode.GetBoolProperty("babylonjs_khr_materials_unlit");
            exportParameters.exportMaterials = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_materials");
            return exportParameters;
        }

        public static void ImportAnimationGroups(string jsonPath)
        {
            AnimationGroupList animationGroups = new AnimationGroupList();
            var fileStream = File.Open(jsonPath, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string jsonContent = reader.ReadToEnd();
                animationGroups.LoadFromJson(jsonContent);
            }
        }

        public static void MergeAnimationGroups(string jsonPath)
        {
            AnimationGroupList animationGroups = new AnimationGroupList();
            var fileStream = File.Open(jsonPath, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string jsonContent = reader.ReadToEnd();
                animationGroups.LoadFromJson(jsonContent,true);
            }
        }

    }
}
