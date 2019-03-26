from .abstract  import AbstractBJSNode, AMBIENT_TEX

from mathutils import Color

#===============================================================================
class AmbientOcclusionBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeAmbientOcclusion'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        input = self.findInput('Color')
        defaultColor = self.findTexture(input, AMBIENT_TEX)
        if defaultColor is not None:
            self.ambientColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))

        self.mustBakeAmbient = input.mustBake if isinstance(input, AbstractBJSNode) else False
