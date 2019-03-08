from ..logging import *
from ..package_level import *

from .nodes.abstract import *
from .texture import BakedTexture

import bpy

DEFAULT_MATERIAL_NAMESPACE = 'Same as Filename'
#===============================================================================
class MultiMaterial:
    def __init__(self, material_slots, idx, nameSpace):
        self.name = nameSpace + '.' + 'Multimaterial#' + str(idx)
        Logger.log('processing begun of multimaterial:  ' + self.name, 2)
        self.material_slots = material_slots
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_json_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.name, True)
        write_string(file_handler, 'id', self.name)

        file_handler.write(',"materials":[')
        first = True
        for material in self.material_slots:
            if first != True:
                file_handler.write(',')
            file_handler.write('"' + material.name +'"')
            first = False
        file_handler.write(']')
        file_handler.write('}')
#===============================================================================
class BJSMaterial:
    # mat can either be a blender material, or a previously instanced BJSMaterial, & now baking
    def __init__(self, mat, exporter):
        # initialize; appended to either in processImageTextures() or bakeChannel()
        self.textures = {}

        self.isPBR = exporter.settings.usePBRMaterials
        self.textureFullPathDir = exporter.textureFullPathDir

        # transfer from either the Blender or previous BJSMaterial
        self.checkReadyOnlyOnce = mat.checkReadyOnlyOnce
        self.maxSimultaneousLights = mat.maxSimultaneousLights
        self.backFaceCulling = mat.backFaceCulling
        self.use_nodes = mat.use_nodes

        if not isinstance(mat, BJSMaterial):
            bpyMaterial = mat
            nameSpace = exporter.nameSpace if bpyMaterial.materialNameSpace == DEFAULT_MATERIAL_NAMESPACE or len(bpyMaterial.materialNameSpace) == 0 else bpyMaterial.materialNameSpace
            self.name = nameSpace + '.' + bpyMaterial.name
            Logger.log('processing begun of material:  ' +  self.name, 2)

            if self.use_nodes:
                self.bjsNodeTree = AbstractBJSNode.readMaterialNodeTree(bpyMaterial.node_tree)
            else:
                self.diffuseColor = bpyMaterial.diffuse_color
                self.specularColor = bpyMaterial.specular_intensity * bpyMaterial.specular_color
                self.metallic = bpyMaterial.metallic
        else:
            self.name = mat.name
            self.bjsNodeTree = mat.bjsNodeTree
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    # broken out, so can be done later, after it is known baking is not going to be required
    # called by Mesh constructor, return whether material has textures or not
    def processImageTextures(self, bpyMesh):
        if not self.use_nodes: return False

        for texType, tex in self.bjsNodeTree.bjsTextures.items():
            self.textures[texType] = tex
            tex.process(self.textureFullPathDir, True, bpyMesh)

        return len(self.textures.items()) > 0
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def bake(self, bpyMesh, recipe):
        # texture is baked from selected mesh(es), need to insure this mesh is only one selected
        bpy.ops.object.select_all(action='DESELECT')
        bpyMesh.select_set(True)

        # store setting to restore; always bake using CYCLES
        scene = bpy.context.scene
        render = scene.render
        engine = render.engine
        render.engine = 'CYCLES'

        # transfer from Mesh custom properties
        bakeSize    = bpyMesh.data.bakeSize
        bakeQuality = bpyMesh.data.bakeQuality # for lossy compression formats
        forceBaking = bpyMesh.data.forceBaking
        usePNG      = bpyMesh.data.usePNG

        # mode_set's only work when there is an active object
        bpy.context.view_layer.objects.active = bpyMesh

        # UV unwrap operates on mesh in only edit mode, procedurals can also give error of 'no images to be found' when not done
        # select all verticies of mesh, since smart_project works only with selected verticies
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')

        # you need UV on a mesh in order to bake image.  This is not reqd for procedural textures, so may not exist
        # need to look if it might already be created, if so use the first one
        uv = bpyMesh.data.uv_layers[0] if len(bpyMesh.data.uv_layers) > 0 else None

        if uv == None or forceBaking:
            uv = bpyMesh.data.uv_layers.new(name='BakingUV')
          #  uv = bpyMesh.data.uv_layers['BakingUV']
            uv.active = True
            uv.active_render = not forceBaking # want the other uv's for the source when combining

            bpy.ops.uv.smart_project(angle_limit = 66.0, island_margin = 0.0, user_area_weight = 1.0, use_aspect = True, stretch_to_bounds = True)

            # syntax for using unwrap enstead of smart project
#            bpy.ops.uv.unwrap(margin = 1.0) # defaulting on all
            self.uvMapName = 'BakingUV'  # issues with cycles when not done this way
        else:
            self.uvMapName = uv.name

        format = 'PNG' if usePNG else 'JPEG'

        # create a temporary image & link it to the UV/Image Editor so bake_image works
        self.image = bpy.data.images.new(name = bpyMesh.name + '_BJS_BAKE', width = bakeSize, height = bakeSize, alpha = usePNG, float_buffer = False)
        self.image.file_format = format
    #    self.image.mapping = 'UV' # default value

        image_settings = render.image_settings
        image_settings.file_format = format
        image_settings.color_mode = 'RGBA' if usePNG else 'RGB'
        image_settings.quality = bakeQuality # for lossy compression formats
        image_settings.compression = bakeQuality  # Amount of time to determine best compression: 0 = no compression with fast file output, 100 = maximum lossless compression with slow file output

        # now go thru all the textures that need to be baked
        if recipe.diffuseChannel:
            self.bakeChannel(DIFFUSE_TEX , 'DIFFUSE', usePNG, recipe.node_trees, bpyMesh)

        if recipe.ambientChannel:
            self.bakeChannel(AMBIENT_TEX , 'AO'     , usePNG, recipe.node_trees, bpyMesh)

        if recipe.emissiveChannel:
            self.bakeChannel(EMMISIVE_TEX, 'EMIT'   , usePNG, recipe.node_trees, bpyMesh)

        if recipe.specularChannel:
            self.bakeChannel(SPECULAR_TEX, 'GLOSSY' , usePNG, recipe.node_trees, bpyMesh)

        if recipe.bumpChannel:
            self.bakeChannel(BUMP_TEX    , 'NORMAL' , usePNG, recipe.node_trees, bpyMesh)

        # Toggle vertex selection & mode, if setting changed their value
        bpy.ops.mesh.select_all(action='TOGGLE')  # still in edit mode toggle select back to previous
        bpy.ops.object.mode_set(toggle=True)      # change back to Object

        bpy.ops.object.select_all(action='TOGGLE') # change scene selection back, not seeming to work

        render.engine = engine
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def bakeChannel(self, bjs_type, bake_type, usePNG, node_trees, bpyMesh):
        Logger.log('Baking texture, type: ' + bake_type + ', mapped using: ' + self.uvMapName, 3)
        legalName = legal_js_identifier(self.name)
        self.image.filepath = legalName + '_' + bake_type + ('.png' if usePNG else '.jpg')

        scene = bpy.context.scene
        scene.render.engine = 'CYCLES'

        # create an unlinked temporary node to bake to for each material
        for tree in node_trees:
            bakeNode = tree.nodes.new(type='ShaderNodeTexImage')
            bakeNode.image = self.image
            bakeNode.select = True
            tree.nodes.active = bakeNode

        bpy.ops.object.bake(type = bake_type, use_clear = True, margin = 5, use_selected_to_active = False)

        for tree in node_trees:
            tree.nodes.remove(tree.nodes.active)

        self.textures[bjs_type] = BakedTexture(bjs_type, self, bpyMesh)
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_json_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.name, True)
        write_string(file_handler, 'id', self.name)
        write_string(file_handler, 'customType', 'BABYLON.PBRMaterial' if self.isPBR else 'BABYLON.StandardMaterial')

        # properties from UI
        write_bool(file_handler, 'backFaceCulling', self.backFaceCulling)
        write_bool(file_handler, 'checkReadyOnlyOnce', self.checkReadyOnlyOnce)
        write_int(file_handler, 'maxSimultaneousLights', self.maxSimultaneousLights)

        if not self.use_nodes:
            propName = 'albedoColor' if self.isPBR else 'diffuseColor'
            write_color(file_handler, propName, self.diffuseColor)

            propName = 'reflectivityColor' if self.isPBR else 'specularColor'
            write_color(file_handler, propName, self.specularColor)

            if self.isPBR:
                write_float(file_handler, 'metallic', self.metallic)

            file_handler.write('}')
            return

        #--- scalar properties, when not also a texture ----

        # sources diffuse & principled nodes
        if self.bjsNodeTree.diffuseColor and DIFFUSE_TEX not in self.textures:
            propName = 'albedo' if self.isPBR else 'diffuse'
            write_color(file_handler, propName, self.bjsNodeTree.diffuseColor)

        # source ambientOcclusion node
        if self.bjsNodeTree.ambientColor:
            write_color(file_handler, 'ambient', self.bjsNodeTree.ambientColor)

        # source emissive node
        if self.bjsNodeTree.emissiveColor:
            write_color(file_handler, 'emissive', self.bjsNodeTree.emissiveColor)

        # sources glossy & principled nodes
        if self.bjsNodeTree.specularColor and SPECULAR_TEX not in self.textures:
            propName = 'reflectivity' if self.isPBR else 'specular'
            write_color(file_handler, propName, self.bjsNodeTree.specularColor)

        roughness = 0.2 # 0.2 is the Blender default for glossy Node; Principled default is 0.5, but if [principled used gets get bubbled up
        if self.bjsNodeTree.specularRoughness: # coming from glossy node
            roughness = self.bjsNodeTree.specularRoughness
        elif self.bjsNodeTree.roughness: # coming from principled node
            roughness = self.bjsNodeTree.roughness

        value = roughness if self.isPBR else 128 - (roughness * 128)
        propName = 'roughness' if self.isPBR else 'specularPower'
        write_float(file_handler, propName, value)

        # sources diffuse, transparency & principled nodes
        alpha = self.bjsNodeTree.diffuseAlpha if self.bjsNodeTree.diffuseAlpha is not None else 1.0
        write_float(file_handler, 'alpha', alpha)

        # sources refraction & principled nodes
        if self.bjsNodeTree.indexOfRefraction and REFRACTION_TEX not in self.textures:
            write_float(file_handler, 'indexOfRefraction', self.bjsNodeTree.indexOfRefraction)

        # properties specific to PBR
        if self.isPBR:
            # source principle node
            if self.bjsNodeTree.metallic and METAL_TEX not in self.textures:
                write_float(file_handler, 'metallic', self.bjsNodeTree.metallic)

            # source emissive node
            if self.bjsNodeTree.emissiveIntensity:
                write_color(file_handler, 'emissiveIntensity', self.bjsNodeTree.emissiveIntensity)

        # ---- add textures ----

        # sources diffuse & principled nodes
        if DIFFUSE_TEX in self.textures:
            tex = self.textures[DIFFUSE_TEX]
            texType = ALBEDO_TEX if self.isPBR else DIFFUSE_TEX
            self.textures[DIFFUSE_TEX].textureType = texType
            tex.to_json_file(file_handler)

            if self.isPBR:
                write_bool(file_handler, 'useAlphaFromAlbedoTexture', tex.hasAlpha)

        # source ambientOcclusion node
        if AMBIENT_TEX in self.textures and not self.isPBR:
            self.textures[AMBIENT_TEX].to_json_file(file_handler)

        # source transparency node
        if OPACITY_TEX in self.textures:
            self.textures[OPACITY_TEX].to_json_file(file_handler)

        # source emissive node
        if EMMISIVE_TEX in self.textures:
            self.textures[EMMISIVE_TEX].to_json_file(file_handler)

        # sources glossy & principled nodes
        if SPECULAR_TEX in self.textures:
            texType = REFLECTION_TEX if self.isPBR else SPECULAR_TEX
            self.textures[SPECULAR_TEX].textureType = texType
            self.textures[SPECULAR_TEX].to_json_file(file_handler)

        # sources normal_map & principled nodes
        if BUMP_TEX in self.textures:
            self.textures[BUMP_TEX].to_json_file(file_handler)

        # sources refraction & principled nodes
        if REFRACTION_TEX in self.textures:
            self.textures[REFRACTION_TEX].to_json_file(file_handler)

        if self.isPBR:
            if METAL_TEX in self.textures or ROUGHNESS_TEX in self.textures or AMBIENT_TEX in self.textures:
                # there is really only ever one texture, but could be in dictionary in multiple places
                if METAL_TEX in self.textures: # source principled node
                    self.textures[METAL_TEX].to_json_file(file_handler)
                elif ROUGHNESS_TEX in self.textures: # source principled node
                    self.textures[ROUGHNESS_TEX].textureType = METAL_TEX
                    self.textures[ROUGHNESS_TEX].to_json_file(file_handler)
                elif AMBIENT_TEX in self.textures: # source ambientOcclusion node
                    self.textures[AMBIENT_TEX].textureType = METAL_TEX
                    self.textures[AMBIENT_TEX].to_json_file(file_handler)

                write_bool(file_handler, 'useMetallnessFromMetallicTextureBlue', METAL_TEX in self.textures)
                write_bool(file_handler, 'useRoughnessFromMetallicTextureGreen', ROUGHNESS_TEX in self.textures)
                write_bool(file_handler, 'useAmbientOcclusionFromMetallicTextureRed', AMBIENT_TEX in self.textures)

        else:
            if METAL_TEX in self.textures or ROUGHNESS_TEX in self.textures:
                Logger.warn('Metal / roughness texture detected, but no meaning outside of PBR, ignored', 3)

            if REFRACTION_TEX in self.textures:
                Logger.warn('Refraction texture detected, but no meaning outside of PBR, ignored', 3)

        file_handler.write('}')
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    @staticmethod
    def meshBakingClean(mesh):
        for uvMap in mesh.data.uv_layers:
            if uvMap.name == 'BakingUV':
                mesh.data.uv_layers.remove(uvMap)
                break

        # remove an image if it was baked
        for image in bpy.data.images:
            if image.name == mesh.name + '_BJS_BAKE':
                image.user_clear() # cannot remove image unless 0 references
                bpy.data.images.remove(image)
                break
#===============================================================================
bpy.types.Material.backFaceCulling = bpy.props.BoolProperty(
    name='Back Face Culling',
    description='When checked, the faces on the inside of the mesh will not be drawn.',
    default = True
)
bpy.types.Material.checkReadyOnlyOnce = bpy.props.BoolProperty(
    name='Check Ready Only Once',
    description='When checked better CPU utilization.  Advanced user option.',
    default = False
)
bpy.types.Material.maxSimultaneousLights = bpy.props.IntProperty(
    name='Max Simultaneous Lights',
    description='BJS property set on each material.\nSet higher for more complex lighting.\nSet lower for armatures on mobile',
    default = 4, min = 0, max = 32
)
bpy.types.Material.materialNameSpace = bpy.props.StringProperty(
    name='Name Space',
    description='Prefix to use for materials for sharing across .blends.',
    default = DEFAULT_MATERIAL_NAMESPACE
)
#===============================================================================
class MaterialsPanel(bpy.types.Panel):
    bl_label = get_title()
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'material'

    def draw(self, context):
        layout = self.layout

        mesh = context.object
        index = mesh.active_material_index
        material = mesh.material_slots[index].material

        layout.prop(material, 'backFaceCulling')
        layout.prop(material, 'checkReadyOnlyOnce')
        layout.prop(material, 'maxSimultaneousLights')
        layout.prop(material, 'materialNameSpace')