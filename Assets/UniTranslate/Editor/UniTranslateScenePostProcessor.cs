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
        var sceneKeys = Resources.FindObjectsOfTypeAll<LocalizedComponent>().Select(text => text.Key);

        var allKeys =
            TranslationKeyDrawer.GetTranslationAssets()
            .SelectMany(asset => asset.TranslationDictionary.AsEnumerable())
            .Select(pair => pair.Key).ToArray();

        foreach (var key in sceneKeys)
        {
            if (!allKeys.Contains(key))
            {
                throw new MissingTranslationKeyException("Translation key found in scene '" + EditorSceneManager.GetActiveScene().name + "' does not exist for one or more languages:\n" + key);
            }
        }
    }

    public class MissingTranslationKeyException : Exception
    {
        public MissingTranslationKeyException(string message) : base(message)
        {
        }
    }
}
