from .abstract  import AbstractBJSNode, SPECULAR_TEX

from mathutils import Color

#===============================================================================
class GlossyBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfGlossy'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        input = self.findInput('Color')
        defaultColor = self.findTexture(input, SPECULAR_TEX)
        if defaultColor is not None:
            self.specularColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))

        self.specularRoughness = self.findInput('Roughness')
        self.mustBakeSpecular = input.mustBake if isinstance(input, AbstractBJSNode) else False
