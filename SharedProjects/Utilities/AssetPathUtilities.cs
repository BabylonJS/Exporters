using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GLTFExport.Entities;
using System.IO;


namespace Utilities
{
    public class TexturesPaths
    {
        public string diffusePath;
        public string opacityPath;
        public string specularName;
        public float[] diffuse;
        public float opacity;
        public float[] specular;
        public float glossiness;

        public bool Equals(TexturesPaths textpaths)
        {
            if ((this.diffusePath == textpaths.diffusePath) && (this.opacityPath == textpaths.opacityPath) && (this.specularName == textpaths.specularName) && (this.diffuse.SequenceEqual(textpaths.diffuse)) && (this.opacity == textpaths.opacity) && (this.specular.SequenceEqual(textpaths.specular)) && (this.glossiness == textpaths.glossiness))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class PairBaseColorMetallicRoughness
    {
        public GLTFTextureInfo baseColor = new GLTFTextureInfo();
        public GLTFTextureInfo metallicRoughness = new GLTFTextureInfo();
    }

    public class PairEmissiveDiffuse
    {
        public string diffusePath = null;
        public string emissivePath = null;
        public float[] defaultEmissive = null;
        public float[] defaultDiffuse = null;

        public bool Equals(PairEmissiveDiffuse textures)
        {
            if ((this.diffusePath == textures.diffusePath) && (this.emissivePath == textures.emissivePath) && (this.defaultEmissive.SequenceEqual(textures.defaultEmissive)) && (this.defaultDiffuse.SequenceEqual(textures.defaultDiffuse)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class AssetPathUtilities
    {
        
        private static Dictionary<TexturesPaths, PairBaseColorMetallicRoughness> _DicoMatTextureGLTF = new Dictionary<TexturesPaths, PairBaseColorMetallicRoughness>();
        private static Dictionary<string, GLTFTextureInfo> _DicoTextNameTextureComponent = new Dictionary<string, GLTFTextureInfo>();
        private static Dictionary<PairEmissiveDiffuse, GLTFTextureInfo> _DicoEmissiveTextureComponent = new Dictionary<PairEmissiveDiffuse, GLTFTextureInfo>();

        public static TexturesPaths SetStandText(BabylonStandardMaterial babylonStandardMaterial)
        {
            var _StandText = new TexturesPaths();
            if (babylonStandardMaterial.diffuseTexture != null)
            {
                _StandText.diffusePath =  babylonStandardMaterial.diffuseTexture.originalPath;
            }
            else
            {
                _StandText.diffusePath =  "none";
            }

            if (babylonStandardMaterial.specularTexture != null)
            {
                _StandText.specularName = babylonStandardMaterial.specularTexture.name;
            }
            else
            {
                _StandText.specularName = "none";
            }

            if ((babylonStandardMaterial.diffuseTexture == null || babylonStandardMaterial.diffuseTexture.hasAlpha == false) && babylonStandardMaterial.opacityTexture != null)
            {
                _StandText.opacityPath = babylonStandardMaterial.opacityTexture.originalPath;
            }
            else
            {
                _StandText.opacityPath = "none";
            }
            _StandText.diffuse = babylonStandardMaterial.diffuse;
            _StandText.opacity = babylonStandardMaterial.alpha;
            _StandText.specular = babylonStandardMaterial.specular;
            _StandText.glossiness = babylonStandardMaterial.specularPower;
            return _StandText;
        }

        public static void AddStandText(TexturesPaths key, GLTFTextureInfo finalTextBC, GLTFTextureInfo finalTextMR)
        {
            var _valuePair = new PairBaseColorMetallicRoughness();
            _valuePair.baseColor = finalTextBC;
            _valuePair.metallicRoughness = finalTextMR;

            _DicoMatTextureGLTF.Add(key, _valuePair);
        }

        public static PairBaseColorMetallicRoughness GetStandTextInfo(TexturesPaths textpaths)
        {
            foreach (TexturesPaths textPathsObject  in _DicoMatTextureGLTF.Keys)
            {
                if (textPathsObject.Equals(textpaths))
                {
                    return _DicoMatTextureGLTF[textPathsObject];
                }
            }
            return null;
        }


        public static bool CheckIfImageIsRegistered(string name)
        {
            foreach(string registeredName in _DicoTextNameTextureComponent.Keys)
            {
                if(registeredName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void RegisterTexture(GLTFTextureInfo textureInfo, string name)
        {
            _DicoTextNameTextureComponent.Add(name, textureInfo);
        }

        public static GLTFTextureInfo GetRegisteredTexture(string name)
        {
            return _DicoTextNameTextureComponent[name];
        }


        public static PairEmissiveDiffuse CreatePair(string diffusePath, string emissivePath, float[] diffuse, float[] emissive)
        {
            var _pair = new PairEmissiveDiffuse();
            _pair.diffusePath = diffusePath;
            _pair.emissivePath = emissivePath;
            _pair.defaultDiffuse = diffuse;
            _pair.defaultEmissive = emissive;
            return _pair;
        }

        public static void RegisterEmissive(GLTFTextureInfo TextureInfo, BabylonStandardMaterial babylonMaterial, float[] diffuse, float[] emissive)
        {
            string pathDiffuse;
            string pathEmissive;

            if (babylonMaterial.diffuseTexture != null)
            {
                pathDiffuse = babylonMaterial.diffuseTexture.originalPath;
            }
            else
            {
                pathDiffuse = "none";
            }

            if (babylonMaterial.emissiveTexture != null)
            {
                pathEmissive = babylonMaterial.emissiveTexture.originalPath;
            }
            else
            {
                pathEmissive = "none";
            }

            var _pair = CreatePair(pathDiffuse, pathEmissive, diffuse, emissive);
            _DicoEmissiveTextureComponent.Add(_pair, TextureInfo);
        }

        public static GLTFTextureInfo GetRegisteredEmissive(BabylonStandardMaterial babylonMaterial, float[] diffuse, float[] emissive)
        {

            string pathDiffuse;
            string pathEmissive;

            if (babylonMaterial.diffuseTexture != null)
            {
                pathDiffuse = babylonMaterial.diffuseTexture.originalPath;
            }
            else
            {
                pathDiffuse = "none";
            }

            if (babylonMaterial.emissiveTexture != null)
            {
                pathEmissive = babylonMaterial.emissiveTexture.originalPath;
            }
            else
            {
                pathEmissive = "none";
            }

            var _pair = CreatePair(pathDiffuse, pathEmissive, diffuse, emissive);

            foreach (PairEmissiveDiffuse registeredText in _DicoEmissiveTextureComponent.Keys)
            {
                if (registeredText.Equals(_pair))
                {
                    return _DicoEmissiveTextureComponent[registeredText];
                }
            }
            return null;
        }
    }
}
