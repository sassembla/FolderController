using System.Collections.Generic;
using System.IO;
 
using UnityEngine;
 
using UnityEditor;
 
using Object = UnityEngine.Object;
 
public class MyAssetFolderInspector : ObjectInspector {
    // この部分を動的に変更するような仕掛けがあればいい。例えばフォルダ名から撮って来て保存してもいいわけで。
    override public bool IsValid() { return HasGUID("7cef9d3dff1914f83a031457c69f67eb"); }
 
    private MyAsset[] assets;
    private SerializedObject[] serializedObjects;
 
    private SerializedProperty[] fileNameProps;
    private SerializedProperty[] descriptionProps;
 
    private GUIContent descriptionGUIContent;
    private GUIContent fileNameGUIContent;
 
    override public void OnEnable() {
        Read();
    }
 
    private void Read() {
        var assetList = new List<MyAsset>();
        foreach (string f in Directory.GetFiles(AssetDatabase.GetAssetPath(target), "*.asset")) {
            var o = (MyAsset) AssetDatabase.LoadAssetAtPath(f, typeof(MyAsset));
            if (o != null) assetList.Add(o);
        }
        assets = assetList.ToArray();
 
        serializedObjects = new SerializedObject[assets.Length];
 
        fileNameProps     = new SerializedProperty[assets.Length];
        descriptionProps  = new SerializedProperty[assets.Length];
 
        fileNameGUIContent    = new GUIContent("File Name");
        descriptionGUIContent = new GUIContent("Description");
 
        for (int i = 0; i < assets.Length; i++) {
            var s = serializedObjects[i] = new SerializedObject(assets[i]);
            fileNameProps[i]    = s.FindProperty("m_Name");
            descriptionProps[i] = s.FindProperty("description");
        }
    }
    
    /*
        GUI
     */
    override public void OnInspectorGUI() {
        var singleHeight = EditorGUIUtility.singleLineHeight;
        var unitHeight = singleHeight * 5 + 5f;
        var controlRect = EditorGUILayout.GetControlRect(false, unitHeight * assets.Length);
        controlRect.height = unitHeight;
        var labelWidth = 100f;
        var otherWidth = controlRect.width - labelWidth;
        var noGUIContent = GUIContent.none;
        var fileMoved = false;
 
        for (int i = 0; i < assets.Length; i++) {
            serializedObjects[i].Update();
            var fileNameLabelTotalRect = new Rect(controlRect.x, controlRect.y, labelWidth + otherWidth, singleHeight);
            var fileNameLabelRect      = new Rect(controlRect.x, controlRect.y, labelWidth             , singleHeight);
 
            var descriptionLabelTotalRect = new Rect(controlRect.x, controlRect.y + singleHeight, labelWidth+otherWidth, singleHeight);
            var descriptionLabelRect      = new Rect(controlRect.x, controlRect.y + singleHeight, labelWidth, singleHeight);
 
            var fileNameRect    = new Rect(controlRect.x + labelWidth,              controlRect.y               , otherWidth,   singleHeight);
            var descriptionRect = new Rect(controlRect.x + labelWidth,              controlRect.y + singleHeight, otherWidth, 4*singleHeight);
 
            var selectRect = new Rect(controlRect.x, controlRect.y+unitHeight-singleHeight-8, labelWidth, singleHeight+2);
 
            EditorGUI.HandlePrefixLabel(fileNameLabelTotalRect, fileNameLabelRect, fileNameGUIContent);
            EditorGUI.BeginChangeCheck();
            var newFileName = EditorGUI.DelayedTextField(fileNameRect, noGUIContent, fileNameProps[i].stringValue);
            if (EditorGUI.EndChangeCheck()) {
                fileNameProps[i].stringValue = newFileName;
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(assets[i]), newFileName);
                fileMoved = true;
            }
 
            EditorGUI.HandlePrefixLabel(descriptionLabelTotalRect, descriptionLabelRect, descriptionGUIContent);
            EditorGUI.PropertyField(descriptionRect, descriptionProps[i], noGUIContent);
 
            if (GUI.Button(selectRect, "Ping")) {
                EditorGUIUtility.PingObject(serializedObjects[i].targetObject);
            }
 
            serializedObjects[i].ApplyModifiedProperties();
 
            controlRect.y += unitHeight;
        }
 
        if (GUILayout.Button("Add")) {
            var newInstance = ScriptableObject.CreateInstance<MyAsset>();
            var standardPath = Path.Combine( AssetDatabase.GetAssetPath(target), "MyAsset 1.asset" );
            var newPath      = AssetDatabase.GenerateUniqueAssetPath(standardPath);
            AssetDatabase.CreateAsset(newInstance, newPath);
            Read();
        } else if (fileMoved) {
            Read();
        }
    }
}