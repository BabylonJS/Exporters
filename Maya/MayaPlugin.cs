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
        bool IExtensionPlugin.InitializePlugin()
        {
            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
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
        /// <summary>
        /// Entry point of the plug in
        /// Write "toBabylon" in the Maya console to start it
        /// </summary>
        /// <param name="argl"></param>
        public override void doIt(MArgList argl)
        {
            MGlobal.displayInfo("Start Maya Plugin\n");
            ExporterForm BabylonExport = new ExporterForm();
            BabylonExport.Show();
            BabylonExport.BringToFront();
            BabylonExport.WindowState = FormWindowState.Normal;
        }
    }
}
