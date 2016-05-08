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
}
