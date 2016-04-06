using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[AddComponentMenu("")] //Hide from components
[CustomEditor(typeof(Translator))]
public class TranslatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("This object is automatically instantiated when needed and intended for internal use. " +
                                "You can also initialize it manually using the Translator.Initialize() method if you adjust the script execution order. " +
                                "Otherwise, it will be created at the first translation request. " +
                                "Please do not modify this object unless you know what you're doing!", MessageType.Warning);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("translation"), new GUIContent("Current language"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var localizedText in FindObjectsOfType<LocalizedComponent>())
            {
                localizedText.UpdateTranslation();
            }
        }
    }
}