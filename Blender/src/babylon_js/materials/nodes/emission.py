from .abstract  import AbstractBJSNode, EMMISIVE_TEX

from mathutils import Color

#===============================================================================
class EmissionBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeEmission'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        input = self.findInput('Color')
        defaultColor = self.findTexture(input, EMMISIVE_TEX)
        if defaultColor is not None:
            self.emissiveColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))

        self.emissiveIntensity = self.findInput('Strength')
        self.mustBakeEmissive = input.mustBake if isinstance(input, AbstractBJSNode) else False
