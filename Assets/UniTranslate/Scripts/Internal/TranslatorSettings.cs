using System;
using UnityEngine;

/// <summary>
/// The internally used settings asset for the <see cref="Translator"/>. Must be called "TranslatorSettings" and placed in the Resources folder.
/// </summary>
[CreateAssetMenu(fileName = "TranslatorSettings", menuName = "Translator Settings (Resource)", order = 100001)]
public class TranslatorSettings : ScriptableObject
{
    [Serializable] public class MappingDictionaryType : SerializableDictionary<SystemLanguage, TranslationAsset> { }

    [SerializeField] private MappingDictionaryType languageMappings;
    [SerializeField] private TranslationAsset defaultLanguage;

    public TranslationAsset[] Languages
    {
        get
        {
            var languages = new TranslationAsset[languageMappings.Count];
            int i = 0;
            foreach (var mapping in languageMappings)
            {
                languages[i] = mapping.Value;
                i++;
            }
            return languages;
        }
    }

    public TranslationAsset StartupLanguage
    {
        get
        {
#if !UNITY_EDITOR
            var currentLanguage = languageMappings[Application.systemLanguage, null];
            if (currentLanguage != null)
                return currentLanguage;
#endif
            if (Application.isPlaying && defaultLanguage == null)
            {
                Debug.LogError("No default langauge is specified in TranslatorSettings", this);
            }
            return defaultLanguage;
        }
    }
}
