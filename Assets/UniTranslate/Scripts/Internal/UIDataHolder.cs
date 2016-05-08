#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class UIDataHolder : ScriptableObject
{
    [SerializeField] private Texture2D googleTranslateIcon;
    [SerializeField] private Translator editorTranslator;

    private static UIDataHolder instance;

    private static UIDataHolder Instance
    {
        get
        {
            if (instance != null)
                return instance;

            string[] guids = AssetDatabase.FindAssets("t:" + typeof (UIDataHolder).Name);
            if (guids == null || guids.Length < 1)
                return null;

            instance = AssetDatabase.LoadAssetAtPath<UIDataHolder>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return instance;
        }
    }

    public static Texture2D GoogleTranslateIcon
    {
        get { return Instance.googleTranslateIcon; }
    }

    public static Translator EditorTranslator
    {
        get { return Instance.editorTranslator; }
    }
}
#endif