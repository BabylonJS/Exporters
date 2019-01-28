from .abstract import AbstractBJSNode, UV_ACTIVE_TEXTURE

#===============================================================================
class TextureCoordBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeTexCoord'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        if len(bpyNode.outputs[2].links) != 0:
            self.uvMapName = UV_ACTIVE_TEXTURE