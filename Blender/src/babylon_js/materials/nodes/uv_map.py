from .abstract import AbstractBJSNode

#===============================================================================
class UVMapBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeUVMap'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        if len(bpyNode.uv_map) > 0:
            self.uvMapName = bpyNode.uv_map