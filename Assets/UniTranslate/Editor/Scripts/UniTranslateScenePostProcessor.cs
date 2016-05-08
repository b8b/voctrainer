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

        var assets = TranslationKeyDrawer.GetTranslationAssets();
        ProcessStringKeys(assets);
        ProcessSpriteKeys(assets);
        ProcessTextureKeys(assets);
        ProcessAudioKeys(assets);
        ProcessFontKeys(assets);
    }

    private static void ProcessStringKeys(TranslationAsset[] assets)
    {
        var sceneComponents = Resources.FindObjectsOfTypeAll<LocalizedStringComponent>();
        var allKeys = assets
                .SelectMany(asset => asset.TranslationDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void ProcessSpriteKeys(TranslationAsset[] assets)
    {
        var sceneComponents = Resources.FindObjectsOfTypeAll<LocalizedSpriteComponent>();
        var allKeys = assets
                .SelectMany(asset => asset.SpriteDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void ProcessTextureKeys(TranslationAsset[] assets)
    {
        var sceneComponents = Resources.FindObjectsOfTypeAll<LocalizedTextureComponent>();
        var allKeys = assets
                .SelectMany(asset => asset.TextureDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void ProcessAudioKeys(TranslationAsset[] assets)
    {
        var sceneComponents = Resources.FindObjectsOfTypeAll<LocalizedAudioComponent>();
        var allKeys = assets
                .SelectMany(asset => asset.AudioDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void ProcessFontKeys(TranslationAsset[] assets)
    {
        var sceneComponents = Resources.FindObjectsOfTypeAll<LocalizedFont>();
        var allKeys = assets
                .SelectMany(asset => asset.FontDictionary.AsEnumerable())
                .Select(pair => pair.Key).ToArray();

        CheckKeysInComponents(sceneComponents, allKeys);
    }

    private static void CheckKeysInComponents(LocalizedComponent[] sceneComponents, string[] allKeys)
    {
        foreach (var comp in sceneComponents)
        {
            if (comp.gameObject.scene != EditorSceneManager.GetActiveScene())
                continue;

            if (!allKeys.Contains(comp.Key))
            {
                Debug.LogWarning("Translation key found in scene '" + EditorSceneManager.GetActiveScene().name +
                                 "' " + (string.IsNullOrEmpty(comp.Key) ? "is empty" : "does not exist") +
                                 " for one or more languages:\nKey '" + comp.Key + "' on component '" + comp +
                                 "'.", comp.gameObject);
            }
        }
    }
}
