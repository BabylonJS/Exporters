using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BabylonExport.Entities;
using Utilities;
using Autodesk.Max;

namespace Max2Babylon
{
    public class MaxScriptManager
    {
        public static void Export()
        {
            string storedModelPath = Loader.Core.RootNode.GetStringProperty(ExportParameters.ModelFilePathProperty, string.Empty);
            string userRelativePath = Tools.ResolveRelativePath(storedModelPath);
            Export(InitParameters(userRelativePath));
        }

        public static void Export(string outputPath)
        {
            Export(InitParameters(outputPath));
        }

        public static void Export(MaxExportParameters exportParameters)
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
        public static MaxExportParameters InitParameters(string outputPath)
        {
            long txtQuality = 100;
            float scaleFactor = 1f;
            MaxExportParameters exportParameters = new MaxExportParameters();
            exportParameters.outputPath = outputPath;
            exportParameters.outputFormat = Path.GetExtension(outputPath)?.Substring(1);
            exportParameters.textureFolder = Loader.Core.RootNode.GetStringProperty("textureFolderPathProperty", string.Empty);
            exportParameters.generateManifest = Loader.Core.RootNode.GetBoolProperty("babylonjs_generatemanifest");
            exportParameters.writeTextures = Loader.Core.RootNode.GetBoolProperty("babylonjs_writetextures");
            exportParameters.overwriteTextures = Loader.Core.RootNode.GetBoolProperty("babylonjs_overwritetextures");
            exportParameters.exportHiddenObjects = Loader.Core.RootNode.GetBoolProperty("babylonjs_exporthidden");
            exportParameters.autoSaveSceneFile = Loader.Core.RootNode.GetBoolProperty("babylonjs_autosave");
            exportParameters.exportOnlySelected = Loader.Core.RootNode.GetBoolProperty("babylonjs_onlySelected");
            exportParameters.exportTangents = Loader.Core.RootNode.GetBoolProperty("babylonjs_exporttangents");
            exportParameters.scaleFactor = float.TryParse(Loader.Core.RootNode.GetStringProperty("babylonjs_txtScaleFactor", "1"), out scaleFactor) ? scaleFactor : 1;
            exportParameters.txtQuality = long.TryParse(Loader.Core.RootNode.GetStringProperty("babylonjs_txtCompression", "100"), out txtQuality) ? txtQuality : 100;
            exportParameters.mergeAOwithMR = Loader.Core.RootNode.GetBoolProperty("babylonjs_mergeAOwithMR");
            exportParameters.dracoCompression = Loader.Core.RootNode.GetBoolProperty("babylonjs_dracoCompression");
            exportParameters.enableKHRLightsPunctual = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrLightsPunctual");
            exportParameters.enableKHRTextureTransform = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrTextureTransform");
            exportParameters.enableKHRMaterialsUnlit = Loader.Core.RootNode.GetBoolProperty("babylonjs_khr_materials_unlit");
            exportParameters.exportMaterials = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_materials");

            exportParameters.pbrFull = Loader.Core.RootNode.GetBoolProperty(ExportParameters.PBRFullPropertyName);
            exportParameters.pbrNoLight = Loader.Core.RootNode.GetBoolProperty(ExportParameters.PBRNoLightPropertyName);
            exportParameters.pbrEnvironment = Loader.Core.RootNode.GetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, string.Empty);
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

        public static void MergeAnimationGroups(string jsonPath, string old_root, string new_root)
        {
            AnimationGroupList animationGroups = new AnimationGroupList();
            var fileStream = File.Open(jsonPath, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string jsonContent = reader.ReadToEnd();
                string textToFind = string.Format(@"\b{0}\b", old_root);
                string overridedJsonContent = Regex.Replace(jsonContent, textToFind, new_root);
                animationGroups.LoadFromJson(overridedJsonContent, true);
            }
        }

        public AnimationGroup GetAnimationGroupByName(string name)
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            foreach (AnimationGroup animationGroup in animationGroupList)
            {
                if (animationGroup.Name == name)
                {
                    return animationGroup;
                }
            }

            return null;
        }

        public AnimationGroup CreateAnimationGroup()
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            AnimationGroup info = new AnimationGroup();

            // get a unique name and guid
            string baseName = info.Name;
            int i = 0;
            bool hasConflict = true;
            while (hasConflict)
            {
                hasConflict = false;
                foreach (AnimationGroup animationGroup in animationGroupList)
                {
                    if (info.Name.Equals(animationGroup.Name))
                    {
                        info.Name = baseName + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                    if (info.SerializedId.Equals(animationGroup.SerializedId))
                    {
                        info.SerializedId = Guid.NewGuid();
                        hasConflict = true;
                        break;
                    }
                }
            }

            // save info and animation list entry
            animationGroupList.Add(info);
            animationGroupList.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);
            return info;
        }

        public string RenameAnimationGroup(AnimationGroup info,string name)
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            AnimationGroup animGroupToRename = animationGroupList.GetAnimationGroupByName(info.Name);

            string baseName = name;
            int i = 0;
            bool hasConflict = true;
            while (hasConflict)
            {
                hasConflict = false;
                foreach (AnimationGroup animationGroup in animationGroupList)
                {
                    if (baseName.Equals(animationGroup.Name))
                    {
                        baseName = name + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                }
            }

            animGroupToRename.Name = baseName;

            // save info and animation list entry
            animationGroupList.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);
            return baseName;
        }

        public void AddNodeInAnimationGroup(AnimationGroup info, uint nodeHandle)
        {
            if (info == null)
                return;

            IINode node = Loader.Core.GetINodeByHandle(nodeHandle);
            if (node == null)
            {
                return;
            }

            List<Guid> newGuids = info.NodeGuids.ToList();
            newGuids.Add(node.GetGuid());
            info.NodeGuids = newGuids;
            info.SaveToData();
        }

        public void SetAnimationGroupTimeRange(AnimationGroup info, int start,int end)
        {
            if (info == null)
                return;

            info.FrameStart = start;
            info.FrameEnd = end;
            info.SaveToData();
        }

        public void RemoveAllNodeFromAnimationGroup(AnimationGroup info)
        {
            if (info == null)
                return;

            info.NodeGuids = new List<Guid>();
            info.SaveToData();
        }

        public void RemoveNodeFromAnimationGroup(AnimationGroup info, uint nodeHandle)
        {
            if (info == null)
                return;

            IINode node = Loader.Core.GetINodeByHandle(nodeHandle);
            if (node == null)
            {
                return;
            }

            List<Guid> newGuids = info.NodeGuids.ToList();
            newGuids.Remove(node.GetGuid());
            info.NodeGuids = newGuids;
            info.SaveToData();
        }
    }
}
