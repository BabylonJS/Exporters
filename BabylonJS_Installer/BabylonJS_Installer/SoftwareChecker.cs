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
                "GDImageLibrary.dll",
                "Max2Babylon.dll",
                "Microsoft.WindowsAPICodePack.dll",
                "Microsoft.WindowsAPICodePack.Shell.dll",
                "Microsoft.WindowsAPICodePack.ShellExtensions.dll",
                "Newtonsoft.Json.dll",
                "SharpDX.dll",
                "SharpDX.Mathematics.dll",
                "TargaImage.dll",
                "TQ.Texture.dll"
            } },
            { "Maya", new string[] {
                "GDImageLibrary.dll",
                "Maya2Babylon.nll.dll",
                "Newtonsoft.Json.dll",
                "TargaImage.dll",
                "TQ.Texture.dll",
                "AEbabylonAiStandardSurfaceMaterialNodeTemplate.mel",
                "AEbabylonStandardMaterialNodeTemplate.mel",
                "AEbabylonStingrayPBSMaterialNodeTemplate.mel",
                "NEbabylonAiStandardSurfaceMaterialNodeTemplate.xml",
                "NEbabylonStandardMaterialNodeTemplate.xml",
                "NEbabylonStingrayPBSMaterialNodeTemplate.xml"
            } }
        };
        public Dictionary<string, string> libFolder = new Dictionary<string, string>()
        {
            { "Max", "bin\\assemblies" },
            { "Maya", "bin\\plug-ins" },
            { "MayaAE", "scripts\\AETemplates" },
            { "MayaNE", "scripts\\NETemplates" }
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

        public DateTime getInstalledExporterTimestamp(string software, string path)
        {
            try
            {
                switch(software)
                {
                    case "Max":
                        return File.GetLastWriteTime(path + "bin\\assemblies\\Max2Babylon.dll");
                    case "Maya":
                        return File.GetLastWriteTime(path + "bin\\plug-ins\\Maya2Babylon.nll.dll");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return DateTime.MinValue;
        }

        public void uninstallExporter(string soft, string version, string path)
        {
            this.form.goTab("");
            this.form.log("\n----- UNINSTALLING " + soft + " v" + version + " EXPORTER -----\n");
            int errors = 0;
            bool needElevatedProgram = false;
            string fileFullPath;

            if(soft == "Max")
            {
                Directory.CreateDirectory(path + "scripts\\Startup");
                File.WriteAllText(
                    path + "scripts\\Startup\\BabylonCleanUp.ms",
                    "/* Remove menu \"Babylon\" from Main menu bar */\n" +
                    "try (menuMan.unRegisterMenu(menuMan.findMenu \"Babylon\")) catch ()\n" +
                    "/* Remove item \"Babylon...\" from quad */\n" +
                    "try (\n" +
                        "quadMenu = menuMan.getViewportRightClickMenu #nonePressed\n" +
                        "menu = quadMenu.getMenu 1\n" +
                        "nbItems = menu.numItems()\n" +
                        "for i = 1 to nbItems do \n" +
                                             "(\n" +
                                                "item = menu.getItem i\n" +
                            "title = item.getTitle()\n" +
                            "if title == \"Babylon...\" do menu.removeItemByPosition i\n" +
                        ")\n" +
                    ")\n" +
                    "catch ()\n" +
                    "/* Self destruction */\n" +
                    "root = getdir #maxroot\n" +
                    "filePath = root + \"scripts\\Startup\\BabylonCleanUp.ms\"\n" +
                    "deleteFile filePath"
                );
            }

            foreach (string file in this.files[soft])
            {
                if (file.Substring(0, 9) == "AEbabylon") fileFullPath = path + this.libFolder[soft + "AE"] + "\\" + file;
                else if (file.Substring(0, 9) == "NEbabylon") fileFullPath = path + this.libFolder[soft + "NE"] + "\\" + file;
                else fileFullPath = path + this.libFolder[soft] + "\\" + file;

                try
                {
                    File.Delete(fileFullPath);
                    this.form.log(file + " deleted.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    needElevatedProgram = true;
                    errors++;
                    this.form.error("Cannot access file: " + fileFullPath);
                }
                catch (Exception ex)
                {
                    errors++;
                    this.form.error(
                        ex.GetType().ToString() + " error while deleting the file : " + file + "\n"
                        + "     At : " + fileFullPath + "\\" + "\n"
                        + "     " + ex.Message);
                }
            }

            if (errors == 0)
            {
                this.form.log("\n----- UNINSTALLING COMPLETE -----\n");
            }
            else
            {
                this.form.log("\n----- UNINSTALL FAILED with " + errors + " errors -----\n");
                if (needElevatedProgram)
                {
                    this.form.error(
                    "Please try to run this tool in ADMINISTRATOR MODE. It's necessary to remove files in \"Program Files\" folder (or other protected folders).\n\n"
                    + String.Format("If you are already running as administrator, Please close {0} {1} and retry uninstalling.\n", soft, version)
                    );
                }
            }

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
