from .abstract import AbstractBJSNode, DIFFUSE_TEX

from mathutils import Color

#===============================================================================
class DiffuseBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfDiffuse'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        defaultColor = self.findTextureInput(DIFFUSE_TEX)
        if defaultColor is not None:
            self.diffuseColor = Color((defaultColor[0], defaultColor[1], defaultColor[2]))
            self.diffuseAlpha = defaultColor[3]
        
        self.mustBakeDiffuse = self.mustBake