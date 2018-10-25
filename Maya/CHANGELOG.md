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