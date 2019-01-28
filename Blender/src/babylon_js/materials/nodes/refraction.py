from .abstract import AbstractBJSNode, REFRACTION_TEX

#===============================================================================
class RefractionBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeBsdfRefraction'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        input = self.findInput('Color')
        self.findTexture(input, REFRACTION_TEX)
        self.indexOfRefraction = self.findInput('IOR')