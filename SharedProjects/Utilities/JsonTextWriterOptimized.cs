using System;
using System.IO;
using Newtonsoft.Json;

namespace Utilities
{
    class JsonTextWriterOptimized : JsonTextWriter
    {
        internal const int DefaultNumberOfDigit = 8;

        int _d;

        public JsonTextWriterOptimized(TextWriter textWriter, int numberOfDigit = DefaultNumberOfDigit)
            : base(textWriter)
        {
            _d = numberOfDigit;
        }

        public override void WriteValue(float value)
        {
            value = (float)Math.Round(value, _d);
            base.WriteValue(value);
        }
    }
}
