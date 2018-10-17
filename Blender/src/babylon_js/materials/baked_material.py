from babylon_js.logger import *
from babylon_js.package_level import *

from .abstract_material import *

import bpy
#===============================================================================
class BakedMaterial(AbstractMaterial):
    def __init__(self, exporter, mesh, recipe):
        super().__init__(mesh.data.checkReadyOnlyOnce, mesh.data.maxSimultaneousLights)
        nameSpace = exporter.nameSpace if mesh.data.materialNameSpace == DEFAULT_MATERIAL_NAMESPACE or len(mesh.data.materialNameSpace) == 0 else mesh.data.materialNameSpace
        self.name = nameSpace + '.' + mesh.name
        Logger.log('processing begun of baked material:  ' +  mesh.name, 2)

        # changes to cycles & smart_project occurred in 2.77; need to know what we are running
        bVersion = blenderMajorMinorVersion()

        # any baking already took in the values. Do not want to apply them again, but want shadows to show.
        # These are the default values from StandardMaterials
        self.ambient = Color((0, 0, 0))
        self.diffuse = Color((0.8, 0.8, 0.8)) # needed for shadows, but not change anything else
        self.specular = Color((1, 1, 1))
        self.emissive = Color((0, 0, 0))
        self.specularPower = 64
        self.alpha = 1.0

        self.backFaceCulling = recipe.backFaceCulling

        # texture is baked from selected mesh(es), need to insure this mesh is only one selected
        bpy.ops.object.select_all(action='DESELECT')
        mesh.select = True

        # mode_set's only work when there is an active object
        exporter.scene.objects.active = mesh

         # UV unwrap operates on mesh in only edit mode, procedurals can also give error of 'no images to be found' when not done
         # select all verticies of mesh, since smart_project works only with selected verticies
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')

        # you need UV on a mesh in order to bake image.  This is not reqd for procedural textures, so may not exist
        # need to look if it might already be created, if so use the first one
        uv = mesh.data.uv_textures[0] if len(mesh.data.uv_textures) > 0 else None

        if uv == None or recipe.forceBaking:
            mesh.data.uv_textures.new('BakingUV')
            uv = mesh.data.uv_textures['BakingUV']
            uv.active = True
            uv.active_render = not recipe.forceBaking # want the other uv's for the source when combining

            bpy.ops.uv.smart_project(angle_limit = 66.0, island_margin = 0.0, user_area_weight = 1.0, use_aspect = True, stretch_to_bounds = True)

            # syntax for using unwrap instead of smart project
#            bpy.ops.uv.unwrap(margin = 1.0) # defaulting on all
            uvName = 'BakingUV'  # issues with cycles when not done this way
        else:
            uvName = uv.name

        format = 'PNG' if recipe.usePNG else 'JPEG'

        # create a temporary image & link it to the UV/Image Editor so bake_image works
        image = bpy.data.images.new(name = mesh.name + '_BJS_BAKE', width = recipe.bakeSize, height = recipe.bakeSize, alpha = recipe.usePNG, float_buffer = False)
        image.file_format = format
        image.mapping = 'UV' # default value

        image_settings = exporter.scene.render.image_settings
        image_settings.file_format = format
        image_settings.color_mode = 'RGBA' if recipe.usePNG else 'RGB'
        image_settings.quality = recipe.bakeQuality # for lossy compression formats
        image_settings.compression = recipe.bakeQuality  # Amount of time to determine best compression: 0 = no compression with fast file output, 100 = maximum lossless compression with slow file output

        # now go thru all the textures that need to be baked
        nodeTrees = recipe.nodeTrees
        if recipe.diffuseBaking:
            self.bake('diffuseTexture', 'DIFFUSE', 'TEXTURE', image, mesh, uvName, exporter, nodeTrees)

        if recipe.ambientBaking:
            self.bake('ambientTexture', 'AO', image, mesh, uvName, exporter, nodeTrees)

        if recipe.opacityBaking:  # no equivalent found for cycles
            self.bake('opacityTexture', None, image, mesh, uvName, exporter, nodeTrees)

        if recipe.reflectionBaking:  # no equivalent found for cycles
            self.bake('reflectionTexture', None, image, mesh, uvName, exporter, nodeTrees)

        if recipe.emissiveBaking:
            self.bake('emissiveTexture', 'EMIT', image, mesh, uvName, exporter, nodeTrees)

        if recipe.bumpBaking:
            self.bake('bumpTexture', 'NORMAL', image, mesh, uvName, exporter, nodeTrees)

        if recipe.specularBaking:
            self.bake('specularTexture', 'GLOSSY', image, mesh, uvName, exporter, nodeTrees)

        # Toggle vertex selection & mode, if setting changed their value
        bpy.ops.mesh.select_all(action='TOGGLE')  # still in edit mode toggle select back to previous
        bpy.ops.object.mode_set(toggle=True)      # change back to Object

        bpy.ops.object.select_all(action='TOGGLE') # change scene selection back, not seeming to work
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def bake(self, bjs_type, bake_type, image, mesh, uvName, exporter, nodeTrees):
        extension = '.png' if recipe.usePNG else '.jpg'

        if cycles_type is None: return
        
        Logger.log('baking texture, type: ' + bake_type + ', mapped using: ' + uvName, 3)
        legalName = legal_js_identifier(self.name)
        image.filepath = legalName + '_' + bake_type + extension

        # create an unlinked temporary node to bake to for each material
        for tree in nodeTrees:
            bakeNode = tree.nodes.new(type='ShaderNodeTexImage')
            bakeNode.image = image
            bakeNode.select = True
            tree.nodes.active = bakeNode

        bpy.ops.object.bake(type = bake_type, use_clear = True, margin = 5, use_selected_to_active = False)

        for tree in nodeTrees:
            tree.nodes.remove(tree.nodes.active)

        self.textures.append(Texture(bjs_type, 1.0, image, mesh, exporter))
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    @staticmethod
    def meshBakingClean(mesh):
        for uvMap in mesh.data.uv_textures:
            if uvMap.name == 'BakingUV':
                mesh.data.uv_textures.remove(uvMap)
                break

        # remove an image if it was baked
        for image in bpy.data.images:
            if image.name == mesh.name + '_BJS_BAKE':
                image.user_clear() # cannot remove image unless 0 references
                bpy.data.images.remove(image)
                break
