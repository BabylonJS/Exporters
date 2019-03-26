from .abstract import AbstractBJSNode
from ..texture import BJSImageTexture

#===============================================================================
class TextureEnvironmentBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeTexEnvironment'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        # extract node specific (non-input) values
        self.image = bpyNode.image

        if self.image is not None:
            # Going to a different class, since many inputs, including inputs to this node.
            # Plus there can also be a BakedTexture class sharing a super with this
            self.unAssignedBjsTexture = BJSImageTexture(self, True)