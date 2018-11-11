﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Max;

namespace Max2Babylon
{
    public partial class ScenePropertiesForm : Form
    {
        public ScenePropertiesForm()
        {
            InitializeComponent();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            Tools.UpdateVector3Control(gravityControl, Loader.Core.RootNode, "babylonjs_gravity");
            Tools.UpdateCheckBox(chkQuaternions, Loader.Core.RootNode, "babylonjs_exportquaternions");
            Tools.UpdateCheckBox(chkAnimations, Loader.Core.RootNode, "babylonjs_donotoptimizeanimations");
            
            Tools.UpdateCheckBox(chkCreateDefaultSkybox, Loader.Core.RootNode, "babylonjs_createDefaultSkybox");
            Tools.UpdateNumericUpDown(nupSkyboxBlurLevel, new List<IINode> { Loader.Core.RootNode }, "babylonjs_skyboxBlurLevel");

            Tools.UpdateCheckBox(chkAddDefaultLight, Loader.Core.RootNode, "babylonjs_addDefaultLight");

            Tools.UpdateCheckBox(chkAutoPlay, Loader.Core.RootNode, "babylonjs_sound_autoplay");
            Tools.UpdateCheckBox(chkLoop, Loader.Core.RootNode, "babylonjs_sound_loop");
            Tools.UpdateNumericUpDown(nupVolume, new List<IINode> { Loader.Core.RootNode }, "babylonjs_sound_volume");

            Tools.UpdateCheckBox(chkMorphExportTangent, Loader.Core.RootNode, "babylonjs_export_Morph_Tangents");
            Tools.UpdateCheckBox(ckkMorphExportNormals, Loader.Core.RootNode, "babylonjs_export_Morph_Normals");

            Tools.UpdateTextBox(txtSound, new List<IINode> { Loader.Core.RootNode }, "babylonjs_sound_filename");
        }

        private void ScenePropertiesForm_Load(object sender, EventArgs e)
        {
            Tools.PrepareVector3Control(gravityControl, Loader.Core.RootNode, "babylonjs_gravity", 0, -0.9f);
            Tools.PrepareCheckBox(chkQuaternions, Loader.Core.RootNode, "babylonjs_exportquaternions", 1);
            Tools.PrepareCheckBox(chkAnimations, Loader.Core.RootNode, "babylonjs_donotoptimizeanimations", 0);

            Tools.PrepareCheckBox(chkCreateDefaultSkybox, Loader.Core.RootNode, "babylonjs_createDefaultSkybox", 1);
            Tools.PrepareNumericUpDown(nupSkyboxBlurLevel, new List<IINode> { Loader.Core.RootNode }, "babylonjs_skyboxBlurLevel", 0.3f);

            Tools.PrepareCheckBox(chkAddDefaultLight, Loader.Core.RootNode, "babylonjs_addDefaultLight", 1);

            Tools.PrepareCheckBox(chkAutoPlay, Loader.Core.RootNode, "babylonjs_sound_autoplay", 1);
            Tools.PrepareCheckBox(chkLoop, Loader.Core.RootNode, "babylonjs_sound_loop", 1);
            Tools.PrepareNumericUpDown(nupVolume, new List<IINode>{Loader.Core.RootNode}, "babylonjs_sound_volume", 1.0f);

            Tools.PrepareCheckBox(chkMorphExportTangent, Loader.Core.RootNode, "babylonjs_export_Morph_Tangents", 0);
            Tools.PrepareCheckBox(ckkMorphExportNormals, Loader.Core.RootNode, "babylonjs_export_Morph_Normals", 1);

            Tools.PrepareTextBox(txtSound, Loader.Core.RootNode, "babylonjs_sound_filename");
        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            if (ofdOpenSound.ShowDialog() == DialogResult.OK)
            {
                txtSound.Text = ofdOpenSound.FileName;
            }
        }
    }
}
