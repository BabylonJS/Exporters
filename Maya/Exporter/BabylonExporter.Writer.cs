using BabylonExport.Entities;
using BabylonFileConverter;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        public void Write(BabylonScene babylonScene, string outputBabylonDirectory, string outputFileName, string outputFormat, bool generateManifest)
        {
            var outputFile = Path.Combine(outputBabylonDirectory, outputFileName);

            RaiseMessage("Saving to output file " + outputFile);

            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings());
            var sb = new StringBuilder();
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
                
            using (var jsonWriter = new JsonTextWriterOptimized(sw))
            {
                jsonWriter.Formatting = Formatting.None;
                jsonSerializer.Serialize(jsonWriter, babylonScene);
            }
            File.WriteAllText(outputFile, sb.ToString());

            if (generateManifest)
            {
                File.WriteAllText(outputFile + ".manifest",
                    "{\r\n\"version\" : 1,\r\n\"enableSceneOffline\" : true,\r\n\"enableTexturesOffline\" : true\r\n}");
            }

            // Binary
            if (outputFormat == "binary babylon")
            {
                RaiseMessage("Generating binary files");
                BinaryConverter.Convert(outputFile, outputBabylonDirectory + "\\Binary",
                    message => RaiseMessage(message, 1),
                    error => RaiseError(error, 1));
            }
        }
    }
}
