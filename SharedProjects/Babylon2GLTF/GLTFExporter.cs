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

        ILoggingProvider logger;

        // from BabylonNode to GLTFNode
        Dictionary<BabylonNode, GLTFNode> nodeToGltfNodeMap;
        Dictionary<BabylonBone, GLTFNode> boneToGltfNodeMap;

        public void ExportGltf(ExportParameters exportParameters, BabylonScene babylonScene, string outputDirectory, string outputFileName, bool generateBinary, ILoggingProvider logger)
        {
            this.exportParameters = exportParameters;
            this.logger = logger;

            logger.RaiseMessage("GLTFExporter | Exportation started", Color.Blue);
#if DEBUG
            var watch = new Stopwatch();
            watch.Start();
#endif

            // Force output file extension to be gltf
            outputFileName = Path.ChangeExtension(outputFileName, "gltf");

            // Update path of output .gltf file to include subdirectory
            var outputFile = Path.Combine(outputDirectory, outputFileName);

            float progressionStep;
            var progression = 0.0f;
            logger.ReportProgressChanged((int)progression);

            // Initialization
            initBabylonNodes(babylonScene);
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
            GLTFScene[] scenes = { scene };
            gltf.scenes = scenes;

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
            var meshesExportTime = watch.ElapsedMilliseconds / 1000.0;
            logger.RaiseMessage(string.Format("GLTFMeshes exported in {0:0.00}s", meshesExportTime), Color.Blue);
#endif
 

            // Root nodes
            logger.RaiseMessage("GLTFExporter | Exporting nodes");
            List<BabylonNode> babylonRootNodes = babylonNodes.FindAll(node => node.parentId == null);
            progressionStep = 40.0f / babylonRootNodes.Count;
            alreadyExportedSkeletons = new Dictionary<BabylonSkeleton, BabylonSkeletonExportData>();
            nodeToGltfNodeMap = new Dictionary<BabylonNode, GLTFNode>();
            boneToGltfNodeMap = new Dictionary<BabylonBone, GLTFNode>();
            NbNodesByName = new Dictionary<string, int>();
            babylonRootNodes.ForEach(babylonNode =>
            {
                exportNodeRec(babylonNode, gltf, babylonScene);
                progression += progressionStep;
                logger.ReportProgressChanged((int)progression);
                logger.CheckCancelled();
            });
#if DEBUG
            var nodesExportTime = watch.ElapsedMilliseconds / 1000.0 -meshesExportTime;
            logger.RaiseMessage(string.Format("GLTFNodes exported in {0:0.00}s", nodesExportTime), Color.Blue);
#endif
            // Materials
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
                var imageBufferViews = SwitchImagesFromUriToBinary(gltf);
                imageBufferViews.ForEach(imageBufferView =>
                {
                    imageBufferView.Buffer.bytesList.AddRange(imageBufferView.bytesList);
                });
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

                try
                {
                    Process gltfPipeline = new Process();

                    // Hide the cmd window that show the gltf-pipeline result
                    //gltfPipeline.StartInfo.UseShellExecute = false;
                    //gltfPipeline.StartInfo.CreateNoWindow = true;
                    gltfPipeline.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    string arg;
                    if (generateBinary)
                    {
                        string outputGlbFile = Path.ChangeExtension(outputFile, "glb");
                        arg = $" -i {outputGlbFile} -o {outputGlbFile} -d";
                    }
                    else
                    {
                        string outputGltfFile = Path.ChangeExtension(outputFile, "gltf");
                        arg = $" -i {outputGltfFile} -o {outputGltfFile} -d -s";
                    }
                    gltfPipeline.StartInfo.FileName = "gltf-pipeline.cmd";
                    gltfPipeline.StartInfo.Arguments = arg;

                    gltfPipeline.Start();
                    gltfPipeline.WaitForExit();
                }
                catch
                {
                    logger.RaiseError("gltf-pipeline module not found.", 1);
                    logger.RaiseError("The exported file wasn't compressed.");
                }
            }

            logger.ReportProgressChanged(100);
        }

        private List<BabylonNode> initBabylonNodes(BabylonScene babylonScene)
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
            return babylonNodes;
        }

        private void exportNodeRec(BabylonNode babylonNode, GLTF gltf, BabylonScene babylonScene, GLTFNode gltfParentNode = null)
        {
            var type = babylonNode.GetType();

            GLTFNode gltfNode = ExportNode(babylonNode, gltf, babylonScene, gltfParentNode);

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
                else
                {
                    logger.RaiseError($"Node named {babylonNode.name} as no exporter", 1);
                }

                logger.CheckCancelled();

                // export its tag
                if(babylonNode.tags != null && babylonNode.tags != "")
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
                jsonWriter.Formatting = Formatting.None;
                jsonSerializer.Serialize(jsonWriter, gltf);
            }
            return sb.ToString();
        }

        private List<GLTFBufferView> SwitchImagesFromUriToBinary(GLTF gltf)
        {
            var imageBufferViews = new List<GLTFBufferView>();

            foreach (GLTFImage gltfImage in gltf.ImagesList)
            {
                var path = Path.Combine(gltf.OutputFolder, Uri.UnescapeDataString(gltfImage.uri));
                byte[] imageBytes = File.ReadAllBytes(path);

                // Chunk must be padded with trailing zeros (0x00) to satisfy alignment requirements
                imageBytes = padChunk(imageBytes, 4, 0x00);

                // BufferView - Image
                var buffer = gltf.buffer;
                var bufferViewImage = new GLTFBufferView
                {
                    name = "bufferViewImage",
                    buffer = buffer.index,
                    Buffer = buffer,
                    byteOffset = buffer.byteLength
                };
                bufferViewImage.index = gltf.BufferViewsList.Count;
                gltf.BufferViewsList.Add(bufferViewImage);
                imageBufferViews.Add(bufferViewImage);


                gltfImage.uri = null;
                gltfImage.bufferView = bufferViewImage.index;
                gltfImage.mimeType = "image/" + gltfImage.FileExtension;

                bufferViewImage.bytesList.AddRange(imageBytes);
                bufferViewImage.byteLength += imageBytes.Length;
                bufferViewImage.Buffer.byteLength += imageBytes.Length;
            }
            return imageBufferViews;
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

            var boneNodePair = boneToGltfNodeMap.FirstOrDefault(pair => pair.Key.id.Equals(babylonNode.id));
            if (boneNodePair.Key != null)
            {
                return boneNodePair.Value;
            }

            // Node
            gltfNode = new GLTFNode
            {
                name = GetUniqueNodeName(babylonNode.name),
                index = gltf.NodesList.Count
            };
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

            // Transform
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

            return gltfNode;
        }
    }
}
