using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BabylonJS_Installer
{
    class SoftwareChecker
    {
        string latestVersionDate;
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
                "Maya2Babylon.nll",
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
                        lastUpdate = File.GetLastWriteTime(path + "bin\\plug-ins\\Maya2Babylon.nll.dll").ToString();
                        break;
                    default:
                        this.form.error("Error : software not found");
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
                    this.form.error(
                        "Error while deleting the file : " + file + ".dll \n"
                        + "At : " + path + this.libFolder[soft] + "\\" + "\n"
                        + ex.Message);
                }
            }

            if(errors == 0) this.form.log("\n----- UNINSTALLING COMPLETE -----\n");
            else this.form.log("\n----- UNINSTALLING COMPLETE with " + errors + "errors -----\n");

            this.form.displayInstall(soft, version);
        }

        public void setLatestVersionDate()
        {
            Downloader downloader = new Downloader();
            Task<string> jsonRequest = Task.Run(async () => { return await downloader.GetJSONBodyRequest(downloader.GetURLGitHubAPI()); });
            //TO DO Find a better way to parse JSON aswell
            string json = jsonRequest.Result;
            string created_at = json.Substring(json.IndexOf("\"created_at\":"));
            created_at = created_at.Remove(created_at.IndexOf("\","));
            this.latestVersionDate = created_at.Remove(0, "\"created_at\":\"".Length);
        }

        public bool isLatestVersionInstalled(string soft, string version, string location)
        {// To ensure latest version, we compare between last modified time of files and the publish date of github release
            var latest = DateTime.Parse(this.latestVersionDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            var isLatestversion = false;
            switch (soft)
            {
                case "Max":
                    if (latest <= File.GetLastWriteTime(location + "bin\\assemblies\\Max2Babylon.dll")) isLatestversion = true;
                    break;

                case "Maya":
                    if (latest <= File.GetLastWriteTime(location + "bin\\plug-ins\\Maya2Babylon.nll.dll")) isLatestversion = true;
                    break;

                default:
                    this.form.error("Error : software not found");
                    break;
            }
            return isLatestversion;
        }

        public bool ensureAdminMode()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
