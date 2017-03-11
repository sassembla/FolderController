using System;
using System.Reflection;
 
using UnityEngine;
 
using UnityEditor;
 
using Object = UnityEngine.Object;
 
[CustomEditor(typeof(Object), true)]
public class ObjectInspectorInitiator : Editor {
    private ObjectInspector inspectorObject;
    private static object[] noArgs = new object[0];
 
    private void OnEnable() {
        inspectorObject = null;
        try {
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (typeof(ObjectInspector).IsAssignableFrom(t) && !t.IsAbstract) {
                    var constructorInfo = t.GetConstructor(Type.EmptyTypes);
                    inspectorObject = (ObjectInspector) constructorInfo.Invoke(noArgs);
                    var isValidInfo  = t.GetMethod("IsValid",  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var targetInfo   = t.GetField("target",  BindingFlags.Instance | BindingFlags.NonPublic);
                    if (targetInfo != null && isValidInfo != null) {
                        targetInfo.SetValue(inspectorObject, target);
                        if ((bool)isValidInfo.Invoke(inspectorObject, noArgs))
                            t.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(inspectorObject, noArgs);
                        else
                            inspectorObject = null;
                    } else inspectorObject = null;
                    break;
                }
            }
        } catch (Exception) {
            inspectorObject = null;
        }
    }
 
    public override void OnInspectorGUI() {
        if (inspectorObject != null) {
            var prevEnabledGUI = GUI.enabled;
            GUI.enabled = true;
            try {
                inspectorObject.OnInspectorGUI();
            } finally {
                GUI.enabled = prevEnabledGUI;
            }
        } else {
            DrawDefaultInspector();
        }
    }
}