# Changelog

## v1.2.23
**Implemented changes:**
- Added changelog for Maya2Babylon

**Fixed bugs:**
- Fixed bug where a missing bitmap in the base color texture would cause export to fail.  Also fixes an error where colons were used in the output file name (https://github.com/BabylonJS/Exporters/issues/341)
- Fixed a bug in exporting KHR_lights_punctual to glTF where ambient was still being indexed, causing the resulting glTF to cause errors in loaders (https://github.com/BabylonJS/Exporters/issues/340)
- Fixed bug where opaque and transparent meshes in Arnold would cause the exporter to fail (https://github.com/BabylonJS/Exporters/issues/339)

## v1.2.24
**Implemented changes:**
- Change scale factor to use scale directly instead of 1/scale

## v1.2.25
**Implemented changes:**
- KHR_texture_transform and KHR_lights_punctual are now toggable
from the export interface
- KHR_texture_transform is set to required on export
- Copywrite has been removed from the exported glTF file

## v1.2.26
**Implemented changes:**
- morph target animations are now included in animation groups when created during export 

## v1.2.27
**Fixed bugs:**
- Default light node is not exported in glTF
**Implemented changes:**
- Add version number in the exporter window (https://github.com/BabylonJS/Exporters/issues/424)

## v1.2.28
**Implemented changes:**
- Joint indices for glTF are now set to 0 instead of joint count

## v1.2.29
**Fixed bugs:**
- Allow animation groups with two key frames of the same value to export

## v1.2.30
**Fixed bugs:**
- Disallow animation groups with two key frames of a TRS value to export if they are the default value (origin, no scale, no rotation)

## v1.2.31
**Implemented changes:**
- Allow building to Maya 2019