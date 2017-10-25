using System.Linq;
using UnityEngine;

public class CurveValueData {

    public Quaternion values;
    public Quaternion inTangent;
    public Quaternion outTangent;

    public float[] getValuesArray()
    {
        var data = new[] {
            values.x,
            values.y,
            values.z,
            values.w
        };
        return data;
    }

    public float[] getInTangentArray()
    {
        var data = new[] {
            inTangent.x,
            inTangent.y,
            inTangent.z,
            inTangent.w
        };
        return data;
    }

    public float[] getOutTangentArray()
    {
        var data = new[] {
            outTangent.x,
            outTangent.y,
            outTangent.z,
            outTangent.w
        };
        return data;
    }

    public float[] getValuesAndTowTangentArray()
    {
        float[] data = getValuesArray();
        data = data.Concat(getInTangentArray()).ToArray();
        data = data.Concat(getOutTangentArray()).ToArray();    
        return data;
    }
}
