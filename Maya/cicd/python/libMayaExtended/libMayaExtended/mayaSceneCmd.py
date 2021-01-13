import maya.cmds as mc

def connected(src_attr, dest_attr):
    if mc.connectionInfo(dest_attr, isDestination=True):
        attr = mc.connectionInfo(dest_attr, getExactDestination=True)
        src = mc.connectionInfo(attr, sourceFromDestination=True)
        if str(src_attr) == str(src):
            return True
    return False

def connect(src_attr, dest_attr, force=True):
    if not connected(src_attr, dest_attr):
        mc.connectAttr(src_attr, dest_attr, force=force)

def disconnect(src_attr, dest_attr):
    if connected(src_attr, dest_attr):
        mc.disconnectAttr(src_attr, dest_attr)

def createAssignShader(node, type, name):
    shader = mc.shadingNode(type, asShader=True, name=name)
    sgraph = mc.sets(name='%s_SG' % name, empty=True, renderable=True, noSurfaceShader=True)   
    mc.connectAttr('%s.outColor' % shader, '%s.surfaceShader' % sgraph)
    mc.sets(node, e=True, forceElement=sgraph)
    return shader

def createTextureFile(type, path, name):
    filePathAttr = "filename" if type == "aiImage" else "fileTextureName"
    texture = mc.shadingNode(type, asTexture=True, name=name)
    mc.setAttr("%s.%s" % (texture, filePathAttr), path, type="string")
    return texture

def select(name):
    mc.select("Sphere")

def sphere(name):
    return mc.polySphere(name=name)[0]

def loadPlugin(plugin):
    if not mc.pluginInfo(plugin, query=True, loaded=True):
        mc.loadPlugin(plugin, quiet=True)