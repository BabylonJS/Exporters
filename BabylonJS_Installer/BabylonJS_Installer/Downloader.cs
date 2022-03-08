using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BabylonJS_Installer
{
    class Downloader
    {
        private static readonly string Url_github = "github.com";
        private static readonly string Api_url_github = $"api.{Url_github}";
        private static readonly string Url_download = $"https://{Url_github}/BabylonJS/Exporters/releases/download";
        private static readonly string Url_github_API_releases = $"https://{Api_url_github}/repos/BabylonJS/Exporters/releases";
        
        private string software = "";
        private string version = "";
        private string installDir = "";
        private string installLibSubDir = "";
        private string latestRelease = "";

        public MainForm form;

        public async Task UpdateAsync(string software, string version, string installDir, string installLibSubDir)
        {
            this.form.goTab("");
            this.form.log("\n----- INSTALLING / DOWNLOADING " + software + " v" + version + " EXPORTER -----\n");

            this.software = software;
            this.version = version;
            this.installDir = installDir;
            this.installLibSubDir = installLibSubDir;

            Action logPostInstall = () =>
            {
                if (software == "Max" && version == "2020")
                {
                    this.form.warn("\nWARNING: Max2Babylon 2020 only supports 3dsMax 2020.2 or later. Earlier versions of 3dsMax WILL crash!");
                }
                if (software == "Maya" && version == "2020")
                {
                    this.form.warn("\nWARNING: Maya2Babylon 2020 only supports Maya 2020.1 or later. Earlier versions of Maya will NOT load!");
                }
            };

            string downloadedFileName = null;

            try
            {
                if (this.latestRelease == "")
                {
                    this.form.log("Trying to get the last version.");
                    try
                    {
                        if (!await TryRetreiveLatestReleaseAsync())
                        {
                            this.form.error("Error : Can't find the last release package.");
                            return;
                        }
                    }
                    catch
                    {
                        this.form.warn("Unable to retreive the last version.\n"
                                        + "Please, try in 1 hour. (The API limitation is 60 queries / hour)");
                        throw;
                    }
                }

                this.form.log( "Downloading files : \n"
                               + Url_download + this.latestRelease);
                downloadedFileName = this.DownloadFile(this.latestRelease);
            }
            catch (Exception ex)
            {
                this.form.warn( "Unable to download the files.\n"
                                + "Error message : \n"
                                + "\"" + ex.Message + "\"");
                return;
            }

            this.form.log( "Download complete.\n"
                         + "Extracting files ...");

            if (!tryInstallDownloaded(downloadedFileName))
            {
                // catch and log are processed into the function.
                return;
            }

            this.form.log("\n----- " + this.software + " " + downloadedFileName + " EXPORTER UP TO DATE ----- \n");

            this.form.displayInstall(this.software, this.version);

            logPostInstall();
        }

        private async Task<bool> TryRetreiveLatestReleaseAsync()
        {
            this.form.log("Trying to get the last version ...");

            // TO DO - Parse the JSON in a more beautiful way...
            String responseBody = await this.GetJSONBodyRequest(Url_github_API_releases);
            String lastestReleaseInfos = responseBody.Substring(responseBody.IndexOf("\"prerelease\":") + "\"prerelease\":".Length);
            //Ensure we are on release version
            if (lastestReleaseInfos.StartsWith("false"))
            {
                //We parse the array to find the dowload URL
                this.latestRelease = lastestReleaseInfos.Substring(lastestReleaseInfos.IndexOf("\"browser_download_url\":") + "\"browser_download_url\": ".Length);

                // We split, remove & substrings to get only the URL starting with https://github.com and lasting with preRelease version
                this.latestRelease = this.latestRelease.Split('"')[0];
                this.latestRelease = this.latestRelease.Remove(this.latestRelease.LastIndexOf("/"));
                this.latestRelease = this.latestRelease.Substring(this.latestRelease.LastIndexOf("/"));
                return true;
            }
            return false;
        }

        private string DownloadFile(string releaseName)
        {
            var downloadVersion = this.version;
            if (this.software.Equals("Maya") && (this.version.Equals("2017") || this.version.Equals("2018")))
            {
                this.form.warn("Maya 2017 and 2018 have the same archive, changing version for proper download");
                downloadVersion = "2017-2018";
            }

            // Download the zip
            var srcUrl = Url_download + releaseName + "/" + this.software + "_" + downloadVersion + ".zip";
            var targetFileName = this.software + "_" + downloadVersion + ".zip";
            using (var client = new WebClient())
            {
               client.DownloadFile(srcUrl,targetFileName);
            }
            return targetFileName;
         }

        private bool tryInstallDownloaded(string downloadVersion)
        {

            try
            {
                using (ZipArchive myZip = ZipFile.OpenRead(downloadVersion))
                {
                    foreach (ZipArchiveEntry entry in myZip.Entries)
                    {
                        if (entry.IsDirectory()) continue;
                        if (entry.Name.Substring(0, 9) == "AEbabylon") entry.ExtractToFile(this.installDir + "scripts\\AETemplates" + "/" + entry.Name, true);
                        else if (entry.Name.Substring(0, 9) == "NEbabylon") entry.ExtractToFile(this.installDir + "scripts\\NETemplates" + "/" + entry.Name, true);
                        else entry.ExtractToFile(this.installDir + this.installLibSubDir + "/" + entry.Name, true);
                    }
                }
            }
            catch(Exception ex)
            {
                this.form.error(
                    "Can't extract the files.\n"
                    + "If you're not, please try to run this tool in ADMINISTRATOR MODE. It's necessary to extract the files in \"Program Files\" folder (or other protected folders).\n"
                    + "Error message : \n"
                    + "\"" + ex.Message + "\""
                    );
                return false;
            }

            this.form.log(
                "Extraction complete.\n"
                + "Deleting temporary files ..."
                );

            try
            {
                File.Delete(this.software + "_" + downloadVersion + ".zip");
            }
            catch (Exception ex)
            {
                this.form.error(
                    "Can't delete temporary files.\n"
                    + "Error message : \n"
                    + "\"" + ex.Message + "\""
                    );
                return false;
            }

            try
            {
                string uninstallScriptPath = this.installDir + "scripts\\Startup\\BabylonCleanUp.ms";
                this.form.log("\nRemoving " + uninstallScriptPath + ".\n");
                File.Delete(uninstallScriptPath);
            }
            catch (Exception ex)
            {
                this.form.warn(
                    "Can't delete temporary script.\n"
                    + "Error message : \n"
                    + "\"" + ex.Message + "\""
                    );
            }

            return true;
        }

        public async Task<string> GetJSONBodyRequest(string requestURI)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "BJS_Installer");
            HttpResponseMessage response = await client.GetAsync(requestURI);
            return await response.Content.ReadAsStringAsync();
        }

        public string GetURLGitHubAPI()
        {
            return Url_github_API_releases;
        }
    }

    public static class ZipArchiveEntryExtension
    {
        public static bool IsDirectory(this ZipArchiveEntry entry)
        {
            return entry.FullName.EndsWith("/");
        }
    }
}
