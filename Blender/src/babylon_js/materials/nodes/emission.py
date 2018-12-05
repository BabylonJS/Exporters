from .abstract  import AbstractBJSNode, EMMISIVE_TEX

from mathutils import Color

#===============================================================================
class EmissionBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeEmission'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        defaultColor = self.findTextureInput(EMMISIVE_TEX)
        if defaultColor is not None:
            self.emissiveColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))

        self.mustBakeEmissive = self.mustBake