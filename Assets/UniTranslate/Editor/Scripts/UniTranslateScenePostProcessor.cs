using UnityEngine;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

public class UniTranslateScenePostProcessor
{
    [PostProcessScene]
    public static void OnPostProcessScene()
    {
        if (!TranslationWindow.CheckMissingKeys)
            return;
        
        ProcessStringKeys(Resources.FindObjectsOfTypeAll<LocalizedStringComponent>());
        ProcessSpriteKeys(Resources.FindObjectsOfTypeAll<LocalizedSpriteComponent>());
    }

    private static void ProcessStringKeys(LocalizedStringComponent[] sceneComponents)
    {
        var allKeys =
            TranslationKeyDrawer.GetTranslationAssets()
                .SelectMany(asset => asset.TranslationDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void ProcessSpriteKeys(LocalizedSpriteComponent[] sceneComponents)
    {
        var allKeys =
            TranslationKeyDrawer.GetTranslationAssets()
                .SelectMany(asset => asset.SpriteDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void CheckKeysInComponents(LocalizedComponent[] sceneComponents, string[] allKeys)
    {
        foreach (var comp in sceneComponents)
        {
            if (!allKeys.Contains(comp.Key))
            {
                Debug.LogWarning("Translation key found in scene '" + EditorSceneManager.GetActiveScene().name +
                                 "' does not exist for one or more languages:\nKey '" + comp.Key + "' on component '" + comp +
                                 "'.", comp.gameObject);
            }
        }
    }
}
