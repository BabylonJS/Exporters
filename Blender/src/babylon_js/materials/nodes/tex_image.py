from .abstract import AbstractBJSNode
from ..texture import *

#===============================================================================
class TextureImageBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeTexImage'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        # extract node specific (non-input) values
        self.image = bpyNode.image
        self.alpha = bpyNode.image.file_format not in NON_ALPHA_FORMATS
        self.coordinatesMode = CUBIC_MODE if bpyNode.interpolation == 'CUBE' else EXPLICIT_MODE
        if bpyNode.extension == 'REPEAT':
            # mirror not in an imageNode, but InvertNode, baking image rather than setting mirror address mode
            self.wrap = WRAP_ADDRESSMODE
        else:
            self.wrap = CLAMP_ADDRESSMODE

        if self.image is not None:
            # Going to a different class, since many inputs, including inputs to this node.
            # Plus there can also be a BakedTexture class sharing a super with this
            self.unAssignedBjsTexture = BJSImageTexture(self)