using System;
 
using UnityEngine;
 
using UnityEditor;
 
using Object = UnityEngine.Object;
 
abstract public class ObjectInspector {
    protected Object target;
    abstract public void OnEnable();
    abstract public bool IsValid();
    abstract public void OnInspectorGUI();
 
    protected bool HasGUID(string guid) {
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target)).Equals(guid, StringComparison.Ordinal);
    }
}