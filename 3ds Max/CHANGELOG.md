# Changelog

## v1.3.2
**Implemented changes:**
- Added changelog

**Fixed bugs:**
- Fixed a bug in exporting KHR_lights_punctual to glTF where ambient was still being indexed, causing the resulting glTF to cause errors in loaders (https://github.com/BabylonJS/Exporters/issues/340)

## v1.3.3
**Implemented changes:**
- Added checkbox to toggle KHR_lights_punctual in exporter
- Remove copyright from glTF files

## v1.3.4
**Fixed bugs:**
- Fixed a bug in exporting occlusion textures without enabling the mergeAO flag

## v1.3.5
**Implemented changes:**
- KHR_texture_transform is now disabled by default but can be enabled with a checkbox in the exporter interface
- KHR_texture_transform is now implemented as a used and required extension in the glTF file

## v1.3.6
**Implemented changes:**
- KHR_materials_unlit can now be toggled from the exporter iterface
- Alpha Test can now be enabled by setting "babylonAlphaTest" as a boolean custom attribute on a material

## v1.3.7
**Implemented changes:**
- Added option to not export materials (https://github.com/BabylonJS/Exporters/issues/362)

## v1.3.8
**Fixed bugs:**
- Fixed normals for models that contain offset transforms (https://github.com/BabylonJS/Exporters/issues/360)

## v1.3.9
**Fixed bugs:**
- Fixed bug where setting the base color texture overwrites the base color factor (https://github.com/BabylonJS/Exporters/issues/367)

## v1.3.10
**Fixed bugs:**
- Fixed bug where export would fail if the number of max material param blocks is 0 (https://github.com/BabylonJS/Exporters/issues/392)

## v1.3.11
**Fixed bugs:**
- Fixed bug where binary babylon files were not copied to the output directory (https://github.com/BabylonJS/Exporters/issues/397)

## v1.3.12
**Fixed bugs:**
- Fixed bug where texture names are changed when exporting from the exporter(https://github.com/BabylonJS/Exporters/issues/379)

## v1.3.13
**Fixed bugs:**
- Fixed bug where babylonbinarymeshdata file was not getting copied to the output directory(https://github.com/BabylonJS/Exporters/issues/401)

## v1.3.14
**Fixed bugs:**
- Fixed bug where texture transform offset and scale is
not exported correctly using glTF
(https://github.com/BabylonJS/Exporters/issues/383)

## v1.3.15
**Fixed bugs:**
- Fixed bug where morph target normals were not exported unless the property menu was opened
(https://github.com/BabylonJS/Exporters/issues/382)

## v1.3.16
**Fixed bugs:**
- Fixed issue where the scale factor was treated as 1 / scale factor in the exported glTF
(https://github.com/BabylonJS/Exporters/issues/394)

## v1.3.17
**Fixed bugs:**
- Fixed issue where textures with different texture transforms applied were not exported properly with glTF
(https://github.com/BabylonJS/Exporters/issues/409)

## v1.3.18
**Fixed bugs:**
- Fixed issue where a texture connected to multiple material inputs would be cloned on export instead of reused
(https://github.com/BabylonJS/Exporters/issues/386)

## v1.3.19
**Fixed bugs:**
- Fixed issue where standard materials with different texture transforms were not exported to glTF
(https://github.com/BabylonJS/Exporters/issues/409)

## v1.3.20
**Fixed bugs:**
- Allow ambient occlusion texture coordinates and texture transforms to be respected when merged with an ORM texture
(https://github.com/BabylonJS/Exporters/issues/385)

## v1.3.21
**Fixed bugs:**
- fixed material-slot allocation for Arnold materials to better withstand changes to slot IDs (due to feature changes on arnold material)
- optimizedmaterial-alphasettings-export by adding .png to valid extensions when exporting opaque materials
(https://github.com/BabylonJS/Exporters/pull/416)

## v1.3.22
**Fixed bugs:**
- modify texture name if it is not a valid uri name
(https://github.com/BabylonJS/Exporters/issues/419)

## v1.3.23
**Changes:**
- preserve texture name if it is not a valid uri name, but modify within glTF file
(https://github.com/BabylonJS/Exporters/issues/419)

## v1.3.24
**Changes:**
- Add version number in exporter window
(https://github.com/BabylonJS/Exporters/issues/424)

## v1.3.25
**Fixed Bugs:**
- Fixed bug in GLTFImage uri which caused glb files to not export
(https://github.com/BabylonJS/Exporters/issues/423)

## v1.3.26
**Fixed Bugs:**
- Fixed bug in GLTFImage uri which caused glb files with textures containing spaces to not export
(https://github.com/BabylonJS/Exporters/issues/429)

## v1.3.27
**Fixed Bugs:**
- Fixed issue where shell material may fail to export
(https://github.com/BabylonJS/Exporters/issues/413)

## v1.3.28
**Implemented Changes:**
- Joint indices for glTF are now set to 0 instead of joint count

## v1.3.29
**Fixed Bugs:**
- Change "tag" to "tags" in babylon file and glTF extras
(https://github.com/BabylonJS/Exporters/issues/436)

## v1.3.30
**Implemented Changes:**
- Added @Selmar 's multi file export
(https://github.com/BabylonJS/Exporters/issues/436)

## v1.3.31
**Fixed Bugs:**
- Disable exporting a multimaterial connect to another multimaterial
(https://github.com/BabylonJS/Exporters/issues/384)

## v1.3.32
**Fixed Bugs:**
- Fixed bug causing DirectX material to fail the exporter
(https://github.com/BabylonJS/Exporters/issues/405)
