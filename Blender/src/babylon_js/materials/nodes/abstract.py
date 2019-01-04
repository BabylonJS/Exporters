# not importing subclasses at the module level, to avoid abstract importing them & they importing abstract
# done as needed in various methods
from babylon_js.logging import *

# various texture types, value contains the BJS name when needed to be written in output
ENVIRON_TEX    = 'value not meaningful 1'

DIFFUSE_TEX    = 'diffuseTexture'
AMBIENT_TEX    = 'ambientTexture'
OPACITY_TEX    = 'opacityTexture'
EMMISIVE_TEX   = 'emissiveTexture'
SPECULAR_TEX   = 'specularTexture'
BUMP_TEX       = 'bumpTexture'

# PBR specific textures
ALBEDO_TEX     = 'albedoTexture'       # PBR equivalent of a diffuse texture
METAL_TEX      = 'metallicTexture'
REFLECTION_TEX = 'reflectivityTexture' # PBR equivalent of a specular texture
ROUGHNESS_TEX  = 'value not meaningful 2'
REFRACTION_TEX = 'refractionTexture'

UV_ACTIVE_TEXTURE = 'value not meaningful 3'
#===============================================================================
class AbstractBJSNode:

    def __init__(self, bpyNode, socketName, isTopLevel = False):
        self.socketName = socketName
        self.bpyType = bpyNode.bl_idname

        # scalar bubbled up channel values for Std Material
        self.diffuseColor  = None  # same as albedoColor
        self.diffuseAlpha = None
        self.ambientColor  = None
        self.emissiveColor = None
        self.emissiveIntensity = None
        self.specularColor = None
        self.specularRoughness = None
        self.roughness = None

        # scalar bubbled up channel values for Pbr Material
        self.metallic = None
        self.indexOfRefraction = None

        # intialize texture dictionary verses an array, since multiple channels can be output multiple times
        self.bjsTextures = {}
        self.unAssignedBjsTexture = None  # allow a texture to buble up through a pass thru a node before being assigned
        self.uvMapName = None

        # baking broken out by channel, so channels which are fine just go as normal
        self.mustBake = False # whether baking ultimately going to be required
        self.mustBakeDiffuse    = False
        self.mustBakeAmbient    = False
        self.mustBakeOpacity    = False # not curently bakeable
        self.mustBakeEmissive   = False
        self.mustBakeSpecular   = False
        self.mustBakeNormal     = False
        self.mustBakeMetal      = False # not currently bakeable
        self.mustBakeRefraction = False # not currently bakeable

        # evaluate each of the inputs; either add the current / default value, linked Node, or None
        self.bjsInputs = {}
        for nodeSocket in bpyNode.inputs:
            # there are a maximum of 1 inputs per socket
            if len(nodeSocket.links) == 1:
                # recursive instancing of inputs with their matching wrapper sub-class
                bjsWrapperNode = AbstractBJSNode.GetBJSWrapperNode(nodeSocket.links[0].from_node, nodeSocket.name)
                self.bubbleUp(bjsWrapperNode)
                self.bjsInputs[nodeSocket.name] = bjsWrapperNode
            #    print (nodeSocket.name + ' @ ' + str(nodeSocket.links[0]))

            else:
                if hasattr(nodeSocket, 'default_value'):
                #    print('\t' + nodeSocket.name + ': ' + str(nodeSocket.default_value))
                    self.bjsInputs[nodeSocket.name] = nodeSocket.default_value
                else:
                #    print('\t' + nodeSocket.name + ': no VALUE')
                    self.bjsInputs[nodeSocket.name] = None

            # when top level, ignore Volume & Displacement; if unsupported nodes, will bake un-necessarily
            if isTopLevel: break

        # End of super class constructor, sub-class constructor now runs.
        # Sub-class can expect all inputs to be already be loaded as wrappers, &
        # is responsible for setting all values & textures to be bubbled up.
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def bubbleUp(self, bjsWrapperNode):
        self.mustBake |= bjsWrapperNode.mustBake
        self.mustBakeDiffuse  |= bjsWrapperNode.mustBakeDiffuse
        self.mustBakeAmbient  |= bjsWrapperNode.mustBakeAmbient
        self.mustBakeOpacity  |= bjsWrapperNode.mustBakeAmbient
        self.mustBakeEmissive |= bjsWrapperNode.mustBakeEmissive
        self.mustBakeSpecular |= bjsWrapperNode.mustBakeSpecular
        self.mustBakeNormal   |= bjsWrapperNode.mustBakeNormal
        self.mustBakeMetal    |= bjsWrapperNode.mustBakeMetal

        # bubble up any scalars (defaults) which may have been set, allow multiples for principled.
        if bjsWrapperNode.diffuseColor is not None:
            self.diffuseColor = bjsWrapperNode.diffuseColor

        # diffuseAlpha broken out from color, since is can be provided by either the diffuse or transparency node
        if bjsWrapperNode.diffuseAlpha is not None:
            self.diffuseAlpha = bjsWrapperNode.diffuseAlpha

        if bjsWrapperNode.ambientColor is not None:
            self.ambientColor = bjsWrapperNode.ambientColor

        if bjsWrapperNode.emissiveColor is not None:
            self.emissiveColor = bjsWrapperNode.emissiveColor
            self.emissiveIntensity = bjsWrapperNode.emissiveIntensity

        if bjsWrapperNode.specularColor is not None:
            self.specularColor = bjsWrapperNode.specularColor
            self.specularRoughness = bjsWrapperNode.specularRoughness

        if bjsWrapperNode.metallic is not None:
            self.metallic = bjsWrapperNode.metallic

        if bjsWrapperNode.roughness is not None:
            self.roughness = bjsWrapperNode.roughness

        if bjsWrapperNode.indexOfRefraction is not None:
            self.indexOfRefraction = bjsWrapperNode.indexOfRefraction

        if bjsWrapperNode.unAssignedBjsTexture is not None:
            self.unAssignedBjsTexture = bjsWrapperNode.unAssignedBjsTexture

        if bjsWrapperNode.uvMapName is not None:
            self.uvMapName = bjsWrapperNode.uvMapName

        # bubble up any textures into the dictionary.
        # can assign over another, last wins, but they were probably duplicate.
        for texType, tex in bjsWrapperNode.bjsTextures.items():
            self.bjsTextures[texType] = tex
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
#     Methods called by BakingRecipe to figure out what to bake, if required
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def usesDiffuseChannel(self):
        return self.diffuseColor is not None or DIFFUSE_TEX in self.bjsTextures

    def usesAmbientChannel(self):
        return self.ambientColor is not None or AMBIENT_TEX in self.bjsTextures

    def usesEmissiveChannel(self):
        return self.emissiveColor is not None or EMMISIVE_TEX in self.bjsTextures

    def usesSpecularChannel(self):
        return self.specularColor is not None or SPECULAR_TEX in self.bjsTextures

    def usesMetalChannel(self):
        return self.metallic is not None or METAL_TEX in self.bjsTextures

    def usesRefractionChannel(self):
        return self.indexOfRefraction is not None or REFRACTION_TEX in self.bjsTextures

    def usesOpacityChannel(self):
        return OPACITY_TEX in self.bjsTextures

    def usesBumpChannel(self):
        return BUMP_TEX in self.bjsTextures

# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
#     Methods for finding inputs to this node
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    # leave out bpyType when value can either be a default value or another node
    def findInput(self, socketName, bpyTypeReqd = None):
        value = self.bjsInputs[socketName]
        if bpyTypeReqd is not None:
            if not hasattr(value, 'bpyType') or value.bpyType != bpyTypeReqd:
                return None

        return value

    # called by many sub-classes, when just a color, return the default value to caller
    def findTexture(self, input, textureType):
        from .tex_image import TextureImageBJSNode
        from .tex_environment import TextureEnvironmentBJSNode
        from .unsupported import UnsupportedNode

        # looking for a Image Texture or Environment Node
        if isinstance(input, TextureImageBJSNode) or isinstance(input, TextureEnvironmentBJSNode):
            if input.unAssignedBjsTexture is not None:
                bjsImageTexture = input.unAssignedBjsTexture
                bjsImageTexture.assignChannel(textureType) # add channel directly to texture, so it also knows what type it is

                # add texture to nodes dictionary, for bubbling up which reduces multiples
                self.bjsTextures[textureType] = bjsImageTexture
            return None

        # when a link of an un-expected type was assigned, need to bake (probably bubbled up already)
        elif isinstance(input, AbstractBJSNode):
            self.mustBake = True
            # unsupported types already logged warning, but not counting on some other thing, technically supported, but wrong not being there
            if not hasattr(input, 'loggedWarning'):
                Logger.error('un-excepted node type(' + input.bpyType +').  Cannot continue.')
            return None

        # assign a color when no image texture node assigned
        else:
            return input

# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    @staticmethod
    def readWorldNodeTree(node_tree):
        return AbstractBJSNode.readNodeTree(node_tree, 'ShaderNodeOutputWorld')

    @staticmethod
    def readLampNodeTree(node_tree):
        return AbstractBJSNode.readNodeTree(node_tree, 'ShaderNodeOutputLamp')

    @staticmethod
    def readMaterialNodeTree(node_tree):
        return AbstractBJSNode.readNodeTree(node_tree, 'ShaderNodeOutputMaterial')

    @staticmethod
    def readNodeTree(node_tree, topLevelId):
        # https://blender.stackexchange.com/questions/30328/how-to-get-the-end-node-of-a-tree-in-python
        output = None
        for node in node_tree.nodes:
            if node.bl_idname == topLevelId and node.is_active_output:
                    output = node
                    break

        if output is None:
            for node in node_tree.nodes:
                if node.bl_idname == topLevelId:
                    output = node
                    break

        if output is None:
            return None

        return AbstractBJSNode(output, topLevelId, True)
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    @staticmethod
    def GetBJSWrapperNode(bpyNode, socketName):
        from .ambient_occlusion import AmbientOcclusionBJSNode
        from .background import BackgroundBJSNode
        from .diffuse import DiffuseBJSNode
        from .emission import EmissionBJSNode
        from .fresnel import FresnelBJSNode
        from .glossy import GlossyBJSNode
        from .gltf import GltfBJSNode
        from .mapping import MappingBJSNode
        from .normal_map import NormalMapBJSNode
        from .passthru import PassThruBJSNode
        from .principled import PrincipledBJSNode
        from .refraction import RefractionBJSNode
        from .tex_coord import TextureCoordBJSNode
        from .tex_environment import TextureEnvironmentBJSNode
        from .tex_image import TextureImageBJSNode
        from .transparency import TransparentBJSNode
        from .uv_map import UVMapBJSNode
        from .unsupported import UnsupportedNode

        if AmbientOcclusionBJSNode.bpyType == bpyNode.bl_idname:
            return AmbientOcclusionBJSNode(bpyNode, socketName)

        elif BackgroundBJSNode.bpyType == bpyNode.bl_idname:
            return BackgroundBJSNode(bpyNode, socketName)

        elif DiffuseBJSNode.bpyType == bpyNode.bl_idname:
            return DiffuseBJSNode(bpyNode, socketName)

        elif EmissionBJSNode.bpyType == bpyNode.bl_idname:
            return EmissionBJSNode(bpyNode, socketName)

        elif FresnelBJSNode.bpyType == bpyNode.bl_idname:
            return FresnelBJSNode(bpyNode, socketName)

        elif GlossyBJSNode.bpyType == bpyNode.bl_idname:
            return GlossyBJSNode(bpyNode, socketName)

        elif GltfBJSNode.bpyType == bpyNode.bl_idname:
            return GltfBJSNode(bpyNode, socketName)

        elif MappingBJSNode.bpyType == bpyNode.bl_idname:
            return MappingBJSNode(bpyNode, socketName)

        elif NormalMapBJSNode.bpyType == bpyNode.bl_idname:
            return NormalMapBJSNode(bpyNode, socketName)

        elif bpyNode.bl_idname in PassThruBJSNode.PASS_THRU_SHADERS:
            return PassThruBJSNode(bpyNode, socketName)

        elif PrincipledBJSNode.bpyType == bpyNode.bl_idname:
            return PrincipledBJSNode(bpyNode, socketName)

        elif RefractionBJSNode.bpyType == bpyNode.bl_idname:
            return RefractionBJSNode(bpyNode, socketName)

        elif TextureCoordBJSNode.bpyType == bpyNode.bl_idname:
            return TextureCoordBJSNode(bpyNode, socketName)

        elif TextureEnvironmentBJSNode.bpyType == bpyNode.bl_idname:
            return TextureEnvironmentBJSNode(bpyNode, socketName)

        elif TextureImageBJSNode.bpyType == bpyNode.bl_idname:
            return TextureImageBJSNode(bpyNode, socketName)

        elif TransparentBJSNode.bpyType == bpyNode.bl_idname:
            return TransparentBJSNode(bpyNode, socketName)

        elif UVMapBJSNode.bpyType == bpyNode.bl_idname:
            return UVMapBJSNode(bpyNode, socketName)

        else:
            return UnsupportedNode(bpyNode, socketName)