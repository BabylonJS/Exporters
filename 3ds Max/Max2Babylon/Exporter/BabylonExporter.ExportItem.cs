using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utilities;

namespace Max2Babylon
{
    public class ExportItem
    {
        public bool IsDirty { get; private set; } = true;


        private Guid itemGuid;

        /*item guid is the same of node
        except when an exportItem is using the rootNode*/
        public Guid ItemGuid
        {
            get
            {
                if (Node != null && !Node.IsRootNode)
                {
                    return Node.GetGuid();
                }
                return itemGuid;
            }
        }

        public uint NodeHandle
        {
            get { return exportNodeHandle; }
            set
            {
                // if the handle is equal, return early so isdirty is not touched
                if (exportNodeHandle == value)
                    return;

                if (value == Loader.Core.RootNode.Handle)
                {
                    exportNode = Loader.Core.RootNode;
                    NodeName = "<SceneRoot>";
                }
                else
                {
                    exportNode = Loader.Core.RootNode.FindChildNode(value);
                    if (exportNode != null) NodeName = exportNode.NodeName;
                }

                IsDirty = true;
                exportNodeHandle = value;
            }
        }
        
        public string ExportFilePathRelative
        {
            get { return exportPathRelative; }
        }
        
        public string ExportFilePathAbsolute
        {
            get { return exportPathAbsolute; }
        }

        public string ExportTexturesesFolderRelative
        {
            get { return exportTexturesFolderRelative; }
        }

        public string ExportTexturesesFolderAbsolute
        {
            get { return exportTexturesFolderAbsolute; }
        }

        public string NodeName { get; private set; } = "<Invalid>";

        public IINode Node
        {
            get
            {
                return exportNode;
            }
        }

        public List<IILayer> Layers
        {
            get
            {
                return exportLayers;
            }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value) return;
                selected = value;
                IsDirty = true;
            }
        }
        public bool KeepPosition
        {
            get { return keepPosition; }
            set
            {
                if (keepPosition == value) return;
                keepPosition = value;
                IsDirty = true;
            }
        }

         const char s_PropertySeparator = ';';
        private const char s_ProperyLayerSeparator = '~';
        const string s_PropertyFormat = "{0};{1};{2};{3};{4}";
        const string s_PropertyNamePrefix = "babylonjs_ExportItem";

        private string outputFileExt;
        private IINode exportNode;
        private List<IILayer> exportLayers;
        private uint exportNodeHandle = uint.MaxValue; // 0 is the scene root node
        private bool selected = true;
        private bool keepPosition = false;
        private string exportPathRelative = "";
        private string exportPathAbsolute = "";
        private string exportTexturesFolderRelative = "";
        private string exportTexturesFolderAbsolute = "";

        public ExportItem(string outputFileExt)
        {
            this.outputFileExt = outputFileExt;
        }

        public ExportItem(string outputFileExt, uint nodeHandle)
        {
            this.outputFileExt = outputFileExt;
            NodeHandle = nodeHandle;
            
            if (nodeHandle == Loader.Core.RootNode.Handle)
            {
                string fileNameNoExt = Path.GetFileNameWithoutExtension(Loader.Core.CurFileName);
                SetExportFilePath(string.IsNullOrEmpty(fileNameNoExt) ? "Untitled" : fileNameNoExt);
            }
            else
            {
                SetExportFilePath(NodeName);
            }
            SetExportTexturesFolderPath(null);
        }

        public ExportItem(List<IILayer> layers)
        {
            itemGuid = Guid.NewGuid();
            SetExportLayers(layers); 
            IsDirty = true;
        }

        public ExportItem(string outputFileExt, string propertyName)
        {
            this.outputFileExt = outputFileExt;
            LoadFromData(propertyName);
        }

        public override string ToString() { return $"{selected} | {keepPosition} | { NodeName} | \"{exportPathRelative}\" | \"{ exportTexturesFolderRelative}\" | { LayersToString(exportLayers)}"; }

        public void SetExportLayers(List<IILayer> layers)
        {
            NodeHandle = 0;
            exportLayers = layers;
            IsDirty = true;
        }

        public void SetExportFilePath(string filePath)
        {
            string dirName = Loader.Core.CurFilePath;
            if (string.IsNullOrEmpty(Loader.Core.CurFilePath))
            {
                dirName = Loader.Core.GetDir((int)MaxDirectory.ProjectFolder);
            }
            else
            {
                dirName = Path.GetDirectoryName(dirName);
            }

            if (dirName.Last() != Path.AltDirectorySeparatorChar || dirName.Last() != Path.DirectorySeparatorChar)
            {
                dirName += Path.DirectorySeparatorChar;
            }

            string absolutePath;
            string relativePath;

            filePath = Path.ChangeExtension(filePath, outputFileExt);

            if (string.IsNullOrWhiteSpace(filePath)) // empty path
            {
                absolutePath = string.Empty;
                relativePath = string.Empty;
            }
            else if (Path.IsPathRooted(filePath)) // absolute path
            {
                absolutePath = Path.GetFullPath(filePath);
                relativePath = PathUtilities.GetRelativePath(dirName, filePath);
            }
            else // relative path
            {
                absolutePath = Path.GetFullPath(Path.Combine(dirName, filePath));
                relativePath = PathUtilities.GetRelativePath(dirName, absolutePath);

                exportPathRelative = relativePath;
            }

            // set absolute path (it may be different even if the relative path is equal, if the root dir changes for some reason)
            exportPathAbsolute = absolutePath;
            if (exportPathRelative.Equals(relativePath))
                return;
            exportPathRelative = relativePath;
            
            IsDirty = true;
        }

        public void SetExportTexturesFolderPath(string textureFolderPath)
        {
            string dirName = Loader.Core.CurFilePath;
            if (string.IsNullOrEmpty(Loader.Core.CurFilePath))
            {
                dirName = Loader.Core.GetDir((int)MaxDirectory.ProjectFolder);
            }
            else
            {
                dirName = Path.GetDirectoryName(dirName);
            }

            if (dirName.Last() != Path.AltDirectorySeparatorChar || dirName.Last() != Path.DirectorySeparatorChar)
            {
                dirName += Path.DirectorySeparatorChar;
            }

            string absolutePath;
            string relativePath;

            if (string.IsNullOrWhiteSpace(textureFolderPath)) // empty path
            {
                absolutePath = string.Empty;
                relativePath = string.Empty;
            }
            else if (Path.IsPathRooted(textureFolderPath)) // absolute path
            {
                absolutePath = Path.GetFullPath(textureFolderPath);
                relativePath = PathUtilities.GetRelativePath(dirName, textureFolderPath);
            }
            else // relative path
            {
                absolutePath = Path.GetFullPath(Path.Combine(dirName, textureFolderPath));
                relativePath = PathUtilities.GetRelativePath(dirName, absolutePath);

                exportTexturesFolderRelative = relativePath;
            }

            // set absolute path (it may be different even if the relative path is equal, if the root dir changes for some reason)
            exportTexturesFolderAbsolute = absolutePath;
            if (exportTexturesFolderRelative.Equals(relativePath))
                return;
            exportTexturesFolderRelative = relativePath;

            IsDirty = true;
        }

        #region Serialization

        public string GetPropertyName()
        {
            return s_PropertyNamePrefix + ItemGuid;
        }

        public void LoadFromData(string propertyName)
        {
            if (!propertyName.StartsWith(s_PropertyNamePrefix))
            {
                throw new Exception("Invalid property name, can't deserialize.");
            }
       
            
            string itemGuidStr = propertyName.Remove(0, s_PropertyNamePrefix.Length);


            if (!Guid.TryParse(itemGuidStr, out itemGuid))
            {   
                Loader.Core.PushPrompt("Error: Invalid ID, can't deserialize.");
                return;
            }

            IINode iNode = Tools.GetINodeByGuid(itemGuid);
            if (iNode == null)
            {
                iNode = Loader.Core.RootNode;
            }

            // set dirty explicitly just before we start loading, set to false when loading is done
            // if any exception is thrown, it will have a correct value
            IsDirty = true;

            NodeHandle = iNode.Handle;

            string propertiesString = string.Empty;
            if (!iNode.GetUserPropString(propertyName, ref propertiesString))
                return; // node has no properties yet

            string[] properties = propertiesString.Split(s_PropertySeparator);
            if (properties.Length < 2)
                throw new Exception("Invalid number of properties, can't deserialize.");

            var i = 0;
            if (!bool.TryParse(properties[i], out selected))
                throw new Exception(string.Format("Failed to parse selected property from string {0}", properties[i]));
            
            if (!bool.TryParse(properties[++i], out keepPosition))
                throw new Exception(string.Format("Failed to parse selected property from string {0}", properties[i]));

            SetExportFilePath(properties[++i]);
            SetExportTexturesFolderPath(properties[++i]);
            List<IILayer> layers = StringToLayers(properties[++i]);

            if (layers.Count > 0)
            {
                SetExportLayers(layers);
            }
            IsDirty = false;
        }

        public bool SaveToData()
        {
            // ' ' and '=' are not allowed by max, ';' is our data separator
            if (exportPathRelative.Contains(' ') || exportPathRelative.Contains('='))
                throw new FormatException("Invalid character(s) in export path: " + exportPathRelative + ". Spaces and equal signs are not allowed.");

            if (exportTexturesFolderRelative.Contains(' ') || exportTexturesFolderRelative.Contains('='))
                throw new FormatException("Invalid character(s) in export path: " + exportTexturesFolderRelative + ". Spaces and equal signs are not allowed.");

            IINode node = Node;
            if (node == null) return false;

            node.SetStringProperty(GetPropertyName(), string.Format(s_PropertyFormat, selected.ToString(), keepPosition.ToString(), exportPathRelative,exportTexturesFolderRelative,LayersToString(exportLayers)));

            IsDirty = false;
            return true;
        }

        public void DeleteFromData()
        {
            IINode node = Node;
            if (node == null) return;
            node.DeleteProperty(GetPropertyName());
            IsDirty = true;
        }

        public string LayersToString( List<IILayer> layers)
        {
            string result = string.Empty;
            if (layers == null) return result;
            List<string> layersName = new List<string>();

            foreach (IILayer iLayer in layers)
            {
                if(iLayer == null) continue;
                layersName.Add(iLayer.Name);
            }

            result = string.Join(s_ProperyLayerSeparator.ToString(), layersName);
            return result;
        }

        public List<IILayer> StringToLayers(string layersProperties)
        {
            List<IILayer> layers = new List<IILayer>();
            if (!string.IsNullOrEmpty(layersProperties))
            {
                string[] layerNames = layersProperties.Split(s_ProperyLayerSeparator);

                foreach (string layerName in layerNames)
                {
                    IILayer l = Loader.Core.LayerManager.GetLayer(layerName);
                    if (l != null)
                    {
                        layers.Add(l);
                    }
                }
            }
            return layers;
        }

        #endregion
    }
    public class ExportItemList : List<ExportItem>
    {
        const string s_ExportItemListPropertyName = "babylonjs_ExportItemList";

        public string OutputFileExtension { get; private set; }

        public ExportItemList(string outputFileExt)
        {
            this.OutputFileExtension = outputFileExt;
        }

        public void LoadFromData()
        {
            string[] exportItemPropertyNames = Loader.Core.RootNode.GetStringArrayProperty(s_ExportItemListPropertyName);

            if (Capacity < exportItemPropertyNames.Length)
                Capacity = exportItemPropertyNames.Length;

            foreach (string propertyNameStr in exportItemPropertyNames)
            {
                ExportItem info = new ExportItem(OutputFileExtension, propertyNameStr);
                if(info.Node != null)
					Add(info);
            }
        }

        public void SaveToData()
        {
            List<string> exportItemPropertyNameList = new List<string>();
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].IsDirty)
                {
                    if (!this[i].SaveToData())
                    {
                        RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        exportItemPropertyNameList.Add(this[i].GetPropertyName());
                    }
                }
                else exportItemPropertyNameList.Add(this[i].GetPropertyName());
            }

            Loader.Core.RootNode.SetStringArrayProperty(s_ExportItemListPropertyName, exportItemPropertyNameList);
        }

        public void DeleteFromData()
        {
            Loader.Core.RootNode.DeleteProperty(s_ExportItemListPropertyName);
        }
    }
}
