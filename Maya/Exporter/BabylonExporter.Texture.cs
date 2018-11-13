using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        private static List<string> validFormats = new List<string>(new string[] { "png", "jpg", "jpeg", "tga", "bmp", "gif" });
        private static List<string> invalidFormats = new List<string>(new string[] { "dds", "tif", "tiff" });

        private int logRankTexture = 2;

        public BabylonTexture ExportTexture(MFnDependencyNode materialDependencyNode, string plugName, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false, bool updateCoordinatesMode = false, float amount = 1.0f)
        {
            logRankTexture = 2;
            return _ExportTexture(materialDependencyNode, plugName, babylonScene, allowCube, forceAlpha, updateCoordinatesMode, amount);
        }

        public BabylonTexture ExportTexture(MFnDependencyNode textureDependencyNode, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false, bool forceSpherical = false, float amount = 1.0f)
        {
            logRankTexture = 2;
            return _ExportTexture(textureDependencyNode, babylonScene, null, allowCube, forceAlpha, forceSpherical, amount);
        }

        private BabylonTexture _ExportTexture(MFnDependencyNode materialDependencyNode, string plugName, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false, bool updateCoordinatesMode = false, float amount = 1.0f)
        {
            if (!materialDependencyNode.hasAttribute(plugName))
            {
                RaiseError("Unknown attribute " + materialDependencyNode.name + "." + plugName, logRankTexture);
                return null;
            }
            List<MFnDependencyNode> textureModifiers = new List<MFnDependencyNode>();
            MFnDependencyNode textureDependencyNode = getTextureDependencyNode(materialDependencyNode, plugName, textureModifiers);

            return _ExportTexture(textureDependencyNode, babylonScene, textureModifiers, allowCube, forceAlpha, updateCoordinatesMode, amount);
        }

        private BabylonTexture _ExportTexture(MFnDependencyNode textureDependencyNode, BabylonScene babylonScene, List<MFnDependencyNode>  textureModifiers = null, bool allowCube = false, bool forceAlpha = false, bool updateCoordinatesMode = false, float amount = 1.0f)
        {
            if (textureDependencyNode == null)
            {
                return null;
            }

            Print(textureDependencyNode, logRankTexture, "Print _ExportTexture textureDependencyNode");

            // Retreive texture file path
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                RaiseError("Texture path is not a valid string.", logRankTexture + 1);
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }

            // Check format
            var validImageFormat = GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                return null;
            }
            RaiseVerbose("validImageFormat=" + validImageFormat, logRankTexture + 1);

            var babylonTexture = new BabylonTexture
            {
                name = Path.GetFileNameWithoutExtension(sourcePath) + "." + validImageFormat
            };

            // Level
            babylonTexture.level = amount;

            // Alpha
            babylonTexture.hasAlpha = forceAlpha;
            // When fileHasAlpha = true:
            // - texture's format has alpha (png, tif, tga...)
            // - and at least one pixel has an alpha value < 255
            babylonTexture.getAlphaFromRGB = !textureDependencyNode.findPlug("fileHasAlpha").asBool();

            // UVs
            _exportUV(textureDependencyNode, babylonTexture, textureModifiers, updateCoordinatesMode);

            // TODO - Animations

            // Copy texture to output
            if (isBabylonExported)
            {
                var destPath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);

                if (textureModifiers == null || textureModifiers.FindAll(textureModifier => textureModifier.objectProperty.hasFn(MFn.Type.kReverse)).Count % 2 == 0)
                {
                    CopyTexture(sourcePath, destPath);
                }
                else
                {
                    // TODO - Reverse texture
                    CopyTexture(sourcePath, destPath);
                }

                // Is cube
                _exportIsCube(Path.Combine(babylonScene.OutputPath, babylonTexture.name), babylonTexture, allowCube);
            }
            else
            {
                babylonTexture.originalPath = sourcePath;
                babylonTexture.isCube = false;
            }

            return babylonTexture;
        }

        /*
        private BabylonTexture ExportBaseColorAlphaTexture(MFnDependencyNode materialDependencyNode, bool useColorMap, bool useOpacityMap, float[] baseColor, float alpha, BabylonScene babylonScene)
        {
            MFnDependencyNode textureDependencyNode = getTextureDependencyNode(materialDependencyNode, "TEX_color_map");

            if (textureDependencyNode == null)
            {
                return null;
            }

            // Prints
            Print(textureDependencyNode, logRankTexture, "Print ExportBaseColorAlphaTexture textureDependencyNode");

            // Retreive texture file path
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }

            // Check format
            string extension = Path.GetExtension(sourcePath);
            var validImageFormat = GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                return null;
            }
            RaiseVerbose("validImageFormat=" + validImageFormat, logRankTexture + 1);

            extension = extension.Substring(1).ToLower(); // remove the dot
            if (useOpacityMap)
            {
                List<string> alphaFormats = new List<string>(new string[] { "png", "tga", "gif" });
                if (!alphaFormats.Contains(extension))
                {
                    validImageFormat = "png";
                }
            }
            else
            {
                List<string> nonAlphaFormats = new List<string>(new string[] { "jpg", "jpeg", "bmp" });
                if (!nonAlphaFormats.Contains(extension))
                {
                    validImageFormat = "jpg";
                }
            }

            var babylonTexture = new BabylonTexture
            {
                name = Path.GetFileNameWithoutExtension(sourcePath) + "." + validImageFormat
            };

            // Level
            babylonTexture.level = 1.0f;

            // UVs
            _exportUV(textureDependencyNode, babylonTexture);

            // Is cube
            _exportIsCube(sourcePath, babylonTexture, false);


            // --- Merge baseColor and alpha maps ---

            if (!File.Exists(sourcePath))
            {
                return null;
            }

            // Alpha
            babylonTexture.hasAlpha = useOpacityMap;
            babylonTexture.getAlphaFromRGB = false;

            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var baseColorBitmap = LoadTexture(sourcePath);

                // Retreive dimensions
                int width = baseColorBitmap.Width;
                int height = baseColorBitmap.Height;

                // Create baseColor+alpha map
                var _baseColor = Color.FromArgb(
                    (int)(baseColor[0] * 255),
                    (int)(baseColor[1] * 255),
                    (int)(baseColor[2] * 255));
                var _alpha = (int)(alpha * 255);
                Bitmap baseColorAlphaBitmap = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var baseColorAtPixel = baseColorBitmap.GetPixel(x, y);

                        var __baseColor = useColorMap ? baseColorAtPixel : _baseColor;
                        var __alpha = useOpacityMap ? baseColorAtPixel.A : _alpha;
                        Color baseColorAlpha = Color.FromArgb(__alpha, __baseColor);
                        baseColorAlphaBitmap.SetPixel(x, y, baseColorAlpha);
                    }
                }

                // Write bitmap
                if (isBabylonExported)
                {
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", logRankTexture + 1);
                    var imageFormat = useOpacityMap ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg;
                    SaveBitmap(baseColorAlphaBitmap, babylonScene.OutputPath, babylonTexture.name, imageFormat);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = baseColorAlphaBitmap;
                }
            }

            return babylonTexture;
        }
        */
        
        private BabylonTexture ExportORMTexture(BabylonScene babylonScene, MFnDependencyNode metallicTextureDependencyNode, MFnDependencyNode roughnessTextureDependencyNode, MFnDependencyNode ambientOcclusionTextureDependencyNode, float defaultMetallic, float defaultRoughness)
        {
            // Prints
            if (metallicTextureDependencyNode != null)
            {
                Print(metallicTextureDependencyNode, logRankTexture, "Print ExportORMTexture metallicTextureDependencyNode");
            }
            if (roughnessTextureDependencyNode != null)
            {
                Print(roughnessTextureDependencyNode, logRankTexture, "Print ExportORMTexture roughnessTextureDependencyNode");
            }
            if (ambientOcclusionTextureDependencyNode != null)
            {
                Print(ambientOcclusionTextureDependencyNode, logRankTexture, "Print ExportORMTexture ambientOcclusionTextureDependencyNode");
            }

            // Use metallic or roughness texture as a reference for UVs parameters
            var textureDependencyNode = metallicTextureDependencyNode != null ? metallicTextureDependencyNode : roughnessTextureDependencyNode;
            if (textureDependencyNode == null)
            {
                return null;
            }

            var babylonTexture = new BabylonTexture
            {
                name = (ambientOcclusionTextureDependencyNode != null ? ambientOcclusionTextureDependencyNode.name : "") +
                       (roughnessTextureDependencyNode != null ? roughnessTextureDependencyNode.name : ("" + (int)(defaultRoughness * 255))) +
                       (metallicTextureDependencyNode != null ? metallicTextureDependencyNode.name : ("" + (int)(defaultMetallic * 255))) + ".jpg" // TODO - unsafe name, may conflict with another texture name
            };

            // UVs
            _exportUV(textureDependencyNode, babylonTexture);

            // Is cube
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }
            _exportIsCube(sourcePath, babylonTexture, false);


            // --- Merge metallic, roughness, ambient occlusion maps ---
            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var metallicBitmap = LoadTexture(metallicTextureDependencyNode);
                var roughnessBitmap = LoadTexture(roughnessTextureDependencyNode);
                var ambientOcclusionBitmap = LoadTexture(ambientOcclusionTextureDependencyNode);

                // Merge bitmaps
                // The occlusion values are sampled from the R channel.
                // The roughness values are sampled from the G channel.
                // The metalness values are sampled from the B channel.
                Bitmap[] bitmaps = new Bitmap[] { ambientOcclusionBitmap, roughnessBitmap, metallicBitmap, null };
                int[] defaultValues = new int[] { 0, (int)(defaultRoughness * 255), (int)(defaultMetallic * 255), 0 };
                Bitmap ormBitmap = MergeBitmaps(bitmaps, defaultValues, ambientOcclusionBitmap != null ? "Occlusion, metallic and roughness" : "Metallic and roughness");
                
                // Write bitmap
                if (isBabylonExported)
                {
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", logRankTexture + 1);
                    SaveBitmap(ormBitmap, babylonScene.OutputPath, babylonTexture.name, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = ormBitmap;
                }
            }

            return babylonTexture;
        }

        private BabylonTexture ExportBaseColorAlphaTexture(MFnDependencyNode baseColorTextureDependencyNode, MFnDependencyNode opacityTextureDependencyNode, BabylonScene babylonScene, string materialName, Color defaultBaseColor, float defaultOpacity = 1.0f)
        {
            // Prints
            if (baseColorTextureDependencyNode != null)
            {
                Print(baseColorTextureDependencyNode, logRankTexture, "Print ExportBaseColorAlphaTexture baseColorTextureDependencyNode");
            }
            if (opacityTextureDependencyNode != null)
            {
                Print(opacityTextureDependencyNode, logRankTexture, "Print ExportBaseColorAlphaTexture opacityTextureDependencyNode");
            }

            // Use one as a reference for UVs parameters
            var textureDependencyNode = baseColorTextureDependencyNode != null ? baseColorTextureDependencyNode : opacityTextureDependencyNode;
            if (textureDependencyNode == null)
            {
                return null;
            }

            var babylonTexture = new BabylonTexture
            {
                name = materialName + "_baseColor" + ".png" // TODO - unsafe name, may conflict with another texture name
            };

            // Level
            babylonTexture.level = 1.0f;

            // Alpha
            babylonTexture.hasAlpha = opacityTextureDependencyNode != null || defaultOpacity != 1.0f;
            babylonTexture.getAlphaFromRGB = false;

            // UVs
            _exportUV(textureDependencyNode, babylonTexture);

            // Is cube
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }
            _exportIsCube(sourcePath, babylonTexture, false);


            // --- Merge base color and opacity maps ---

            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var baseColorBitmap = LoadTexture(baseColorTextureDependencyNode);
                var opacityBitmap = LoadTexture(opacityTextureDependencyNode);

                // Merge bitmaps
                Bitmap[] bitmaps = new Bitmap[] { baseColorBitmap, baseColorBitmap, baseColorBitmap, opacityBitmap };
                int[] defaultValues = new int[] { defaultBaseColor.R, defaultBaseColor.G, defaultBaseColor.B, (int)(defaultOpacity * 255) };
                Bitmap baseColorAlphaBitmap = MergeBitmaps(bitmaps, defaultValues, "Base color and opacity");

                // Write bitmap
                if (isBabylonExported)
                {
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", logRankTexture + 1);
                    SaveBitmap(baseColorAlphaBitmap, babylonScene.OutputPath, babylonTexture.name, System.Drawing.Imaging.ImageFormat.Png);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = baseColorAlphaBitmap;
                }
            }

            return babylonTexture;
        }

        private void _exportUV(MFnDependencyNode textureDependencyNode, BabylonTexture babylonTexture, List<MFnDependencyNode> textureModifiers = null, bool updateCoordinatesMode = false)
        {
            // Coordinates mode
            if (updateCoordinatesMode)
            {
                // Default is spherical
                babylonTexture.coordinatesMode = BabylonTexture.CoordinatesMode.SPHERICAL_MODE;

                if (textureModifiers != null)
                {
                    MFnDependencyNode lastProjectionTextureModifier = textureModifiers.FindLast(textureModifier => textureModifier.objectProperty.hasFn(MFn.Type.kProjection));
                    if (lastProjectionTextureModifier != null)
                    {
                        var projType = lastProjectionTextureModifier.findPlug("projType").asIntProperty;
                        switch (projType)
                        {
                            case 1: // Planar
                                babylonTexture.coordinatesMode = BabylonTexture.CoordinatesMode.PLANAR_MODE;
                                break;
                            case 2: // Spherical
                                babylonTexture.coordinatesMode = BabylonTexture.CoordinatesMode.SPHERICAL_MODE;
                                break;
                        }
                    }
                }
            }

            // UV set
            MStringArray uvLinks = new MStringArray();
            MGlobal.executeCommand($@"uvLink -query -texture {textureDependencyNode.name};", uvLinks);
            if (uvLinks.Count == 0)
            {
                babylonTexture.coordinatesIndex = 0;
            }
            else
            {
                // Retreive UV set indices
                HashSet<int> uvSetIndices = new HashSet<int>();
                foreach (string uvLink in uvLinks)
                {
                    int indexOpenBracket = uvLink.LastIndexOf("[");
                    int indexCloseBracket = uvLink.LastIndexOf("]");
                    string uvSetIndexAsString = uvLink.Substring(indexOpenBracket + 1, indexCloseBracket - indexOpenBracket - 1);
                    int uvSetIndex = int.Parse(uvSetIndexAsString);
                    uvSetIndices.Add(uvSetIndex);
                }
                if (uvSetIndices.Count > 1)
                {
                    // Check that all uvSet indices are all 0 or all not 0
                    int nbZero = 0;
                    foreach (int uvSetIndex in uvSetIndices) {
                        if (uvSetIndex == 0)
                        {
                            nbZero++;
                        }
                    }
                    if (nbZero != 0 && nbZero != uvSetIndices.Count)
                    {
                        RaiseWarning("Texture is linked to UV1 and UV2. Only one UV set per texture is supported.", logRankTexture + 1);
                    }
                }
                // The first UV set of a mesh is special because it can never be deleted
                // Thus if the UV set index is 0 then the binded mesh UV set is always UV1
                // Other UV sets of a mesh can be created / deleted at will
                // Thus the UV set index can have any value (> 0)
                // In this case, the exported UV set is always UV2 even though it would be UV3 or UV4 in Maya
                babylonTexture.coordinatesIndex = (new List<int>(uvSetIndices))[0] == 0 ? 0 : 1;
            }

            // For more information about UV
            // see http://help.autodesk.com/view/MAYAUL/2018/ENU/?guid=GUID-94070C7E-C550-42FD-AFC9-FBE82B173B1D
            babylonTexture.uOffset = textureDependencyNode.findPlug("offsetU").asFloat();
                ;
            babylonTexture.vOffset = textureDependencyNode.findPlug("offsetV").asFloat();
            
            babylonTexture.uScale = textureDependencyNode.findPlug("repeatU").asFloat();
            babylonTexture.vScale = textureDependencyNode.findPlug("repeatV").asFloat();
            
            if (Path.GetExtension(babylonTexture.name).ToLower() == ".dds")
            {
                babylonTexture.vScale *= -1; // Need to invert Y-axis for DDS texture
            }

            // Maya only has a W rotation
            babylonTexture.uAng = 0;
            babylonTexture.vAng = 0;
            babylonTexture.wAng = textureDependencyNode.findPlug("rotateFrame").asFloat();

            // TODO - rotation and scale
            if (babylonTexture.wAng != 0f && (babylonTexture.uScale != 1f || babylonTexture.vScale != 1f))
            {
                RaiseWarning("Rotation and tiling (scale) on a texture are only supported separatly. You can use the map UV of the mesh for those transformation.", logRankTexture + 1);
            }

            // Adress mode U
            // TODO - What is adress mode when both wrap and mirror?
            if (textureDependencyNode.findPlug("mirrorU").asBool())
            {
                babylonTexture.wrapU = BabylonTexture.AddressMode.MIRROR_ADDRESSMODE;
            }
            else if (textureDependencyNode.findPlug("wrapU").asBool())
            {
                babylonTexture.wrapU = BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
            else
            {
                // TODO - What is adress mode when not wrap nor mirror?
                babylonTexture.wrapU = BabylonTexture.AddressMode.CLAMP_ADDRESSMODE;
            }

            // Adress mode V
            // TODO - What is adress mode when both wrap and mirror?
            if (textureDependencyNode.findPlug("mirrorV").asBool())
            {
                babylonTexture.wrapV = BabylonTexture.AddressMode.MIRROR_ADDRESSMODE;
            }
            else if (textureDependencyNode.findPlug("wrapV").asBool())
            {
                babylonTexture.wrapV = BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
            else
            {
                // TODO - What is adress mode when not wrap nor mirror?
                babylonTexture.wrapV = BabylonTexture.AddressMode.CLAMP_ADDRESSMODE;
            }

            // Animation
            babylonTexture.animations = GetTextureAnimations(textureDependencyNode).ToArray();
        }

        private void _exportIsCube(string absolutePath, BabylonTexture babylonTexture, bool allowCube)
        {
            if (Path.GetExtension(absolutePath).ToLower() != ".dds")
            {
                babylonTexture.isCube = false;
            }
            else
            {
                try
                {
                    if (File.Exists(absolutePath))
                    {
                        babylonTexture.isCube = _isTextureCube(absolutePath);
                    }
                    else
                    {
                        RaiseWarning(string.Format("Texture {0} not found.", absolutePath), logRankTexture + 1);
                    }

                }
                catch
                {
                    // silently fails
                }

                if (babylonTexture.isCube && !allowCube)
                {
                    RaiseWarning(string.Format("Cube texture are only supported for reflection channel"), logRankTexture + 1);
                }
            }
        }

        private bool _isTextureCube(string filepath)
        {
            try
            {
                var data = File.ReadAllBytes(filepath);
                var intArray = new int[data.Length / 4];

                Buffer.BlockCopy(data, 0, intArray, 0, intArray.Length * 4);


                int width = intArray[4];
                int height = intArray[3];
                int mipmapsCount = intArray[7];

                if ((width >> (mipmapsCount - 1)) > 1)
                {
                    var expected = 1;
                    var currentSize = Math.Max(width, height);

                    while (currentSize > 1)
                    {
                        currentSize = currentSize >> 1;
                        expected++;
                    }

                    RaiseWarning(string.Format("Mipmaps chain is not complete: {0} maps instead of {1} (based on texture max size: {2})", mipmapsCount, expected, width), logRankTexture + 1);
                    RaiseWarning(string.Format("You must generate a complete mipmaps chain for .dds)"), logRankTexture + 1);
                    RaiseWarning(string.Format("Mipmaps will be disabled for this texture. If you want automatic texture generation you cannot use a .dds)"), logRankTexture + 1);
                }

                bool isCube = (intArray[28] & 0x200) == 0x200;

                return isCube;
            }
            catch
            {
                return false;
            }
        }

        // -------------------------
        // --------- Utils ---------
        // -------------------------

        private string getSourcePathFromFileTexture(MFnDependencyNode textureDependencyNode)
        {
            MObject sourceObject = textureDependencyNode.objectProperty;

            // Retreive texture file path
            if (!sourceObject.hasFn(MFn.Type.kFileTexture))
            {
                RaiseError("Only file texture is supported.", logRankTexture + 1);
                return null;
            }
            MPlug fileTextureNamePlug = textureDependencyNode.findPlug("fileTextureName");
            if (fileTextureNamePlug == null || fileTextureNamePlug.isNull)
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }
            string sourcePath = fileTextureNamePlug.asString();
            return sourcePath;
        }

        private MFnDependencyNode getTextureDependencyNode(MFnDependencyNode materialDependencyNode, string plugName, List<MFnDependencyNode> textureModifiers = null)
        {
            MPlug mPlug = materialDependencyNode.findPlug(plugName);

            if (mPlug == null || mPlug.isNull || !mPlug.isConnected)
            {
                return null;
            }

            MObject sourceObject = mPlug.source.node;
            MFnDependencyNode textureDependencyNode = new MFnDependencyNode(sourceObject);

            RaiseMessage(materialDependencyNode.name + "." + plugName, logRankTexture);

            // Bump texture uses an intermediate node
            if (sourceObject.hasFn(MFn.Type.kBump))
            {
                Print(textureDependencyNode, logRankTexture, "Print bump node");
                logRankTexture++;
                if (textureModifiers != null)
                {
                    textureModifiers.Add(textureDependencyNode);
                }
                return getTextureDependencyNode(textureDependencyNode, "bumpValue", textureModifiers);
            }

            // If a reverse node is used as an intermediate node
            if (sourceObject.hasFn(MFn.Type.kReverse))
            {
                Print(textureDependencyNode, logRankTexture, "Print reverse node");
                // TODO - reverse?
                logRankTexture++;
                if (textureModifiers != null)
                {
                    textureModifiers.Add(textureDependencyNode);
                }
                return getTextureDependencyNode(textureDependencyNode, "input", textureModifiers);
            }

            // If a projection node is used as an intermediate node
            if (sourceObject.hasFn(MFn.Type.kProjection))
            {
                Print(textureDependencyNode, logRankTexture, "Print projection node");
                logRankTexture++;
                if (textureModifiers != null)
                {
                    textureModifiers.Add(textureDependencyNode);
                }
                return getTextureDependencyNode(textureDependencyNode, "image", textureModifiers);
            }

            return textureDependencyNode;
        }

        private bool _getMinimalBitmapDimensions(out int width, out int height, params Bitmap[] bitmaps)
        {
            var haveSameDimensions = true;

            var bitmapsNoNull = ((new List<Bitmap>(bitmaps)).FindAll(bitmap => bitmap != null)).ToArray();
            if (bitmapsNoNull.Length > 0)
            {
                // Init with first element
                width = bitmapsNoNull[0].Width;
                height = bitmapsNoNull[0].Height;

                // Update with others
                for (int i = 1; i < bitmapsNoNull.Length; i++)
                {
                    var bitmap = bitmapsNoNull[i];
                    if (width != bitmap.Width || height != bitmap.Height)
                    {
                        haveSameDimensions = false;
                    }
                    width = Math.Min(width, bitmap.Width);
                    height = Math.Min(height, bitmap.Height);
                }
            }
            else
            {
                width = 0;
                height = 0;
            }

            return haveSameDimensions;
        }

        private Bitmap LoadTexture(MFnDependencyNode textureDependencyNode)
        {
            if (textureDependencyNode == null)
            {
                return null;
            }

            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing for node " + textureDependencyNode.name + ".", logRankTexture + 1);
                return null;
            }
            return LoadTexture(sourcePath);
        }

        private Bitmap LoadTexture(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                try
                {
                    switch (Path.GetExtension(absolutePath).ToLower())
                    {
                        case ".dds":
                            // External library GDImageLibrary.dll + TQ.Texture.dll
                            return GDImageLibrary._DDS.LoadImage(absolutePath);
                        case ".tga":
                            // External library TargaImage.dll
                            return Paloma.TargaImage.LoadTargaImage(absolutePath);
                        case ".bmp":
                        case ".gif":
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".tif":
                        case ".tiff":
                            return new Bitmap(absolutePath);
                        default:
                            RaiseError(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(absolutePath)), logRankTexture + 1);
                            return null;
                    }
                }
                catch (Exception e)
                {
                    RaiseError(string.Format("Failed to load texture {0}: {1}", Path.GetFileName(absolutePath), e.Message), logRankTexture + 1);
                    return null;
                }
            }
            else
            {
                RaiseError(string.Format("Texture {0} not found.", Path.GetFileName(absolutePath)), logRankTexture + 1);
                return null;
            }
        }

        private void CopyTexture(string sourcePath, string destPath)
        {
            _copyTexture(sourcePath, destPath, validFormats, invalidFormats);
        }

        private string GetValidImageFormat(string extension)
        {
            return _getValidImageFormat(extension, validFormats, invalidFormats);
        }

        private string _getValidImageFormat(string extension, List<string> validFormats, List<string> invalidFormats)
        {
            var imageFormat = extension.Substring(1).ToLower(); // remove the dot

            if (validFormats.Contains(imageFormat))
            {
                return imageFormat;
            }
            else if (invalidFormats.Contains(imageFormat))
            {
                switch (imageFormat)
                {
                    case "dds":
                    case "tga":
                    case "tif":
                    case "tiff":
                    case "gif":
                    case "png":
                        return "png";
                    case "bmp":
                    case "jpg":
                    case "jpeg":
                        return "jpg";
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Copy image from source to dest.
        /// The copy process may include a conversion to another image format:
        /// - a source with a valid format is copied directly
        /// - a source with an invalid format is converted to png or jpg before being copied
        /// - a source with neither a valid nor an invalid format raises a warning and is not copied
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="validFormats"></param>
        /// <param name="invalidFormats"></param>
        private void _copyTexture(string sourcePath, string destPath, List<string> validFormats, List<string> invalidFormats)
        {
            if (CopyTexturesToOutput)
            {
                try
                {
                    if (File.Exists(sourcePath))
                    {
                        string imageFormat = Path.GetExtension(sourcePath).Substring(1).ToLower(); // remove the dot

                        if (validFormats.Contains(imageFormat))
                        {
                            if (sourcePath != destPath)
                            {
                                File.Copy(sourcePath, destPath, true);
                            }
                        }
                        else if (invalidFormats.Contains(imageFormat))
                        {
                            _convertToBitmapAndSave(sourcePath, destPath, imageFormat);
                        }
                        else
                        {
                            RaiseError(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                        }
                    }
                }
                catch (Exception c)
                {
                    RaiseError(string.Format("Exporting texture {0} failed: {1}", sourcePath, c.ToString()), logRankTexture + 1);
                }
            }
        }

        /// <summary>
        /// Load image from source to a bitmap and save it to dest as png or jpg.
        /// Loading process to a bitmap depends on extension.
        /// Saved image format depends on alpha presence.
        /// png and jpg are copied directly.
        /// Unsupported format raise a warning and are not copied.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="imageFormat"></param>
        private void _convertToBitmapAndSave(string sourcePath, string destPath, string imageFormat)
        {
            Bitmap bitmap;
            switch (imageFormat)
            {
                case "dds":
                    // External libraries GDImageLibrary.dll + TQ.Texture.dll
                    try
                    {
                        bitmap = GDImageLibrary._DDS.LoadImage(sourcePath);
                        SaveBitmap(bitmap, destPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception e)
                    {
                        RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), logRankTexture + 1);
                    }
                    break;
                case "tga":
                    // External library TargaImage.dll
                    try
                    {
                        bitmap = Paloma.TargaImage.LoadTargaImage(sourcePath);
                        SaveBitmap(bitmap, destPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception e)
                    {
                        RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), logRankTexture + 1);
                    }
                    break;
                case "bmp":
                    bitmap = new Bitmap(sourcePath);
                    SaveBitmap(bitmap, destPath, System.Drawing.Imaging.ImageFormat.Jpeg); // no alpha
                    break;
                case "tif":
                case "tiff":
                case "gif":
                    bitmap = new Bitmap(sourcePath);
                    SaveBitmap(bitmap, destPath, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case "jpeg":
                case "png":
                    File.Copy(sourcePath, destPath, true);
                    break;
                default:
                    RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                    break;
            }
        }

        private void SaveBitmap(Bitmap bitmap, string path, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            SaveBitmap(bitmap, Path.GetDirectoryName(path), Path.GetFileName(path), imageFormat);
        }

        private void SaveBitmap(Bitmap bitmap, string directoryName, string fileName, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            List<char> invalidCharsInString = GetInvalidChars(directoryName, Path.GetInvalidPathChars());
            if (invalidCharsInString.Count > 0)
            {
                RaiseError($"Failed to save bitmap: directory name '{directoryName}' contains invalid character{(invalidCharsInString.Count > 1 ? "s" : "")} {invalidCharsInString.ToArray().toString(false)}", logRankTexture + 1);
                return;
            }
            invalidCharsInString = GetInvalidChars(fileName, Path.GetInvalidFileNameChars());
            if (invalidCharsInString.Count > 0)
            {
                RaiseError($"Failed to save bitmap: file name '{fileName}' contains invalid character{(invalidCharsInString.Count > 1 ? "s" : "")} {invalidCharsInString.ToArray().toString(false)}", logRankTexture + 1);
                return;
            }

            string path = Path.Combine(directoryName, fileName);
            using (FileStream fs = File.Open(path, FileMode.Create))
            {
                ImageCodecInfo encoder = GetEncoder(imageFormat);

                if (encoder != null)
                {
                    // Create an Encoder object based on the GUID for the Quality parameter category
                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    EncoderParameter encoderQualityParameter = new EncoderParameter(Encoder.Quality, _quality);
                    encoderParameters.Param[0] = encoderQualityParameter;

                    bitmap.Save(fs, encoder, encoderParameters);
                }
                else
                {
                    bitmap.Save(fs, imageFormat);
                }
            }
        }

        private List<char> GetInvalidChars(string s, char[] invalidChars)
        {
            List<char> invalidCharsInString = new List<char>();
            foreach (char ch in invalidChars)
            {
                int indexInvalidChar = s.IndexOf(ch);
                if (indexInvalidChar != -1)
                {
                    invalidCharsInString.Add(s[indexInvalidChar]);
                }
            }
            return invalidCharsInString;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Merge bitmaps into a single bitmap. Each channel has an associated bitmap to retreive data from.
        /// </summary>
        /// <param name="bitmaps">R, G, B, A bitmaps to merge</param>
        /// <param name="defaultValues">Default R, G, B, A values if related bitmap is null. Base 255.</param>
        /// <param name="sizeErrorMessage">Map names to display when maps don't have same size</param>
        /// <returns></returns>
        private Bitmap MergeBitmaps(Bitmap[] bitmaps, int[] defaultValues, string sizeErrorMessage)
        {
            // Retreive dimensions
            int width = 0;
            int height = 0;
            var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, bitmaps);
            if (!haveSameDimensions)
            {
                RaiseError(sizeErrorMessage + " maps should have same dimensions", logRankTexture + 1);
            }

            // Create merged bitmap
            Bitmap mergedBitmap = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var r = bitmaps[0] != null ? bitmaps[0].GetPixel(x, y).R : defaultValues[0];
                    var g = bitmaps[1] != null ? bitmaps[1].GetPixel(x, y).G : defaultValues[1];
                    var b = bitmaps[2] != null ? bitmaps[2].GetPixel(x, y).B : defaultValues[2];
                    var a = bitmaps[3] != null ? bitmaps[3].GetPixel(x, y).A : defaultValues[3];
                    mergedBitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            return mergedBitmap;
        }
    }
}
