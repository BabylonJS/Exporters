using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Utilities
{
    public class SwapChannel : TextureOperation
    {
        int _channelA;
        int _channelB;
        /// <summary>
        /// Invert channel (R,G or B) using value = MAX_CHANNEL_VALUE - value.
        /// </summary>
        /// <param name="channel">R = 0, G = 1, B = 2</param>
        /// <param name="name"></param>
        /// <Exception name="ArgumentOutOfRangeException">channel value is invalid.</Exception>
        public SwapChannel(int channelA, int channelB, string name = null) : base(name ?? $"s{channelA}{channelB}")
        {
            if (channelA < 0 || channelA > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(channelA));
            }
            if (channelB < 0 || channelB > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(channelB));
            }
            _channelA = channelA;
            _channelB = channelB;
        }

        public override void Apply(byte[] values, BitmapData infos)
        {
            if (_channelA == _channelB)
            {
                return;
            }
            int pixelSize = Image.GetPixelFormatSize(infos.PixelFormat) >> 3;
            int i = 0;
            int j = 0;
            int l = values.Length;
            if (i < l)
            {
                switch (infos.PixelFormat)
                {
                    case PixelFormat.Canonical:
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        {
                            i += _channelA;
                            j += _channelB;
                            do
                            {
                                byte tmp = values[i];
                                values[i] = values[j];
                                values[j] = tmp;
                                i += pixelSize;
                                j += pixelSize;
                            } while (i < l);
                            break;
                        }
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        {
                            i += (_channelA + 1);
                            j += (_channelB + 1);
                            do
                            {
                                byte tmp = values[i];
                                values[i] = values[j];
                                values[j] = tmp;
                                i += pixelSize;
                                j += pixelSize;
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
