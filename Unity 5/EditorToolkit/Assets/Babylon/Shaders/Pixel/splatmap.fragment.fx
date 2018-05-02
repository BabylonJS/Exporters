#include<__decl__defaultFragment>

#if defined(BUMP) || !defined(NORMAL)
#extension GL_OES_standard_derivatives : enable
#endif

#ifdef LOGARITHMICDEPTH
#extension GL_EXT_frag_depth : enable
#endif

// Constants
#define RECIPROCAL_PI2 0.15915494

uniform vec3 vEyePosition;
uniform vec3 vAmbientColor;

// Input
varying vec3 vPositionW;

#ifdef NORMAL
varying vec3 vNormalW;
#endif

#ifdef VERTEXCOLOR
varying vec4 vColor;
#endif

#ifdef MAINUV1
	varying vec2 vMainUV1;
#endif

#ifdef MAINUV2
	varying vec2 vMainUV2;
#endif

// Helper functions
#include<helperFunctions>

// Lights
#include<__decl__lightFragment>[0..maxSimultaneousLights]

#include<lightsFragmentFunctions>
#include<shadowsFragmentFunctions>

// Splatmaps
#ifdef splatmapDef
uniform sampler2D splatmap;
varying vec2 splatmapUV;
uniform vec2 splatmapInfos;
uniform float splatmapRects;
uniform vec4 splatmapRect1;
uniform vec4 splatmapRect2;
#endif
#ifdef atlasTexture1Def
uniform sampler2D atlasTexture1;
varying vec2 atlasTexture1UV;
uniform vec2 atlasTexture1Infos;
#endif
#ifdef atlasTexture2Def
uniform sampler2D atlasTexture2;
varying vec2 atlasTexture2UV;
uniform vec2 atlasTexture2Infos;
#endif
#ifdef atlasTexture3Def
uniform sampler2D atlasTexture3;
varying vec2 atlasTexture3UV;
uniform vec2 atlasTexture3Infos;
#endif
#ifdef atlasTexture4Def
uniform sampler2D atlasTexture4;
varying vec2 atlasTexture4UV;
uniform vec2 atlasTexture4Infos;
#endif
#ifdef atlasTexture5Def
uniform sampler2D atlasTexture5;
varying vec2 atlasTexture5UV;
uniform vec2 atlasTexture5Infos;
#endif
#ifdef bumpTexture1Def
uniform sampler2D bumpTexture1;
varying vec2 bumpTexture1UV;
uniform vec2 bumpTexture1Infos;
#endif
#ifdef bumpTexture2Def
uniform sampler2D bumpTexture2;
varying vec2 bumpTexture2UV;
uniform vec2 bumpTexture2Infos;
#endif
#ifdef bumpTexture3Def
uniform sampler2D bumpTexture3;
varying vec2 bumpTexture3UV;
uniform vec2 bumpTexture3Infos;
#endif
#ifdef bumpTexture4Def
uniform sampler2D bumpTexture4;
varying vec2 bumpTexture4UV;
uniform vec2 bumpTexture4Infos;
#endif
#ifdef bumpTexture5Def
uniform sampler2D bumpTexture5;
varying vec2 bumpTexture5UV;
uniform vec2 bumpTexture5Infos;
#endif

// Samplers
#ifdef DIFFUSE
	#if DIFFUSEDIRECTUV == 1
		#define vDiffuseUV vMainUV1
	#elif DIFFUSEDIRECTUV == 2
		#define vDiffuseUV vMainUV2
	#else
		varying vec2 vDiffuseUV;
	#endif
	uniform sampler2D diffuseSampler;
#endif

#ifdef AMBIENT
	#if AMBIENTDIRECTUV == 1
		#define vAmbientUV vMainUV1
	#elif AMBIENTDIRECTUV == 2
		#define vAmbientUV vMainUV2
	#else
		varying vec2 vAmbientUV;
	#endif
	uniform sampler2D ambientSampler;
#endif

#ifdef OPACITY	
	#if OPACITYDIRECTUV == 1
		#define vOpacityUV vMainUV1
	#elif OPACITYDIRECTUV == 2
		#define vOpacityUV vMainUV2
	#else
		varying vec2 vOpacityUV;
	#endif
	uniform sampler2D opacitySampler;
#endif

#ifdef EMISSIVE
	#if EMISSIVEDIRECTUV == 1
		#define vEmissiveUV vMainUV1
	#elif EMISSIVEDIRECTUV == 2
		#define vEmissiveUV vMainUV2
	#else
		varying vec2 vEmissiveUV;
	#endif
	uniform sampler2D emissiveSampler;
#endif

#ifdef LIGHTMAP
	#if LIGHTMAPDIRECTUV == 1
		#define vLightmapUV vMainUV1
	#elif LIGHTMAPDIRECTUV == 2
		#define vLightmapUV vMainUV2
	#else
		varying vec2 vLightmapUV;
	#endif
	uniform sampler2D lightmapSampler;
#endif

#ifdef REFRACTION

#ifdef REFRACTIONMAP_3D
uniform samplerCube refractionCubeSampler;
#else
uniform sampler2D refraction2DSampler;
#endif

#endif

#if defined(SPECULAR) && defined(SPECULARTERM)
	#if SPECULARDIRECTUV == 1
		#define vSpecularUV vMainUV1
	#elif SPECULARDIRECTUV == 2
		#define vSpecularUV vMainUV2
	#else
		varying vec2 vSpecularUV;
	#endif
	uniform sampler2D specularSampler;
#endif

// Fresnel
#include<fresnelFunction>

// Reflection
#ifdef REFLECTION
#ifdef REFLECTIONMAP_3D
uniform samplerCube reflectionCubeSampler;
#else
uniform sampler2D reflection2DSampler;
#endif

#ifdef REFLECTIONMAP_SKYBOX
varying vec3 vPositionUVW;
#else
#if defined(REFLECTIONMAP_EQUIRECTANGULAR_FIXED) || defined(REFLECTIONMAP_MIRROREDEQUIRECTANGULAR_FIXED)
varying vec3 vDirectionW;
#endif

#endif

#include<reflectionFunction>

#endif

#include<imageProcessingDeclaration>

#include<imageProcessingFunctions>

#include<bumpFragmentFunctions>
#include<clipPlaneFragmentDeclaration>
#include<logDepthDeclaration>
#include<fogFragmentDeclaration>

//////////////////////////////////////////////////////////////////////////////////////
// Texture Atlas Support
//////////////////////////////////////////////////////////////////////////////////////
vec4 textureFract2D(sampler2D atlas, vec4 rect, vec2 size, vec2 scale, vec2 uv, vec2 offset) {
	vec2 fractUV = fract(uv * scale);
	vec2 atlasUV = vec2((fractUV.x * rect.w) + rect.x, (fractUV.y * rect.z) + rect.y);
	vec2 tiledUV = atlasUV + offset;
	return texture2D(atlas, tiledUV);
}
vec4 textureAtlas2D(sampler2D atlas, vec4 rect, vec2 uv, vec2 offset) {
	vec2 atlasUV = vec2((uv.x * rect.w) + rect.x, (uv.y * rect.z) + rect.y);
	vec2 tiledUV = atlasUV + offset;
	return texture2D(atlas, tiledUV);
}
//////////////////////////////////////////////////////////////////////////////////////

void main(void) {
#include<clipPlaneFragment>
	vec3 viewDirectionW = normalize(vEyePosition - vPositionW);

	// Base color
	vec4 baseColor = vec4(1.0, 1.0, 1.0, 1.0);
	vec3 diffuseColor = vDiffuseColor.rgb;

	// Alpha
	float alpha = vDiffuseColor.a;

	// Bump
	#ifdef NORMAL
		vec3 normalW = normalize(vNormalW);
	#else
		vec3 normalW = normalize(-cross(dFdx(vPositionW), dFdy(vPositionW)));
	#endif

#ifdef splatmapDef
	vec2 uvOffset = vec2(0.0, 0.0);
	#if defined(BUMP) || defined(PARALLAX)
		#ifdef NORMALXYSCALE
			float normalScale = 1.0;
		#else		
			float normalScale = vBumpInfos.y;
		#endif

		#if defined(TANGENT) && defined(NORMAL)
			mat3 TBN = vTBN;
		#else
			mat3 TBN = cotangent_frame(normalW * normalScale, vPositionW, vBumpUV);
		#endif
	#endif

	#ifdef PARALLAX
		mat3 invTBN = transposeMat3(TBN);

		#ifdef PARALLAXOCCLUSION
			uvOffset = parallaxOcclusion(invTBN * -viewDirectionW, invTBN * normalW, vBumpUV, vBumpInfos.z);
		#else
			uvOffset = parallaxOffset(invTBN * viewDirectionW, vBumpInfos.z);
		#endif
	#endif
#else
	#include<bumpFragment>
	#ifdef TWOSIDEDLIGHTING
		normalW = gl_FrontFacing ? normalW : -normalW;
	#endif
#endif

#ifdef DIFFUSE
	// Splatmaps
	#if defined(SPLATMAPS_ENABLED) && defined(splatmapDef)
		vec4 baseColor1 = vec4(0.0, 0.0, 0.0, 0.0);
		vec4 baseColor2 = vec4(0.0, 0.0, 0.0, 0.0);
		// Base splat colors
		if (splatmapRects > 0.0) {
			baseColor1 = textureAtlas2D(splatmap, splatmapRect1, splatmapUV, uvOffset);
		}
		if (splatmapRects > 1.0) {
			baseColor2 = textureAtlas2D(splatmap, splatmapRect2, splatmapUV, uvOffset);
		}
		// Primary splat colors
		baseColor = texture2D(diffuseSampler, vDiffuseUV + uvOffset) * baseColor1.r;
		#ifdef atlasTexture1Def
			baseColor += texture2D(atlasTexture1, atlasTexture1UV + uvOffset) * baseColor1.g;
		#endif
		#ifdef atlasTexture2Def
			baseColor += texture2D(atlasTexture2, atlasTexture2UV + uvOffset) * baseColor1.b;
		#endif
		// Second splat colors
		#ifdef atlasTexture3Def
			baseColor += texture2D(atlasTexture3, atlasTexture3UV + uvOffset) * baseColor2.r;
		#endif
		#ifdef atlasTexture4Def
			baseColor += texture2D(atlasTexture4, atlasTexture4UV + uvOffset) * baseColor2.g;
		#endif
		#ifdef atlasTexture5Def
			baseColor += texture2D(atlasTexture5, atlasTexture5UV + uvOffset) * baseColor2.b;
		#endif
	#else
		baseColor = texture2D(diffuseSampler, vDiffuseUV + uvOffset);
	#endif

	#ifdef ALPHATEST
		if (baseColor.a < 0.4)
			discard;
	#endif

	#ifdef ALPHAFROMDIFFUSE
		alpha *= baseColor.a;
	#endif

	baseColor.rgb *= vDiffuseInfos.y;

#ifdef splatmapDef
	#ifdef BUMP
		//normalW = perturbNormal(TBN, vBumpUV + uvOffset);
	#endif

	#ifdef TWOSIDEDLIGHTING
		normalW = gl_FrontFacing ? normalW : -normalW;
	#endif
#endif
	
#endif

#include<depthPrePass>

#ifdef VERTEXCOLOR
	baseColor.rgb *= vColor.rgb;
#endif

	// Ambient color
	vec3 baseAmbientColor = vec3(1., 1., 1.);

#ifdef AMBIENT
	baseAmbientColor = texture2D(ambientSampler, vAmbientUV + uvOffset).rgb * vAmbientInfos.y;
#endif

	// Specular map
#ifdef SPECULARTERM
	float glossiness = vSpecularColor.a;
	vec3 specularColor = vSpecularColor.rgb;

#ifdef SPECULAR
	vec4 specularMapColor = texture2D(specularSampler, vSpecularUV + uvOffset);
	specularColor = specularMapColor.rgb;
#ifdef GLOSSINESS
	glossiness = glossiness * specularMapColor.a;
#endif
#endif
#else
	float glossiness = 0.;
#endif

	// Lighting
	vec3 diffuseBase = vec3(0., 0., 0.);
	lightingInfo info;
#ifdef SPECULARTERM
	vec3 specularBase = vec3(0., 0., 0.);
#endif
	float shadow = 1.;

#ifdef LIGHTMAP
	vec3 lightmapColor = texture2D(lightmapSampler, vLightmapUV + uvOffset).rgb * vLightmapInfos.y;
#endif

#include<lightFragment>[0..maxSimultaneousLights]

	// Refraction
	vec3 refractionColor = vec3(0., 0., 0.);

#ifdef REFRACTION
	vec3 refractionVector = normalize(refract(-viewDirectionW, normalW, vRefractionInfos.y));
#ifdef REFRACTIONMAP_3D

	refractionVector.y = refractionVector.y * vRefractionInfos.w;

	if (dot(refractionVector, viewDirectionW) < 1.0)
	{
		refractionColor = textureCube(refractionCubeSampler, refractionVector).rgb * vRefractionInfos.x;
	}
#else
	vec3 vRefractionUVW = vec3(refractionMatrix * (view * vec4(vPositionW + refractionVector * vRefractionInfos.z, 1.0)));

	vec2 refractionCoords = vRefractionUVW.xy / vRefractionUVW.z;

	refractionCoords.y = 1.0 - refractionCoords.y;

	refractionColor = texture2D(refraction2DSampler, refractionCoords).rgb * vRefractionInfos.x;
#endif
#endif

	// Reflection
	vec3 reflectionColor = vec3(0., 0., 0.);

#ifdef REFLECTION
	vec3 vReflectionUVW = computeReflectionCoords(vec4(vPositionW, 1.0), normalW);

#ifdef REFLECTIONMAP_3D
#ifdef ROUGHNESS
	float bias = vReflectionInfos.y;

#ifdef SPECULARTERM
	#ifdef SPECULAR
		#ifdef GLOSSINESS
			bias *= (1.0 - specularMapColor.a);
		#endif
	#endif
#endif

	reflectionColor = textureCube(reflectionCubeSampler, vReflectionUVW, bias).rgb * vReflectionInfos.x;
#else
	reflectionColor = textureCube(reflectionCubeSampler, vReflectionUVW).rgb * vReflectionInfos.x;
#endif

#else
	vec2 coords = vReflectionUVW.xy;

#ifdef REFLECTIONMAP_PROJECTION
	coords /= vReflectionUVW.z;
#endif

	coords.y = 1.0 - coords.y;

	reflectionColor = texture2D(reflection2DSampler, coords).rgb * vReflectionInfos.x;
#endif

#ifdef REFLECTIONFRESNEL
	float reflectionFresnelTerm = computeFresnelTerm(viewDirectionW, normalW, reflectionRightColor.a, reflectionLeftColor.a);

#ifdef REFLECTIONFRESNELFROMSPECULAR
#ifdef SPECULARTERM
	reflectionColor *= specularColor.rgb * (1.0 - reflectionFresnelTerm) + reflectionFresnelTerm * reflectionRightColor.rgb;
#else
	reflectionColor *= reflectionLeftColor.rgb * (1.0 - reflectionFresnelTerm) + reflectionFresnelTerm * reflectionRightColor.rgb;
#endif
#else
	reflectionColor *= reflectionLeftColor.rgb * (1.0 - reflectionFresnelTerm) + reflectionFresnelTerm * reflectionRightColor.rgb;
#endif
#endif
#endif

#ifdef REFRACTIONFRESNEL
	float refractionFresnelTerm = computeFresnelTerm(viewDirectionW, normalW, refractionRightColor.a, refractionLeftColor.a);

	refractionColor *= refractionLeftColor.rgb * (1.0 - refractionFresnelTerm) + refractionFresnelTerm * refractionRightColor.rgb;
#endif

#ifdef OPACITY
	vec4 opacityMap = texture2D(opacitySampler, vOpacityUV + uvOffset);

#ifdef OPACITYRGB
	opacityMap.rgb = opacityMap.rgb * vec3(0.3, 0.59, 0.11);
	alpha *= (opacityMap.x + opacityMap.y + opacityMap.z)* vOpacityInfos.y;
#else
	alpha *= opacityMap.a * vOpacityInfos.y;
#endif

#endif

#ifdef VERTEXALPHA
	alpha *= vColor.a;
#endif

#ifdef OPACITYFRESNEL
	float opacityFresnelTerm = computeFresnelTerm(viewDirectionW, normalW, opacityParts.z, opacityParts.w);

	alpha += opacityParts.x * (1.0 - opacityFresnelTerm) + opacityFresnelTerm * opacityParts.y;
#endif

	// Emissive
	vec3 emissiveColor = vEmissiveColor;
#ifdef EMISSIVE
	emissiveColor += texture2D(emissiveSampler, vEmissiveUV + uvOffset).rgb * vEmissiveInfos.y;
#endif

#ifdef EMISSIVEFRESNEL
	float emissiveFresnelTerm = computeFresnelTerm(viewDirectionW, normalW, emissiveRightColor.a, emissiveLeftColor.a);

	emissiveColor *= emissiveLeftColor.rgb * (1.0 - emissiveFresnelTerm) + emissiveFresnelTerm * emissiveRightColor.rgb;
#endif

	// Fresnel
#ifdef DIFFUSEFRESNEL
	float diffuseFresnelTerm = computeFresnelTerm(viewDirectionW, normalW, diffuseRightColor.a, diffuseLeftColor.a);

	diffuseBase *= diffuseLeftColor.rgb * (1.0 - diffuseFresnelTerm) + diffuseFresnelTerm * diffuseRightColor.rgb;
#endif

	// Composition
#ifdef EMISSIVEASILLUMINATION
	vec3 finalDiffuse = clamp(diffuseBase * diffuseColor + vAmbientColor, 0.0, 1.0) * baseColor.rgb;
#else
#ifdef LINKEMISSIVEWITHDIFFUSE
	vec3 finalDiffuse = clamp((diffuseBase + emissiveColor) * diffuseColor + vAmbientColor, 0.0, 1.0) * baseColor.rgb;
#else
	vec3 finalDiffuse = clamp(diffuseBase * diffuseColor + emissiveColor + vAmbientColor, 0.0, 1.0) * baseColor.rgb;
#endif
#endif

#ifdef SPECULARTERM
	vec3 finalSpecular = specularBase * specularColor;
	#ifdef SPECULAROVERALPHA
		alpha = clamp(alpha + dot(finalSpecular, vec3(0.3, 0.59, 0.11)), 0., 1.);
	#endif
#else
	vec3 finalSpecular = vec3(0.0);
#endif

#ifdef REFLECTIONOVERALPHA
	alpha = clamp(alpha + dot(reflectionColor, vec3(0.3, 0.59, 0.11)), 0., 1.);
#endif

	// Composition
#ifdef EMISSIVEASILLUMINATION
	vec4 color = vec4(clamp(finalDiffuse * baseAmbientColor + finalSpecular + reflectionColor + emissiveColor + refractionColor, 0.0, 1.0), alpha);
#else
	vec4 color = vec4(finalDiffuse * baseAmbientColor + finalSpecular + reflectionColor + refractionColor, alpha);
#endif

//Old lightmap calculation method
#ifdef LIGHTMAP
    #ifndef LIGHTMAPEXCLUDED
        #ifdef USELIGHTMAPASSHADOWMAP
            color.rgb *= lightmapColor;
        #else
            color.rgb += lightmapColor;
        #endif
    #endif
#endif

#include<logDepthFragment>
#include<fogFragment>

// Apply image processing if relevant. As this applies in linear space, 
// We first move from gamma to linear.
#ifdef IMAGEPROCESSINGPOSTPROCESS
	color.rgb = toLinearSpace(color.rgb);
#else
	#ifdef IMAGEPROCESSING
		color.rgb = toLinearSpace(color.rgb);
		color = applyImageProcessing(color);
	#endif
#endif

	gl_FragColor = color;
}