
import bpy

#===============================================================================
class AbstractNode:
    
    def __init__(self, shader):
        self.mustBake = False
        self.inputs = []
        for input in shader.inputs:
            node = AbstractNode.determineNodeClass(shader)
            if node.mustBake:
                self.mustBake = True
                break
            else:
                self.inputs.append(node)
    
         # end of super class constructor, sub-class constructor now runs
                
    
    @staticmethod
    def determineNodeClass(shader):
        shaderName = shader.bl_idname
    
        if shaderName == 'ShaderNodeBsdfPrincipled':
            return BsdfPrincipled(shader)
    
        elif shaderName == 'ShaderNodeBsdfDiffuse':
            return BsdfDiffuse(shader)
    
        else:
            return MustBakeNode(shader)
#===============================================================================
class MustBakeNode(AbstractNode):
    def __init__(self, shader):
        self.mustBake = True
