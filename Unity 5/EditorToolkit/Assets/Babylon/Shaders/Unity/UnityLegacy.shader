// Unity built-in shader source. Babylon Edition. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Babylon/Standard Material"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
		_OpacityMap("Opacity Map", 2D) = "black" {}
        
        _AmbientColor ("Ambient Color", Color) = (0,0,0,1)
        _SpecColor ("Specular Color", Color) = (0.12,0.12,0.12,1)
		_Shininess("Specular Power", Range(0.0, 1.0)) = 0.5
        _Emission ("Emissive Color", Color) = (0,0,0,1)
		_EmissionMap("Emissive Texture", 2D) = "white" {}

		[ToggleOff] _Wireframe("Show Wireframe", Int) = 0
		[ToggleOff] _BackFaceCulling("Back Face Culling", Int) = 1
		[ToggleOff] _TwoSidedLighting("Two Sided Lighting", Int) = 0
		[ToggleOff] _DisableLighting("Disable Surface Lighting", Int) = 0
		_MaxSimultaneousLights("Max Simultaneous Lights", Int) = 4
		[ToggleOff] _UseEmissiveAsIllumination("Use Emissive Illumination", Int) = 0
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
        }
        ENDCG  
    }

    FallBack "Legacy Shaders/Diffuse"
}
