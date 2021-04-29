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
        // Expose this entrypoint to MaxScript, as it does not seem to support C# optional parameters 
        public static void Export()
        {
            Export(false);
        }
        public static void Export( bool logInListener )
        {
            string storedModelPath = Loader.Core.RootNode.GetStringProperty(MaxExportParameters.ModelFilePathProperty, string.Empty);
            string userRelativePath = Tools.ResolveRelativePath(storedModelPath);
            Export(InitParameters(userRelativePath),logInListener);
        }

        // Expose this entrypoint to MaxScript, as it does not seem to support C# optional parameters 
        public static void Export(string outputPath)
        {
            Export(outputPath, false);
        }
        public static void Export(string outputPath, bool logInListener )
        {
            Export(InitParameters(outputPath),logInListener);
        }

        // Expose this entrypoint to MaxScript, as it does not seem to support C# optional parameters 
        public static void Export(MaxExportParameters exportParameters)
        {
            Export(exportParameters, false); 
        }

        public static void Export(MaxExportParameters exportParameters, bool logInListener )
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

            if (logInListener)
            {
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
                    Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(message + "\n");
                };
                exporter.OnVerbose += (message, color, rank, emphasis) =>
                {
                    Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(message + "\n");
                };
            }

            // Start export
            exporter.Export(exportParameters);
        }


        //leave the possibility to do get the outputh path from the babylon exporter with all the settings as presaved
        public static MaxExportParameters InitParameters(string outputPath = null)
        {
            long txtQuality = 100;
            float scaleFactor = 1f;
            MaxExportParameters exportParameters = new MaxExportParameters();
            var ext = Path.GetExtension(outputPath);

            exportParameters.outputPath = outputPath;
            exportParameters.outputFormat = string.IsNullOrEmpty(ext)? null : ext.Substring(1);
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
            exportParameters.mergeAO = Loader.Core.RootNode.GetBoolProperty("babylonjs_mergeAOwithMR");
            exportParameters.dracoCompression = Loader.Core.RootNode.GetBoolProperty("babylonjs_dracoCompression");
            exportParameters.enableKHRLightsPunctual = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrLightsPunctual");
            exportParameters.enableKHRTextureTransform = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrTextureTransform");
            exportParameters.enableKHRMaterialsUnlit = Loader.Core.RootNode.GetBoolProperty("babylonjs_khr_materials_unlit");
            exportParameters.animgroupExportNonAnimated = Loader.Core.RootNode.GetBoolProperty("babylonjs_animgroupexportnonanimated");
            exportParameters.optimizeAnimations = !Loader.Core.RootNode.GetBoolProperty("babylonjs_donotoptimizeanimations");
            exportParameters.exportMaterials = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_materials");
            exportParameters.exportAnimations = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_animations");
            exportParameters.exportAnimationsOnly = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_animations_only");

            exportParameters.exportMorphTangents = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_Morph_Tangents");
            exportParameters.exportMorphNormals = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_Morph_Normals");
            exportParameters.usePreExportProcess = Loader.Core.RootNode.GetBoolProperty("babylonjs_preproces");
            exportParameters.flattenScene = Loader.Core.RootNode.GetBoolProperty("babylonjs_flattenScene");
            exportParameters.mergeContainersAndXRef = Loader.Core.RootNode.GetBoolProperty("babylonjs_mergecontainersandxref");
            exportParameters.bakeAnimationType = (BakeAnimationType) Loader.Core.RootNode.GetFloatProperty("babylonjs_bakeAnimationsType", 0);

            exportParameters.pbrFull = Loader.Core.RootNode.GetBoolProperty(ExportParameters.PBRFullPropertyName);
            exportParameters.pbrNoLight = Loader.Core.RootNode.GetBoolProperty(ExportParameters.PBRNoLightPropertyName);
            exportParameters.pbrEnvironment = Loader.Core.RootNode.GetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, string.Empty);
            exportParameters.exportNode = null;
            return exportParameters;
        }

        public static void DisableBabylonAutoSave()
        {
            Loader.Core.RootNode.SetUserPropBool("babylonjs_autosave",false);
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

        public int GetTimeRange(AnimationGroup info)
        {
            return Tools.CalculateEndFrameFromAnimationGroupNodes(info);
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
