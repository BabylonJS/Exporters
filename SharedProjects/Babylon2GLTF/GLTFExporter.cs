using BabylonExport.Entities;
using GLTFExport.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using Color = System.Drawing.Color;
using System.Linq;
using System.Diagnostics;
using Utilities;

namespace Babylon2GLTF
{
    internal partial class GLTFExporter
    {
        List<BabylonMaterial> babylonMaterialsToExport;
        ExportParameters exportParameters;

        private List<BabylonNode> babylonNodes;
        private BabylonScene babylonScene;

        ILoggingProvider logger;

        // from BabylonNode to GLTFNode
        Dictionary<BabylonNode, GLTFNode> nodeToGltfNodeMap;

        public void ExportGltf(ExportParameters exportParameters, BabylonScene babylonScene, string outputDirectory, string outputFileName, bool generateBinary, ILoggingProvider logger)
        {
            this.exportParameters = exportParameters;
            this.logger = logger;

            logger.RaiseMessage("GLTFExporter | Exportation started", Color.Blue);
#if DEBUG
            var watch = new Stopwatch();
            watch.Start();
#endif
            this.babylonScene = babylonScene;

            // Force output file extension to be gltf
            outputFileName = Path.ChangeExtension(outputFileName, "gltf");

            // Update path of output .gltf file to include subdirectory
            var outputFile = Path.Combine(outputDirectory, outputFileName);

            float progressionStep;
            var progression = 0.0f;
            logger.ReportProgressChanged((int)progression);

            babylonMaterialsToExport = new List<BabylonMaterial>();

            var gltf = new GLTF(outputFile);

            // Asset
            gltf.asset = new GLTFAsset
            {
                version = "2.0"
                // no minVersion
            };

            var softwarePackageName = babylonScene.producer != null ? babylonScene.producer.name : "";
            var softwareVersion = babylonScene.producer != null ? babylonScene.producer.version : "";
            var exporterVersion = babylonScene.producer != null ? babylonScene.producer.exporter_version : "";

            gltf.asset.generator = $"babylon.js glTF exporter for {softwarePackageName} {softwareVersion} v{exporterVersion}";

            // Scene
            gltf.scene = 0;

            // Scenes
            GLTFScene scene = new GLTFScene();
            ExportGLTFExtension( babylonScene, ref scene,gltf);
            GLTFScene[] scenes = { scene };
            gltf.scenes = scenes;

            // Initialization
            initBabylonNodes(babylonScene,gltf);            
 

            // Root nodes
            logger.RaiseMessage("GLTFExporter | Exporting nodes");
            progression = 30.0f;
            logger.ReportProgressChanged((int)progression);
            List<BabylonNode> babylonRootNodes = babylonNodes.FindAll(node => node.parentId == null);
            progressionStep = 30.0f / babylonRootNodes.Count;
            alreadyExportedSkeletons = new Dictionary<BabylonSkeleton, BabylonSkeletonExportData>();
            nodeToGltfNodeMap = new Dictionary<BabylonNode, GLTFNode>();
            NbNodesByName = new Dictionary<string, int>();
            babylonRootNodes.ForEach(babylonNode =>
            {
                exportNodeRec(babylonNode, gltf, babylonScene);
                progression += progressionStep;
                logger.ReportProgressChanged((int)progression);
                logger.CheckCancelled();
            });
#if DEBUG
            var nodesExportTime = watch.ElapsedMilliseconds / 1000.0;
            logger.RaiseMessage(string.Format("GLTFNodes exported in {0:0.00}s", nodesExportTime), Color.Blue);
#endif


            // Meshes
            logger.RaiseMessage("GLTFExporter | Exporting meshes");
            progression = 10.0f;
            logger.ReportProgressChanged((int)progression);
            progressionStep = 40.0f / babylonScene.meshes.Length;
            foreach (var babylonMesh in babylonScene.meshes)
            {
                ExportMesh(babylonMesh, gltf, babylonScene);
                progression += progressionStep;
                logger.ReportProgressChanged((int)progression);
                logger.CheckCancelled();
            }
#if DEBUG
            var meshesExportTime = watch.ElapsedMilliseconds / 1000.0 - nodesExportTime;
            logger.RaiseMessage(string.Format("GLTFMeshes exported in {0:0.00}s", meshesExportTime), Color.Blue);
#endif
 
            //Mesh Skins, light and Cameras
            logger.RaiseMessage("GLTFExporter | Exporting skins, lights and cameras");
            progression = 50.0f;
            logger.ReportProgressChanged((int)progression);
            progressionStep = 50.0f / babylonRootNodes.Count;
            babylonRootNodes.ForEach(babylonNode =>
            {
                exportNodeTypeRec(babylonNode, gltf, babylonScene);
                progression += progressionStep;
                logger.ReportProgressChanged((int)progression);
                logger.CheckCancelled();
            });
#if DEBUG
            var skinLightCameraExportTime = watch.ElapsedMilliseconds / 1000.0 -meshesExportTime;
            logger.RaiseMessage(string.Format("GLTFSkin GLTFLights GLTFCameras exported in {0:0.00}s", skinLightCameraExportTime), Color.Blue);
#endif
            // Materials
            progression = 70.0f;
            logger.ReportProgressChanged((int)progression);
            logger.RaiseMessage("GLTFExporter | Exporting materials");
            foreach (var babylonMaterial in babylonMaterialsToExport)
            {
                ExportMaterial(babylonMaterial, gltf);
                logger.CheckCancelled();
            };
            logger.RaiseMessage(string.Format("GLTFExporter | Nb materials exported: {0}", gltf.MaterialsList.Count), Color.Gray, 1);
#if DEBUG
            var materialsExportTime = watch.ElapsedMilliseconds / 1000.0 -nodesExportTime;
            logger.RaiseMessage(string.Format("GLTFMaterials exported in {0:0.00}s", materialsExportTime), Color.Blue);
#endif
            // Animations
            progression = 90.0f;
            logger.ReportProgressChanged((int)progression);
            logger.RaiseMessage("GLTFExporter | Exporting Animations");
            ExportAnimationGroups(gltf, babylonScene);
#if DEBUG
            var animationGroupsExportTime = watch.ElapsedMilliseconds / 1000.0 -materialsExportTime;
            logger.RaiseMessage(string.Format("GLTFAnimations exported in {0:0.00}s", animationGroupsExportTime), Color.Blue);
#endif
            // Prepare buffers
            gltf.BuffersList.ForEach(buffer =>
            {
                buffer.BufferViews.ForEach(bufferView =>
                {
                    bufferView.Accessors.ForEach(accessor =>
                    {
                        // Chunk must be padded with trailing zeros (0x00) to satisfy alignment requirements
                        accessor.bytesList = new List<byte>(padChunk(accessor.bytesList.ToArray(), 4, 0x00));

                        // Update byte properties
                        accessor.byteOffset = bufferView.byteLength;
                        bufferView.byteLength += accessor.bytesList.Count;
                        // Merge bytes
                        bufferView.bytesList.AddRange(accessor.bytesList);
                    });
                    // Update byte properties
                    bufferView.byteOffset = buffer.byteLength;
                    buffer.byteLength += bufferView.bytesList.Count;
                    // Merge bytes
                    buffer.bytesList.AddRange(bufferView.bytesList);
                });
            });

            // Cast lists to arrays
            gltf.Prepare();

            // Output
            logger.RaiseMessage("GLTFExporter | Saving to output file");
            if (!generateBinary) {

                // Write .gltf file
                string outputGltfFile = Path.ChangeExtension(outputFile, "gltf");
                File.WriteAllText(outputGltfFile, gltfToJson(gltf));

                // Write .bin file
                string outputBinaryFile = Path.ChangeExtension(outputFile, "bin");
                using (BinaryWriter writer = new BinaryWriter(File.Open(outputBinaryFile, FileMode.Create)))
                {
                    gltf.BuffersList.ForEach(buffer =>
                    {
                        buffer.bytesList.ForEach(b => writer.Write(b));
                    });
                }
            }
            else
            {
                // Export glTF data to binary format .glb

                // Header
                UInt32 magic = 0x46546C67; // ASCII code for glTF
                UInt32 version = 2;
                UInt32 length = 12; // Header length

                // --- JSON chunk ---
                UInt32 chunkTypeJson = 0x4E4F534A; // ASCII code for JSON
                // Remove buffers uri
                foreach (GLTFBuffer gltfBuffer in gltf.BuffersList)
                {
                    gltfBuffer.uri = null;
                }
                // Switch images to binary
                gltf.Prepare();
                // Serialize gltf data to JSON string then convert it to bytes
                byte[] chunkDataJson = Encoding.ASCII.GetBytes(gltfToJson(gltf));
                // JSON chunk must be padded with trailing Space chars (0x20) to satisfy alignment requirements 
                chunkDataJson = padChunk(chunkDataJson, 4, 0x20);
                UInt32 chunkLengthJson = (UInt32)chunkDataJson.Length;
                length += chunkLengthJson + 8; // 8 = JSON chunk header length
                
                // bin chunk
                UInt32 chunkTypeBin = 0x004E4942; // ASCII code for BIN
                UInt32 chunkLengthBin = 0;
                if (gltf.BuffersList.Count > 0)
                {
                    foreach (GLTFBuffer gltfBuffer in gltf.BuffersList)
                    {
                        chunkLengthBin += (uint)gltfBuffer.byteLength;
                    }
                    length += chunkLengthBin + 8; // 8 = bin chunk header length
                }

                // Write binary file
                string outputGlbFile = Path.ChangeExtension(outputFile, "glb");
                using (BinaryWriter writer = new BinaryWriter(File.Open(outputGlbFile, FileMode.Create)))
                {
                    // Header
                    writer.Write(magic);
                    writer.Write(version);
                    writer.Write(length);
                    
                    // JSON chunk
                    writer.Write(chunkLengthJson);
                    writer.Write(chunkTypeJson);
                    writer.Write(chunkDataJson);

                    // bin chunk
                    if (gltf.BuffersList.Count > 0)
                    {
                        writer.Write(chunkLengthBin);
                        writer.Write(chunkTypeBin);
                        gltf.BuffersList[0].bytesList.ForEach(b => writer.Write(b));
                    }
                };
            }

            // Draco compression
            if(exportParameters.dracoCompression)
            {
                logger.RaiseMessage("GLTFExporter | Draco compression");
                GLTFPipelineUtilities.DoDracoCompression(logger, generateBinary, outputFile);
            }

            logger.ReportProgressChanged(100);
        }

        private List<BabylonNode> initBabylonNodes(BabylonScene babylonScene,GLTF gltf)
        {
            babylonNodes = new List<BabylonNode>();
            if (babylonScene.meshes != null)
            {
                int idGroupInstance = 0;
                foreach (var babylonMesh in babylonScene.meshes)
                {
                    var babylonAbstractMeshes = new List<BabylonAbstractMesh>();
                    babylonAbstractMeshes.Add(babylonMesh);
                    if (babylonMesh.instances != null)
                    {
                        babylonAbstractMeshes.AddRange(babylonMesh.instances);
                    }

                    // Add mesh and instances to node list
                    babylonNodes.AddRange(babylonAbstractMeshes);

                    // Tag mesh and instances with an identifier
                    babylonAbstractMeshes.ForEach(babylonAbstractMesh => babylonAbstractMesh.idGroupInstance = idGroupInstance);

                    idGroupInstance++;
                }
            }
            if (babylonScene.lights != null)
            {
                babylonNodes.AddRange(babylonScene.lights);
            }
            if (babylonScene.cameras != null)
            {
                babylonNodes.AddRange(babylonScene.cameras);
            }

            if (babylonScene.SkeletonsList != null)
            {
                foreach (BabylonSkeleton babylonSkeleton in babylonScene.SkeletonsList)
                {
                    foreach (BabylonBone babylonSkeletonBone in babylonSkeleton.bones)
                    {
                        if(!babylonNodes.Exists(x => x.id == babylonSkeletonBone.id))
                        {
                            babylonNodes.Add(BoneToNode(babylonSkeletonBone));
                        }
                    }
                   
                }
            }

            return babylonNodes;
        }

        private void exportNodeRec(BabylonNode babylonNode, GLTF gltf, BabylonScene babylonScene, GLTFNode gltfParentNode = null)
        {
            GLTFNode gltfNode = ExportNode(babylonNode, gltf, babylonScene, gltfParentNode);

            if (gltfNode != null)
            {
                logger.CheckCancelled();

                // export its tag
                if(!string.IsNullOrEmpty(babylonNode.tags))
                {
                    if (gltfNode.extras == null)
                    {
                        gltfNode.extras = new Dictionary<string, object>();
                    }
                    gltfNode.extras["tags"] = babylonNode.tags;
                }

                // ...export its children
                List<BabylonNode> babylonDescendants = getDescendants(babylonNode);
                babylonDescendants.ForEach(descendant => exportNodeRec(descendant, gltf, babylonScene, gltfNode));
            }
        }

        
        private void exportNodeTypeRec(BabylonNode babylonNode, GLTF gltf, BabylonScene babylonScene, GLTFNode gltfParentNode = null)
        {
            var type = babylonNode.GetType();
            logger.RaiseMessage($"GLTFExporter | ExportNode {babylonNode.name} of Type {type.ToString()}", 1);

            var nodeNodePair = nodeToGltfNodeMap.FirstOrDefault(pair => pair.Key.id.Equals(babylonNode.id));
            GLTFNode gltfNode = nodeNodePair.Value;

            if (gltfNode != null)
            {
                if (type == typeof(BabylonAbstractMesh) || type.IsSubclassOf(typeof(BabylonAbstractMesh)))
                {
                    gltfNode = ExportAbstractMesh(ref gltfNode, babylonNode as BabylonAbstractMesh, gltf, gltfParentNode, babylonScene);
                }
                else if (type == typeof(BabylonCamera))
                {
                    GLTFCamera gltfCamera = ExportCamera(ref gltfNode, babylonNode as BabylonCamera, gltf, gltfParentNode);
                }
                else if (type == typeof(BabylonLight) || type.IsSubclassOf(typeof(BabylonLight)))
                {
                    if(((BabylonLight)babylonNode).type != 3)
                    {
                        ExportLight(ref gltfNode, babylonNode as BabylonLight, gltf, gltfParentNode, babylonScene);
                    }
                }
                else if (type == typeof(BabylonNode))
                {
                    logger.RaiseVerbose($"Node named {babylonNode.name} already exported as gltfNode", 1);
                }
                else
                {
                    logger.RaiseError($"Node named {babylonNode.name} has no exporter", 1);
                }

                logger.CheckCancelled();

                // ...export its children
                List<BabylonNode> babylonDescendants = getDescendants(babylonNode);
                babylonDescendants.ForEach(descendant => exportNodeTypeRec(descendant, gltf, babylonScene, gltfNode));
            }
        }

        private List<BabylonNode> getDescendants(BabylonNode babylonNode)
        {
            return babylonNodes.FindAll(node => node.parentId == babylonNode.id);
        }

        /// <summary>
        /// Return true if node descendant hierarchy has any Mesh or Camera to export
        /// </summary>
        private bool isNodeRelevantToExport(BabylonNode babylonNode)
        {
            var type = babylonNode.GetType();
            if (type == typeof(BabylonAbstractMesh) ||
                type.IsSubclassOf(typeof(BabylonAbstractMesh)) ||
                type == typeof(BabylonCamera))
            {
                return true;
            }

            // Descandant recursivity
            List<BabylonNode> babylonDescendants = getDescendants(babylonNode);
            int indexDescendant = 0;
            while (indexDescendant < babylonDescendants.Count) // while instead of for to stop as soon as a relevant node has been found
            {
                if (isNodeRelevantToExport(babylonDescendants[indexDescendant]))
                {
                    return true;
                }
                indexDescendant++;
            }

            // No relevant node found in hierarchy
            return false;
        }

        private string gltfToJson(GLTF gltf)
        {
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings());
            var sb = new StringBuilder();
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            // Do not use the optimized writer because it's not necessary to truncate values
            // Use the bounded writer in case some values are infinity ()
            using (var jsonWriter = new JsonTextWriterBounded(sw))
            {
#if DEBUG
                jsonWriter.Formatting = Formatting.Indented;
#else
                jsonWriter.Formatting = Formatting.None;
#endif
                jsonSerializer.Serialize(jsonWriter, gltf);
            }
            return sb.ToString();
        }

        private byte[] padChunk(byte[] chunk, int padding, byte trailingChar)
        {
            var chunkModuloPadding = chunk.Length % padding;
            var nbCharacterToAdd = chunkModuloPadding == 0 ? 0 : (padding - chunkModuloPadding);
            var chunkList = new List<byte>(chunk);
            for (int i = 0; i < nbCharacterToAdd; i++)
            {
                chunkList.Add(trailingChar);
            }
            return chunkList.ToArray();
        }


        /// <summary>
        /// Create a gltf node from the babylon node.
        /// </summary>
        /// <param name="babylonNode"></param>
        /// <param name="gltf"></param>
        /// <param name="babylonScene"></param>
        /// <param name="gltfParentNode">The parent of the glTF node that will be created.</param>
        /// <returns>The gltf node created.</returns>
        private GLTFNode ExportNode(BabylonNode babylonNode, GLTF gltf, BabylonScene babylonScene, GLTFNode gltfParentNode)
        {
            logger.RaiseMessage($"GLTFExporter | ExportNode {babylonNode.name}", 1);
            GLTFNode gltfNode = null;
            var type = babylonNode.GetType();

            var nodeNodePair = nodeToGltfNodeMap.FirstOrDefault(pair => pair.Key.id.Equals(babylonNode.id));
            if (nodeNodePair.Key != null)
            {
                return nodeNodePair.Value;
            }

            // Node
            gltfNode = new GLTFNode
            {
                name = GetUniqueNodeName(babylonNode.name),
                index = gltf.NodesList.Count
            };

            // User Custom Attributes
            if (babylonNode.metadata != null && babylonNode.metadata.Count != 0)
            {
                gltfNode.extras = babylonNode.metadata;
            }

            gltf.NodesList.Add(gltfNode);   // add the node to the gltf list
            nodeToGltfNodeMap.Add(babylonNode, gltfNode);   // add the node to the global map

            // Hierarchy
            if (gltfParentNode != null)
            {
                logger.RaiseMessage("GLTFExporter.Node| Add " + babylonNode.name + " as child to " + gltfParentNode.name, 2);
                gltfParentNode.ChildrenList.Add(gltfNode.index);
                gltfNode.parent = gltfParentNode;
            }
            else
            {
                // It's a root node
                // Only root nodes are listed in a gltf scene
                logger.RaiseMessage("GLTFExporter.Node | Add " + babylonNode.name + " as root node to scene", 2);
                gltf.scenes[0].NodesList.Add(gltfNode.index);
            }

            // TRS
            if (exportParameters.exportAnimationsOnly == false)
            {
                // Position
                gltfNode.translation = babylonNode.position;

                // Rotation
                if (babylonNode.rotationQuaternion != null)
                {
                    gltfNode.rotation = babylonNode.rotationQuaternion;
                }
                else
                {
                    // Convert rotation vector to quaternion
                    BabylonVector3 rotationVector3 = new BabylonVector3
                    {
                        X = babylonNode.rotation[0],
                        Y = babylonNode.rotation[1],
                        Z = babylonNode.rotation[2]
                    };
                    gltfNode.rotation = rotationVector3.toQuaternion().ToArray();
                }

                // Scale
                gltfNode.scale = babylonNode.scaling;

                // Switch coordinate system at object level
                gltfNode.translation[2] *= -1;
                gltfNode.translation[0] *= exportParameters.scaleFactor;
                gltfNode.translation[1] *= exportParameters.scaleFactor;
                gltfNode.translation[2] *= exportParameters.scaleFactor;
                gltfNode.rotation[0] *= -1;
                gltfNode.rotation[1] *= -1;
            }

            ExportGLTFExtension(babylonNode,ref gltfNode, gltf);
            
            return gltfNode;
        }

        private void ExportGLTFExtension<T1,T2>(T1 babylonObject, ref T2 gltfObject, GLTF gltf) where T2:GLTFProperty
        {
            GLTFExtensions nodeExtensions = gltfObject.extensions;
            if (nodeExtensions == null) nodeExtensions = new GLTFExtensions();
            
            foreach (var extensionExporter in babylonScene.BabylonToGLTFExtensions)
            {
                if (extensionExporter.Value == typeof(T2))
                {
                    string extensionName = extensionExporter.Key.GetGLTFExtensionName();
                    object extensionObject = extensionExporter.Key.ExportBabylonExtension(babylonObject);
                    if (extensionObject != null && !string.IsNullOrEmpty(extensionName))
                    {
                        nodeExtensions.Add(extensionName,extensionObject);
                    }
                }
            }
            if (nodeExtensions.Count > 0)
            {
                gltfObject.extensions = nodeExtensions;

                if (gltf.extensionsUsed == null)
                {
                    gltf.extensionsUsed = new System.Collections.Generic.List<string>();
                }

                foreach (KeyValuePair<string, object> extension in gltfObject.extensions)
                {
                    if (!gltf.extensionsUsed.Contains(extension.Key))
                    {
                        gltf.extensionsUsed.Add(extension.Key);
                    }
                }
            }

            
        }
    }
}
