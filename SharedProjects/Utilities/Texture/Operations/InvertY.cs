using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Utilities
{
    public class InvertY : TextureOperation
    {
        public InvertY(string name = null) : base(name ?? "fy")
        {
        }

        public override void Apply(byte[] values, BitmapData infos)
        {
            int stride = infos.Stride;
            int from = 0;
            int to = (infos.Height - 1) * stride;

            if (from < to)
            {
                byte[] tmp = new byte[infos.Stride];
                do
                {
                    Array.Copy(values, to, tmp, 0, infos.Stride);
                    Array.Copy(values, from, values, to, infos.Stride);
                    Array.Copy(tmp, 0, values, from, infos.Stride);
                    from += stride;
                    to -= stride;
                } while (from < to);
            }
        }
    }

}
