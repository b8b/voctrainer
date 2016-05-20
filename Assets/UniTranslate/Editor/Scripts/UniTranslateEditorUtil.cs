using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public class UniTranslateEditorUtil : MonoBehaviour
{
    [MenuItem("Window/UniTranslate/Update Translatable Components In Scene")]
    public static void UpdateTranslatableComponentsInScene()
    {
        Translator.UpdateTranslations();
    }

    [MenuItem("Window/UniTranslate/Clear Remote Update Cache")]
    public static void ClearRemoteUpdateCache()
    {
        PlayerPrefs.DeleteKey("UniTranslate_LocalVersion");
        PlayerPrefs.DeleteKey("UniTranslate_LastUpdateCheck");
        if (File.Exists(Application.persistentDataPath + "/" + RemoteUpdate.cacheFileName))
        {
            File.Delete(Application.persistentDataPath + "/" + RemoteUpdate.cacheFileName);
        }
        Debug.Log("Cache cleared.");
    }

    [MenuItem("Window/UniTranslate/Documentation")]
    public static void OpenDocsInBrowser()
    {
        Application.OpenURL("http://skaillz.net/docs/unitranslate/");
    }

    [MenuItem("Window/UniTranslate/Online Manual (with Quick Start Guide)")]
    public static void OpenManualInBrowser()
    {
        Application.OpenURL("http://skaillz.net/unitranslate/UniTranslate%20Manual.pdf");
    }

    [MenuItem("Window/UniTranslate/Create Translation Asset", false, 100)]
    public static void CreateTranslationAsset()
    {
        CreateAsset<TranslationAsset>("Translation");
    }

    [MenuItem("Window/UniTranslate/Create Translator Settings Asset (place in Resources)", false, 100)]
    public static void CreateTranslatorSettings()
    {
        CreateAsset<TranslatorSettings>("TranslatorSettings");
    }

    public static void CreateAsset<T>(string defaultName) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + defaultName + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
