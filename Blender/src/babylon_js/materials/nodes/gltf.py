from .abstract import AbstractBJSNode
from babylon_js.logging import *

from mathutils import Color

#===============================================================================
class GltfBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeGroup'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)
        
        # gltf nodes are really groups; weakly linked by name
        if bpyNode.name.startswith('glTF '): # 'glTF Metallic Roughness' or 'glTF Specular Glossiness'
            Logger.error('Legacy glTF material encountered.  Convert to standard nodes or export using the glTF exporter')
        else:
            self.mustBake = True
            self.loggedWarning = True
            Logger.warn('unsupported node group(' + bpyNode.name +') will trigger baking', 3)
#===============================================================================
    # abandoned in place
    def metallicRoughness(self):
        input = self.bpyNode.inputs.get('BaseColor')
        defaultDiffuse = self.findTexture(input, DIFFUSE_TEX)
        if defaultDiffuse is not None:
            print(defaultDiffuse)
            self.diffuseChannelColor = Color((defaultDiffuse[0], defaultDiffuse[1], defaultDiffuse[2]))
            self.diffuseAlpha = defaultDiffuse[3]

        self.mustBakeDiffuse = input.mustBake if isinstance(input, AbstractBJSNode) else False
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('MetallicFactor')
        defaultMetallic = self.findTexture(input, METAL_TEX)
        if defaultMetallic is not None:
            self.metallic = defaultMetallic
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('RoughnessFactor')
        defaultRoughness = self.findTexture(input, ROUGHNESS_TEX)
        if defaultRoughness is not None:
            self.roughness = defaultRoughness
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Normal')
        self.findTexture(input, BUMP_TEX)
        self.mustBakeNormal = input.mustBake if isinstance(input, AbstractBJSNode) else False   
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Occlusion')
        defaultColor = self.findTexture(input, AMBIENT_TEX)
        if defaultColor is not None:
            self.ambientColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))

        self.mustBakeAmbient = input.mustBake if isinstance(input, AbstractBJSNode) else False
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Emissive')
        defaultEmissive = self.findTexture(input, EMMISIVE_TEX)
        if defaultEmissive is not None:
            self.emissiveColor = Color((defaultEmissive[0], defaultEmissive[1], defaultEmissive[2]))
            
        self.mustBakeEmissive = input.mustBake if isinstance(input, AbstractBJSNode) else False
#===============================================================================        
    # abandoned in place
    def specularGlossiness(self):
        print('specularGlossiness not yet supported')
        self.mustBake = True

