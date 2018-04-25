using BabylonExport.Entities;
using System.Collections.Generic;

public class MaterialDuplicationData
{
    public List<BabylonMesh> meshesOpaque;
    public List<BabylonMesh> meshesTransparent;
    public int nbMeshesOpaqueMulti;
    public int nbMeshesTransparentMulti;

    public string idOpaque;

    public bool isDuplicationSuccess;

    public bool isDuplicationRequired()
    {
        return isArnoldOpaque() && isArnoldTransparent();
    }

    public bool isArnoldOpaque()
    {
        return meshesOpaque.Count > 0 || nbMeshesOpaqueMulti > 0;
    }

    public bool isArnoldTransparent()
    {
        return meshesTransparent.Count > 0 || nbMeshesTransparentMulti > 0;
    }
}
