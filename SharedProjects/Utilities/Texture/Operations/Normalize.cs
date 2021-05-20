using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Utilities
{
    public class Normalize : TextureOperation
    {
        public Normalize(string name = null) : base(name ?? "N")
        {
        }

        public override void Apply(byte[] values, BitmapData infos)
        {
            int pixelSize = Image.GetPixelFormatSize(infos.PixelFormat) >> 3;
            int i = 0;
            int l = values.Length;
            if (i < l)
            {
                switch (infos.PixelFormat)
                {
                    case PixelFormat.Canonical:
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        {
                            do
                            {
                                var k = i;
                                int r = values[k++];
                                int g = values[k++];
                                int b = values[k];
                                var n = Math.Sqrt(r * r + g * g + b * b);
                                values[k--] = (byte)(b / n);
                                values[k--] = (byte)(g / n);
                                values[k] = (byte)(r / n);
                                i += pixelSize;
                            } while (i < l);
                            break;
                        }
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        {
                            do
                            {
                                var k = i + 1;
                                int r = values[k++];
                                int g = values[k++];
                                int b = values[k];
                                var n = Math.Sqrt(r * r + g * g + b * b);
                                values[k--] = (byte)(b / n);
                                values[k--] = (byte)(g / n);
                                values[k] = (byte)(r / n);
                                i += pixelSize;
                            } while (i < l);
                            break;
                        }
                    default:
                        throw new NotSupportedException($"Pixel format not supported :{infos.PixelFormat}");
                }
            }
        }
    }

}
