using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CurveUtil
{
    /**
     * return all Quatanion values and inTangent and outTangent List for one mesh
     */
    public static List<CurveValueData> GetQuatanionItems(EditorCurveBinding[] curveBindings, AnimationClip clip, string name)
    {
        // return data
        List<CurveValueData> curveValueDatas;

        var exist = curveBindings.Where(binding => binding.propertyName == "m_LocalRotation.x" && binding.path == name).ToArray().Count() > 0;
        if (exist)
        {
            var binding_x = curveBindings.Where(binding => binding.propertyName == "m_LocalRotation.x" && binding.path == name).ToArray().First();
            var binding_y = curveBindings.Where(binding => binding.propertyName == "m_LocalRotation.y" && binding.path == name).ToArray().First();
            var binding_z = curveBindings.Where(binding => binding.propertyName == "m_LocalRotation.z" && binding.path == name).ToArray().First();
            var binding_w = curveBindings.Where(binding => binding.propertyName == "m_LocalRotation.w" && binding.path == name).ToArray().First();

            // get every curve
            var curveX = AnimationUtility.GetEditorCurve(clip, binding_x);
            var curveY = AnimationUtility.GetEditorCurve(clip, binding_y);
            var curveZ = AnimationUtility.GetEditorCurve(clip, binding_z);
            var curveW = AnimationUtility.GetEditorCurve(clip, binding_w);

            // Quaternion data set
            curveValueDatas = new List<CurveValueData>();

            // tangent coefficient for unity animation curve
            var ratio = 1 / clip.frameRate;

            for (var i = 0; i < curveX.keys.Length; i++)
            {
                Keyframe xKey = curveX.keys[i];
                Keyframe yKey = curveY.keys[i];
                Keyframe zKey = curveZ.keys[i];
                Keyframe wKey = curveW.keys[i];

                Quaternion rotationQuaternion = new Quaternion(xKey.value, yKey.value, zKey.value, wKey.value);
                
                var curveValueData = new CurveValueData();
                curveValueData.values = rotationQuaternion;
                curveValueData.inTangent = new Quaternion(xKey.inTangent * ratio, yKey.inTangent * ratio, zKey.inTangent * ratio, wKey.inTangent * ratio);
                curveValueData.outTangent = new Quaternion(xKey.outTangent * ratio, yKey.outTangent * ratio, zKey.outTangent * ratio, wKey.outTangent * ratio);
                curveValueDatas.Add(curveValueData);
            }
        }
        else
        {
            curveValueDatas = null;
        }
        return curveValueDatas;
    }
}