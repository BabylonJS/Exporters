using System.Drawing.Imaging;

namespace Utilities
{
    public abstract class TextureOperation
    {
        string _name;

        public TextureOperation(string name)
        {
            _name = name ?? string.Empty;
        }

        public string Name => _name;
        public abstract void Apply(byte[] values, BitmapData infos);
    }
}
