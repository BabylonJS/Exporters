from .abstract import AbstractBJSNode, OPACITY_TEX

#===============================================================================
class TransparentBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfTransparent'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        self.findTextureInput(OPACITY_TEX)