using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Particle Systems", 30)]
	public sealed class ParticleSystems : EditorScriptComponent
	{
        [BabylonProperty]
        public BabylonParticleExporter exportOption = BabylonParticleExporter.ScriptEngine;

        [Header("[System Properties]")]

        [BabylonProperty]
        public Texture2D textureImage = null;
        [BabylonProperty]
        public string particleName = null;
        [BabylonProperty, Range(0.0f, 100000.0f)]
        public float duration = 0.0f;
        [BabylonProperty]
        public bool autoStart = true;
        [BabylonProperty]
        public bool loopPlay = false;
        [BabylonProperty]
        public float delayTime = 0.0f;
        [BabylonProperty]
        public BabylonParticleBlend blendMode = BabylonParticleBlend.OneOne;

        [Header("[Particle Properties]")]
        
        [BabylonProperty]
        public int capacity = 1000;
        [BabylonProperty]
        public float startSpeed = 0.01f;
        [BabylonProperty]
        public BabylonParticleEmission emitType = BabylonParticleEmission.Rate;
        [BabylonProperty]
        public float emitRate = 10.0f;
        [BabylonProperty]
        public BabylonParticleBusrt[] emitBurst = null;
        [BabylonProperty]
        public Vector2 emitPower = new Vector2(1.0f, 1.0f);
        [BabylonProperty]
        public Vector2 lifeTime = new Vector2(1.0f, 1.0f);
        [BabylonProperty]
        public Vector2 particleSize = new Vector2(1.0f, 1.0f);
        [BabylonProperty]
        public Vector2 angularSpeed = new Vector2(0.0f, 0.0f);

        [Header("[Color Properties]")]
        
        [BabylonProperty]
        public Color color1 = Color.white;
        [BabylonProperty]
        public Color color2 = Color.white;
        [BabylonProperty]
        public Color colorDead = Color.black;
        [BabylonProperty]
        public Color textureMask = Color.white;

        [Header("[Shape Properties]")]

        [BabylonProperty]
        public BabylonShapePreset shapePreset = BabylonShapePreset.ManualShape;
        [BabylonProperty]
        public Vector3 direction1 = new Vector3(0.0f, 1.0f, 0.0f);
        [BabylonProperty]
        public Vector3 direction2 = new Vector3(0.0f, 1.0f, 0.0f);
        [BabylonProperty]
        public Vector3 minEmitBox = new Vector3(0.0f, 0.0f, 0.0f);
        [BabylonProperty]
        public Vector3 maxEmitBox = new Vector3(0.0f, 0.0f, 0.0f);

        [Header("[Gravity Properties]")]

        [BabylonProperty]
        public BabylonGravityMode gravityMode = BabylonGravityMode.ManualVector;
        [BabylonProperty]
        public Vector3 gravityVector = new Vector3(0.0f, 0.0f, 0.0f);
        [Range(0.0f, 1.0f)]
        [BabylonProperty]
        public float gravityMultiplier = 0.0f;

        [Header("[Custom Properties]")]
        
        [BabylonProperty]
        public string customShaderEffect = null;
        [BabylonProperty]
        public BabylonEffectTiming customShaderTiming = null;
        [BabylonProperty]
        public string[] customShaderDefines = null;
        [BabylonProperty]
        public string[] customShaderUniforms = null;
        [BabylonProperty]
        public string[] customShaderSamplers = null;
        ///////////////////////////////////////////////////////////
        // Note: Using Manual Property Serialzation
        ///////////////////////////////////////////////////////////
        public BabylonShurikenModule customUpdateFunctions = null;
        
		public ParticleSystems()
		{
			this.babylonClass = "BABYLON.UniversalParticleSystem";
            this.OnExportProperties = this.OnExportPropertiesHandler;
		}

        public void OnExportPropertiesHandler(GameObject unityGameObject, Dictionary<string, object> propertyBag)
        {
            bool updateEffectTime = false;
            float maximumEffectTime = 0.0f;
            if (customShaderTiming != null) {
                updateEffectTime = customShaderTiming.updateEffectTime;
                maximumEffectTime = customShaderTiming.maximumEffectTime;
            }

            bool updateOverTime = false;
            int framesPerSecond = 30;
            if (customUpdateFunctions != null) {
                updateOverTime = customUpdateFunctions.updateOverTime;
                framesPerSecond = customUpdateFunctions.framesPerSecond;
            }

            // Attach custom update module properties
            Dictionary<string, object> objectInfo = new Dictionary<string, object>();
            objectInfo.Add("updateOverTime", updateOverTime);
            objectInfo.Add("framesPerSecond", framesPerSecond);
            objectInfo.Add("speedModule", null);
            objectInfo.Add("emissionModule", null);
            objectInfo.Add("velocityModule", null);
            objectInfo.Add("colorModule", null);
            objectInfo.Add("sizingModule", null);
            objectInfo.Add("rotationModule", null);
            objectInfo.Add("updateEffectTime", updateEffectTime);
            objectInfo.Add("maximumEffectTime", maximumEffectTime);
            propertyBag.Add("ShurikenUpdateModules", objectInfo);
        }

        public void SetSceneGravityVector()
        {
            if (this.gravityMultiplier != 0.0f) this.gravityMultiplier = 0.0f;
        }

        public void SetSceneGravityMultiplier()
        {
            if (this.gravityVector.x != 0.0f) this.gravityVector.x = 0.0f;
            if (this.gravityVector.y != 0.0f) this.gravityVector.y = 0.0f;
            if (this.gravityVector.z != 0.0f) this.gravityVector.z = 0.0f;
        }

        public void SetDefaultPresetShape()
        {
            if (this.direction1.x != 0.0f) this.direction1.x = 0.0f; 
            if (this.direction1.y != 1.0f) this.direction1.y = 1.0f; 
            if (this.direction1.z != 0.0f) this.direction1.z = 0.0f; 

            if (this.direction2.x != 0.0f) this.direction2.x = 0.0f; 
            if (this.direction2.y != 1.0f) this.direction2.y = 1.0f; 
            if (this.direction2.z != 0.0f) this.direction2.z = 0.0f; 

            if (this.minEmitBox.x != 0.0f) this.minEmitBox.x = 0.0f; 
            if (this.minEmitBox.y != 0.0f) this.minEmitBox.y = 0.0f; 
            if (this.minEmitBox.z != 0.0f) this.minEmitBox.z = 0.0f; 

            if (this.maxEmitBox.x != 0.0f) this.maxEmitBox.x = 0.0f; 
            if (this.maxEmitBox.y != 0.0f) this.maxEmitBox.y = 0.0f; 
            if (this.maxEmitBox.z != 0.0f) this.maxEmitBox.z = 0.0f; 
        }

        public void SetBoxVolumePresetShape()
        {
            if (this.direction1.x != 0.0f) this.direction1.x = 0.0f; 
            if (this.direction1.y != 1.0f) this.direction1.y = 1.0f; 
            if (this.direction1.z != 0.0f) this.direction1.z = 0.0f; 

            if (this.direction2.x != 0.0f) this.direction2.x = 0.0f; 
            if (this.direction2.y != 1.0f) this.direction2.y = 1.0f; 
            if (this.direction2.z != 0.0f) this.direction2.z = 0.0f; 

            if (this.minEmitBox.x != -0.5f) this.minEmitBox.x = -0.5f; 
            if (this.minEmitBox.y != -0.5f) this.minEmitBox.y = -0.5f; 
            if (this.minEmitBox.z != -0.5f) this.minEmitBox.z = -0.5f; 

            if (this.maxEmitBox.x != 0.5f) this.maxEmitBox.x = 0.5f; 
            if (this.maxEmitBox.y != 0.5f) this.maxEmitBox.y = 0.5f; 
            if (this.maxEmitBox.z != 0.5f) this.maxEmitBox.z = 0.5f; 
        }
	}

    [CustomEditor(typeof(ParticleSystems)), CanEditMultipleObjects]
    public class ParticleSystemsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ParticleSystems myScript = (ParticleSystems)target;
            // Validate Duration Based Properties
            if (myScript.duration <= 0.0f) {
                if (myScript.loopPlay == true) {
                    myScript.loopPlay = false;
                }
                if (myScript.emitType == BabylonParticleEmission.Burst) {
                    myScript.emitType = BabylonParticleEmission.Rate;
                }
                if (myScript.customUpdateFunctions.updateOverTime == true) {
                    myScript.customUpdateFunctions.updateOverTime = false;
                }
            }
            // TODO: Force Shape Preset Property Values
            if (myScript.shapePreset != BabylonShapePreset.ManualShape) {
                switch(myScript.shapePreset) {
                    case BabylonShapePreset.BoxVolume:
                        myScript.SetBoxVolumePresetShape();
                        break;
                    default:
                        myScript.SetDefaultPresetShape();
                        break;
                }
            }
            // Force Scene Gravity Vector And Multiplier Property Values
            if (myScript.gravityMode == BabylonGravityMode.ManualVector) {
                myScript.SetSceneGravityVector();
            } else if (myScript.gravityMode == BabylonGravityMode.SceneMultiplier) {
                myScript.SetSceneGravityMultiplier();
            }
            //if(GUILayout.Button("Do Somthing"))
            //{
                //myScript.DoSomthing();
            //}
        }
    }

    [System.Serializable]
    public enum BabylonParticleExporter {
        ScriptEngine = 0,
        NativeSceneFile = 1
    }

    [System.Serializable]
    public enum BabylonParticleEmission {
        Rate = 0,
        Burst = 1
    }

    [System.Serializable]
    public enum BabylonGravityMode
    {
        ManualVector = 0,
        SceneMultiplier = 1
    }

    [System.Serializable]
    public enum BabylonShapePreset
    {
        ManualShape = -1,
        DefaultPreset = 0,
        BoxVolume = 1,
        BoxShell = 2,
        BoxEdge = 3,
        ConeVolume = 4,
        ConeBottom = 5,
        SphereRadius = 6,
        SphereShell = 7,
        hemisphereRadius = 8,
        hemisphereShell = 9
    }

    [System.Serializable]
    public class BabylonParticleBusrt
    {
        [BabylonProperty]
        public float time = 0.0f;

        [BabylonProperty]
        public int minCount = 100;
        
        [BabylonProperty]
        public int maxCount = 100;
    }

    [System.Serializable]
    public class BabylonShurikenModule
    {
        public bool updateOverTime = false;
        [Range(30, 60)] public int framesPerSecond = 30;

        // TODO: Add Shape Properties Here 

        public BabylonShurikenSpeedModule updateSpeedModule = null;
        public BabylonShurikenEmissionModule updateEmissionModule = null;
        public BabylonShurikenVelocityModule updateVelocityModule = null;
        public BabylonShurikenColorModule updateColorModule = null;
        public BabylonShurikenSizeModule updateSizingModule = null;
        public BabylonShurikenRotationModule updateRotationModule = null;
    }

    [System.Serializable]
    public class BabylonEffectTiming
    {
        public bool updateEffectTime = false;
        public int maximumEffectTime = 0;
    }

    [System.Serializable]
    public class BabylonShurikenSizeModule
    {
        public bool updateSize = false;

        [SerializeField]
        public AnimationCurve sizeCurve = null;
    }

    [System.Serializable]
    public class BabylonShurikenColorModule
    {
        public bool updateColor = false;

        [SerializeField]
        public AnimationCurve colorCurve = null;
    }
    

    [System.Serializable]
    public class BabylonShurikenSpeedModule
    {
        public bool updateSpeed = false;

        [SerializeField]
        public AnimationCurve speedCurve = null;
    }

    [System.Serializable]
    public class BabylonShurikenVelocityModule
    {
        public bool updateVelocity = false;

        [SerializeField]
        public AnimationCurve velocityCurve = null;
    }

    [System.Serializable]
    public class BabylonShurikenEmissionModule
    {
        public bool updateEmission = false;

        [SerializeField]
        public AnimationCurve emissionCurve = null;
    }

    [System.Serializable]
    public class BabylonShurikenRotationModule
    {
        public bool updateRotation = false;

        [SerializeField]
        public AnimationCurve rotationCurve = null;
    }
}
