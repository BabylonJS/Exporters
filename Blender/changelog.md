# Blender2Babylon add-on changelog

## Blender Exporter Version 6.0.0
*01 January 2019*

* Supports Blender 2.80 redesign (not completely yet)
* Removed Internal render materials support
* Relocated Game Engine render properties used
* Moved all exporter level custom properties from scene tab to world tab
* Changes to world tab:
	* Properties from scene tab
	* Added Sky Box / Environment Textures section (before knowing BJS does not use .hdr textures.  May need to hide.)
	* Added 'Use PBR' checkbox
* Changes to mesh tab:
	* Relocated Billboard Mode from Game Engine to here
	* Relocated most of material section to new panel in Materials tab
	* Remaining materials stuff now in 'Baking Settings' section.  Added 'Force Baking' checkbox
* Added new custom properties panel for Materials:
	* Relocated Back Face Culling checkbox from Game Engine to here
	* Relocated Check Ready Only Once checkbox from Mesh tab to here
	* Relocated Max Simultaneous Lights from Mesh tab to here
	* Relocated Name Space from Mesh tab to here (might be in TOB only, since JSON files cannot share materials)
* Mesh baking can be reduced to only the texture channels required, keeping other image texture based channels (not for multi-material meshes)
* Nodes based renders (Cycles & eevee) not always just baked.  See chart for properties / textures & where values are from.  Properties are assigned when no texture input to socket (except refraction).

|  STD Property / Tex | PBR Property / Tex | From Nodes-Socket
| --- | --- | --- |
| diffuseColor / diffuseTexture | albedoColor / albedoTexture |Diffuse BSDF - Color, Principled BSDF - Base Color |
| ambientColor / ambientTexture | ambientColor / useAmbientOcclusionFromMetallicTextureRed  | Ambient Occlusion - Color |
| emissiveColor / emissiveTexture | emissiveColor / emissiveTexture | Emission - Color |
| specularColor / specularTexture | reflectivityColor / reflectivityTexture | Glossy BSDF - Color, Principled BSDF - Specular |
| specularPower (inverted & 0 - 128) | roughness / useRoughnessFromMetallicTextureGreen | Glossy BSDF - Roughness, Principled BSDF - Roughness |
| -- | metallic / metallicTexture | Principled - Specular |
| -- | indexOfRefraction | Refraction BSDF - IOR, Frensel - IOR, Principled BSDF - IOR|
| -- | refractionTexture | Refraction BSDF - Color, Principled BSDF - IOR|
| alpha / opacityTexture | alpha / opacityTexture |Diffuse BSDF - Color, Transparency BSDF - Color, Principled BSDF - Base Color |
| bumpTexture | bumpTexture | Normal Map - Color, Principled BSDF - Normal |

* Certain Nodes are allowed, and are either ignored or just passed thru
	* Mix Shader, used mostly for non-principled trees
	* Separate RGB, for metallic textures wt roughness / AO
	* Frensel, when not PBR
* Texture / UV parameters are optional Nodes, when input to a texture node (ignored when must be baked, baking uses them though)
	* Mapping node for (translation to offset), (rotation to ang), (scale to scale)
	* Texture Coordinate & UVMap nodes for coordinatesIndex
* When a material channel cannot really be represented by mapping, then it will be baked.  Examples:
	* A Noise or other procedureal texture to Principled BSDF - Normal, then a bump texture will be baked
	* Any node which is not explicitly supported or ignored

## Blender Exporter Version 5.6.4

*17 July 2018*

* Fix typo for exporting only visible layers

* Copy tags also to instances

## Blender Exporter Version 5.6.3

*01 June 2018*

* Fix exporter settings panel (in Properties > Scene tab)

* Remove active layers export, replace with 3 options (All, Selected objects, and visible Layers)

* Show message in exporter settings panel to redirect user to Scene tab

* Fix bl_idname to allow setting of a shortcut

## Blender Exporter Version 5.6.2

*23 March 2018*

The custom property, textureDir, originally for [Tower of Babel](https://github.com/BabylonJS/Extensions/tree/master/QueuedInterpolation/Blender) to
indicate the directory to write images, is now joined into the name
field of the texture in the *.babylon* file.

If the field does not end with `/`, then one will be added between the
directory & file name.

The field is relative to the *.babylon* file.  For this to work probably
implies that the *.babylon* is in the same directory as the html file.
Still it now allows images to be in a separate directory.

Have not tested where the the *.babylon* is in a different directory from
html file.

## Blender Exporter Version 5.6.1

*07 March 2018*

- zip file now has manifest option code

## Blender Exporter Version 5.6

*02 March 2018*

Essentially, this is an obsolete feature dump.  This in preparation for Blender 2.80 which could cause many changes.  Things removed:
-  Remove the checking for, and splitting up of meshes with more than 64k vertices.
-  Flat shading both for the mesh and the entire scene.  This can be replaced with a Split Edges modifier.
-  Generate ES6 instead of ES3 (Tower of Babel variant).

There were a few additions:
-  Separate scene level controls for the # of decimals of (Default):
    - positions / shape keys (4)
    - normals (3)
    - UVS (3)
    - vertex colors (3)
    - matrix weights (2)
-  Pickable property for meshes
-  Redesign of checking for un-applied transforms on meshes with armatures

Due to arguments changes to some functions, you will need to restart Blender if you had a prior version.
