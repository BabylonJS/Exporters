#include<__decl__defaultVertex>
// Attributes
attribute vec3 position;
#ifdef NORMAL
attribute vec3 normal;
#endif
#ifdef TANGENT
attribute vec4 tangent;
#endif
#ifdef UV1
attribute vec2 uv;
#endif
#ifdef UV2
attribute vec2 uv2;
#endif
#ifdef VERTEXCOLOR
attribute vec4 color;
#endif

#include<bonesDeclaration>

// Splatmaps
#ifdef splatmapDef
varying vec2 splatmapUV;
uniform vec2 splatmapInfos;
uniform mat4 splatmapMatrix;
#endif
#ifdef atlasTexture1Def
varying vec2 atlasTexture1UV;
uniform vec2 atlasTexture1Infos;
uniform mat4 atlasTexture1Matrix;
#endif
#ifdef atlasTexture2Def
varying vec2 atlasTexture2UV;
uniform vec2 atlasTexture2Infos;
uniform mat4 atlasTexture2Matrix;
#endif
#ifdef atlasTexture3Def
varying vec2 atlasTexture3UV;
uniform vec2 atlasTexture3Infos;
uniform mat4 atlasTexture3Matrix;
#endif
#ifdef atlasTexture4Def
varying vec2 atlasTexture4UV;
uniform vec2 atlasTexture4Infos;
uniform mat4 atlasTexture4Matrix;
#endif
#ifdef atlasTexture5Def
varying vec2 atlasTexture5UV;
uniform vec2 atlasTexture5Infos;
uniform mat4 atlasTexture5Matrix;
#endif
#ifdef bumpTexture1Def
varying vec2 bumpTexture1UV;
uniform vec2 bumpTexture1Infos;
uniform mat4 bumpTexture1Matrix;
#endif
#ifdef bumpTexture2Def
varying vec2 bumpTexture2UV;
uniform vec2 bumpTexture2Infos;
uniform mat4 bumpTexture2Matrix;
#endif
#ifdef bumpTexture3Def
varying vec2 bumpTexture3UV;
uniform vec2 bumpTexture3Infos;
uniform mat4 bumpTexture3Matrix;
#endif
#ifdef bumpTexture4Def
varying vec2 bumpTexture4UV;
uniform vec2 bumpTexture4Infos;
uniform mat4 bumpTexture4Matrix;
#endif
#ifdef bumpTexture5Def
varying vec2 bumpTexture5UV;
uniform vec2 bumpTexture5Infos;
uniform mat4 bumpTexture5Matrix;
#endif

// Uniforms
#include<instancesDeclaration>

#ifdef MAINUV1
	varying vec2 vMainUV1;
#endif

#ifdef MAINUV2
	varying vec2 vMainUV2;
#endif

#if defined(DIFFUSE) && DIFFUSEDIRECTUV == 0
varying vec2 vDiffuseUV;
#endif

#if defined(AMBIENT) && AMBIENTDIRECTUV == 0
varying vec2 vAmbientUV;
#endif

#if defined(OPACITY) && OPACITYDIRECTUV == 0
varying vec2 vOpacityUV;
#endif

#if defined(EMISSIVE) && EMISSIVEDIRECTUV == 0
varying vec2 vEmissiveUV;
#endif

#if defined(LIGHTMAP) && LIGHTMAPDIRECTUV == 0
varying vec2 vLightmapUV;
#endif

#if defined(SPECULAR) && defined(SPECULARTERM) && SPECULARDIRECTUV == 0
varying vec2 vSpecularUV;
#endif

#if defined(BUMP) && BUMPDIRECTUV == 0
varying vec2 vBumpUV;
#endif

// Output
varying vec3 vPositionW;
#ifdef NORMAL
varying vec3 vNormalW;
#endif

#ifdef VERTEXCOLOR
varying vec4 vColor;
#endif

#include<bumpVertexDeclaration>

#include<clipPlaneVertexDeclaration>

#include<fogVertexDeclaration>
#include<__decl__lightFragment>[0..maxSimultaneousLights]

#include<morphTargetsVertexGlobalDeclaration>
#include<morphTargetsVertexDeclaration>[0..maxSimultaneousMorphTargets]

#ifdef REFLECTIONMAP_SKYBOX
varying vec3 vPositionUVW;
#endif

#if defined(REFLECTIONMAP_EQUIRECTANGULAR_FIXED) || defined(REFLECTIONMAP_MIRROREDEQUIRECTANGULAR_FIXED)
varying vec3 vDirectionW;
#endif

#include<logDepthDeclaration>

void main(void) {
	vec3 positionUpdated = position;
#ifdef NORMAL	
	vec3 normalUpdated = normal;
#endif
#ifdef TANGENT
	vec4 tangentUpdated = tangent;
#endif

#include<morphTargetsVertex>[0..maxSimultaneousMorphTargets]

#ifdef REFLECTIONMAP_SKYBOX
	vPositionUVW = positionUpdated;
#endif 

#include<instancesVertex>
#include<bonesVertex>

	gl_Position = viewProjection * finalWorld * vec4(positionUpdated, 1.0);

	vec4 worldPos = finalWorld * vec4(positionUpdated, 1.0);
	vPositionW = vec3(worldPos);

#ifdef NORMAL
	vNormalW = normalize(vec3(finalWorld * vec4(normalUpdated, 0.0)));
#endif

#if defined(REFLECTIONMAP_EQUIRECTANGULAR_FIXED) || defined(REFLECTIONMAP_MIRROREDEQUIRECTANGULAR_FIXED)
	vDirectionW = normalize(vec3(finalWorld * vec4(positionUpdated, 0.0)));
#endif

	// UV coordinates
#ifndef UV1
	vec2 uv = vec2(0., 0.);
#endif
#ifndef UV2
	vec2 uv2 = vec2(0., 0.);
#endif

	// Main coordinates
#ifdef MAINUV1
	vMainUV1 = uv;
#endif

#ifdef MAINUV2
	vMainUV2 = uv2;
#endif

	// Splatmap coordinates
#ifdef splatmapDef
	if (splatmapInfos.x == 0.)
	{
		splatmapUV = vec2(splatmapMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		splatmapUV = vec2(splatmapMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef atlasTexture1Def
	if (atlasTexture1Infos.x == 0.)
	{
		atlasTexture1UV = vec2(atlasTexture1Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		atlasTexture1UV = vec2(atlasTexture1Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef atlasTexture2Def
	if (atlasTexture2Infos.x == 0.)
	{
		atlasTexture2UV = vec2(atlasTexture2Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		atlasTexture2UV = vec2(atlasTexture2Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef atlasTexture3Def
	if (atlasTexture3Infos.x == 0.)
	{
		atlasTexture3UV = vec2(atlasTexture3Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		atlasTexture3UV = vec2(atlasTexture3Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef atlasTexture4Def
	if (atlasTexture4Infos.x == 0.)
	{
		atlasTexture4UV = vec2(atlasTexture4Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		atlasTexture4UV = vec2(atlasTexture4Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef atlasTexture5Def
	if (atlasTexture5Infos.x == 0.)
	{
		atlasTexture5UV = vec2(atlasTexture5Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		atlasTexture5UV = vec2(atlasTexture5Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

	// Texture coordinates
#if defined(DIFFUSE) && DIFFUSEDIRECTUV == 0
	if (vDiffuseInfos.x == 0.)
	{
		vDiffuseUV = vec2(diffuseMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vDiffuseUV = vec2(diffuseMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#if defined(AMBIENT) && AMBIENTDIRECTUV == 0
	if (vAmbientInfos.x == 0.)
	{
		vAmbientUV = vec2(ambientMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vAmbientUV = vec2(ambientMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#if defined(OPACITY) && OPACITYDIRECTUV == 0
	if (vOpacityInfos.x == 0.)
	{
		vOpacityUV = vec2(opacityMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vOpacityUV = vec2(opacityMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#if defined(EMISSIVE) && EMISSIVEDIRECTUV == 0
	if (vEmissiveInfos.x == 0.)
	{
		vEmissiveUV = vec2(emissiveMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vEmissiveUV = vec2(emissiveMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#if defined(LIGHTMAP) && LIGHTMAPDIRECTUV == 0
	if (vLightmapInfos.x == 0.)
	{
		vLightmapUV = vec2(lightmapMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vLightmapUV = vec2(lightmapMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#if defined(SPECULAR) && defined(SPECULARTERM) && SPECULARDIRECTUV == 0
	if (vSpecularInfos.x == 0.)
	{
		vSpecularUV = vec2(specularMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vSpecularUV = vec2(specularMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#if defined(BUMP) && BUMPDIRECTUV == 0
	if (vBumpInfos.x == 0.)
	{
		vBumpUV = vec2(bumpMatrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		vBumpUV = vec2(bumpMatrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef bumpTexture1Def
	if (bumpTexture1Infos.x == 0.)
	{
		bumpTexture1UV = vec2(bumpTexture1Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		bumpTexture1UV = vec2(bumpTexture1Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef bumpTexture2Def
	if (bumpTexture2Infos.x == 0.)
	{
		bumpTexture2UV = vec2(bumpTexture2Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		bumpTexture2UV = vec2(bumpTexture2Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef bumpTexture3Def
	if (bumpTexture3Infos.x == 0.)
	{
		bumpTexture3UV = vec2(bumpTexture3Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		bumpTexture3UV = vec2(bumpTexture3Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef bumpTexture4Def
	if (bumpTexture4Infos.x == 0.)
	{
		bumpTexture4UV = vec2(bumpTexture4Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		bumpTexture4UV = vec2(bumpTexture4Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#ifdef bumpTexture5Def
	if (bumpTexture5Infos.x == 0.)
	{
		bumpTexture5UV = vec2(bumpTexture5Matrix * vec4(uv, 1.0, 0.0));
	}
	else
	{
		bumpTexture5UV = vec2(bumpTexture5Matrix * vec4(uv2, 1.0, 0.0));
	}
#endif

#include<bumpVertex>
#include<clipPlaneVertex>
#include<fogVertex>
#include<shadowsVertex>[0..maxSimultaneousLights]

#ifdef VERTEXCOLOR
	// Vertex color
	vColor = color;
#endif

#include<pointCloudVertex>
#include<logDepthVertex>

}