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

    [SerializeField] private string languageName;
    [SerializeField] private string languageCode;
    [SerializeField] private TranslationDictionaryType translationDictionary;

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
    /// The <see cref="SerializableDictionary{TKey,TValue}"/> which stores the translation keys and values.
    /// </summary>
    public TranslationDictionaryType TranslationDictionary
    {
        get { return translationDictionary; }
        set { translationDictionary = value; }
    }

    public override string ToString()
    {
        return languageName + " (" + languageCode + ")";
    }
}
