using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GLTFExport.Entities;
using System.IO;


namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mDagPath">DAG path to the transform above camera</param>
        /// <param name="babylonScene"></param>
        /// <returns></returns>
        /// 

        private Dictionary<TexturesPaths, PairBaseColorMetallicRoughness> _DicoMatTextureGLTF = new Dictionary<TexturesPaths, PairBaseColorMetallicRoughness>();
        private Dictionary<string, GLTFTextureInfo> _DicoTextNameTextureComponent = new Dictionary<string, GLTFTextureInfo>();
        private Dictionary<PairEmissiveDiffuse, GLTFTextureInfo> _DicoEmissiveTextureComponent = new Dictionary<PairEmissiveDiffuse, GLTFTextureInfo>();

        public TexturesPaths SetStandText(BabylonStandardMaterial babylonStandardMaterial)
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
                _StandText.specularPath = babylonStandardMaterial.specularTexture.originalPath;
            }
            else
            {
                _StandText.specularPath = "none";
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

        public void AddStandText(TexturesPaths key, GLTFTextureInfo finalTextBC, GLTFTextureInfo finalTextMR)
        {
            var _valuePair = new PairBaseColorMetallicRoughness();
            _valuePair.baseColor = finalTextBC;
            _valuePair.metallicRoughness = finalTextMR;

            _DicoMatTextureGLTF.Add(key, _valuePair);
        }

        public PairBaseColorMetallicRoughness GetStandTextInfo(TexturesPaths textpaths)
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


        public bool CheckIfImageIsRegistered(string name)
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

        public void RegisterTexture(GLTFTextureInfo TextureInfo, string name)
        {
            _DicoTextNameTextureComponent.Add(name, TextureInfo);
        }

        public GLTFTextureInfo GetRegisteredTexture(string name)
        {
            return _DicoTextNameTextureComponent[name];
        }


        public PairEmissiveDiffuse CreatePair(string diffusePath, string emissivePath, float[] diffuse, float[] emissive)
        {
            var _pair = new PairEmissiveDiffuse();
            _pair.diffusePath = diffusePath;
            _pair.emissivePath = emissivePath;
            _pair.defaultDiffuse = diffuse;
            _pair.defaultEmissive = emissive;
            return _pair;
        }

        public void RegisterEmissive(GLTFTextureInfo TextureInfo, BabylonStandardMaterial babylonMaterial, float[] diffuse, float[] emissive)
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

        public GLTFTextureInfo GetRegisteredEmissive(BabylonStandardMaterial babylonMaterial, float[] diffuse, float[] emissive)
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


        public class TexturesPaths
        {
            public string diffusePath;
            public string opacityPath;
            public string specularPath;
            public float[] diffuse;
            public float opacity;
            public float[] specular;
            public float glossiness;

            public bool Equals(TexturesPaths textpaths)
            {
                if((this.diffusePath == textpaths.diffusePath) && (this.opacityPath == textpaths.opacityPath) && (this.specularPath == textpaths.specularPath) && (this.diffuse.SequenceEqual(textpaths.diffuse)) && (this.opacity == textpaths.opacity) && (this.specular.SequenceEqual(textpaths.specular)) && (this.glossiness == textpaths.glossiness))
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
    }
}
