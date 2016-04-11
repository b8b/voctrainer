using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An asset used to store information about a language and assign language-dependent values to translation keys.
/// </summary>
[CreateAssetMenu(fileName = "Translation", menuName = "Translation", order = 100000)]
public class TranslationAsset : ScriptableObject
{
    /// <summary>
    /// The serializable dictionary type used for translatable string values.
    /// </summary>
    [Serializable] public class TranslationDictionaryType : SerializableDictionary<string, string> { }

    /// <summary>
    /// The serializable dictionary type used for translatable sprites.
    /// </summary>
    [Serializable] public class SpriteDictionaryType : SerializableDictionary<string, Sprite> { }

    [SerializeField] private string languageName;
    [SerializeField] private string languageCode;
    [SerializeField] private TranslationDictionaryType translationDictionary;
    [SerializeField] private SpriteDictionaryType spriteDictionary;

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
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores the translation keys and values for string translations.
    /// </summary>
    public TranslationDictionaryType TranslationDictionary
    {
        get { return translationDictionary; }
        set { translationDictionary = value; }
    }
    
    /// <summary>
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores the translation keys and values for sprite translations.
    /// </summary>
    public SpriteDictionaryType SpriteDictionary
    {
        get { return spriteDictionary; }
        set { spriteDictionary = value; }
    }

    public override string ToString()
    {
        return languageName + " (" + languageCode + ")";
    }
}
