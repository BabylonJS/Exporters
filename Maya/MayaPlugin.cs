using Autodesk.Maya.OpenMaya;
using Maya2Babylon.Forms;
using System;
using System.Windows.Forms;

[assembly: MPxCommandClass(typeof(Maya2Babylon.toBabylon), "toBabylon")]
[assembly: ExtensionPlugin(typeof(Maya2Babylon.MayaPlugin), "Any")]

namespace Maya2Babylon
{
    public class MayaPlugin : IExtensionPlugin
    {
        /// <summary>
        /// Path to the Babylon menu
        /// </summary>
        string MenuPath;

        bool IExtensionPlugin.InitializePlugin()
        {
            // Add menu to main menu bar
            MenuPath = MGlobal.executeCommandStringResult($@"menu - parent MayaWindow - label ""Babylon"";");
            // Add item to this menu
            MGlobal.executeCommand($@"menuItem - label ""Babylon File Exporter..."" - command ""toBabylon"";");

            MGlobal.displayInfo("Babylon plug-in initialized");
            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
            // Remove menu from main menu bar
            MGlobal.executeCommand($@"deleteUI -menu ""{MenuPath}"";");

            MGlobal.displayInfo("Babylon plug-in uninitialized");
            return true;
        }

        string IExtensionPlugin.GetMayaDotNetSdkBuildVersion()
        {
            String version = "20171207";
            return version;
        }
    }

    public class toBabylon : MPxCommand, IMPxCommand
    {
        private static ExporterForm form;

        /// <summary>
        /// Entry point of the plug in
        /// Write "toBabylon" in the Maya console to start it
        /// </summary>
        /// <param name="argl"></param>
        public override void doIt(MArgList argl)
        {
            if (form == null)
                form = new ExporterForm();
            form.Show();
            form.BringToFront();
            form.WindowState = FormWindowState.Normal;
            form.FormClosed += (object sender, FormClosedEventArgs e) =>
            {
                if (form == null)
                {
                    return;
                }
                form.Dispose();
                form = null;
            };
        }
    }
}
