using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Utilities
{
    public class FlipChannel : TextureOperation
    {
        public const int ChannelRed = 0;
        public const int ChannelGreen = 1;
        public const int ChannelBlue = 2;

        int _channel;

        /// <summary>
        /// Invert channel (R,G or B) using value = MAX_CHANNEL_VALUE - value.
        /// </summary>
        /// <param name="channel">R = 0, G = 1, B = 2</param>
        /// <param name="name"></param>
        /// <Exception name="ArgumentOutOfRangeException">channel value is invalid.</Exception>
        public FlipChannel(int channel, string name = null) : base(name ?? $"i{channel}")
        {
            if (channel < ChannelRed || channel > ChannelBlue)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }
            _channel = channel;
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
                            i += _channel;
                            do
                            {
                                values[i] = (byte)(0xFF - values[i]);
                                i += pixelSize;
                            } while (i < l);
                            break;
                        }
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        {
                            i += (_channel + 1);
                            do
                            {
                                values[i] = (byte)(0xFF - values[i]);
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
