from .abstract import AbstractBJSNode

#===============================================================================
class MappingBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeMapping'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        self.offset = bpyNode.translation
        self.ang    = bpyNode.rotation
        self.scale  = bpyNode.scale
