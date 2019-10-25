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
        private readonly string url_github = "github.com";
        private readonly string url_download = "https://github.com/BabylonJS/Exporters/releases/download";
        private readonly string url_github_API_releases = "https://api.github.com/repos/BabylonJS/Exporters/releases";
        private string software = "";
        private string version = "";
        private string installDir = "";
        private string latestRelease = "";

        public MainForm form;

        public void init(string software, string version, string installDir)
        {
            this.form.goTab("");
            this.form.log("\n----- INSTALLING / DOWNLOADING " + software + " v" + version + " EXPORTER -----\n");

            this.software = software;
            this.version = version;
            this.installDir = installDir;

            if (this.pingSite(this.url_github))
            {
                this.form.log("Info : Connection to GitHub OK");
                if (this.latestRelease == "")
                    this.getLatestRelease();
                else
                    this.download(this.latestRelease);

            }
        }

        private bool pingSite(string url_toTest)
        {

            var ping = new System.Net.NetworkInformation.Ping();

            try
            {
                var result = ping.Send(url_toTest);

                if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    this.form.error("Error : Can't reach Github.");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.form.error(
                    "Can't reach Github.\n"
                    + "Error : \n"
                    + "\"" + ex.Message + "\""
                    );
                return false;
            }
        }

        private async void getLatestRelease()
        {
            this.form.log("Trying to get the last version ...");

            try
            {
                // TO DO - Parse the JSON in a more beautiful way...

                String responseBody = await this.GetJSONBodyRequest(this.url_github_API_releases);

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
                    this.download(this.latestRelease);
                }
                else
                    this.form.error("Error : Can't find the last release package.");
            }
            catch(Exception ex)
            {
                this.form.error(
                    "Can't reach the GitHub API\n"
                    + "Please, try in 1 hour. (The API limitation is 60 queries / hour\n"
                    + "Error message : \n"
                    + "\"" + ex.Message + "\""
                    );
            }
        }

        private void download(string releaseName)
        {
            var downloadVersion = this.version;
            if (this.software.Equals("Maya") && (this.version.Equals("2017") || this.version.Equals("2018")))
            {
                this.form.warn("Maya 2017 and 2018 have the same archive, changing version for proper download");
                downloadVersion = "2017-2018";
            }
            this.form.log(
                "Downloading files : \n"
                + this.url_download + releaseName + "/" + this.software + "_" + downloadVersion + ".zip"
                );

            // Download the zip
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(
                        this.url_download + releaseName + "/" + this.software + "_" + downloadVersion + ".zip",
                        this.software + "_" + downloadVersion + ".zip"
                        );
                }
            }
            catch (Exception ex)
            {
                this.form.warn(
                    "Can't download the files.\n"
                    + "Error message : \n"
                    + "\"" + ex.Message + "\""
                    );
            }

            this.downloadComplete(downloadVersion);
        }

        private void downloadComplete(string downloadVersion)
        {
            this.form.log(
                "Download complete.\n"
                + "Extracting files ..."
                );

            try
            {
                String zipFileName = this.software + "_" + downloadVersion + ".zip";
                using (ZipArchive myZip = ZipFile.OpenRead(zipFileName))
                {
                    foreach (ZipArchiveEntry entry in myZip.Entries)
                    {
                        entry.ExtractToFile(this.installDir + "/" + entry.Name, true);
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
            }

            this.form.log(
                "Extraction complete.\n"
                + "Deleting temporary files ..."
                );

            try
            {
                File.Delete(this.software + "_" + downloadVersion + ".zip");
                this.form.log("\n----- " + this.software + " " + downloadVersion + " EXPORTER UP TO DATE ----- \n");
            }
            catch (Exception ex)
            {
                this.form.error(
                    "Can't delete temporary files.\n"
                    + "Error message : \n"
                    + "\"" + ex.Message + "\""
                    );
            }

            this.form.displayInstall(this.software, this.version);
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
            return this.url_github_API_releases;
        }
    }
}
