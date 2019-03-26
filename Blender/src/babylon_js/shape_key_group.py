from .logging import *
from .package_level import *

import bpy
#===============================================================================
# extract data in Mesh order, no optimization from group analysis yet; mapped into a copy of position
#
class RawShapeKey:
    def __init__(self, keyBlock, group, state, keyOrderMap, basis, precision):
        self.group = group
        self.state = state
        self.precision = precision
        self.vertices = []

        retSz = len(keyOrderMap)
        for i in range(retSz):
            self.vertices.append(None)

        nDifferent = 0
        for i in range(retSz):
            pair = keyOrderMap[i]
            value = keyBlock.data[pair[0]].co
            self.vertices[pair[1]] = value
            if not same_vertex(value, basis.data[pair[0]].co, precision):
                nDifferent += 1

        # only log when groups / allowVertReduction
        if state != 'BASIS' and group is not None:
            Logger.log('shape key "' + group + '-' + state + '":  n verts different from basis: ' + str(nDifferent), 3)
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_json_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.state, True)
        write_vector_array(file_handler, 'positions', self.vertices, self.precision)
        write_int(file_handler, 'influence', 0)
        file_handler.write('}')
