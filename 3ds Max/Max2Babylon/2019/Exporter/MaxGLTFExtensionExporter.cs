using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GltfExport.Entities;
using GLTFExport.Entities;
using Max2Babylon;
using Utilities;

internal class MaxGLTFExtensionExporter : IGLTFExtension
{
    public static List<Type> extendedBabylonType;

    #region Implementation of IEnumerable

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Implementation of ICollection<KeyValuePair<string,object>>

    public void Add(KeyValuePair<string, object> item)
    {
    }

    public void Clear()
    {
    }

    public bool Contains(KeyValuePair<string, object> item)
    {
        return false;
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
    }

    public bool Remove(KeyValuePair<string, object> item)
    {
        return false;
    }

    public int Count { get; }
    public bool IsReadOnly { get; }

    #endregion

    #region Implementation of IDictionary<string,object>

    public bool ContainsKey(string key)
    {
        return false;
    }

    public void Add(string key, object value)
    {
    }

    public bool Remove(string key)
    {
        return false;
    }

    public bool TryGetValue(string key, out object value)
    {
        value = null;
        return false;
    }

    public object this[string key]
    {
        get => null;
        set { }
    }

    public ICollection<string> Keys { get; }
    public ICollection<object> Values { get; }

    #endregion
}
