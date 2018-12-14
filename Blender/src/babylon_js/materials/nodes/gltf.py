from .abstract import AbstractBJSNode

#===============================================================================
class GltfBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeGroup'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)
        
        # gltf nodes are really groups; weakly linked by name
        if bpyNode.name.startswith('glTF Metallic Roughness'):
            self.metallicRoughness()
        elif bpyNode.name.startswith('glTF Specular Glossiness'):
            self.specularGlossiness()
        else:
            self.mustBake = True
            print('unsupported node group: ' + bpyNode.name)
#===============================================================================
    def metallicRoughness(self):
        input = self.findInput('BaseColor')
        defaultDiffuse = self.findTexture(input, DIFFUSE_TEX)
        if defaultDiffuse is not None:
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
    def specularGlossiness(self):
        print('specularGlossiness not yet supported')
        self.mustBake = True
        
