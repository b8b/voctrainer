using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

public class UniTranslateScenePostProcessor
{

    [PostProcessScene]
    public static void OnPostProcessScene()
    {
        if (!TranslationWindow.CheckMissingKeys)
            return;
        var sceneComponents = Resources.FindObjectsOfTypeAll<LocalizedComponent>();

        var allKeys =
            TranslationKeyDrawer.GetTranslationAssets()
            .SelectMany(asset => asset.TranslationDictionary.AsEnumerable())
            .Select(pair => pair.Key).ToArray();
        
        foreach (var comp in sceneComponents)
        {
            if (!allKeys.Contains(comp.Key))
            {
                Debug.LogError("Translation key found in scene '" + EditorSceneManager.GetActiveScene().name +
                    "' does not exist for one or more languages:\nKey '" + comp.Key + "' on component '" + comp + "'.", comp.gameObject);
            }
        }
    }
}
