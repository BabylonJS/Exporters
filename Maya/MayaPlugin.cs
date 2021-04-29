using Autodesk.Maya.OpenMaya;
using Maya2Babylon.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BabylonExport.Entities;

[assembly: MPxCommandClass(typeof(Maya2Babylon.toBabylon), "toBabylon")]
[assembly: ExtensionPlugin(typeof(Maya2Babylon.MayaPlugin), "Any")]
[assembly: MPxCommandClass(typeof(Maya2Babylon.AnimationGroups), "AnimationGroups")]
[assembly: MPxCommandClass(typeof(Maya2Babylon.ScriptToBabylon), "ScriptToBabylon")]
[assembly: MPxCommandClass(typeof(Maya2Babylon.GenerateExportersParameter), "GenerateExportersParameter")]

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
            MGlobal.executeCommand($@"menuItem - label ""Animation groups"" - command ""AnimationGroups"";");

            MGlobal.displayInfo("Babylon plug-in initialized");
            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
            // Remove menu from main menu bar and close the form
            MGlobal.executeCommand($@"deleteUI -menu ""{MenuPath}"";");

            MGlobal.displayInfo("Babylon plug-in uninitialized");

            toBabylon.disposeForm();
            
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
            form.closingByUser = () => { return disposeForm(); };
            // TODO - save states - FORM: checkboxes and inputs. MEL: reselected meshes / nodes?
            form.closingByShutDown = () => { return disposeForm(); };
            // form.closingByCrash = () => { return disposeForm(); };
        }
        public static bool disposeForm()
        {
            if (form != null)
            {
                form.Dispose();
                form = null;
            }
            return true;
        }
    }

    /// <summary>
    /// For the animation groups form
    /// </summary>
    public class AnimationGroups : MPxCommand, IMPxCommand
    {
        public static AnimationForm animationForm = null;

        public override void doIt(MArgList args)
        {
            if (animationForm == null)
            {
                animationForm = new AnimationForm();
                animationForm.On_animationFormClosed += On_animationFormClosed;
            }

            animationForm.Show();
            animationForm.BringToFront();
            animationForm.WindowState = FormWindowState.Normal;
        }

        private void On_animationFormClosed()
        {
            animationForm = null;
        }
    }

    [MPxCommandSyntaxFlag("-ep", "-exportParameters", Arg1 = typeof(System.String[]))]
    public class ScriptToBabylon : MPxCommand, IMPxCommand
    {

        private ExportParameters ScriptExportParameters = new ExportParameters();

        /// <summary>
        /// Write "ScriptToBabylon" in the Maya console to export with MEL
        /// </summary>
        /// <param name="argl"></param>
        /// 
        public override void doIt(MArgList argl)
        {
            uint index = 1;
            MStringArray argExportParameters = argl.asStringArray(ref index);
            string errorMessage = null;

            for (var i = 0; i < argExportParameters.length; i++)
            {
                switch (i)
                {
                    case 0:
                        if (argExportParameters[i] != "")
                        {
                            ScriptExportParameters.outputPath = argExportParameters[i];
                        }
                        else
                        {
                            errorMessage = "The specified path is not valid";
                        }
                        break;
                    case 1:
                        if (argExportParameters[i] != "babylon" || argExportParameters[i] != "gltf" || argExportParameters[i] != "glb" || argExportParameters[i] != "binary babylon")
                        {
                            ScriptExportParameters.outputFormat = argExportParameters[i];
                        }
                        else
                        {
                            errorMessage = "The specified output format is not valid";
                        }
                        break;
                    case 2:
                        ScriptExportParameters.textureFolder = argExportParameters[i];
                        break;
                    case 3:
                        ScriptExportParameters.scaleFactor = float.Parse(argExportParameters[i]);
                        break;
                    case 4:
                        ScriptExportParameters.writeTextures = bool.Parse(argExportParameters[i]);
                        break;
                    case 5:
                        ScriptExportParameters.overwriteTextures = bool.Parse(argExportParameters[i]);
                        break;
                    case 6:
                        ScriptExportParameters.exportHiddenObjects = bool.Parse(argExportParameters[i]);
                        break;
                    case 7:
                        ScriptExportParameters.exportMaterials = bool.Parse(argExportParameters[i]);
                        break;
                    case 8:
                        ScriptExportParameters.exportOnlySelected = bool.Parse(argExportParameters[i]);
                        break;
                    case 9:
                        ScriptExportParameters.bakeAnimationFrames = bool.Parse(argExportParameters[i]);
                        break;
                    case 10:
                        ScriptExportParameters.optimizeAnimations = bool.Parse(argExportParameters[i]);
                        break;
                    case 11:
                        ScriptExportParameters.optimizeVertices = bool.Parse(argExportParameters[i]);
                        break;
                    case 12:
                        ScriptExportParameters.animgroupExportNonAnimated = bool.Parse(argExportParameters[i]);
                        break;
                    case 13:
                        ScriptExportParameters.generateManifest = bool.Parse(argExportParameters[i]);
                        break;
                    case 14:
                        ScriptExportParameters.autoSaveSceneFile = bool.Parse(argExportParameters[i]);
                        break;
                    case 15:
                        ScriptExportParameters.exportTangents = bool.Parse(argExportParameters[i]);
                        break;
                    case 16:
                        ScriptExportParameters.exportSkins = bool.Parse(argExportParameters[i]);
                        break;
                    case 17:
                        ScriptExportParameters.exportMorphTangents = bool.Parse(argExportParameters[i]);
                        break;
                    case 18:
                        ScriptExportParameters.exportMorphNormals = bool.Parse(argExportParameters[i]);
                        break;
                    case 19:
                        ScriptExportParameters.txtQuality = long.Parse(argExportParameters[i]);
                        break;
                    case 20:
                        ScriptExportParameters.mergeAO = bool.Parse(argExportParameters[i]);
                        break;
                    case 21:
                        ScriptExportParameters.dracoCompression = bool.Parse(argExportParameters[i]);
                        break;
                    case 22:
                        ScriptExportParameters.enableKHRLightsPunctual = bool.Parse(argExportParameters[i]);
                        break;
                    case 23:
                        ScriptExportParameters.enableKHRTextureTransform = bool.Parse(argExportParameters[i]);
                        break;
                    case 24:
                        ScriptExportParameters.enableKHRMaterialsUnlit = bool.Parse(argExportParameters[i]);
                        break;
                    case 25:
                        ScriptExportParameters.pbrFull = bool.Parse(argExportParameters[i]);
                        break;
                    case 26:
                        ScriptExportParameters.pbrNoLight = bool.Parse(argExportParameters[i]);
                        break;
                    case 27:
                        ScriptExportParameters.createDefaultSkybox = bool.Parse(argExportParameters[i]);
                        break;
                    case 28:
                        ScriptExportParameters.pbrEnvironment = argExportParameters[i];
                        break;
                    case 29:
                        ScriptExportParameters.exportAnimations = bool.Parse(argExportParameters[i]);
                        break;
                    case 30:
                        ScriptExportParameters.exportAnimationsOnly = bool.Parse(argExportParameters[i]);
                        break;
                    case 31:
                        ScriptExportParameters.exportTextures = bool.Parse(argExportParameters[i]);
                        break;

                }
            }

            if (errorMessage == null)
            {
                try
                {
                    BabylonExporter exporterInstance = new BabylonExporter();

                    exporterInstance.OnError += (error, rank) =>
                    {
                        try
                        {
                            displayError(error);
                        }
                        catch
                        {
                        }
                    };

                    exporterInstance.OnWarning += (error, rank) =>
                    {
                        try
                        {
                            displayWarning(error);
                        }
                        catch
                        {
                        }
                    };

                    exporterInstance.OnMessage += (message, color, rank, emphasis) =>
                    {
                        try
                        {
                            displayInfo(message);
                        }
                        catch
                        {
                        }
                    };

                    exporterInstance.Export(ScriptExportParameters);
                }
                catch (Exception ex)
                {
                    displayError("Export cancelled: " + ex.Message);
                }
            }
            else
            {
                displayError(errorMessage);
            }

        }
    }

    public class GenerateExportersParameter : MPxCommand, IMPxCommand
    {
        private ExportParameters ScriptExportParameters;

        /// <summary>
        /// Write "GenerateExportersParameter" in a Maya MEL script to get the default export parameters
        /// </summary>
        /// <param name="argl"></param>
        /// 
        public override void doIt(MArgList args)
        {
            ScriptExportParameters = new ExportParameters();

            MStringArray result = new MStringArray();
            result.append("");
            result.append("babylon");
            result.append("");
            result.append(ScriptExportParameters.scaleFactor.ToString());
            result.append(ScriptExportParameters.writeTextures.ToString());
            result.append(ScriptExportParameters.overwriteTextures.ToString());
            result.append(ScriptExportParameters.exportHiddenObjects.ToString());
            result.append(ScriptExportParameters.exportMaterials.ToString());
            result.append(ScriptExportParameters.exportOnlySelected.ToString());
            result.append(ScriptExportParameters.bakeAnimationFrames.ToString());
            result.append(ScriptExportParameters.optimizeAnimations.ToString());
            result.append(ScriptExportParameters.optimizeVertices.ToString());
            result.append(ScriptExportParameters.animgroupExportNonAnimated.ToString());
            result.append(ScriptExportParameters.generateManifest.ToString());
            result.append(ScriptExportParameters.autoSaveSceneFile.ToString());
            result.append(ScriptExportParameters.exportTangents.ToString());
            result.append(ScriptExportParameters.exportMorphTangents.ToString());
            result.append(ScriptExportParameters.exportMorphNormals.ToString());
            result.append(ScriptExportParameters.txtQuality.ToString());
            result.append(ScriptExportParameters.mergeAO.ToString());
            result.append(ScriptExportParameters.dracoCompression.ToString());
            result.append(ScriptExportParameters.enableKHRLightsPunctual.ToString());
            result.append(ScriptExportParameters.enableKHRTextureTransform.ToString());
            result.append(ScriptExportParameters.enableKHRMaterialsUnlit.ToString());
            result.append(ScriptExportParameters.pbrFull.ToString());
            result.append(ScriptExportParameters.pbrNoLight.ToString());
            result.append(ScriptExportParameters.createDefaultSkybox.ToString());
            result.append("");
            setResult(result);
        }

    }
}
