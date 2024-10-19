using System;
using System.Collections.Generic;
using System.IO;

namespace Max2Babylon
{
    static class ScriptsUtilities
    {
        public static void ExecutePythonFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cmd = $@"python.ExecuteFile ""{filePath}""";
                ExecuteMaxScriptCommand(cmd);
            }
        }

        public static void ExecutePythonCommand(string pythonCmd)
        {
            string cmd = $@"python.Execute ""{pythonCmd}""";
            ExecuteMaxScriptCommand(cmd);
        }

        public static void ExecuteMaxScriptCommand(string maxScriptCmd)
        {
            if (!string.IsNullOrEmpty(maxScriptCmd))
            {
#if MAX2022 || MAX2023 || MAX2024 || MAX2025 || MAX2026
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(maxScriptCmd, ManagedServices.MaxscriptSDK.ScriptSource.NotSpecified);
#else
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(maxScriptCmd);
#endif
            }
        }

        public static void ExecuteMaxScriptFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string maxScriptCmd = File.ReadAllText(filePath);
                ExecuteMaxScriptCommand(maxScriptCmd);
            }
        }
    }
}
