using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace BabylonJS_Installer
{
    public partial class MainForm : Form
    {
        private Downloader downloader = null;
        private SoftwareChecker checker = null;

        private Dictionary<string, Dictionary<string, string>> versions; // Soft, Version - Year
        private Dictionary<string, Dictionary<string, bool>> latestInstalled; // Soft, Version - isLatestInstalled
        private Dictionary<string, Dictionary<string, string>> locations; // Soft, Year - Location
        private Dictionary<string, Dictionary<string, Label[]>> labels; // Soft, Year - [lab. location, lab. expDate]
        private Dictionary<string, Dictionary<string, Button[]>> buttons; // Soft, Year - [but.Update, but.Uninstall, but.Locate]

        public MainForm()
        {
            InitializeComponent();
            this.mainTabControl.Visible = false;
            
            // FILL THE DICTIONNARIES

            this.versions = new Dictionary<string, Dictionary<string, string>>();
            this.versions["Max"] = new Dictionary<string, string>();
            this.versions["Max"].Add("2020", "22");
            this.versions["Max"].Add("2019", "21");
            this.versions["Max"].Add("2018", "20");
            this.versions["Max"].Add("2017", "19");
            this.versions["Max"].Add("2015", "17");
            this.versions["Maya"] = new Dictionary<string, string>();
            this.versions["Maya"].Add("2019", "20");
            this.versions["Maya"].Add("2018", "19");
            this.versions["Maya"].Add("2017", "18");

            this.latestInstalled = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var dcc in this.versions.Keys)
            {
                this.latestInstalled[dcc] = new Dictionary<string, bool>();
                foreach (var dccVersion in this.versions[dcc].Keys)
                {
                    this.latestInstalled[dcc][dccVersion] = true;
                }
            }

            this.labels = new Dictionary<string, Dictionary<string, Label[]>>();
            this.labels["Max"] = new Dictionary<string, Label[]>();
            this.labels["Max"].Add("2020", new[] { this.label_Max20_Info, this.label_Max20_ExpDate });
            this.labels["Max"].Add("2019", new[] { this.label_Max19_Info, this.label_Max19_ExpDate });
            this.labels["Max"].Add("2018", new[] { this.label_Max18_Info, this.label_Max18_ExpDate });
            this.labels["Max"].Add("2017", new[] { this.label_Max17_Info, this.label_Max17_ExpDate });
            this.labels["Max"].Add("2015", new[] { this.label_Max15_Info, this.label_Max15_ExpDate });
            this.labels["Maya"] = new Dictionary<string, Label[]>();
            this.labels["Maya"].Add("2019", new[] { this.label_Maya19_Info, this.label_Maya19_ExpDate });
            this.labels["Maya"].Add("2018", new[] { this.label_Maya18_Info, this.label_Maya18_ExpDate });
            this.labels["Maya"].Add("2017", new[] { this.label_Maya17_Info, this.label_Maya17_ExpDate });

            this.buttons = new Dictionary<string, Dictionary<string, Button[]>>();
            this.buttons["Max"] = new Dictionary<string, Button[]>();
            this.buttons["Max"].Add("2020", new[] { this.button_Max20_Update, this.button_Max20_Delete, this.button_Max20_Locate });
            this.buttons["Max"].Add("2019", new[] { this.button_Max19_Update, this.button_Max19_Delete, this.button_Max19_Locate });
            this.buttons["Max"].Add("2018", new[] { this.button_Max18_Update, this.button_Max18_Delete, this.button_Max18_Locate });
            this.buttons["Max"].Add("2017", new[] { this.button_Max17_Update, this.button_Max17_Delete, this.button_Max17_Locate });
            this.buttons["Max"].Add("2015", new[] { this.button_Max15_Update, this.button_Max15_Delete, this.button_Max15_Locate });
            this.buttons["Maya"] = new Dictionary<string, Button[]>();
            this.buttons["Maya"].Add("2019", new[] { this.button_Maya19_Update, this.button_Maya19_Delete, this.button_Maya19_Locate });
            this.buttons["Maya"].Add("2018", new[] { this.button_Maya18_Update, this.button_Maya18_Delete, this.button_Maya18_Locate });
            this.buttons["Maya"].Add("2017", new[] { this.button_Maya17_Update, this.button_Maya17_Delete, this.button_Maya17_Locate });

            this.log("---------- BABYLON.JS EXPORTERS TOOL STARTED ----------\n");

            this.downloader = new Downloader();
            this.downloader.form = this;

            this.checker = new SoftwareChecker();
            this.checker.form = this;
            this.checker.setLatestVersionDate();

            this.checker.checkNewInstallerVersion();

            this.locations = new Dictionary<string, Dictionary<string, string>>();
            this.checkInstall("Max");
            this.checkInstall("Maya");

            this.mainTabControl.Visible = true;

            if(!this.checker.ensureAdminMode())
            {
                this.goTab("");
                this.error("\nApplication is not running in Administrator mode.\nYou should restart the application to ensure its functionnalities.\n");
            }
        }

        public void goTab(string soft)
        {
            switch (soft)
            {
                case "Max":
                    this.mainTabControl.SelectTab(0);
                    break;
                case "Maya":
                    this.mainTabControl.SelectTab(1);
                    break;
                default:
                    this.mainTabControl.SelectTab(2);
                    break;
            }
        }


        #region ---------- CONSOLE

        public void log(string text)
        {
            this.log_text.SelectionColor = Color.Blue;
            this.log_text.AppendText(text + "\n");
        }

        public void warn(string text)
        {
            this.log_text.SelectionColor = Color.Orange;
            this.log_text.AppendText(text + "\n");
        }

        public void error(string text)
        {
            this.log_text.SelectionColor = Color.Red;
            this.log_text.AppendText(text + "\n");
        }

        #endregion
        #region ---------- Check the installation folders

        private void checkInstall (string soft)
        {
            this.locations[soft] = new Dictionary<string, string>();
            foreach(KeyValuePair<string, string> keyValue in this.versions[soft])
            {
                this.locations[soft][keyValue.Key] = this.checker.checkPath(soft, keyValue.Value, keyValue.Key);
                this.displayInstall(soft, keyValue.Key);
            }
        }

        public void displayInstall(string soft, string year)
        {
            string version = this.versions[soft][year];
            Label labelPath = this.labels[soft][year][0];
            Label labelDate = this.labels[soft][year][1];
            Button buttonUpdate = this.buttons[soft][year][0];
            Button buttonUninstall = this.buttons[soft][year][1];
            Button buttonLocate = this.buttons[soft][year][2];
            string location = this.locations[soft][year];
            DateTime expDate;

            if (location != null && location != "")
            {
                this.locations[soft][year] = location;
                labelPath.Text = "Path : " + location;
                labelDate.Visible = true;
                this.log("Installation found for " + soft + " " + year + "  -> " + location);
                expDate = this.checker.getInstalledExporterTimestamp(soft, location);
                if (expDate > DateTime.FromFileTime(0)) // we need to use FromFileTime(0) because (FILETIME)0 is 1/1/1601, which windows returns on file not existing.
                {
                    labelDate.Text = "Exporter last updated : " + expDate.ToShortDateString();
                    this.log(String.Format("Exporter for {0} {1} last updated : {2}", soft, year, expDate.ToString()));
                    buttonUninstall.Visible = true;

                    var isLatest = this.checker.isLatestVersionInstalled(soft, version, location);
                    this.latestInstalled[soft][version] = isLatest;
                    buttonUpdate.Enabled = !isLatest;
                    buttonUpdate.Visible = true;
                    buttonUpdate.Text = "Update";
                }
                else
                {
                    labelDate.Text = "Exporter not installed.";
                    this.log("No exporter installed for " + soft + " " + year);
                    buttonUninstall.Visible = false;
                    buttonUpdate.Text = "Install";
                    buttonUpdate.Visible = true;
                    buttonUpdate.Enabled = true;
                }
            }
            else
            {
                labelPath.Text = "No installation found.";
                this.log("No installation found for " + soft + " " + year);

                labelDate.Visible = false;
                buttonUpdate.Visible = false;
                buttonUninstall.Visible = false;
            }

            var oneEnabled = this.latestInstalled.Any(dccTools => dccTools.Value.Any(installedTools => installedTools.Value));
            button_All_Update.Enabled = !oneEnabled;
        }

        #endregion
        #region ---------- UPDATE

        private void button_update(string soft, string year)
        {
            this.downloader.init(soft, year, this.locations[soft][year], this.checker.libFolder[soft]);
        }

        private void Button_All_Update_Click(object sender, EventArgs e)
        {
            foreach(KeyValuePair<string, Dictionary<string, string>> softYear in this.versions)
            {
                foreach (KeyValuePair<string, string> yearVersion in softYear.Value)
                {
                    //We check if the uninstall button is visible for the current soft and if the current version is the latest
                    //If not, there is no need to update a soft that isn't there
                    if (this.buttons[softYear.Key][yearVersion.Key][0].Visible && this.buttons[softYear.Key][yearVersion.Key][0].Enabled)
                        this.button_update(softYear.Key, yearVersion.Key);
                }
            }
        }

        private void Button_Max20_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Max", "2020");
        }

        private void Button_Max19_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Max", "2019");
        }

        private void Button_Max18_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Max", "2018");
        }

        private void Button_Max17_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Max", "2017");
        }

        private void Button_Max15_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Max", "2015");
        }

        private void Button_Maya19_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Maya", "2019");
        }

        private void Button_Maya18_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Maya", "2018");
        }

        private void Button_Maya17_Update_Click(object sender, EventArgs e)
        {
            this.button_update("Maya", "2017");
        }

        #endregion
        #region ---------- DELETE

        private void button_delete(string soft, string year)
        {
            this.checker.uninstallExporter(soft, year, this.locations[soft][year]);
        }

        private void Button_All_Delete_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> softYear in this.versions)
            {
                foreach (KeyValuePair<string, string> yearVersion in softYear.Value)
                {
                    //There is no need to delete a soft that isnt there, so we check for the uninstall button
                    if (this.buttons[softYear.Key][yearVersion.Key][1].Visible)
                        this.button_delete(softYear.Key, yearVersion.Key);
                }
            }
        }

        private void Button_Max20_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Max", "2020");
        }

        private void Button_Max19_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Max", "2019");
        }

        private void Button_Max18_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Max", "2018");
        }

        private void Button_Max17_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Max", "2017");
        }

        private void Button_Max15_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Max", "2015");
        }

        private void Button_Maya19_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Maya", "2019");
        }

        private void Button_Maya18_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Maya", "2018");
        }

        private void Button_Maya17_Delete_Click(object sender, EventArgs e)
        {
            this.button_delete("Maya", "2017");
        }

        #endregion
        #region ---------- LOCATE

        private void button_locate(string soft, string year)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Locate the software installation folder (ex: \"C:\\Program Files\\Autodesk\\3ds Max 2019\\\")";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = fbd.SelectedPath;
                if (selectedPath != null && selectedPath != "")
                {
                    this.locations[soft][year] = selectedPath + "\\";
                    this.displayInstall(soft, year);
                }
            }
        }

        private void Button_Max20_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Max", "2020");
        }

        private void Button_Max19_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Max", "2019");
        }

        private void Button_Max18_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Max", "2018");
        }

        private void Button_Max17_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Max", "2017");
        }

        private void Button_Max15_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Max", "2015");
        }

        private void Button_Maya19_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Maya", "2019");
        }

        private void Button_Maya18_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Maya", "2018");
        }

        private void Button_Maya17_Locate_Click(object sender, EventArgs e)
        {
            this.button_locate("Maya", "2017");
        }

        #endregion

    }
}
