import maya.cmds as mc

def createAssignShader(node, type, name):
    shader = mc.shadingNode(type, asShader=True, name=name)
    sgraph = mc.sets(name='%s_SG' % name, empty=True, renderable=True, noSurfaceShader=True)   
    mc.connectAttr('%s.outColor' % shader, '%s.surfaceShader' % sgraph)
    mc.sets(node, e=True, forceElement=sgraph)

def select(name):
    mc.select("Sphere")

def sphere(name):
    return mc.polySphere(name=name)[0]

def loadPlugin(plugin):
    if not mc.pluginInfo(plugin, query=True, loaded=True):
        mc.loadPlugin(plugin)