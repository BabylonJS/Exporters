using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;


namespace Unity3D2Babylon
{
    public enum BabylonCurveValues
    {
        StartCurveValue = 0,
        EndCurveValue = 1
    }
    
    [ExecuteInEditMode]
    public class ExporterParticles : EditorWindow
    {
        ParticleSystems babylonParticles = null;
        ParticleSystem shurikenParticles = null;
        Color defaultColor = Color.white;
        float emitRateModifier = 5.0f;
        float updateSpeedModifier = 0.01f;
        BabylonCurveValues convertCurveValues = BabylonCurveValues.StartCurveValue;
        bool exportShurikenData = true;
        bool keepGeneratorOpen = true;

        [MenuItem("BabylonJS/Particle Generator", false, 209)]
        public static void InitConverter()
        {
            ExporterParticles particles = ScriptableObject.CreateInstance<ExporterParticles>();
            particles.OnInitialize();
            particles.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(500, 248);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Particle Generator");
            if(babylonParticles == null && Selection.activeObject is ParticleSystems) {
                babylonParticles = Selection.activeObject as ParticleSystems;                
            }
            if(shurikenParticles == null && Selection.activeObject is ParticleSystem) {
                shurikenParticles = Selection.activeObject as ParticleSystem;                
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            babylonParticles = EditorGUILayout.ObjectField("Babylon Particle System:", babylonParticles, typeof(ParticleSystems), true) as ParticleSystems;
            EditorGUILayout.Space();
            shurikenParticles = EditorGUILayout.ObjectField("Shuriken Particle System:", shurikenParticles, typeof(ParticleSystem), true) as ParticleSystem;
            EditorGUILayout.Space();
            defaultColor = EditorGUILayout.ColorField("Default Particle Color:", defaultColor);
            EditorGUILayout.Space();
            updateSpeedModifier = (float)EditorGUILayout.Slider("Start Speed Modifier:", updateSpeedModifier, 0.0f, 1.0f);
            EditorGUILayout.Space();
            emitRateModifier = (float)EditorGUILayout.Slider("Emit Rate Modifier:", emitRateModifier, 0.0f, 100.0f);
            EditorGUILayout.Space();
            convertCurveValues = (BabylonCurveValues)EditorGUILayout.EnumPopup("Convert Curve Values:", convertCurveValues, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            exportShurikenData = EditorGUILayout.Toggle("Custom Shuriken Data:", exportShurikenData);
            EditorGUILayout.Space();
            keepGeneratorOpen = EditorGUILayout.Toggle("Keep Generator Open:", keepGeneratorOpen);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Parse Shuriken Particle System"))
            {
                if (shurikenParticles && babylonParticles)
                {
                    Parse();
                }
                if (!shurikenParticles) {
                    ExporterWindow.ShowMessage("You must select a shuriken particle system");
                } else if (!babylonParticles) {
                    ExporterWindow.ShowMessage("You must select a babylon particle system");
                }
            }
        }

        public void Parse()
        {
            // Validate Project Platform
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;

            try
            {
                ExporterWindow.ReportProgress(1, "Generating particle system data... This may take a while.");
                System.Threading.Thread.Sleep(500);
                bool starting = (convertCurveValues == BabylonCurveValues.StartCurveValue);
                var renderer = shurikenParticles.GetComponent<Renderer>();
                if (renderer != null) babylonParticles.textureImage = renderer.sharedMaterial.mainTexture as Texture2D;
                babylonParticles.loopPlay = shurikenParticles.main.loop;
                babylonParticles.duration = shurikenParticles.main.duration;
                babylonParticles.capacity = shurikenParticles.main.maxParticles;
                
                // Bursting
                babylonParticles.emitBurst = null;
                if (shurikenParticles.emission.burstCount > 0) {
                    ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[shurikenParticles.emission.burstCount];
                    shurikenParticles.emission.GetBursts(bursts);
                    var burstList = new List<BabylonParticleBusrt>();
                    foreach (var burst in bursts) {
                        burstList.Add(new BabylonParticleBusrt() {
                            time = burst.time,
                            minCount = burst.minCount,
                            maxCount = burst.maxCount
                         });
                    }
                    if (burstList.Count > 0) babylonParticles.emitBurst = burstList.ToArray();
                }

                // TODO: Direction

                // TODO: Volume

                // Gravity
                if (shurikenParticles.main.gravityModifier.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.gravityMultiplier = shurikenParticles.main.gravityModifier.constant;
                } else if (shurikenParticles.main.gravityModifier.mode == ParticleSystemCurveMode.TwoConstants) {
                    babylonParticles.gravityMultiplier = (starting) ? shurikenParticles.main.gravityModifier.constantMin : shurikenParticles.main.gravityModifier.constantMax;
                } else if (shurikenParticles.main.gravityModifier.mode == ParticleSystemCurveMode.Curve) {
                    int curves = (starting) ? 0 : shurikenParticles.main.gravityModifier.curve.keys.Length - 1;
                    babylonParticles.gravityMultiplier = shurikenParticles.main.gravityModifier.curve.keys[curves].value;
                } else if (shurikenParticles.main.gravityModifier.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = (starting) ? 0 : shurikenParticles.main.gravityModifier.curveMin.keys.Length - 1;
                    babylonParticles.gravityMultiplier = shurikenParticles.main.gravityModifier.curveMin.keys[curves].value;
                } else {
                    babylonParticles.gravityMultiplier = 1.0f;
                }

                // Delay
                if (shurikenParticles.main.startDelay.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.delayTime = shurikenParticles.main.startDelay.constant;
                } else if (shurikenParticles.main.startDelay.mode == ParticleSystemCurveMode.TwoConstants) {
                    babylonParticles.delayTime = (starting) ? shurikenParticles.main.startDelay.constantMin : shurikenParticles.main.startDelay.constantMax;
                } else if (shurikenParticles.main.startDelay.mode == ParticleSystemCurveMode.Curve) {
                    int curves = (starting) ? 0 : shurikenParticles.main.startDelay.curve.keys.Length - 1;
                    babylonParticles.delayTime = shurikenParticles.main.startDelay.curve.keys[curves].value;
                } else if (shurikenParticles.main.startDelay.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = (starting) ? 0 : shurikenParticles.main.startDelay.curveMin.keys.Length - 1;
                    babylonParticles.delayTime = shurikenParticles.main.startDelay.curveMin.keys[curves].value;
                } else {
                    babylonParticles.delayTime = 0.0f;
                }

                // Speed
                if (shurikenParticles.main.startSpeed.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.startSpeed = shurikenParticles.main.startSpeed.constant * this.updateSpeedModifier;
                } else if (shurikenParticles.main.startSpeed.mode == ParticleSystemCurveMode.TwoConstants) {
                    float constant = (starting) ? shurikenParticles.main.startSpeed.constantMin : shurikenParticles.main.startSpeed.constantMax;
                    babylonParticles.startSpeed = constant * this.updateSpeedModifier;
                } else if (shurikenParticles.main.startSpeed.mode == ParticleSystemCurveMode.Curve) {
                    int curves = (starting) ? 0 : shurikenParticles.main.startSpeed.curve.keys.Length - 1;
                    babylonParticles.startSpeed = shurikenParticles.main.startSpeed.curve.keys[curves].value * this.updateSpeedModifier;
                } else if (shurikenParticles.main.startSpeed.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = (starting) ? 0 : shurikenParticles.main.startSpeed.curveMin.keys.Length - 1;
                    babylonParticles.startSpeed = shurikenParticles.main.startSpeed.curveMin.keys[curves].value * this.updateSpeedModifier;
                } else {
                    babylonParticles.startSpeed = 0.01f;
                }

                // Emission
                babylonParticles.emitPower = new Vector2(1.0f, 1.0f);
                if (shurikenParticles.emission.rateOverTime.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.emitRate = shurikenParticles.emission.rateOverTime.constant * this.emitRateModifier;
                } else if (shurikenParticles.emission.rateOverTime.mode == ParticleSystemCurveMode.TwoConstants) {
                    float constant = (starting) ? shurikenParticles.emission.rateOverTime.constantMin : shurikenParticles.emission.rateOverTime.constantMax;
                    babylonParticles.emitRate = constant * this.emitRateModifier;
                } else if (shurikenParticles.emission.rateOverTime.mode == ParticleSystemCurveMode.Curve) {
                    int curves = (starting) ? 0 : shurikenParticles.emission.rateOverTime.curve.keys.Length - 1;
                    babylonParticles.emitRate = shurikenParticles.emission.rateOverTime.curve.keys[curves].value * this.emitRateModifier;
                } else if (shurikenParticles.emission.rateOverTime.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = (starting) ? 0 : shurikenParticles.emission.rateOverTime.curveMin.keys.Length - 1;
                    babylonParticles.emitRate = shurikenParticles.emission.rateOverTime.curveMin.keys[curves].value * this.emitRateModifier;
                } else {
                    babylonParticles.emitRate = 10.0f;
                }

                // Lifetime
                if (shurikenParticles.main.startLifetime.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.lifeTime.x = shurikenParticles.main.startLifetime.constant;
                    babylonParticles.lifeTime.y = shurikenParticles.main.startLifetime.constant;
                } else if (shurikenParticles.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants) {
                    babylonParticles.lifeTime.x = shurikenParticles.main.startLifetime.constantMin;
                    babylonParticles.lifeTime.y = shurikenParticles.main.startLifetime.constantMax;
                } else if (shurikenParticles.main.startLifetime.mode == ParticleSystemCurveMode.Curve) {
                    int curves = shurikenParticles.main.startLifetime.curve.keys.Length - 1;
                    babylonParticles.lifeTime.x = shurikenParticles.main.startLifetime.curve.keys[0].value;
                    babylonParticles.lifeTime.y = shurikenParticles.main.startLifetime.curve.keys[curves].value;
                } else if (shurikenParticles.main.startLifetime.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = shurikenParticles.main.startLifetime.curveMin.keys.Length - 1;
                    babylonParticles.lifeTime.x = shurikenParticles.main.startLifetime.curveMin.keys[0].value;
                    babylonParticles.lifeTime.y = shurikenParticles.main.startLifetime.curveMin.keys[curves].value;
                } else {
                    babylonParticles.lifeTime.x = 1.0f;
                    babylonParticles.lifeTime.y = 1.0f;
                }

                // Sizing
                if (shurikenParticles.main.startSize.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.particleSize.x = shurikenParticles.main.startSize.constant;
                    babylonParticles.particleSize.y = shurikenParticles.main.startSize.constant;
                } else if (shurikenParticles.main.startSize.mode == ParticleSystemCurveMode.TwoConstants) {
                    babylonParticles.particleSize.x = shurikenParticles.main.startSize.constantMin;
                    babylonParticles.particleSize.y = shurikenParticles.main.startSize.constantMax;
                } else if (shurikenParticles.main.startSize.mode == ParticleSystemCurveMode.Curve) {
                    int curves = shurikenParticles.main.startSize.curve.keys.Length - 1;
                    babylonParticles.particleSize.x = shurikenParticles.main.startSize.curve.keys[0].value;
                    babylonParticles.particleSize.y = shurikenParticles.main.startSize.curve.keys[curves].value;
                } else if (shurikenParticles.main.startSize.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = shurikenParticles.main.startSize.curveMin.keys.Length - 1;
                    babylonParticles.particleSize.x = shurikenParticles.main.startSize.curveMin.keys[0].value;
                    babylonParticles.particleSize.y = shurikenParticles.main.startSize.curveMin.keys[curves].value;
                } else {
                    babylonParticles.particleSize.x = 1.0f;
                    babylonParticles.particleSize.y = 1.0f;
                }
                
                // Rotation
                if (shurikenParticles.main.startRotation.mode == ParticleSystemCurveMode.Constant) {
                    babylonParticles.angularSpeed.x = shurikenParticles.main.startRotation.constant;
                    babylonParticles.angularSpeed.y = shurikenParticles.main.startRotation.constant;
                } else if (shurikenParticles.main.startRotation.mode == ParticleSystemCurveMode.TwoConstants) {
                    babylonParticles.angularSpeed.x = shurikenParticles.main.startRotation.constantMin;
                    babylonParticles.angularSpeed.y = shurikenParticles.main.startRotation.constantMax;
                } else if (shurikenParticles.main.startRotation.mode == ParticleSystemCurveMode.Curve) {
                    int curves = shurikenParticles.main.startRotation.curve.keys.Length - 1;
                    babylonParticles.angularSpeed.x = shurikenParticles.main.startRotation.curve.keys[0].value;
                    babylonParticles.angularSpeed.y = shurikenParticles.main.startRotation.curve.keys[curves].value;
                } else if (shurikenParticles.main.startRotation.mode == ParticleSystemCurveMode.TwoCurves) {
                    int curves = shurikenParticles.main.startRotation.curveMin.keys.Length - 1;
                    babylonParticles.angularSpeed.x = shurikenParticles.main.startRotation.curveMin.keys[0].value;
                    babylonParticles.angularSpeed.y = shurikenParticles.main.startRotation.curveMax.keys[curves].value;
                } else {
                    babylonParticles.angularSpeed.x = 0.0f;
                    babylonParticles.angularSpeed.y = 0.0f;
                }

                // Color
                if (shurikenParticles.main.startColor.mode == ParticleSystemGradientMode.Color) {
                    babylonParticles.color1 = shurikenParticles.main.startColor.color;
                    babylonParticles.color2 = shurikenParticles.main.startColor.color;
                } else if (shurikenParticles.main.startColor.mode == ParticleSystemGradientMode.TwoColors) {
                    babylonParticles.color1 = shurikenParticles.main.startColor.colorMin;
                    babylonParticles.color2 = shurikenParticles.main.startColor.colorMax;
                } else if (shurikenParticles.main.startColor.mode == ParticleSystemGradientMode.Gradient || shurikenParticles.main.startColor.mode == ParticleSystemGradientMode.RandomColor) {
                    int gradients = shurikenParticles.main.startColor.gradient.colorKeys.Length -1;
                    babylonParticles.color1 = shurikenParticles.main.startColor.gradient.colorKeys[0].color;
                    babylonParticles.color2 = shurikenParticles.main.startColor.gradient.colorKeys[gradients].color;
                } else if (shurikenParticles.main.startColor.mode == ParticleSystemGradientMode.TwoGradients) {
                    int gradients = shurikenParticles.main.startColor.gradientMin.colorKeys.Length - 1;
                    babylonParticles.color1 = shurikenParticles.main.startColor.gradientMin.colorKeys[0].color;
                    babylonParticles.color2 = shurikenParticles.main.startColor.gradientMin.colorKeys[gradients].color;
                } else {
                    babylonParticles.color1 = defaultColor;
                    babylonParticles.color2 = defaultColor;
                }

                // Shuriken
                if (this.exportShurikenData) {
                    // TODO: Parse Shuriken Particle System Metadata
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                ExporterWindow.ReportProgress(1, "Refresing assets database...");
                AssetDatabase.Refresh();
            }
            System.Threading.Thread.Sleep(500);
            ExporterWindow.ReportProgress(1, "Particle system generation complete.");
            EditorUtility.ClearProgressBar();
            if (this.keepGeneratorOpen) {
                ExporterWindow.ShowMessage("Particle system generation complete.", "Babylon.js");
            } else {
                this.Close();
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}