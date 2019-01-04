from .abstract import *

from mathutils import Color

#===============================================================================
class PrincipledBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfPrincipled'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        input = self.findInput('Base Color')
        defaultDiffuse = self.findTexture(input, DIFFUSE_TEX)
        if defaultDiffuse is not None:
            self.diffuseColor = Color((defaultDiffuse[0], defaultDiffuse[1], defaultDiffuse[2]))
            self.diffuseAlpha = defaultDiffuse[3]

        self.mustBakeDiffuse = input.mustBake if isinstance(input, AbstractBJSNode) else False
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Metallic')
        defaultMetallic = self.findTexture(input, METAL_TEX)
        if defaultMetallic is not None:
            self.metallic = defaultMetallic

        self.mustBakeMetal = input.mustBake if isinstance(input, AbstractBJSNode) else False
       # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Specular')
        defaultSpecular = self.findTexture(input, SPECULAR_TEX)
        if defaultSpecular is not None:
            self.specularColor = Color((defaultSpecular, defaultSpecular, defaultSpecular))

        self.mustBakeSpecular = input.mustBake if isinstance(input, AbstractBJSNode) else False
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Roughness')
        defaultRoughness = self.findTexture(input, ROUGHNESS_TEX)
        if defaultRoughness is not None:
            self.roughness = defaultRoughness
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('IOR')
        defaultIOR = self.findTexture(input, REFRACTION_TEX)
        if defaultIOR is not None:
            self.indexOfRefraction = defaultIOR

        self.mustBakeRefraction = input.mustBake if isinstance(input, AbstractBJSNode) else False
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        input = self.findInput('Normal')
        self.findTexture(input, BUMP_TEX)
        self.mustBakeNormal = input.mustBake if isinstance(input, AbstractBJSNode) else False
