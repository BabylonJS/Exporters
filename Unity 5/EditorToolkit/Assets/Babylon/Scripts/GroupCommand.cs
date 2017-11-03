using UnityEditor;
using UnityEngine;

public static class GroupCommand
{
    [MenuItem("GameObject/Group Selected %g", false, 9999)]
    private static void GroupSelected()
    {
        if (!Selection.activeTransform) return;
        var go = new GameObject(Selection.activeTransform.name + " Group");
        Undo.RegisterCreatedObjectUndo(go, "Group Selected");
        go.transform.SetParent(Selection.activeTransform.parent, false);
        foreach (var transform in Selection.transforms) Undo.SetTransformParent(transform, go.transform, "Group Selected");
        Selection.activeGameObject = go;
    }
    
    public static Transform root (this Transform tform, bool ignoreFolderTransform)
    {
        
        if(tform.parent == null)
            return null;
        Transform temp = tform.parent.root(true);
        if( temp == null)
            temp = tform;
        return temp;
    }    
}