using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        /*
         * This class is used to handle Arnold Opacity attribute on shapes (meshes),
         * since Babylon and glTF formats don't have such Mesh attribute.
         * 
         * When facing a case where a AiStandardSurface material is assigned to both opaque and non opaque meshes,
         * the exporter get round this missing by duplicating the material:
         *  - The original material is assigned to non opaque meshes, and all its opacity attributes remain untouched.
         *  - The duplicated material is assigned to opaque meshes, and all its opacity attributes are erased.
         * 
         * Once meshes are exported but before exporting materials, each material is analyzed
         * to know if it is assigned to Opaque meshes, Transparent ones, or both.
         * It's like doing a shopping list:
         * - Red material is assigned to Shape1 Opaque => export red material as opaque
         * - Green material is assigned to Shape2 Transparent => export green material as transparent
         * - Blue material is assigned to Shape3 Opaque and Shape4 Transparent => export blue material as transparent and duplicate it for opaque meshes
         * 
         * Once all materials are exported, for each material requiring duplication (ex: Blue),
         * the material id of all opaque meshes is updated to point at duplicated material
         * 
         * In both analyse and update steps, multimaterials are much more complicated to handle.
         */

        /// <summary>
        /// Material duplication data binded to each (simple) material
        /// </summary>
        Dictionary<string, MaterialDuplicationData> materialDuplicationDatas;

        /// <summary>
        /// List of meshes tagged as 'Opaque' for Arnold binded to each multimaterial
        /// </summary>
        Dictionary<string, List<BabylonMesh>> meshesOpaqueMultis;

        /// <summary>
        /// List of meshes tagged as 'Non Opaque' for Arnold binded to each multimaterial
        /// </summary>
        Dictionary<string, List<BabylonMesh>> meshesTransparentMultis;

        /// <summary>
        /// Analyse each material to know if it is assigned to Opaque meshes, Transparent ones, or both
        /// Generate an ID for each material requiring to be duplicated. This ID is used later.
        /// </summary>
        /// <param name="babylonScene"></param>
        private void GenerateMaterialDuplicationDatas(BabylonScene babylonScene)
        {
            materialDuplicationDatas = new Dictionary<string, MaterialDuplicationData>();

            // --- Get the meshes binded to each simple AiStandardSurface material ---
            foreach (MFnDependencyNode material in referencedMaterials)
            {
                if (isAiStandardSurfaceNotStdNotPBS(material))
                {
                    string multiMaterialId = material.uuid().asString();

                    // Retreive meshes using this material directly
                    // Meshes are split into 2 categories: Opaque or Transparent
                    List<BabylonMesh> meshesOpaque = new List<BabylonMesh>();
                    List<BabylonMesh> meshesTransparent = new List<BabylonMesh>();
                    getMeshesByMaterialId(babylonScene, material.uuid().asString(), meshesOpaque, meshesTransparent);

                    // Get material duplication data or create new ones
                    if (!materialDuplicationDatas.ContainsKey(multiMaterialId))
                    {
                        materialDuplicationDatas.Add(multiMaterialId, new MaterialDuplicationData());
                    }
                    MaterialDuplicationData materialDuplicationData = materialDuplicationDatas[multiMaterialId];

                    // Store meshes
                    materialDuplicationData.meshesOpaque = meshesOpaque;
                    materialDuplicationData.meshesTransparent = meshesTransparent;
                }
            }

            // --- Get the meshes binded to each multimaterial ---
            meshesOpaqueMultis = new Dictionary<string, List<BabylonMesh>>();
            meshesTransparentMultis = new Dictionary<string, List<BabylonMesh>>();
            foreach (KeyValuePair<string, List<MFnDependencyNode>> multiMaterialKeyValue in multiMaterials)
            {
                string multiMaterialId = multiMaterialKeyValue.Key;
                List<MFnDependencyNode> subMaterials = multiMaterialKeyValue.Value;

                // Retreive meshes using this material as a multi material with at least one AiStandardSurface sub material
                // Meshes are split into 2 categories: Opaque or Transparent
                List<BabylonMesh> meshesOpaqueMulti = new List<BabylonMesh>();
                List<BabylonMesh> meshesTransparentMulti = new List<BabylonMesh>();
                if (subMaterials.Find(isAiStandardSurfaceNotStdNotPBS) != null)
                {
                    getMeshesByMaterialId(babylonScene, multiMaterialId, meshesOpaqueMulti, meshesTransparentMulti);
                }

                meshesOpaqueMultis.Add(multiMaterialId, meshesOpaqueMulti);
                meshesTransparentMultis.Add(multiMaterialId, meshesTransparentMulti);
            }

            // --- Compute the number of opaque and transparent meshes from multimaterials ---
            foreach (KeyValuePair<string, List<MFnDependencyNode>> multiMaterialKeyValue in multiMaterials)
            {
                string multiMaterialId = multiMaterialKeyValue.Key;
                List<MFnDependencyNode> subMaterials = multiMaterialKeyValue.Value;

                // For each multi material, get the number of opaque and transparent meshes using it
                int nbMeshesOpaqueMulti = meshesOpaqueMultis[multiMaterialId].Count;
                int nbMeshesTransparentMulti = meshesTransparentMultis[multiMaterialId].Count;

                // For each AiStandardSurface submaterial
                foreach (MFnDependencyNode subMaterial in subMaterials.FindAll(isAiStandardSurfaceNotStdNotPBS))
                {
                    string subMaterialId = subMaterial.uuid().asString();

                    // Get material duplication data or create new ones
                    if (!materialDuplicationDatas.ContainsKey(subMaterialId))
                    {
                        materialDuplicationDatas.Add(subMaterialId, new MaterialDuplicationData());
                    }
                    MaterialDuplicationData materialDuplicationData = materialDuplicationDatas[subMaterialId];

                    // Update the number of opaque and transparent meshes using submaterial as part of the multi material
                    materialDuplicationData.nbMeshesOpaqueMulti += nbMeshesOpaqueMulti;
                    materialDuplicationData.nbMeshesTransparentMulti += nbMeshesTransparentMulti;
                }
            }

            // Generate here a new UUID for each AiStandardSurface material assigned to both opaque and transparent meshes
            // This UUID is used later when exporting (simple) materials
            foreach (KeyValuePair<string, MaterialDuplicationData> materialDuplicationDataKeyValue in materialDuplicationDatas)
            {
                MaterialDuplicationData materialDuplicationValue = materialDuplicationDataKeyValue.Value;

                if (materialDuplicationValue.isDuplicationRequired())
                {
                    string duplicationId = Tools.GenerateUUID();
                    materialDuplicationValue.idOpaque = duplicationId;
                }
            }
        }

        private void UpdateMeshesMaterialId(BabylonScene babylonScene)
        {
            // --- Update meshes using simple materials ---
            foreach (MFnDependencyNode material in referencedMaterials)
            {
                string materialId = material.uuid().asString();

                // If it's an AiStandardSurface material
                if (materialDuplicationDatas.ContainsKey(materialId))
                {
                    MaterialDuplicationData materialDuplicationValue = materialDuplicationDatas[materialId];

                    // If both Opaque and Transparent meshes are using this material
                    // And the material contains alpha data
                    if (materialDuplicationValue.idOpaque != null && materialDuplicationValue.isDuplicationSuccess)
                    {
                        // The duplicated material is used for Opaque meshes instead of the original one
                        foreach (BabylonMesh meshOpaque in materialDuplicationValue.meshesOpaque)
                        {
                            meshOpaque.materialId = materialDuplicationValue.idOpaque;
                        }
                    }
                }
            }

            // --- Update meshes using multimaterials ---
            var multiMaterialsList = new List<BabylonMultiMaterial>(babylonScene.MultiMaterialsList);
            foreach (BabylonMultiMaterial babylonMultiMaterial in multiMaterialsList)
            {
                string multiMaterialId = babylonMultiMaterial.id;

                // If at least one Opaque mesh is using this multimaterial
                if (meshesOpaqueMultis[multiMaterialId].Count > 0)
                {
                    List<MFnDependencyNode> subMaterials = multiMaterials[multiMaterialId];

                    // Build a list of all opaque sub material ids
                    List<string> opaqueSubMaterials = new List<string>();
                    foreach (MFnDependencyNode subMaterial in subMaterials)
                    {
                        string subMaterialId = subMaterial.uuid().asString();

                        // If it's an AiStandardSurface material
                        if (materialDuplicationDatas.ContainsKey(subMaterialId))
                        {
                            MaterialDuplicationData materialDuplicationValue = materialDuplicationDatas[subMaterialId];

                            // If both Opaque and Transparent meshes are using this sub material
                            // And the sub material contains alpha data
                            if (materialDuplicationValue.idOpaque != null && materialDuplicationValue.isDuplicationSuccess)
                            {
                                // The duplicated material is used for Opaque meshes instead of the original one
                                subMaterialId = materialDuplicationValue.idOpaque;
                            }
                        }

                        opaqueSubMaterials.Add(subMaterialId);
                    }

                    // If both Opaque and Transparent meshes are using this multimaterial
                    if (meshesTransparentMultis[multiMaterialId].Count > 0)
                    {
                        // Duplicate multimaterial
                        BabylonMultiMaterial multiMaterialCloned = new BabylonMultiMaterial(babylonMultiMaterial);

                        // Update id
                        string idMultiMaterialCloned = Tools.GenerateUUID();
                        multiMaterialCloned.id = idMultiMaterialCloned;

                        // Update sub materials
                        multiMaterialCloned.materials = opaqueSubMaterials.ToArray();

                        // Store duplication
                        babylonScene.MultiMaterialsList.Add(multiMaterialCloned);

                        // Update meshes id
                        foreach (BabylonMesh meshOpaque in meshesOpaqueMultis[multiMaterialId])
                        {
                            meshOpaque.materialId = idMultiMaterialCloned;
                        }
                    }
                    else
                    {
                        // Only Opaque meshes are using this multimaterial
                        // Simply override sub materials with all opaque ones
                        babylonMultiMaterial.materials = opaqueSubMaterials.ToArray();
                    }
                }
            }
        }

        private BabylonPBRMetallicRoughnessMaterial DuplicateMaterial(BabylonPBRMetallicRoughnessMaterial babylonMaterial, MaterialDuplicationData materialDuplicationData)
        {
            // Duplicate material
            BabylonPBRMetallicRoughnessMaterial babylonMaterialCloned = new BabylonPBRMetallicRoughnessMaterial(babylonMaterial);

            // Give it a new UUID
            babylonMaterialCloned.id = materialDuplicationData.idOpaque;

            // This material is used for opaque meshes
            // Reset its alpha
            babylonMaterialCloned.alpha = 1;
            if (babylonMaterialCloned.baseTexture != null)
            {
                babylonMaterialCloned.baseTexture = new BabylonTexture(babylonMaterialCloned.baseTexture);
                babylonMaterialCloned.baseTexture.hasAlpha = false;
            }

            // Set transparency for opaque material
            babylonMaterialCloned.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.OPAQUE;

            // Duplication is done correctly, thus meshes material id can be updated accordingly
            materialDuplicationData.isDuplicationSuccess = true;

            return babylonMaterialCloned;
        }

        // --- Utilities ---

        /// <summary>
        /// Retreive meshes having specified material id
        /// Meshes are split into 2 categories: Opaque or Transparent
        /// </summary>
        /// <param name="babylonScene"></param>
        /// <param name="materialId"></param>
        /// <param name="meshesOpaque"></param>
        /// <param name="meshesTransparent"></param>
        private void getMeshesByMaterialId(BabylonScene babylonScene, string materialId, List<BabylonMesh> meshesOpaque, List<BabylonMesh> meshesTransparent)
        {
            babylonScene.MeshesList.ForEach(mesh =>
            {
                if (mesh.materialId == materialId)
                {
                    // Get mesh full path name (unique)
                    MStringArray meshFullPathName = new MStringArray();
                    // Surround uuid with quotes like so: ls "18D0785F-4E8E-1621-01E1-84AD39F92289";
                    // ls command output must be an array
                    MGlobal.executeCommand($@"ls ""{mesh.id}"";", meshFullPathName);

                    int meshOpaqueInt;
                    MGlobal.executeCommand($@"getAttr {meshFullPathName[0]}.aiOpaque;", out meshOpaqueInt);
                    if (meshOpaqueInt == 1)
                    {
                        meshesOpaque.Add(mesh);
                    }
                    else
                    {
                        meshesTransparent.Add(mesh);
                    }
                }
            });
        }

        private bool isAiStandardSurfaceNotStdNotPBS(MFnDependencyNode materialDependencyNode)
        {
            // TODO - This is a fix until found a better way to identify AiStandardSurface material
            MObject materialObject = materialDependencyNode.objectProperty;
            return !materialObject.hasFn(MFn.Type.kLambert) && !isStingrayPBSMaterial(materialDependencyNode) && isAiStandardSurface(materialDependencyNode);
        }
    }
}
