using System;
using UnityEngine;

/// <summary>
/// An asset used to store information about a language and assign language-dependent values to translation keys.
/// </summary>
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
[CreateAssetMenu(fileName = "Translation", menuName = "Translation", order = 100000)]
#endif
public class TranslationAsset : ScriptableObject
{
    #region Type declarations
    /// <summary>
    /// The serializable dictionary type used for localizable string values.
    /// </summary>
    [Serializable] public class StringDictionaryType : SerializableDictionary<string, string> { }

    /// <summary>
    /// The serializable dictionary type used for localizable <see cref="Sprite"/>s.
    /// </summary>
    [Serializable] public class SpriteDictionaryType : SerializableDictionary<string, Sprite> { }

    /// <summary>
    /// The serializable dictionary type used for localizable <see cref="Texture"/>s.
    /// </summary>
    [Serializable] public class TextureDictionaryType : SerializableDictionary<string, Texture> { }

    /// <summary>
    /// The serializable dictionary type used for localizable <see cref="AudioClip"/>s.
    /// </summary>
    [Serializable] public class AudioDictionaryType : SerializableDictionary<string, AudioClip> { }

    /// <summary>
    /// The serializable dictionary type used for localizable <see cref="Font"/>s.
    /// </summary>
    [Serializable] public class FontDictionaryType : SerializableDictionary<string, Font> { }

    /// <summary>
    /// The serializable dictionary type used for custom localizable data (see <see cref="ScriptableObject"/>).
    /// </summary>
    [Serializable] public class ScriptableObjectDictionaryType : SerializableDictionary<string, ScriptableObject> { }
    #endregion

    [SerializeField] private string languageName;
    [SerializeField] private string languageCode;
    [SerializeField] private bool rightToLeftLanguage;

    [SerializeField] private StringDictionaryType translationDictionary;
    [SerializeField] private SpriteDictionaryType spriteDictionary;
    [SerializeField] private TextureDictionaryType textureDictionary;
    [SerializeField] private AudioDictionaryType audioDictionary;
    [SerializeField] private FontDictionaryType fontDictionary;
    [SerializeField] private ScriptableObjectDictionaryType scriptableObjectDictionary;

    private StringDictionaryType cachedTranslationDictionary;

    /// <summary>
    /// The user-friendly name of the language.
    /// </summary>
    public string LanguageName
    {
        get { return languageName; }
    }

    /// <summary>
    /// A code used to identify the language internally.
    /// </summary>
    public string LanguageCode
    {
        get { return languageCode; }
    }

    /// <summary>
    /// Is the language described by the translation asset a right-to-left language (like Arabic)?
    /// </summary>
    public bool IsRightToLeftLanguage
    {
        get { return rightToLeftLanguage; }
    }

    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores translation keys and values for string localizations.
    /// </summary>
    public StringDictionaryType TranslationDictionary
    {
        get { return translationDictionary; }
        set { translationDictionary = value; }
    }

    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores translation keys and values for <see cref="Sprite"/> localizations.
    /// </summary>
    public SpriteDictionaryType SpriteDictionary
    {
        get { return spriteDictionary; }
        set { spriteDictionary = value; }
    }

    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores translation keys and values for <see cref="Texture"/> localizations.
    /// </summary>
    public TextureDictionaryType TextureDictionary
    {
        get { return textureDictionary; }
        set { textureDictionary = value; }
    }

    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores translation keys and values for <see cref="AudioClip"/> localizations.
    /// </summary>
    public AudioDictionaryType AudioDictionary
    {
        get { return audioDictionary; }
        set { audioDictionary = value; }
    }

    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores translation keys and values for <see cref="Font"/> localizations.
    /// </summary>
    public FontDictionaryType FontDictionary
    {
        get { return fontDictionary; }
        set { fontDictionary = value; }
    }

    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores translation keys and values for custom localizable data (see <see cref="ScriptableObject"/>).
    /// </summary>
    public ScriptableObjectDictionaryType ScriptableObjectDictionary
    {
        get { return scriptableObjectDictionary; }
        set { scriptableObjectDictionary = value; }
    }

    /// <summary>
    /// Used internally when working with remote updates.
    /// </summary>
    public StringDictionaryType CachedTranslationDictionary
    {
        get { return cachedTranslationDictionary; }
        set { cachedTranslationDictionary = value; }
    }

    public override string ToString()
    {
        return languageName + " (" + languageCode + ")";
    }
}
