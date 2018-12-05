from .abstract  import AbstractBJSNode, BUMP_TEX

#===============================================================================
class NormalMapBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeNormalMap'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        self.findTextureInput(BUMP_TEX)