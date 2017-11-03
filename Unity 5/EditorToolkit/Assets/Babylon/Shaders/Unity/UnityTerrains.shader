Shader "BabylonJS/Nature/Terrain Splatmap"
{
    Properties
    {
		_TextureLevel("Texture Level", Range(0.0, 5.0)) = 1.0
        _AmbientColor ("Ambient Color", Color) = (0,0,0,1)
        _SpecColor ("Specular Color", Color) = (0.12,0.12,0.12,1)
		_Shininess("Specular Power", Range(0.0, 1.0)) = 0.5
        _Emission ("Emissive Color", Color) = (0,0,0,1)
		_EmissionMap("Emissive Texture", 2D) = "white" {}
		[ToggleOff] _SkyboxReflections("Skybox Reflections", Int) = 0
		_ReflectionScale("Reflection Scale", Range(0.0, 10.0)) = 0.25
		_LightmapScale("Lightmap Scale", Range(0.0, 10.0)) = 1.0

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.0
		[ToggleOff] _Wireframe("Show Wireframe", Int) = 0
		[ToggleOff] _BackFaceCulling("Back Face Culling", Int) = 1
		[ToggleOff] _TwoSidedLighting("Two Sided Lighting", Int) = 0
		[Enum(Disable,0,Additive,1,Combine,2,Subtract,3,Multiply,4,Maximized,5,OneOne,6)] _AlphaMode ("Alpha Blending Mode", int) = 2
		[ToggleOff] _DisableLighting("Disable Surface Lighting", Int) = 0
		_MaxSimultaneousLights("Max Simultaneous Lights", Int) = 4
		[ToggleOff] _UseEmissiveAsIllumination("Use Emissive As Illumination", Int) = 0
        
        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
    }

    CGINCLUDE
        #pragma surface surf Lambert vertex:SplatmapVert finalcolor:SplatmapFinalColor finalprepass:SplatmapFinalPrepass finalgbuffer:SplatmapFinalGBuffer noinstancing
        #pragma multi_compile_fog
        #include "TerrainSplatmapCommon.cginc"

        void surf(Input IN, inout SurfaceOutput o)
        {
            half4 splat_control;
            half weight;
            fixed4 mixedDiffuse;
            SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb;
            o.Alpha = weight;
        }
    ENDCG

    Category {
        Tags {
            "Queue" = "Geometry-99"
            "RenderType" = "Opaque"
        }
        // TODO: Seems like "#pragma target 3.0 _TERRAIN_NORMAL_MAP" can't fallback correctly on less capable devices?
        // Use two sub-shaders to simulate different features for different targets and still fallback correctly.
        SubShader { // for sm3.0+ targets
            CGPROGRAM
                #pragma target 3.0
                #pragma multi_compile __ _TERRAIN_NORMAL_MAP
            ENDCG
        }
        SubShader { // for sm2.0 targets
            CGPROGRAM
            ENDCG
        }
    }

    Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Diffuse-AddPass"
    Dependency "BaseMapShader" = "Diffuse"
    Dependency "Details0"      = "Hidden/TerrainEngine/Details/Vertexlit"
    Dependency "Details1"      = "Hidden/TerrainEngine/Details/WavingDoublePass"
    Dependency "Details2"      = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
    Dependency "Tree0"         = "Hidden/TerrainEngine/BillboardTree"

    Fallback "Diffuse"
}
