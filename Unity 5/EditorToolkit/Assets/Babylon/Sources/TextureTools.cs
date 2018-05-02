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

    public static class TextureTools
    {
        public static void Destroy(this Texture2D source, float delay = 0.0f)
        {
            if (delay > 0.0f) Object.Destroy(source, delay);
            else Object.Destroy(source);
        }
        
        public static void Clear(this Texture2D source, Color color)
        {
            if (source == null) return;
            int index = 0;
            Color[] pixels = new Color[source.width * source.height];
            for (int y=0; y < source.height; y++) 
            {
                for (int x=0; x < source.width; x++) 
                {
                    pixels[index] = color;
                    index++;
                }
            }
            source.SetPixels(pixels);
            source.Apply();
        }
        
        public static Texture2D Copy(this Texture2D source, TextureFormat format = TextureFormat.RGBA32, CopyFilterMode filter = CopyFilterMode.Source, bool getSafePixels = false)
        {
            if (source == null) return null;
            Color[] pixels = (getSafePixels == true) ? source.GetSafePixels() : source.GetPixels();
            var result = new Texture2D(source.width, source.height, format, false);
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
            if (pixels != null) {
                result.SetPixels(pixels);
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

        public static Texture2D Crop(this Texture2D source, Rect area, TextureFormat format = TextureFormat.RGBA32, CopyFilterMode filter = CopyFilterMode.Source)
        {
            if (source == null) return null;
            int top = (int)area.yMin;
            int left = (int)area.xMin;
            int width = (int)area.width;
            int height = (int)area.height;
            Texture2D result = new Texture2D(width, height, format, false);
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
            Color[] pixels = source.GetPixels(left, top, width, height);
            if (pixels != null) {
                result.SetPixels(pixels);
                result.Apply();
            }
            return result;
        }

        public static void NineCrop(this Texture2D source, int gutter, TextureFormat format = TextureFormat.RGBA32)
        {
            if (source.width != source.height) throw new Exception("Failed to nine crop image, source image width and height must be equal.");
            int realSize = source.width;
            int realOffset = source.width - 1;
            int canvasWidth = source.width * 3;
            int canvasHeight = source.width * 3;
            Color[] canvasPixels = source.GetPixels(0, 0, realSize, realSize);
            Texture2D canvasTexture = new Texture2D(canvasWidth, canvasHeight, format, false);
            canvasTexture.SetPixels((realOffset * 0), (realOffset * 0), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 0), (realOffset * 1), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 0), (realOffset * 2), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 1), (realOffset * 0), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 1), (realOffset * 1), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 1), (realOffset * 2), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 2), (realOffset * 0), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 2), (realOffset * 1), realSize, realSize, canvasPixels);
            canvasTexture.SetPixels((realOffset * 2), (realOffset * 2), realSize, realSize, canvasPixels);
            canvasTexture.Apply();
            int cropSize = realSize * 2;
            int cropOffset = (canvasWidth - cropSize) / 2;
            Color[] centerPixels = canvasTexture.GetPixels(cropOffset, cropOffset, cropSize, cropSize);
            Texture2D centerTexture = new Texture2D(cropSize, cropSize, format, false);
            centerTexture.SetPixels(centerPixels);
            centerTexture.Apply();
            if (gutter > 0) {
                int gutterSize = realSize - (gutter * 2);
                int gutterOffset = (gutter - 1);
                centerTexture.Scale(gutterSize, gutterSize);
                Color[] gutterPixels = centerTexture.GetPixels(0, 0, gutterSize, gutterSize);
                var gutterTexture = new Texture2D(realSize, realSize, format, false);
                gutterTexture.Clear(Color.green);
                gutterTexture.SetPixels(gutterOffset, gutterOffset, gutterSize, gutterSize, gutterPixels);
                gutterTexture.Apply();
                source.Resize(realSize, realSize);
                source.SetPixels(gutterTexture.GetPixels());
                source.Apply();
            } else {
                centerTexture.Scale(realSize, realSize);
                source.Resize(realSize, realSize);
                source.SetPixels(centerTexture.GetPixels());
                source.Apply();
            }
        }

        public static Texture2D Blur(this Texture2D source, int blurSize, int iterations = 2, TextureFormat format = TextureFormat.RGBA32)
        {
            float avgR = 0, avgG = 0, avgB = 0, avgA = 0, blurPixelCount = 0;
            return FastBlur(source, format, blurSize, iterations, ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
        }
        private static Texture2D FastBlur(Texture2D image, TextureFormat format, int radius, int iterations, ref float avgR, ref float avgG, ref float avgB, ref float avgA, ref float blurPixelCount)
        {
            Texture2D tex = image;
            for (var i = 0; i < iterations; i++) {
                tex = BlurImage(tex, format, radius, true, ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
                tex = BlurImage(tex, format, radius, false, ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
            }
            return tex;
        }
        private static Texture2D BlurImage(Texture2D image, TextureFormat format, int blurSize, bool horizontal, ref float avgR, ref float avgG, ref float avgB, ref float avgA, ref float blurPixelCount)
        {
            Texture2D blurred = new Texture2D(image.width, image.height, format, false);
            int _W = image.width;
            int _H = image.height;
            int xx, yy, x, y;
            if (horizontal) {
                for (yy = 0; yy < _H; yy++) {
                    for (xx = 0; xx < _W; xx++) {
                        ResetPixel(ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);

                        //Right side of pixel
                        for (x = xx; (x < xx + blurSize && x < _W); x++) {
                            AddPixel(image.GetPixel(x, yy), ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
                        }
        
                        //Left side of pixel
                        for (x = xx; (x > xx - blurSize && x > 0); x--) {
                            AddPixel(image.GetPixel(x, yy), ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
                        }
        
                        CalcPixel(ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
        
                        for (x = xx; x < xx + blurSize && x < _W; x++) {
                            blurred.SetPixel(x, yy, new Color(avgR, avgG, avgB, 1.0f));
                        }
                    }
                }
            } else {
                for (xx = 0; xx < _W; xx++) {
                    for (yy = 0; yy < _H; yy++) {
                        ResetPixel(ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
        
                        //Over pixel
                        for (y = yy; (y < yy + blurSize && y < _H); y++) {
                            AddPixel(image.GetPixel(xx, y), ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
                        }
        
                        //Under pixel
                        for (y = yy; (y > yy - blurSize && y > 0); y--) {
                            AddPixel(image.GetPixel(xx, y), ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
                        }
        
                        CalcPixel(ref avgR, ref avgG, ref avgB, ref avgA, ref blurPixelCount);
        
                        for (y = yy; y < yy + blurSize && y < _H; y++) {
                            blurred.SetPixel(xx, y, new Color(avgR, avgG, avgB, 1.0f));
                        }
                    }
                }
            }
            blurred.Apply();
            return blurred;
        }
        private static void AddPixel(Color pixel, ref float avgR, ref float avgG, ref float avgB, ref float avgA, ref float blurPixelCount)
        {
            avgR += pixel.r;
            avgG += pixel.g;
            avgB += pixel.b;
            blurPixelCount++;
        }
        private static void ResetPixel(ref float avgR, ref float avgG, ref float avgB, ref float avgA, ref float blurPixelCount)
        {
            avgR = 0.0f;
            avgG = 0.0f;
            avgB = 0.0f;
            blurPixelCount = 0;
        }
        private static void CalcPixel(ref float avgR, ref float avgG, ref float avgB, ref float avgA, ref float blurPixelCount)
        {
            avgR = avgR / blurPixelCount;
            avgG = avgG / blurPixelCount;
            avgB = avgB / blurPixelCount;
        }
        private static Texture2D LegacyBlur(Texture2D image, TextureFormat format, int blurSize)
        {
            Texture2D blurred = new Texture2D(image.width, image.height, format, false);
            // look at every pixel in the blur rectangle
            for (int xx = 0; xx < image.width; xx++)
            {
                for (int yy = 0; yy < image.height; yy++)
                {
                    float avgR = 0, avgG = 0, avgB = 0, avgA = 0;
                    int blurPixelCount = 0;
                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.height); y++)
                        {
                            Color pixel = image.GetPixel(x, y);
        
                            avgR += pixel.r;
                            avgG += pixel.g;
                            avgB += pixel.b;
                            avgA += pixel.a;
        
                            blurPixelCount++;
                        }
                    }
                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;
                    avgA = avgA / blurPixelCount;
                    // now that we know the average for the blur size, set each pixel to that color
                    for (int x = xx; x < xx + blurSize && x < image.width; x++)
                        for (int y = yy; y < yy + blurSize && y < image.height; y++)
                            blurred.SetPixel(x, y, new Color(avgR, avgG, avgB, avgA));
                }
            }
            blurred.Apply();
            return blurred;
        }

        public static bool WriteImage(this Texture2D source, string filename, BabylonImageFormat format)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
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
                    if (result == false) {
                        throw new Exception("Failed to save texture file to disk: " + filename);
                    }
                } else {
                    throw new Exception("Failed to encode texture image to disk: " + filename);
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }

        public static bool WriteImageTGA(this Texture2D source, string filename)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                byte[] bytes = source.EncodeToTGA();
                if (bytes != null) {
                    MemoryStream buffer = new MemoryStream(bytes);
                    buffer.Position = 0;
                    buffer.CopyTo(file, CopyToOptions.FlushFinal);
                    buffer.Close();
                    buffer.Dispose();
                    buffer = null;
                    bytes = null;
                    result = File.Exists(filename);
                    if (result == false) {
                        throw new Exception("Failed to save texture file to disk: " + filename);
                    }
                } else {
                    throw new Exception("Failed to encode texture image to disk: " + filename);
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }

        public static bool WriteImageEXR(this Texture2D source, string filename)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                byte[] bytes = source.EncodeToEXR();
                if (bytes != null) {
                    MemoryStream buffer = new MemoryStream(bytes);
                    buffer.Position = 0;
                    buffer.CopyTo(file, CopyToOptions.FlushFinal);
                    buffer.Close();
                    buffer.Dispose();
                    buffer = null;
                    bytes = null;
                    result = File.Exists(filename);
                    if (result == false) {
                        throw new Exception("Failed to save texture file to disk: " + filename);
                    }
                } else {
                    throw new Exception("Failed to encode texture image to disk: " + filename);
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }
        
        public static bool WriteImageHDR(this Texture2D source, string filename)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                Color[] pixels = source.GetPixels();
                Unity3D2Babylon.Tools.ImageInfo info = new Unity3D2Babylon.Tools.ImageInfo();
                info.pixelFormat = Unity3D2Babylon.Tools.PixelFormat.RGBAF;
                info.freeImageType = FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGBAF;
                FreeImageAPI.FREE_IMAGE_COLOR_DEPTH colorDepth = FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO;
                Unity3D2Babylon.Tools.WriteFreeImage(info, source.width, source.height, pixels, file, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_HDR, colorDepth);
                result = File.Exists(filename);
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }

        public static bool WriteImageEXR16(this Texture2D source, string filename)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                Color[] pixels = source.GetPixels();
                Unity3D2Babylon.Tools.ImageInfo info = new Unity3D2Babylon.Tools.ImageInfo();
                info.pixelFormat = Unity3D2Babylon.Tools.PixelFormat.RGBA16;
                info.freeImageType = FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGBA16;
                FreeImageAPI.FREE_IMAGE_COLOR_DEPTH colorDepth = FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO;
                Unity3D2Babylon.Tools.WriteFreeImage(info, source.width, source.height, pixels, file, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_EXR, colorDepth);
                result = File.Exists(filename);
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }
        
        
        public static bool WriteImagePNG16(this Texture2D source, string filename, bool grayscale = false)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                Color[] pixels = source.GetPixels();
                Unity3D2Babylon.Tools.ImageInfo info = new Unity3D2Babylon.Tools.ImageInfo();
                info.pixelFormat = (grayscale == true) ? Unity3D2Babylon.Tools.PixelFormat.UINT16 : Unity3D2Babylon.Tools.PixelFormat.RGB16;
                info.freeImageType = (grayscale == true) ? FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT16 : FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGB16;
                FreeImageAPI.FREE_IMAGE_COLOR_DEPTH colorDepth = (grayscale == true) ? FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_FORCE_GREYSCALE : FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO;
                Unity3D2Babylon.Tools.WriteFreeImage(info, source.width, source.height, pixels, file, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG, colorDepth);
                result = File.Exists(filename);
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }

        public static bool WriteHeightmapRAW(this Texture2D source, string filename, bool flip = true)
        {
            if (source == null) {
                UnityEngine.Debug.LogException(new Exception("No source image to save to disk: " + filename));
                return false;
            } 
            bool result = false;
            FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try {
                Texture2D raw = (flip) ? Unity3D2Babylon.Tools.FlipTexture(source) : source;
                Color[] pixels = raw.GetPixels();
                Unity3D2Babylon.Tools.WriteRawHeightmapImage(pixels, file);
                result = File.Exists(filename);
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                file.Close();
            }
            return result;
        }

        public static bool HasAlpha(this Texture2D source, bool getSafePixels = false)
        {
            bool hasAlpha = source.alphaIsTransparency;
            Color[] apixels = (getSafePixels == true) ? source.GetSafePixels(new Rect(0, 0, source.width, source.height)) : source.GetPixels(0, 0, source.width, source.height);
            for (int index = 0; index < apixels.Length; index++) {
                hasAlpha |= apixels[index].a <= 0.99999f;
            }
            return hasAlpha;
        }

        public static void MakeAlpha(this Texture2D source, float alpha = 1.0f, bool getSafePixels = false)
        {
            if (source == null) return;
            Color[] colors = (getSafePixels == true) ? source.GetSafePixels(): source.GetPixels();
            Color[] pixels = new Color[source.width * source.height];
            for (int index=0; index < pixels.Length; index++)  {
                pixels[index].r = Mathf.Clamp(colors[index].r, 0.0f, 1.0f);
                pixels[index].g = Mathf.Clamp(colors[index].g, 0.0f, 1.0f);
                pixels[index].b = Mathf.Clamp(colors[index].b, 0.0f, 1.0f);
                pixels[index].a = alpha;
            }
            source.SetPixels(pixels);
            source.Apply();
        }

        public static void ScaleAlpha(this Texture2D source, float scale, bool getSafePixels = false)
        {
            if (source == null) return;
            Color[] colors = (getSafePixels == true) ? source.GetSafePixels(): source.GetPixels();
            for (int index=0; index < colors.Length; index++)  {
                colors[index].a = Mathf.Clamp((colors[index].a * scale), 0.0f, 1.0f);
            }
            source.SetPixels(colors);
            source.Apply();
        }
        
        public static void InvertAlpha(this Texture2D source, bool getSafePixels = false)
        {
            if (source == null) return;
            Color[] colors = (getSafePixels == true) ? source.GetSafePixels(): source.GetPixels();
            for (int index=0; index < colors.Length; index++)  {
                colors[index].a = (1.0f - colors[index].a);
            }
            source.SetPixels(colors);
            source.Apply();
        }

        public static void MakeGrayscale(this Texture2D source, bool getSafePixels = false)
        {
            if (source == null) return;
            Color[] colors = (getSafePixels == true) ? source.GetSafePixels(): source.GetPixels();
            Color[] pixels = new Color[source.width * source.height];
            for (int index=0; index < pixels.Length; index++)  {
                float gray = colors[index].grayscale;
                pixels[index] = new Color(gray, gray, gray, 1.0f);
            }
            source.SetPixels(pixels);
            source.Apply();
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

        public static bool IsSRGB(this Texture2D texture)
        {
            bool result = true;
            string srcTexturePath = AssetDatabase.GetAssetPath(texture);
            if (!String.IsNullOrEmpty(srcTexturePath)) {
                var importTool = new BabylonTextureImporter(srcTexturePath);
                if (importTool.textureImporter != null) {
                    result = importTool.textureImporter.sRGBTexture;
                }
            }
            return result;
        }

        public static Color[] GetSafePixels(this Texture2D texture, Rect? crop = null)
        {
            Color[] pixels = null;
            string srcTexturePath = AssetDatabase.GetAssetPath(texture);
            var importTool = new BabylonTextureImporter(srcTexturePath);
            bool isReadable = importTool.IsReadable();
            if (!isReadable) importTool.SetReadable();
            try
            {
                if (crop != null && crop.HasValue) {
                    pixels = texture.GetPixels((int)crop.Value.x, (int)crop.Value.y, (int)crop.Value.width, (int)crop.Value.height);
                } else {
                    pixels = texture.GetPixels();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                if (!isReadable) importTool.ForceUpdate();
            }
            return pixels;
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
        }

        ///////////////////////////////////////////////////////////////////
        //
        // Convert Six Faces To Spherical Image
        //
        ///////////////////////////////////////////////////////////////////

        public const int LEFT   = 0;
        public const int RIGHT  = 1;
        public const int TOP    = 2;
        public const int FRONT  =  3;
        public const int BACK   =  4;
        public const int DOWN   =  5;

        public class XYZ
        {
            public double x, y, z;
        }
        public class PLANE
        {
            public double a, b, c, d;
        }
        public class PARAMS
        {
            public int outwidth, outheight; // Dimensions of spherical projection
            public int width1, width2;      // Dimensions for sub images
            public int antialias;           // Supersampling antialising
            public double thetay;           // Rotation about up axis
	        public bool eac;                // Equalangular cubemaps
            public PLANE[] faces;
            public bool usesine;
            
            public PARAMS(int width, int height, int antialias = 2, double rotation = -90, bool usesine = false, bool eac = false)
            {
                this.outwidth = width;
                this.outheight = height;
                this.width1 = 0;
                this.width2 = this.outwidth;
                this.antialias = antialias;
                this.usesine = usesine;
                this.eac = eac;
                this.thetay = rotation * (Math.PI / 180);
                this.faces = new TextureTools.PLANE[6];
                // ..
                // Parameters for the 6 cube planes, ax + by +cz + d = 0
                // ..
                this.faces[TextureTools.LEFT] = new TextureTools.PLANE();
                this.faces[TextureTools.LEFT].a   =  1.0f; 
                this.faces[TextureTools.LEFT].b   =  0.0f;
                this.faces[TextureTools.LEFT].c   =  0.0f;
                this.faces[TextureTools.LEFT].d   = -1.0f;
                this.faces[TextureTools.RIGHT] = new TextureTools.PLANE();
                this.faces[TextureTools.RIGHT].a  = -1.0f; 
                this.faces[TextureTools.RIGHT].b  =  0.0f;
                this.faces[TextureTools.RIGHT].c  =  0.0f;
                this.faces[TextureTools.RIGHT].d  = -1.0f;
                this.faces[TextureTools.TOP] = new TextureTools.PLANE();
                this.faces[TextureTools.TOP].a    =  0.0f;
                this.faces[TextureTools.TOP].b    =  1.0f; 
                this.faces[TextureTools.TOP].c    =  0.0f;
                this.faces[TextureTools.TOP].d    = -1.0f;
                this.faces[TextureTools.DOWN] = new TextureTools.PLANE();
                this.faces[TextureTools.DOWN].a   =  0.0f;
                this.faces[TextureTools.DOWN].b   = -1.0f; 
                this.faces[TextureTools.DOWN].c   =  0.0f;
                this.faces[TextureTools.DOWN].d   = -1.0f;
                this.faces[TextureTools.FRONT] = new TextureTools.PLANE();
                this.faces[TextureTools.FRONT].a  =  0.0f;
                this.faces[TextureTools.FRONT].b  =  0.0f;
                this.faces[TextureTools.FRONT].c  =  1.0f; 
                this.faces[TextureTools.FRONT].d  = -1.0f;
                this.faces[TextureTools.BACK] = new TextureTools.PLANE();
                this.faces[TextureTools.BACK].a   =  0.0f;
                this.faces[TextureTools.BACK].b   =  0.0f;
                this.faces[TextureTools.BACK].c   = -1.0f; 
                this.faces[TextureTools.BACK].d   = -1.0f;
            }
        }

        public static XYZ RotateSphericalY(XYZ p, double theta)
        {
            XYZ q = new XYZ();
            q.x = p.x * Math.Cos(theta) - p.z * Math.Sin(theta);
            q.y = p.y;
            q.z = p.x * Math.Sin(theta) + p.z * Math.Cos(theta);
            return(q);
        }

        public static void CreateSphericalData(int width, int height, PARAMS parameters, ref Color[] spherical, ref Color[] left, ref Color[] right, ref Color[] front, ref Color[] back, ref Color[] down, ref Color[] top)
        {
            int i, j, k, aj, ai, index, found, u = 0, v = 0;
            double x, y, longitude, latitude, denom, mu;
            double rsum, gsum, bsum;
            XYZ p = new XYZ();
            p.x = 0;
            p.y = 0;
            p.z = 0;
            XYZ q = new XYZ();
            q.x = 0;
            q.y = 0;
            q.z = 0;

            for (j = 0; j < parameters.outheight; j++)
            {
                for (i = 0; i < parameters.outwidth; i++)
                {
                    if (i < parameters.width1 || i >= parameters.width2) // Subset image
                        continue;

                    // Supersampling antialising sum
                    rsum = 0;
                    gsum = 0;
                    bsum = 0;

                    // Antialiasing loops 
                    for (ai = 0; ai < parameters.antialias; ai++)
                    {
                        x = (i + ai / (double)parameters.antialias) / (double)parameters.outwidth; // 0 ... 1

                        for (aj = 0; aj < parameters.antialias; aj++)
                        {
                            y = 2 * (j + aj / (double)parameters.antialias) / (double)parameters.outheight - 1; // -1 ... 1

                            // Calculate latitude and longitude
                            if (parameters.usesine) latitude = Math.Asin(y);
                            else latitude = y * 0.5 * Math.PI;      // -pi/2 ... pi/2 
                            longitude = x * 2 * Math.PI;      // 0 ... 2pi

                            // p is the ray from the camera position into the scene 
                            p.x = Math.Cos(latitude) * Math.Cos(longitude);
                            p.y = Math.Sin(latitude);
                            p.z = Math.Cos(latitude) * Math.Sin(longitude);

                            // Apply rotation aboutup vector
                            p = TextureTools.RotateSphericalY(p, parameters.thetay);

                            // Find which face the vector intersects 
                            found = -1;
                            for (k = 0; k < 6; k++)
                            {
                                denom = -(parameters.faces[k].a * p.x + parameters.faces[k].b * p.y + parameters.faces[k].c * p.z);

                                // Is p parallel to face? Shouldn't actually happen.
                                if (Math.Abs(denom) < 0.000001)
                                    continue;

                                // Is the intersection on the back pointing ray? 
                                if ((mu = parameters.faces[k].d / denom) < 0)
                                    continue;

                                // q is the intersection point 
                                q.x = mu * p.x;
                                q.y = mu * p.y;
                                q.z = mu * p.z;

                                // Find out which face it is on 
                                switch (k)
                                {
                                    case LEFT:
                                    case RIGHT:
                                        if (q.y <= 1 && q.y >= -1 && q.z <= 1 && q.z >= -1)
                                            found = k;
                                        if (parameters.eac)
                                        {
                                            q.y = Math.Atan(q.y) * 4 / Math.PI;
                                            q.z = Math.Atan(q.z) * 4 / Math.PI;
                                        }
                                        break;
                                    case FRONT:
                                    case BACK:
                                        if (q.x <= 1 && q.x >= -1 && q.y <= 1 && q.y >= -1)
                                            found = k;
                                        if (parameters.eac)
                                        {
                                            q.x = Math.Atan(q.x) * 4 / Math.PI;
                                            q.y = Math.Atan(q.y) * 4 / Math.PI;
                                        }
                                        break;
                                    case TOP:
                                    case DOWN:
                                        if (q.x <= 1 && q.x >= -1 && q.z <= 1 && q.z >= -1)
                                            found = k;
                                        if (parameters.eac)
                                        {
                                            q.x = Math.Atan(q.x) * 4 / Math.PI;
                                            q.z = Math.Atan(q.z) * 4 / Math.PI;
                                        }
                                        break;
                                }
                                if (found >= 0)
                                    break;
                            }
                            if (found < 0 || found > 5)
                            {
                                UnityEngine.Debug.LogWarning("TEXTURE: Didn't find an intersecting face - shouldn't happen!");
                                continue;
                            }

                            // Determine the u,v coordinate 
                            switch (found)
                            {
                                case LEFT:
                                    u = (int)(0.5 * width * (q.z + 1));
                                    v = (int)(0.5 * height * (q.y + 1));
                                    break;
                                case RIGHT:
                                    u = (int)(0.5 * width * (1 - q.z));
                                    v = (int)(0.5 * height * (q.y + 1));
                                    break;
                                case FRONT:
                                    u = (int)(0.5 * width * (1 - q.x));
                                    v = (int)(0.5 * height * (q.y + 1));
                                    break;
                                case BACK:
                                    u = (int)(0.5 * width * (q.x + 1));
                                    v = (int)(0.5 * height * (q.y + 1));
                                    break;
                                case DOWN:
                                    u = (int)(0.5 * width * (1 - q.x));
                                    v = (int)(0.5 * height * (1 + q.z));
                                    break;
                                case TOP:
                                    u = (int)(0.5 * width * (1 - q.x));
                                    v = (int)(0.5 * height * (1 - q.z));
                                    break;
                            }
                            if (u >= width)
                                u = width - 1;
                            if (v >= height)
                                v = height - 1;
                            if (u < 0 || u >= width || v < 0 || v >= height)
                            {
                                UnityEngine.Debug.LogWarning(String.Format("TEXTURE: Illegal (u,v) coordinate ({1},{2}) on face {3}", u, v, found));
                                continue;
                            }

                            // Sum over the supersampling set 
                            index = v * width + u;
                            switch (found)
                            {
                                case LEFT:
                                    rsum += left[index].r;
                                    gsum += left[index].g;
                                    bsum += left[index].b;
                                    break;
                                case RIGHT:
                                    rsum += right[index].r;
                                    gsum += right[index].g;
                                    bsum += right[index].b;
                                    break;
                                case FRONT:
                                    rsum += front[index].r;
                                    gsum += front[index].g;
                                    bsum += front[index].b;
                                    break;
                                case BACK:
                                    rsum += back[index].r;
                                    gsum += back[index].g;
                                    bsum += back[index].b;
                                    break;
                                case DOWN:
                                    rsum += down[index].r;
                                    gsum += down[index].g;
                                    bsum += down[index].b;
                                    break;
                                case TOP:
                                    rsum += top[index].r;
                                    gsum += top[index].g;
                                    bsum += top[index].b;
                                    break;
                            }
                        }
                    }
                    
                    // Finally update the spherical image
                    index = j * (parameters.width2 - parameters.width1) + i - parameters.width1;
                    spherical[index].r = (float)rsum / (parameters.antialias * parameters.antialias);
                    spherical[index].g = (float)gsum / (parameters.antialias * parameters.antialias);
                    spherical[index].b = (float)bsum / (parameters.antialias * parameters.antialias);
                }
            }
        }
    }
}
