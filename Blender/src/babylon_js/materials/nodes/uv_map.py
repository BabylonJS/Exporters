from .abstract import AbstractBJSNode

import bpy

#===============================================================================
class UVMapBJSNode(AbstractBJSNode):
    bpyType = 'ShaderNodeUVMap'

    def __init__(self, bpyNode, socketName):
        super().__init__(bpyNode, socketName)

        self.mapName = bpyNode.uv_map
