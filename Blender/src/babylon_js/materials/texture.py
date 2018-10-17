from babylon_js.logger import *
from babylon_js.package_level import *

import bpy
#===============================================================================
class Texture:
    def __init__(self, slot, level, textureOrImage, mesh, exporter):
        wasBaked = not hasattr(textureOrImage, 'uv_layer')
        if wasBaked:
            image = textureOrImage
            texture = None

            repeat = False
            self.hasAlpha = False
            self.coordinatesIndex = 0
        else:
            texture = textureOrImage
            image = texture.texture.image

            repeat = texture.texture.extension == 'REPEAT'
            self.hasAlpha = texture.texture.use_alpha

            usingMap = texture.uv_layer
            if len(usingMap) == 0:
                usingMap = mesh.data.uv_textures[0].name

            Logger.log('Image texture found, type:  ' + slot + ', mapped using: "' + usingMap + '"', 4)
            if mesh.data.uv_textures[0].name == usingMap:
                self.coordinatesIndex = 0
            elif mesh.data.uv_textures[1].name == usingMap:
                self.coordinatesIndex = 1
            else:
                Logger.warn('Texture is not mapped as UV or UV2, assigned 1', 5)
                self.coordinatesIndex = 0

        # always write the file out, since base64 encoding is easiest from a file
        try:
            imageFilepath = path.normpath(bpy.path.abspath(image.filepath))
            self.fileNoPath = path.basename(imageFilepath)

            internalImage = image.packed_file or wasBaked

            # when coming from either a packed image or a baked image, then save_render
            if internalImage:
                if exporter.scene.inlineTextures:
                    textureFile = path.join(exporter.textureFullPath, self.fileNoPath + 'temp')
                else:
                    textureFile = path.join(exporter.textureFullPath, self.fileNoPath)

                image.save_render(textureFile)

            # when backed by an actual file, copy to target dir, unless inlining
            else:
                textureFile = bpy.path.abspath(image.filepath)
                if not exporter.scene.inlineTextures:
                    copy(textureFile, exporter.textureFullPath)
        except:
            ex = exc_info()
            Logger.warn('Error encountered processing image file:  ' + ', Error:  '+ str(ex[1]))

        if exporter.scene.inlineTextures:
            # base64 is easiest from a file, so sometimes a temp file was made above;  need to delete those
            with open(textureFile, "rb") as image_file:
                asString = b64encode(image_file.read()).decode()
            self.encoded_URI = 'data:image/' + image.file_format + ';base64,' + asString

            if internalImage:
                remove(textureFile)

        else:
            # adjust name to reflect path
            relPath = exporter.scene.textureDir
            if len(relPath) > 0:
                if not relPath.endswith('/'): relPath += '/'
                self.fileNoPath = relPath + self.fileNoPath

        # capture texture attributes
        self.slot = slot
        self.level = level

        if (texture and texture.mapping == 'CUBE'):
            self.coordinatesMode = CUBIC_MODE
        if (texture and texture.mapping == 'SPHERE'):
            self.coordinatesMode = SPHERICAL_MODE
        else:
            self.coordinatesMode = EXPLICIT_MODE

        self.uOffset = texture.offset.x if texture else 0.0
        self.vOffset = texture.offset.y if texture else 0.0
        self.uScale  = texture.scale.x  if texture else 1.0
        self.vScale  = texture.scale.y  if texture else 1.0
        self.uAng = 0
        self.vAng = 0
        self.wAng = 0

        if (repeat):
            if (texture.texture.use_mirror_x):
                self.wrapU = MIRROR_ADDRESSMODE
            else:
                self.wrapU = WRAP_ADDRESSMODE

            if (texture.texture.use_mirror_y):
                self.wrapV = MIRROR_ADDRESSMODE
            else:
                self.wrapV = WRAP_ADDRESSMODE
        else:
            self.wrapU = CLAMP_ADDRESSMODE
            self.wrapV = CLAMP_ADDRESSMODE
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_scene_file(self, file_handler):
        file_handler.write(', \n"' + self.slot + '":{')
        write_string(file_handler, 'name', self.fileNoPath, True)

        write_float(file_handler, 'level', self.level)
        write_float(file_handler, 'hasAlpha', self.hasAlpha)
        write_int(file_handler, 'coordinatesMode', self.coordinatesMode)
        write_float(file_handler, 'uOffset', self.uOffset)
        write_float(file_handler, 'vOffset', self.vOffset)
        write_float(file_handler, 'uScale', self.uScale)
        write_float(file_handler, 'vScale', self.vScale)
        write_float(file_handler, 'uAng', self.uAng)
        write_float(file_handler, 'vAng', self.vAng)
        write_float(file_handler, 'wAng', self.wAng)
        write_int(file_handler, 'wrapU', self.wrapU)
        write_int(file_handler, 'wrapV', self.wrapV)
        write_int(file_handler, 'coordinatesIndex', self.coordinatesIndex)
        if hasattr(self,'encoded_URI'):
            write_string(file_handler, 'base64String', self.encoded_URI)
        file_handler.write('}')
