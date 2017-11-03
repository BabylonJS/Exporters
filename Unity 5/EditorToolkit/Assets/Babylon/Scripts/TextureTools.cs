using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEditor;
using Unity3D2Babylon;

namespace UnityEngine
{
    public enum CopyFilterMode
    {
        Source = 0,
        Point = 1,
        Bilinear = 2,
        Trilinear = 3
    }

    public enum TextureExtrude
    {
        Default = 0,
        Mirror = 1,
        Black = 2,
        White = 3,
        Red = 4,
        Green = 5,
        Blue = 6,
        Yellow = 7,
        Gray = 8
    }

    public static class TextureHelpers
    {
        public static void Clear(this Texture2D source, Color color)
        {
            if (source == null) return;
            int index = 0;
            Color32[] pixels = new Color32[source.width * source.height];
            for (int y=0; y < source.height; y++) 
            {
                for (int x=0; x < source.width; x++) 
                {
                    pixels[index] = color;
                    index++;
                }
            }
            source.SetPixels32(pixels);
            source.Apply();
        }
        
        public static Texture2D Copy(this Texture2D source, TextureFormat format = TextureFormat.RGBA32, CopyFilterMode filter = CopyFilterMode.Source)
        {
            if (source == null) return null;
            Texture2D result = new Texture2D(source.width, source.height, format, false);
            result.name = source.name;
            switch(filter) {
                case CopyFilterMode.Source:
                    result.filterMode = source.filterMode;
                    break;
                case CopyFilterMode.Point:
                    result.filterMode = FilterMode.Point;
                    break;
                case CopyFilterMode.Bilinear:
                    result.filterMode = FilterMode.Bilinear;
                    break;
                case CopyFilterMode.Trilinear:
                    result.filterMode = FilterMode.Trilinear;
                    break;
            }
            Color32[] pixels = source.GetPixels32();
            if (pixels != null)
            {
                result.SetPixels32(source.GetPixels32());
                result.Apply();
            }
            return result;
        }

        public static void Scale(this Texture2D source, int newWidth, int newHeight, bool bilinearScaling = true)
        {
            if (source == null) return;
            int w = 0;
            int w2 = 0;
            float ratioX = 0;
            float ratioY = 0;
            Color32[] texColors = source.GetPixels32();
            Color32[] newColors = new Color32[newWidth * newHeight];
            if (bilinearScaling)
            {
                ratioX = 1.0f / ((float)newWidth / (source.width - 1));
                ratioY = 1.0f / ((float)newHeight / (source.height - 1));
            }
            else
            {
                ratioX = ((float)source.width) / newWidth;
                ratioY = ((float)source.height) / newHeight;
            }
            w = source.width;
            w2 = newWidth;
            if (bilinearScaling)
            {
                for (var y = 0; y < newHeight; y++)
                {
                    int yFloor = (int)Mathf.Floor(y * ratioY);
                    var y1 = yFloor * w;
                    var y2 = (yFloor + 1) * w;
                    var yw = y * w2;

                    for (var x = 0; x < w2; x++)
                    {
                        int xFloor = (int)Mathf.Floor(x * ratioX);
                        var xLerp = x * ratioX - xFloor;
                        newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                            ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                            y * ratioY - yFloor);
                    }
                }
            }
            else
            {
                for (var y = 0; y < newHeight; y++)
                {
                    var thisY = (int)(ratioY * y) * w;
                    var yw = y * w2;
                    for (var x = 0; x < w2; x++)
                    {
                        newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                    }
                }
            }
            source.Resize(newWidth, newHeight);
            source.SetPixels32(newColors);
            source.Apply();
        }

        public static Texture2D Extrude(this Texture2D source, int padding, bool bilinearScaling = true, TextureExtrude extrudeColor = TextureExtrude.Default)
        {
            if (source == null) return null;
            int ratio = padding * 2;
            int width = source.width - ratio;
            int height = source.height - ratio;
            Vector2 tileSize = new Vector2(width, height);
            Vector2 borderSize = new Vector2(padding, padding);
            source.Scale(width, height, bilinearScaling);
            return source.Extrude(tileSize, borderSize, extrudeColor);
        }

        public static Texture2D Extrude(this Texture2D source, Vector2 tileSize, Vector2 borderSize, TextureExtrude extrudeColor = TextureExtrude.Default)
        {
            if (source == null) return null;
            int cols = (int)Mathf.Floor(source.width / tileSize.x);
            int rows = (int)Mathf.Floor(source.height / tileSize.y);
            Color border = Color.black;
            switch(extrudeColor)
            {
                case TextureExtrude.White:
                    border = Color.white;
                    break;
                case TextureExtrude.Red:
                    border = Color.red;
                    break;
                case TextureExtrude.Green:
                    border = Color.green;
                    break;
                case TextureExtrude.Blue:
                    border = Color.blue;
                    break;
                case TextureExtrude.Yellow:
                    border = Color.yellow;
                    break;
                case TextureExtrude.Gray:
                    border = Color.gray;
                    break;
                default:
                    border = Color.black;
                    break;
            }
            Texture2D texture = new Texture2D((int)(cols * (tileSize.x + borderSize.x * 2f)), source.height, source.format, false);
            texture.filterMode = source.filterMode;
            for(int i = 0; i < cols; i++) {
                Color[] c1 = source.GetPixels((int)(i * tileSize.x), 0, 1, source.height);
                Color[] c2 = source.GetPixels((int)((i + 1) * tileSize.x - 1), 0, 1, source.height);
                // Format border pixels
                if (extrudeColor != TextureExtrude.Default && extrudeColor != TextureExtrude.Mirror) {
                    for (int index = 0; index < c1.Length; index++) {
                        c1[index] = border;
                    }
                    for (int index = 0; index < c2.Length; index++) {
                        c2[index] = border;
                    }
                } else if (extrudeColor == TextureExtrude.Mirror) {
                    // TODO: Mirror Edge Pixels
                }
                for(int j = 0; j < borderSize.x; j++) {
                    texture.SetPixels((int)(i * (tileSize.x + borderSize.x * 2) + j), 0, 1, source.height, c1);
                    texture.SetPixels((int)(i * (tileSize.x + borderSize.x * 2) + j + tileSize.x + borderSize.x), 0, 1, source.height, c2);
                }
                texture.SetPixels((int)(i * (tileSize.x + borderSize.x * 2) + borderSize.x), 0, (int)tileSize.x, source.height, source.GetPixels((int)(i * tileSize.x), 0, (int)tileSize.x, source.height));
            }
            
            Texture2D temp = texture;
            texture = new Texture2D(temp.width, (int)(rows * (tileSize.y + borderSize.y * 2f)), source.format, false);
            texture.filterMode =source.filterMode;
            for(int i = 0; i < rows; i++) {
                Color [] c1 = temp.GetPixels(0, (int)(i * tileSize.y), temp.width, 1);
                Color [] c2 = temp.GetPixels(0, (int)((i + 1) * tileSize.y - 1), temp.width, 1);
                // Format border pixels
                if (extrudeColor != TextureExtrude.Default && extrudeColor != TextureExtrude.Mirror) {
                    for (int index = 0; index < c1.Length; index++) {
                        c1[index] = border;
                    }
                    for (int index = 0; index < c2.Length; index++) {
                        c2[index] = border;
                    }
                } else if (extrudeColor == TextureExtrude.Mirror) {
                    // TODO: Mirror Edge Pixels
                }
                for(int j=0; j < borderSize.y; j++) {
                    texture.SetPixels(0, (int)(i * (tileSize.y + borderSize.y * 2) + j), temp.width, 1, c1);
                    texture.SetPixels(0, (int)(i * (tileSize.y + borderSize.y * 2) + j + tileSize.y + borderSize.y), temp.width, 1, c2);
                }
                texture.SetPixels(0, (int)(i * (tileSize.y + borderSize.y * 2) + borderSize.y), temp.width, (int)tileSize.y, temp.GetPixels(0, (int)(i * tileSize.y), temp.width, (int)tileSize.y));
            }
            texture.Apply();
            return texture;
        }

        public static bool WriteImage(this Texture2D source, string filename, BabylonImageFormat format)
        {
            bool result = false;
            if (source == null) return result;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                byte[] bytes = null;
                if (format == BabylonImageFormat.PNG) {
                    bytes = source.EncodeToPNG();
                } else {
                    bytes = source.EncodeToJPG(ExporterWindow.exportationOptions.DefaultTextureQuality);
                }
                if (bytes != null) {
                    MemoryStream buffer = new MemoryStream(bytes);
                    buffer.Position = 0;
                    buffer.CopyTo(file, CopyToOptions.FlushFinal);
                    buffer.Close();
                    buffer.Dispose();
                    buffer = null;
                    bytes = null;
                    result = File.Exists(filename);
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }

        public static void ForceReadable(this Texture texture)
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            if (!String.IsNullOrEmpty(texturePath)) {
                var importTool = new BabylonTextureImporter(texturePath);
                if (!importTool.IsReadable()) {
                    importTool.SetReadable();
                    importTool.ForceUpdate();
                }
            }
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
        }
    }
}