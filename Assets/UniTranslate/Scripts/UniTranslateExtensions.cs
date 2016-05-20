using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class UniTranslateExtensions
{
    /// <summary>
    /// Translates the key by performing a lookup in the currently assigned <see cref="TranslationAsset"/>.
    /// Returns the key string itself if the key does not exist in the active <see cref="TranslationAsset"/>. 
    /// </summary>
    /// <param name="key">The translation key string.</param>
    /// <returns>The translated value assigned to the given key or the key string itself 
    /// if the key does not exist in the active <see cref="TranslationAsset"/>.</returns>
    public static string TranslateKey(this string key)
    {
        return Translator.Translate(key);
    }

    /// <summary>
    /// Translates the key by performing a lookup in the currently assigned <see cref="TranslationAsset"/>. 
    /// Returns the key string itself if the key does not exist in the active <see cref="TranslationAsset"/>. 
    /// <para />
    /// The parameter "replacementTokens" can contain properties you want to replace with dynamic values.
    /// If you want to replace tokens in your translation string dynamically, you can put those values in {curly brackets}
    /// and pass an anonymous object with the desired values:<para></para>
    ///Translator.Translate("Test.XYZ", new {val1 = "Hello", val2 = "World"});
    ///turns "{val1}, {val2}!" into "Hello, World!" if "{val1}, {val2}!" is assigned to the key "Text.XYZ".
    /// </summary>
    /// <param name="key">The translation key string.</param>
    /// <param name="replacementTokens">An object with properties which should be replaced with their assigned values.</param>
    /// <returns>The translated value assigned to the given key or the key string itself 
    /// if the key does not exist in the active <see cref="TranslationAsset"/>.</returns>
    public static string TranslateKey(this string key, object replacementTokens)
    {
        return Translator.Translate(key, replacementTokens);
    }

    /// <summary>
    /// Checks if a specified key exists in the active <see cref="TranslationAsset"/>.
    /// </summary>
    /// <param name="key">The key to locate in the active <see cref="TranslationAsset"/>.</param>
    /// <returns>true if the active <see cref="TranslationAsset"/> contains an element with the specified key; otherwise, false.</returns>
    public static bool TranslationKeyExists(this string key)
    {
        return Translator.StringExists(key);
    }

    internal static string FlipString(string str)
    {
        string[] lineSplit = str.Split('\n');
        StringBuilder builder = new StringBuilder(str.Length);
        foreach (var line in lineSplit)
        {
            char[] lineArr = line.ToCharArray();
            Array.Reverse(lineArr);
            builder.AppendLine(new string(lineArr));
        }
        return builder.ToString();
    }

#if UNITY_5_2
    //Implementation of Dropdown.RefreshShownValue for Unity 5.2
    //Based on https://bitbucket.org/Unity-Technologies/ui/src/2ab730c794ce2278a12285578e0154028bdb68c6/UnityEngine.UI/UI/Core/Dropdown.cs?at=5.3&fileviewer=file-view-default
    public static void RefreshShownValue(this Dropdown dropdown)
    {
        Dropdown.OptionData data = new Dropdown.OptionData();

        if (dropdown.options.Count > 0)
            data = dropdown.options[Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1)];

        if (dropdown.captionText)
        {
            if (data != null && data.text != null)
                dropdown.captionText.text = data.text;
            else
                dropdown.captionText.text = "";
        }

        if (dropdown.captionImage)
        {
            if (data != null)
                dropdown.captionImage.sprite = data.image;
            else
                dropdown.captionImage.sprite = null;
            dropdown.captionImage.enabled = (dropdown.captionImage.sprite != null);
        }
    }
#endif
}

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
//Implementation of required EditorSceneManager functions for older versions
namespace UnityEditor.SceneManagement
{
    public class EditorSceneManager
    {
        public static Scene GetActiveScene()
        {
            string scenePath = EditorApplication.currentScene;
            string[] split = scenePath.Split('/');
            return new Scene {path = scenePath, name = split[split.Length - 1]};
        }
    }

    public struct Scene
    {
        public string path { get; set; }
        public string name { get; set; }
    }
}
#endif