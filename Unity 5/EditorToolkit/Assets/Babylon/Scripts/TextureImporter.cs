using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity3D2Babylon
{
    public class BabylonTextureImporter
    {
        public TextureImporter textureImporter { get; private set; }
        private bool previousIsReadable;
        private string texturePath;

        public BabylonTextureImporter(string path)
        {
            try
            {
                texturePath = path;
                textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (textureImporter != null) {
                    previousIsReadable = textureImporter.isReadable;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }

        public bool IsValid()
        {
            return (textureImporter != null);
        }

        public bool IsReadable()
        {
            return (textureImporter != null) ? textureImporter.isReadable : false;
        }

        public bool WasReadable()
        {
            return previousIsReadable;
        }

        public bool SetReadable()
        {
            bool result = false;
            if (textureImporter != null)
            {
                try
                {
                    if (!IsReadable())
                    {
                        textureImporter.isReadable = true;
                        ForceUpdate();
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            return result;
        }

        public bool SetDefault()
        {
            bool result = false;
            if (textureImporter != null)
            {
                try
                {
                    if (textureImporter.textureType != TextureImporterType.Default)
                    {
                        textureImporter.textureType = TextureImporterType.Default;
                        ForceUpdate();
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            return result;
        }

        public bool SetCubemap()
        {
            bool result = false;
            if (textureImporter != null)
            {
                try
                {   
                    bool update = false;
                    if (!IsReadable()) {
                        textureImporter.isReadable = true;
                        update = true;
                    }
                    //var ps = textureImporter.GetPlatformTextureSettings("Standalone");
                    //if (ps.format != TextureImporterFormat.RGBA32) {
                    //    ps.format = TextureImporterFormat.RGBA32;
                    //    textureImporter.SetPlatformTextureSettings(ps);
                    //    update = true;
                    //}
                    if (update == true) ForceUpdate();
                    result = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            return result;
        }

        public bool SetNormalMap()
        {
            bool result = false;
            if (textureImporter != null)
            {
                try
                {
                    if (textureImporter.textureType != TextureImporterType.NormalMap)
                    {
                        textureImporter.textureType = TextureImporterType.NormalMap;
                        ForceUpdate();
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            return result;
        }

        public bool SetLightmapMap()
        {
            bool result = false;
            if (textureImporter != null)
            {
                try
                {
                    if (textureImporter.textureType != TextureImporterType.Lightmap)
                    {
                        textureImporter.textureType = TextureImporterType.Lightmap;
                        ForceUpdate();
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            return result;
        }

        public void ForceUpdate(bool full = true)
        {
            if (textureImporter != null)
            {
                try
                {
                    if (full)
                    {
                        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
                    }
                    else
                    {
                        AssetDatabase.ImportAsset(texturePath);
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }
    }
}
