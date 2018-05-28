using System.Collections.Generic;
using System.IO;

namespace Max2Babylon
{
    public class MaxScriptManager
    {
        public static void Export(string outputPath)
        {
            Export(InitParameters(outputPath));
        }

        public static void Export(ExportParameters exportParameters)
        {
            // Check output format is valid
            List<string> validFormats = new List<string>(new string[] { "babylon", "binary babylon", "gltf", "glb" });
            if (!validFormats.Contains(exportParameters.outputFormat))
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf("ERROR - Valid output formats are: "+ validFormats.ToArray().ToString(true) + "\n");
                return;
            }

            BabylonExporter exporter = new BabylonExporter();

            // Init log system
            exporter.OnWarning += (warning, rank) =>
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(warning+"\n");
            };
            exporter.OnError += (error, rank) =>
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(error + "\n");
            };
            exporter.OnMessage += (message, color, rank, emphasis) =>
            {
                // TODO - Add a log level parameter (Error, Warning, Message, Verbose)
                if (rank < 1)
                {
                    Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(message + "\n");
                }
            };
            
            // Start export
            exporter.Export(exportParameters);
        }

        public static ExportParameters InitParameters(string outputPath)
        {
            ExportParameters exportParameters = new ExportParameters();
            exportParameters.outputPath = outputPath;
            exportParameters.outputFormat = Path.GetExtension(outputPath).Substring(1);
            return exportParameters;
        }
    }
}
