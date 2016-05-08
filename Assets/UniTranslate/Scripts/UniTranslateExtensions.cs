using System;
using System.Text;

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
}
