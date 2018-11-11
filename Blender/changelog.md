# Blender2Babylon add-on changelog

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
