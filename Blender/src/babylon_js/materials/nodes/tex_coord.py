from .abstract import AbstractBJSNode

#===============================================================================
class TextureCoordBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeTexCoord'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)
