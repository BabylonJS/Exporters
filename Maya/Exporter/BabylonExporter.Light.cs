using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        void ExportDefaultLight(BabylonScene babylonScene)
        {
            var babylonLight = new BabylonLight();
            babylonLight.name = "Default light";
            babylonLight.id = Guid.NewGuid().ToString();
            babylonLight.type = 3;
            babylonLight.groundColor = new float[] { 0, 0, 0 };
            babylonLight.position = new float[] { 0, 0, 0 };
            babylonLight.direction = new[] { 0, 1.0f, 0 };
 
            babylonLight.intensity = 1;

            babylonLight.diffuse = new[] { 1.0f, 1.0f, 1.0f };
            babylonLight.specular = new[] { 1.0f, 1.0f, 1.0f };

            babylonScene.LightsList.Add(babylonLight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mDagPath">DAG path to the transform above light</param>
        /// <param name="babylonScene"></param>
        /// <returns></returns>
        private BabylonNode ExportLight(MDagPath mDagPath, BabylonScene babylonScene)
        {

            RaiseMessage(mDagPath.partialPathName, 1);

            // Transform above light
            MFnTransform mFnLightTransform = new MFnTransform(mDagPath);

            // Light direct child of the transform
            MFnLight mFnLight = null;
            bool createDummy = false;
            for (uint i = 0; i < mFnLightTransform.childCount; i++)
            {
                MObject childObject = mFnLightTransform.child(i);
                if (childObject.hasFn(MFn.Type.kLight))
                {
                    var _mFnLight = new MFnLight(childObject);
                    if (!_mFnLight.isIntermediateObject)
                    {
                        mFnLight = _mFnLight;
                    }
                }
                else
                {
                    if (childObject.hasFn(MFn.Type.kTransform))
                    {
                        createDummy = true;
                    }
                }
            }
            if (mFnLight == null)
            {
                RaiseError("No light found has child of " + mDagPath.fullPathName);
                return null;
            }

            RaiseMessage("mFnLight.fullPathName=" + mFnLight.fullPathName, 2);
            
            // --- prints ---
            #region prints

            // MFnLight
            RaiseVerbose("BabylonExporter.Light | mFnLight data", 2);
            RaiseVerbose("BabylonExporter.Light | mFnLight.color.toString()=" + mFnLight.color.toString(), 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.intensity=" + mFnLight.intensity, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.useRayTraceShadows=" + mFnLight.useRayTraceShadows, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.shadowColor.toString()=" + mFnLight.shadowColor.toString(), 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.centerOfIllumination=" + mFnLight.centerOfIllumination, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.numShadowSamples=" + mFnLight.numShadowSamples, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.rayDepthLimit=" + mFnLight.rayDepthLimit, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.opticalFXvisibility.toString()=" + mFnLight.opticalFXvisibility.toString(), 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.lightIntensity.toString()=" + mFnLight.lightIntensity.toString(), 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.instanceCount(true)=" + mFnLight.instanceCount(true), 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.lightDirection(0).toString()=" + mFnLight.lightDirection(0).toString(), 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.lightAmbient=" + mFnLight.lightAmbient, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.lightDiffuse=" + mFnLight.lightDiffuse, 3);
            RaiseVerbose("BabylonExporter.Light | mFnLight.lightSpecular=" + mFnLight.lightSpecular, 3);

            switch (mFnLight.objectProperty.apiType)
            {
                case MFn.Type.kSpotLight:
                    MFnSpotLight mFnSpotLight = new MFnSpotLight(mFnLight.objectProperty);
                    // MFnNonAmbientLight
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.decayRate=" + mFnSpotLight.decayRate, 3); // dropdown enum value
                    // MFnNonExtendedLight
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.shadowRadius=" + mFnSpotLight.shadowRadius, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.castSoftShadows=" + mFnSpotLight.castSoftShadows, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.useDepthMapShadows=" + mFnSpotLight.useDepthMapShadows, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.depthMapFilterSize()=" + mFnSpotLight.depthMapFilterSize(), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.depthMapResolution()=" + mFnSpotLight.depthMapResolution(), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.depthMapBias()=" + mFnSpotLight.depthMapBias(), 3);
                    // MFnSpotLight
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.coneAngle=" + mFnSpotLight.coneAngle, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.penumbraAngle=" + mFnSpotLight.penumbraAngle, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.dropOff=" + mFnSpotLight.dropOff, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.barnDoors=" + mFnSpotLight.barnDoors, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.useDecayRegions=" + mFnSpotLight.useDecayRegions, 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.startDistance(MFnSpotLight.MDecayRegion.kFirst)=" + mFnSpotLight.startDistance(MFnSpotLight.MDecayRegion.kFirst), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kFirst)=" + mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kFirst), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.startDistance(MFnSpotLight.MDecayRegion.kSecond)=" + mFnSpotLight.startDistance(MFnSpotLight.MDecayRegion.kSecond), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kSecond)=" + mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kSecond), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.startDistance(MFnSpotLight.MDecayRegion.kThird)=" + mFnSpotLight.startDistance(MFnSpotLight.MDecayRegion.kThird), 3);
                    RaiseVerbose("BabylonExporter.Light | mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kThird)=" + mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kThird), 3);
                    break;
            }

            Print(mFnLightTransform, 2, "Print ExportLight mFnTransform");

            Print(mFnLight, 2, "Print ExportLight mFnLight");

            #endregion

            if (IsLightExportable(mFnLight, mDagPath) == false)
            {
                return null;
            }

            var babylonLight = new BabylonLight { name = mFnLightTransform.name, id = mFnLightTransform.uuid().asString() };
            // User custom attributes
            babylonLight.metadata = ExportCustomAttributeFromTransform(mFnLightTransform);

            MVector vDir = new MVector(0, 0, -1);
            MTransformationMatrix transformationMatrix;

            // Hierarchy
            BabylonNode dummy = null;
            if (createDummy)
            {
                dummy = ExportDummy(mDagPath, babylonScene);
                dummy.name = "_" + dummy.name + "_";
                babylonLight.id = Guid.NewGuid().ToString();
                babylonLight.parentId = dummy.id;
                babylonLight.hasDummy = true;

                // The position is stored by the dummy parent and the default direction is downward and it is updated by the rotation of the parent dummy
                babylonLight.position = new[] { 0f, 0f, 0f };
                babylonLight.direction = new[] { 0f, -1f, 0f };
            }
            else
            {
                ExportHierarchy(babylonLight, mFnLightTransform);
                // Position / rotation / scaling
                ExportTransform(babylonLight, mFnLightTransform);

                // Direction
                vDir = new MVector(0, 0, -1);
                transformationMatrix = new MTransformationMatrix(mFnLightTransform.transformationMatrix);
                vDir = vDir.multiply(transformationMatrix.asMatrixProperty);
                vDir.normalize();
                babylonLight.direction = new[] { (float)vDir.x, (float)vDir.y, -(float)vDir.z };
            }

            // Common fields 
            babylonLight.intensity = mFnLight.intensity;
            babylonLight.diffuse = mFnLight.lightDiffuse ? mFnLight.color.toArrayRGB() : new float[] { 0, 0, 0 };
            babylonLight.specular = mFnLight.lightSpecular ? mFnLight.color.toArrayRGB() : new float[] { 0, 0, 0 };

            // Type
            switch (mFnLight.objectProperty.apiType)
            {
                case MFn.Type.kPointLight:
                    babylonLight.type = 0;
                    break;
                case MFn.Type.kSpotLight:
                    MFnSpotLight mFnSpotLight = new MFnSpotLight(mFnLight.objectProperty);
                    babylonLight.type = 2;
                    babylonLight.angle = (float)mFnSpotLight.coneAngle;
                    babylonLight.exponent = 1;

                    if (mFnSpotLight.useDecayRegions)
                    {
                        babylonLight.range = mFnSpotLight.endDistance(MFnSpotLight.MDecayRegion.kThird); // Max distance
                    }
                    break;
                case MFn.Type.kDirectionalLight:
                    babylonLight.type = 1;
                    break;
                case MFn.Type.kAmbientLight:
                    babylonLight.type = 3;
                    babylonLight.groundColor = new float[] { 0, 0, 0 };

                    // No emit diffuse /specular checkbox for ambient light
                    babylonLight.diffuse = mFnLight.color.toArrayRGB();
                    babylonLight.specular = babylonLight.diffuse;

                    // Direction
                    if (!createDummy)
                    {
                        vDir = new MVector(0, 1, 0);
                        transformationMatrix = new MTransformationMatrix(mFnLightTransform.transformationMatrix);
                        vDir = vDir.multiply(transformationMatrix.asMatrixProperty);
                        vDir.normalize();
                        babylonLight.direction = new[] { (float)vDir.x, (float)vDir.y, -(float)vDir.z };
                    }
                    break;
                case MFn.Type.kAreaLight:
                case MFn.Type.kVolumeLight:
                    RaiseError("Unsupported light type '" + mFnLight.objectProperty.apiType + "' for DAG path '" + mFnLight.fullPathName + "'. Light is ignored. Supported light types are: ambient, directional, point and spot.", 1);
                    return null;
                default:
                    RaiseWarning("Unknown light type '" + mFnLight.objectProperty.apiType + "' for DAG path '" + mFnLight.fullPathName + "'. Light is ignored.", 1);
                    return null;
            }

            // TODO - Shadows
            
            //Variable declaration
            MStringArray enlightedMeshesFullPathNames = new MStringArray();
            List<string> includeMeshesIds = new List<string>();
            MStringArray kTransMesh = new MStringArray();
            String typeMesh = null;
            MStringArray UUIDMesh = new MStringArray();

            //MEL Command that get the enlighted mesh for a given light
            MGlobal.executeCommand($@"lightlink -query -light {mFnLightTransform.fullPathName};", enlightedMeshesFullPathNames);

            //For each enlighted mesh
            foreach (String Mesh in enlightedMeshesFullPathNames)
            {
                //MEL Command use to get the type of each mesh
                typeMesh = MGlobal.executeCommandStringResult($@"nodeType -api {Mesh};");

                //We are targeting the type kMesh and not kTransform (for parenting)
                if (typeMesh == "kMesh")
                {
                    MGlobal.executeCommand($@"listRelatives -parent -fullPath {Mesh};", kTransMesh);

                    //And finally the MEL Command for the uuid of each mesh
                    MGlobal.executeCommand($@"ls -uuid {kTransMesh[0]};", UUIDMesh);
                    includeMeshesIds.Add(UUIDMesh[0]);
                }
            }

            babylonLight.includedOnlyMeshesIds = includeMeshesIds.ToArray();

            // Animations
            if (exportParameters.bakeAnimationFrames)
            {
                ExportNodeAnimationFrameByFrame(babylonLight, mFnLightTransform);
            }
            else
            {
                ExportNodeAnimation(babylonLight, mFnLightTransform);
            }

            babylonScene.LightsList.Add(babylonLight);

            return babylonLight;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mFnDagNode">DAG function set of the node (light) below the transform</param>
        /// <param name="mDagPath">DAG path of the transform above the node</param>
        /// <returns></returns>
        private bool IsLightExportable(MFnDagNode mFnDagNode, MDagPath mDagPath)
        {
            return IsNodeExportable(mFnDagNode, mDagPath);
        }
    }
}
