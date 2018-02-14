Maya exporter
====================

Plug-in for Maya 2018 to export scene to babylon and gltf formats.

This is work in progress.

To use the plug-in:
- copy/paste all .dll files from [MayaExporterPath]\assemblies folder into [MayaPath]\bin\plug-ins folder. Or follow Maya plug-ins install [guide](http://help.autodesk.com/view/MAYAUL/2017/ENU/?guid=__files_GUID_B7E63390_8397_4148_9CA7_B1E14117BE05_htm)
- launch Maya with admin rights
- load plug-in inside Plug-in manager
- write 'toBabylon' inside Maya command window (without quotes)
- the output file is located at 'C:\MyFirstExportToBabylon.babylon', you can visualize the result by dragging the file [here](http://sandbox.babylonjs.com/)

If you are a user of this plug-in, you can open issues [here](https://github.com/BabylonJS/Exporters/issues) in case you experiment bugs.

If you want to contribute to this project, clone the whole Exporters repo since the Maya exporter has shared projects with other exporters. Also update the path to your Maya location in the post-build events (last command).