using BabylonExport.Entities;
using System.Collections.Generic;

public class MaterialDuplicationData
{
    public List<BabylonMesh> meshesOpaque = new List<BabylonMesh>();
    public List<BabylonMesh> meshesTransparent = new List<BabylonMesh>();
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
