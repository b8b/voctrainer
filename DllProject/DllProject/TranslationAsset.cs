using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Translation", menuName = "Translation", order = 100000)]
public class TranslationAsset : ScriptableObject
{
    [Serializable] public class TranslationDictionaryType : SerializableDictionary<string, string> { }

    [SerializeField] private string languageName;
    [SerializeField] private string languageCode;
    [SerializeField] private TranslationDictionaryType translationDictionary;

    public string LanguageName
    {
        get { return languageName; }
    }

    public string LanguageCode
    {
        get { return languageCode; }
    }

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
