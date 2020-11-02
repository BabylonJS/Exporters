using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GLTFExport.Entities;
using System.IO;


namespace Babylon2GLTF
{
    partial class GLTFExporter
    {

        private Dictionary<TexturesPaths, PairBaseColorMetallicRoughness> _DicoMatTextureGLTF = new Dictionary<TexturesPaths, PairBaseColorMetallicRoughness>();
        private Dictionary<string, GLTFTextureInfo> _DicoTextNameTextureComponent = new Dictionary<string, GLTFTextureInfo>();
        private Dictionary<PairEmissiveDiffuse, GLTFTextureInfo> _DicoEmissiveTextureComponent = new Dictionary<PairEmissiveDiffuse, GLTFTextureInfo>();
        private Dictionary<PairBaseColorAlpha, string> _DicoPairBaseColorAlphaImageName = new Dictionary<PairBaseColorAlpha, string>();


        public TexturesPaths SetStandText(BabylonStandardMaterial babylonStandardMaterial)
        {
            var _StandText = new TexturesPaths();
            if (babylonStandardMaterial.diffuseTexture != null)
            {
                _StandText.diffusePath = babylonStandardMaterial.diffuseTexture.originalPath;
            }
            else
            {
                _StandText.diffusePath = "none";
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

        public void AddStandText(TexturesPaths key, GLTFTextureInfo finalTextBC, GLTFTextureInfo finalTextMR)
        {
            var _valuePair = new PairBaseColorMetallicRoughness();
            _valuePair.baseColor = finalTextBC;
            _valuePair.metallicRoughness = finalTextMR;

            _DicoMatTextureGLTF.Add(key, _valuePair);
        }

        public PairBaseColorMetallicRoughness GetStandTextInfo(TexturesPaths textpaths)
        {
            foreach (TexturesPaths textPathsObject in _DicoMatTextureGLTF.Keys)
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
            foreach (string registeredName in _DicoTextNameTextureComponent.Keys)
            {
                if (registeredName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void RegisterTexture(GLTFTextureInfo textureInfo, string name)
        {
            _DicoTextNameTextureComponent.Add(name, textureInfo);
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

        internal void RegisterBaseColorAlphaImageName(BabylonTexture texture, string imageName)
        {
            var key = new PairBaseColorAlpha(texture);

            if (!_DicoPairBaseColorAlphaImageName.ContainsKey(key))

            {
                _DicoPairBaseColorAlphaImageName.Add(key, imageName);
            }
        }

        internal string BaseColorAlphaImageNameLookup(BabylonTexture texture, string defaultName = null)
        {
            var key = new PairBaseColorAlpha(texture);
            string imageName = null;
            if (_DicoPairBaseColorAlphaImageName.TryGetValue(key, out imageName))
            {
                return imageName;
            }
            key = _DicoPairBaseColorAlphaImageName.Keys.Where(k => k.baseColorPath.Equals(key.baseColorPath)).FirstOrDefault();

            return key != null ? _DicoPairBaseColorAlphaImageName[key] : defaultName;
        }

        internal IEnumerable<BabylonMaterial> SortMaterialPriorToOptimizeTextureUsage(GLTF gltf, IEnumerable<BabylonMaterial> materials)
        {
            List<BabylonMaterial> original = materials.ToList();
            List<BabylonMaterial> sorted = new List<BabylonMaterial>(original.Count());
            
            // reorder ALL BabylonPBRMetallicRoughnessMaterial with Alpha at first place
            foreach (var m in original)
            {
                if (m is BabylonPBRMetallicRoughnessMaterial a)
                {
                    if (a.baseTexture.hasAlpha)
                    {
                        sorted.Insert(0, m);
                        continue;
                    }
                    sorted.Add(m);
                }
            }
 
            // however because we changed the order , we MUST re-assign material indexes for mesh primitives.
            List<Tuple<int, GLTFMeshPrimitive>> links = new List<Tuple<int, GLTFMeshPrimitive>>(sorted.Count);
            for (int i = 0; i != original.Count; i++)
            {
                // get new index
                int k = -1;
                while (++k < sorted.Count && sorted[k].id != original[i].id) ;

                // research meshPrimitives to update the indexes and put the result ito temporary buffer
                links.AddRange(gltf.MeshesList.SelectMany(m => m.primitives).Where(p => p.material == i).Select(m=>new Tuple<int, GLTFMeshPrimitive>(k, m)));
            }

            // finally update the indexes from temporary buffer
            foreach (var l in links)
            {
                l.Item2.material = l.Item1;
            }

            return sorted;
        }

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

        /// <summary>
        ///  To optimze the usage of texture when Base Color only and Base Color + Alpha exist at the same time. 
        /// </summary>
        public class PairBaseColorAlpha
        {
            public string baseColorPath = null;
            public string alphaPath = null;

            public PairBaseColorAlpha(BabylonTexture texture)
            {
                baseColorPath = texture.baseColorPath;
                alphaPath = texture.alphaPath;
            }
            public override int GetHashCode()
            {
                return new Tuple<string, string>(baseColorPath, alphaPath).GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return (obj != null && obj is PairBaseColorAlpha pca) ? this.Equals(pca) : false;
            }

            public bool Equals(PairBaseColorAlpha other)
            {
                return baseColorPath == other.baseColorPath && alphaPath == other.alphaPath;
            }

            public bool HasAlpha => !String.IsNullOrEmpty(alphaPath);
        }
    }
}