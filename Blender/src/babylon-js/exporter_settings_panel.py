from .package_level import *

import bpy
# Panel displayed in Scene Tab of properties, so settings can be saved in a .blend file
class ExporterSettingsPanel(bpy.types.Panel):
    bl_label = get_title()
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'scene'

    bpy.types.Scene.exportScope = bpy.props.EnumProperty(
        items=(
            ('ALL', "All", "Export the whole scene"),
            ('SELECTED', "Selected", "Export the selected objects only"),
            ('VISIBLE', "Layers", "Export only objects in the active layers"),
        ),
        name="ui_tab",
        description="Export selection control",
    )
    bpy.types.Scene.positionsPrecision = bpy.props.IntProperty(
        name='Positions / Shape Keys:',
        description='Max number of digits for positions / shape keys.  Reducing useful to reduce\nfile size when units of meshes already small, .e.g inches',
        default = 4, min = 0, max = 5
    )
    bpy.types.Scene.normalsPrecision = bpy.props.IntProperty(
        name='Normals:',
        description='Max number of digits for normals',
        default = 3, min = 1, max = 5
    )
    bpy.types.Scene.UVsPrecision = bpy.props.IntProperty(
        name='UVs:',
        description='Max number of digits for UVs',
        default = 3, min = 1, max = 5
    )
    bpy.types.Scene.vColorsPrecision = bpy.props.IntProperty(
        name='Vertex Colors:',
        description='Number of digits for colors',
        default = 3, min = 1, max = 5
    )
    bpy.types.Scene.mWeightsPrecision = bpy.props.IntProperty(
        name='Matrix Weights:',
        description='Max number of digits for armature weights',
        default = 2, min = 1, max = 5
    )
    bpy.types.Scene.attachedSound = bpy.props.StringProperty(
        name='Sound',
        description='',
        default = ''
        )
    bpy.types.Scene.loopSound = bpy.props.BoolProperty(
        name='Loop sound',
        description='',
        default = True
        )
    bpy.types.Scene.autoPlaySound = bpy.props.BoolProperty(
        name='Auto play sound',
        description='',
        default = True
        )
    bpy.types.Scene.inlineTextures = bpy.props.BoolProperty(
        name='inline',
        description='turn textures into encoded strings, for direct inclusion into source code',
        default = False
    )
    bpy.types.Scene.textureDir = bpy.props.StringProperty(
        name='Sub-directory',
        description='The path below the output directory to write texture files (any separators OS dependent)',
        default = ''
        )
    bpy.types.Scene.ignoreIKBones = bpy.props.BoolProperty(
        name='Ignore IK Bones',
        description="Do not export bones with either '.ik' or 'ik.'(not case sensitive) in the name",
        default = False,
        )
    bpy.types.Scene.writeManifestFile = bpy.props.BoolProperty(
        name='Write .manifest file',
        description="Automatically create or update [filename].babylon.manifest for this file",
        default = True,
        )
    bpy.types.Scene.currentActionOnly = bpy.props.BoolProperty(
        name='Only Currently Assigned Actions',
        description="When true, only the currently assigned action is exported",
        default = False,
        )

    bpy.types.Scene.autoAnimate = bpy.props.BoolProperty(
        name='Auto launch non-skeleton animations',
        description='Start all animations, except for bones.',
        default = False
    )

    def draw(self, context):
        layout = self.layout

        scene = context.scene

        layout.label('Export')
        layout.prop(scene, 'exportScope', expand=True)
        layout.separator()

        layout.prop(scene, 'ignoreIKBones')
        layout.prop(scene, 'writeManifestFile')

        box = layout.box()
        box.label(text='Max Decimal Precision:')
        box.prop(scene, 'positionsPrecision')
        box.prop(scene, 'normalsPrecision')
        box.prop(scene, 'UVsPrecision')
        box.prop(scene, 'vColorsPrecision')
        box.prop(scene, 'mWeightsPrecision')

        box = layout.box()
        box.label(text='Texture Location:')
        box.prop(scene, 'inlineTextures')
        row = box.row()
        row.enabled = not scene.inlineTextures
        row.prop(scene, 'textureDir')

        box = layout.box()
        box.prop(scene, 'attachedSound')
        box.prop(scene, 'autoPlaySound')
        box.prop(scene, 'loopSound')

        box = layout.box()
        box.label(text='Animation:')
        box.prop(scene, 'currentActionOnly')
        box.prop(scene, 'autoAnimate')
