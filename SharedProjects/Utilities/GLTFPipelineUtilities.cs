using System;
using System.Diagnostics;
using System.IO;

namespace Utilities
{
    class GLTFPipelineUtilities
    {
        public static bool IsGLTFPipelineInstalled()
        {
            try
            {
                Process gltfPipeline = new Process();
                gltfPipeline.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                gltfPipeline.StartInfo.FileName = "cmd.exe";
                gltfPipeline.StartInfo.Arguments = "/C echo \"Checking for gltf-pipeline installation\" && gltf-pipeline --version";
                
                gltfPipeline.Start();
                gltfPipeline.WaitForExit();

                return gltfPipeline.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static void DoDracoCompression(ILoggingProvider logger, bool generateBinary, string outputFile)
        {
            Action onError = delegate
                {
                    logger.RaiseError("gltf-pipeline module not found.", 1);
                    logger.RaiseError("The exported file wasn't compressed.");
                };
            try
            {
                Process gltfPipeline = new Process();

                // Hide the cmd window that show the gltf-pipeline result
                //gltfPipeline.StartInfo.UseShellExecute = false;
                //gltfPipeline.StartInfo.CreateNoWindow = true;
                gltfPipeline.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                string arg;
                if (generateBinary)
                {
                    string outputGlbFile = Path.ChangeExtension(outputFile, "glb");
                    arg = $"/C gltf-pipeline -i {outputGlbFile} -o {outputGlbFile} -d";
                }
                else
                {
                    string outputGltfFile = Path.ChangeExtension(outputFile, "gltf");
                    arg = $"/C gltf-pipeline -i {outputGltfFile} -o {outputGltfFile} -d -s";
                }
                gltfPipeline.StartInfo.FileName = "cmd.exe";
                gltfPipeline.StartInfo.Arguments = arg;

                gltfPipeline.Start();
                gltfPipeline.WaitForExit();

                if (gltfPipeline.ExitCode != 0)
                {
                    onError();
                }
            }
            catch
            {
                onError();
            }
        }
    }
}
