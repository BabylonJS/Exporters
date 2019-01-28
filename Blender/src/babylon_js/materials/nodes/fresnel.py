from .abstract import AbstractBJSNode

#===============================================================================
class FresnelBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeFresnel'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        self.indexOfRefraction = self.findInput('IOR')