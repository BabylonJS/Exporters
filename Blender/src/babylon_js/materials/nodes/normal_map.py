from .abstract  import AbstractBJSNode, BUMP_TEX

#===============================================================================
class NormalMapBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeNormalMap'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        input = self.findInput('Color')
        self.findTexture(input, BUMP_TEX)

        if len(bpyNode.uv_map) > 0:
            self.uvMapName = bpyNode.uv_map