from .abstract import *

#===============================================================================
class PrincipledBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfPrincipled'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        defaultDiffuse = self.findTextureInput(DIFFUSE_TEX, 'Base Color')
        if defaultDiffuse is not None:
            self.diffuseChannelColor = Color((defaultDiffuse[0], defaultDiffuse[1], defaultDiffuse[2]))
            self.diffuseAlpha = defaultDiffuse[3]

        input = self.findInput('Base Color')
        self.mustBakeDiffuse = input and input.mustBake
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        defaultMetallic = self.findTextureInput(METAL_TEX, 'Metallic')
        if defaultMetallic is not None:
            self.metallic = defaultMetallic
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        defaultSpecular = self.findTextureInput(SPECULAR_TEX, 'Specular')
        if defaultSpecular is not None:
            self.specularColor = Color((defaultSpecular, defaultSpecular, defaultColor))  # for STD
            self.specular = defaultSpecular # for PBR

        input = self.findInput('Specular')
        self.mustBakeSpecular = input and input.mustBake
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        defaultRoughness = self.findTextureInput(ROUGHNESS_TEX, 'Roughness')
        if defaultRoughness is not None:
            self.roughness = defaultRoughness
        # - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        self.findTextureInput(BUMP_TEX, 'Normal')

        input = self.findInput('Normal')
        self.mustBakeNormal = input and input.mustBake
