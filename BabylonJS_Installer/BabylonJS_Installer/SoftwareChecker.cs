using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace BabylonJS_Installer
{
    class SoftwareChecker
    {
        private Dictionary<string, string[]> files = new Dictionary<string, string[]>() {
            { "Max", new string[] {
                "GDImageLibrary",
                "Max2Babylon",
                "Newtonsoft.Json",
                "SharpDX",
                "SharpDX.Mathematics",
                "TargaImage",
                "TQ.Texture"
            } },
            { "Maya", new string[] {
                "GDImageLibrary",
                "Maya2Babylon",
                "Newtonsoft.Json",
                "TargaImage",
                "TQ.Texture"
            } }
        };
        public Dictionary<string, string> libFolder = new Dictionary<string, string>()
        {
            { "Max", "bin\\assemblies" },
            { "Maya", "bin\\plug-ins" }
        };

        public MainForm form;

        public string checkPath(string software, string version, string year)
        {
            RegistryKey localKey;
            if (Environment.Is64BitOperatingSystem)
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            if (software == "Max")
            {
                try
                {
                    return localKey.OpenSubKey(@"SOFTWARE\Autodesk\3dsMax\" + version + ".0").GetValue("Installdir").ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }
            else if (software == "Maya")
            {
                try
                {
                    return localKey.OpenSubKey(@"SOFTWARE\Autodesk\Maya\" + year + @"\Setup\InstallPath").GetValue("MAYA_INSTALL_LOCATION").ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }
            return "";
        }

        public string checkExporterDate(string software, string path)
        {
            try
            {
                string lastUpdate = "";

                switch(software)
                {
                    case "Max":
                        lastUpdate = File.GetLastWriteTime(path + "bin\\assemblies\\Max2Babylon.dll").ToString();
                        break;
                    case "Maya":
                        lastUpdate = File.GetLastWriteTime(path + "bin\\plug-ins\\Max2Babylon.dll").ToString();
                        break;
                    default:
                        break;
                }

                if (lastUpdate.Substring(0, 10) == "01/01/1601") lastUpdate = "";
                return lastUpdate;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public void uninstallExporter(string soft, string version, string path)
        {
            this.form.goTab("");
            this.form.log("\n----- UNINSTALLING " + soft + " v" + version + " EXPORTER -----\n");
            int errors = 0;

            foreach (string file in this.files[soft])
            {
                try
                {
                    System.IO.File.Delete(path + this.libFolder[soft] + "\\" + file + ".dll");
                    this.form.log(file + ".dll deleted.");
                }
                catch(Exception ex)
                {
                    errors++;
                    this.form.log(
                        "Error while deleting the file : " + file + ".dll \n"
                        + "At : " + path + this.libFolder[soft] + "\\" + "\n"
                        + ex.Message);
                }
            }

            if(errors == 0) this.form.log("\n----- UNINSTALLING COMPLETE -----\n");
            else this.form.log("\n----- UNINSTALLING COMPLETE with " + errors + "errors -----\n");
        }
    }
}
