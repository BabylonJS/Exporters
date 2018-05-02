using System;
using System.IO;
using UnityEngine;
using FreeImageAPI;

namespace Unity3D2Babylon
{
	public static class ImageTools
	{
		public static byte[] EncodeToTGA(this Texture2D texture)
		{
			byte[] array = new byte[18 + texture.width * texture.height * 4];
			array[0] = 0;
			array[1] = 0;
			array[2] = 2;
			array[3] = 0;
			array[4] = 0;
			array[5] = 0;
			array[6] = 0;
			array[7] = 0;
			array[8] = 0;
			array[9] = 0;
			array[10] = 0;
			array[11] = 0;
			array[12] = (byte)texture.width;
			array[13] = (byte)(texture.width >> 8);
			array[14] = (byte)texture.height;
			array[15] = (byte)(texture.height >> 8);
			array[16] = 32;
			array[17] = 32;
			Color pixel = new Color(0f, 0f, 0f, 1f);
			for (int i = 0; i < texture.height; i++) {
				for (int j = 0; j < texture.width; j++) {
					pixel = texture.GetPixel(j, texture.height - 1 - i);
					int num = j * 4 + i * texture.width * 4 + 18;
					array[num] = (byte)(pixel.b * 255f);
					array[1 + num] = (byte)(pixel.g * 255f);
					array[2 + num] = (byte)(pixel.r * 255f);
					if (texture.format == TextureFormat.RGBA32) {
						array[3 + num] = (byte)(pixel.a * 255f);
					} else {
						array[3 + num] = 255;
					}
				}
			}
			return array;
		}

		public static byte[] EncodeToPNG16(this Texture2D source, bool grayscale = false)
		{
			MemoryStream bytes = new MemoryStream();
			Color[] pixels = source.GetPixels();
			Unity3D2Babylon.Tools.ImageInfo info = new Unity3D2Babylon.Tools.ImageInfo();
			info.pixelFormat = (grayscale == true) ? Unity3D2Babylon.Tools.PixelFormat.UINT16 : Unity3D2Babylon.Tools.PixelFormat.RGB16;
			info.freeImageType = (grayscale == true) ? FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT16 : FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGB16;
			FreeImageAPI.FREE_IMAGE_COLOR_DEPTH colorDepth = (grayscale == true) ? FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_FORCE_GREYSCALE : FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO;
			Unity3D2Babylon.Tools.WriteFreeImage(info, source.width, source.height, pixels, bytes, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG, colorDepth);
			return (bytes.Length > 0) ? bytes.ToArray() : null;
		}		
	}
}
