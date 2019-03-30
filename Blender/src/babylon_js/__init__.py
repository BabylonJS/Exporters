bl_info = {
    'name': 'Babylon.js',
    'author': 'David Catuhe, Jeff Palmer',
    'version': (6, 0, -10),
    'blender': (2, 80, 0),
    'location': 'File > Export > Babylon.js (.babylon)',
    'description': 'Export Babylon.js scenes (.babylon)',
    'wiki_url': 'https://github.com/BabylonJS/Babylon.js/tree/master/Exporters/Blender',
    'tracker_url': '',
    'category': 'Babylon.JS'}

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper

# allow module to be changed during a session (dev purposes)
if "bpy" in locals():
    print('Reloading .babylon exporter')
    import imp
    if 'materials' in locals():
        imp.reload(materials)  # directory
    if 'animation' in locals():
        imp.reload(animation)
    if 'armature' in locals():
        imp.reload(armature)
    if 'camera' in locals():
        imp.reload(camera)
    if 'f_curve_animatable' in locals():
        imp.reload(f_curve_animatable)
    if 'js_exporter' in locals():
        imp.reload(js_exporter)
    if 'light_shadow' in locals():
        imp.reload(light_shadow)
    if 'logging' in locals():
        imp.reload(logging)
    if 'mesh' in locals():
        imp.reload(mesh)
    if 'package_level' in locals():
        imp.reload(package_level)
    if 'shape_key_group' in locals():
        imp.reload(shape_key_group)
    if 'sound' in locals():
        imp.reload(sound)
    if 'world' in locals():
        imp.reload(world)

#===============================================================================
class JsonMain(bpy.types.Operator, ExportHelper):
    bl_idname = 'export.bjs'
    bl_label = 'Export Babylon.js scene' # used on the label of the actual 'save' button
    bl_options = {'REGISTER', 'UNDO'}
    filename_ext = '.babylon'            # used as the extension on file selector

    filepath = bpy.props.StringProperty(subtype = 'FILE_PATH') # assigned once the file selector returns
    filter_glob = bpy.props.StringProperty(name='.babylon',default='*.babylon', options={'HIDDEN'})

    def execute(self, context):
        from .json_exporter import JsonExporter
        from .package_level import get_title, verify_min_blender_version

        if not verify_min_blender_version():
            self.report({'ERROR'}, 'version of Blender too old.')
            return {'FINISHED'}

        exporter = JsonExporter()
        exporter.execute(context, self.filepath)

        if (exporter.fatalError):
            self.report({'ERROR'}, exporter.fatalError)

        elif (exporter.nErrors > 0):
            self.report({'ERROR'}, 'Output cancelled due to data error, See log file.')

        elif (exporter.nWarnings > 0):
            self.report({'WARNING'}, 'Processing completed, but ' + str(exporter.nWarnings) + ' WARNINGS were raised,  see log file.')

        return {'FINISHED'}

    def draw(self, context):
        self.layout.label(
            text='Find export settings in the properties panels',
            icon='INFO'
        )
#===============================================================================
# The list of classes which sub-class a Blender class, which needs to be registered
from . import camera
from . import light_shadow
from . import materials # directory
from . import world # must be defined before mesh
from . import mesh
classes = (
    # Operator sub-classes
    JsonMain,

    # Panel sub-classes
    camera.CameraPanel,
    light_shadow.LightPanel,
    materials.material.MaterialsPanel,
    mesh.MeshPanel,
    world.WorldPanel
)

def register():
    from bpy.utils import register_class
    for cls in classes:
        register_class(cls)
    bpy.types.TOPBAR_MT_file_export.append(menu_func)

def unregister():
    from bpy.utils import unregister_class
    for cls in reversed(classes):
        unregister_class(cls)

    bpy.types.TOPBAR_MT_file_export.remove(menu_func)

# Registration the calling of the INFO_MT_file_export file selector
def menu_func(self, context):
    from .package_level import get_title
    # the info for get_title is in this file, but getting it the same way as others
    self.layout.operator(JsonMain.bl_idname, text=get_title())

if __name__ == '__main__':
    unregister()
    register()
