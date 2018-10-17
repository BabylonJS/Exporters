from .logger import *
from .package_level import *

import bpy
from base64 import b64encode
from mathutils import Color
from os import path, remove
from shutil import copy
from sys import exc_info # for writing errors to log file

# used in Texture constructor, defined in BABYLON.Texture
CLAMP_ADDRESSMODE = 0
WRAP_ADDRESSMODE = 1
MIRROR_ADDRESSMODE = 2

# used in Texture constructor, defined in BABYLON.Texture
EXPLICIT_MODE = 0
SPHERICAL_MODE = 1
#PLANAR_MODE = 2
CUBIC_MODE = 3
#PROJECTION_MODE = 4
#SKYBOX_MODE = 5

DEFAULT_MATERIAL_NAMESPACE = 'Same as Filename'
#===============================================================================
class MultiMaterial:
    def __init__(self, material_slots, idx, nameSpace):
        self.name = nameSpace + '.' + 'Multimaterial#' + str(idx)
        Logger.log('processing begun of multimaterial:  ' + self.name, 2)
        self.material_slots = material_slots
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_scene_file(self, file_handler):
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
# need to evaluate the need to bake a mesh before even starting; class also stores specific types of bakes
class BakingRecipe:
    def __init__(self, mesh):
        # transfer from Mesh custom properties
        self.bakeSize    = mesh.data.bakeSize
        self.bakeQuality = mesh.data.bakeQuality # for lossy compression formats
        self.forceBaking = mesh.data.forceBaking # in mesh, but not currently exposed
        self.usePNG      = mesh.data.usePNG      # in mesh, but not currently exposed

        # initialize all members
        self.needsBaking      = self.forceBaking
        self.diffuseBaking    = self.forceBaking
        self.ambientBaking    = False
        self.opacityBaking    = False
        self.reflectionBaking = False
        self.emissiveBaking   = False
        self.bumpBaking       = False
        self.specularBaking   = False

        # accumulators set by Blender Game
        self.backFaceCulling = True  # used only when baking
        self.isBillboard = False # len(mesh.material_slots) == 1 and mesh.material_slots[0] is not None and mesh.material_slots[0].material.game_settings.face_orientation == 'BILLBOARD'

        # Cycles specific, need to get the node trees of each material
        self.nodeTrees = []

        for material_slot in mesh.material_slots:
            # a material slot is not a reference to an actual material; need to look up
            material = material_slot.material

            self.backFaceCulling &= True # material.game_settings.use_backface_culling

            # testing for Cycles renderer has to be different
            if material.use_nodes == True:
                self.needsBaking = True
                self.nodeTrees.append(material.node_tree)

                for node in material.node_tree.nodes:
                    id = node.bl_idname
                    if id == 'ShaderNodeBsdfDiffuse':
                        self.diffuseBaking = True

                    if id == 'ShaderNodeAmbientOcclusion':
                        self.ambientBaking = True

                    # there is no opacity baking for Cycles AFAIK
                    if id == '':
                        self.opacityBaking = True

                    if id == 'ShaderNodeEmission':
                        self.emissiveBaking = True

                    if id == 'ShaderNodeNormal' or id == 'ShaderNodeNormalMap':
                        self.bumpBaking = True

                    if id == '':
                        self.specularBaking = True

            else:
                nDiffuseImages = 0
                nReflectionImages = 0
                nAmbientImages = 0
                nOpacityImages = 0
                nEmissiveImages = 0
                nBumpImages = 0
                nSpecularImages = 0

                textures = [mtex for mtex in material.texture_slots if mtex and mtex.texture]
                for mtex in textures:
                    # ignore empty slots
                    if mtex.texture.type == 'NONE':
                        continue

                    # just need to make sure there is only 1 per type
                    if mtex.texture.type == 'IMAGE' and not self.forceBaking:
                        if mtex.use_map_diffuse or mtex.use_map_color_diffuse:
                            if mtex.texture_coords == 'REFLECTION':
                                nReflectionImages += 1
                            else:
                                nDiffuseImages += 1

                        if mtex.use_map_ambient:
                            nAmbientImages += 1

                        if mtex.use_map_alpha:
                            nOpacityImages += 1

                        if mtex.use_map_emit:
                            nEmissiveImages += 1

                        if mtex.use_map_normal:
                            nBumpImages += 1

                        if mtex.use_map_color_spec:
                            nSpecularImages += 1

                # 2nd pass 2 check for multiples of a given image type
                if nDiffuseImages > 1:
                    self.needsBaking = self.diffuseBaking = True
                if nReflectionImages > 1:
                    self.needsBaking = self.nReflectionImages = True
                if nAmbientImages > 1:
                    self.needsBaking = self.ambientBaking = True
                if nOpacityImages > 1:
                    self.needsBaking = self.opacityBaking = True
                if nEmissiveImages > 1:
                    self.needsBaking = self.emissiveBaking = True
                if nBumpImages > 1:
                    self.needsBaking = self.bumpBaking = True
                if nSpecularImages > 1:
                    self.needsBaking = self.specularBaking = True

#===============================================================================

#===============================================================================

#===============================================================================
