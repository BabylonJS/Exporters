if "bpy" in locals():
    import imp
    imp.reload(abstract_material)
    imp.reload(abstract_node)
    imp.reload(baked_material)
    imp.reload(simple_material)
    imp.reload(texture)
else:
    from . import abstract_material
    from . import abstract_node
    from . import baked_material
    from . import simple_material
    from . import texture

import bpy