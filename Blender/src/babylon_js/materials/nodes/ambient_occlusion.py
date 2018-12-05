from .abstract  import AbstractBJSNode, AMBIENT_TEX

from mathutils import Color

#===============================================================================
class AmbientOcclusionBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeAmbientOcclusion'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        defaultColor = self.findTextureInput(AMBIENT_TEX)
        if defaultColor is not None:
            self.ambientColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))

        self.mustBakeAmbient = self.mustBake