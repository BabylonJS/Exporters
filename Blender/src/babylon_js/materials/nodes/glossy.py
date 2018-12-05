from .abstract  import AbstractBJSNode, SPECULAR_TEX

from mathutils import Color

#===============================================================================
class GlossyBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfGlossy'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        defaultColor = self.findTextureInput(SPECULAR_TEX)
        if defaultColor is not None:
            self.specularColor = Color((defaultColor[0], defaultColor[1], defaultColor[2])) # for STD
            self.specular = (defaultColor[0] + defaultColor[1] + defaultColor[2]) / 3 # for PBR

        self.specularRoughness = self.findInput('Roughness')
        self.mustBakeSpecular = self.mustBake