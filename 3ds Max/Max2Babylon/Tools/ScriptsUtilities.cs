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
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(maxScriptCmd);
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
