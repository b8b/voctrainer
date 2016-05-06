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
    [SerializeField] private TranslationAsset fallbackDefaultLanguage;
    [SerializeField] private string remoteManifestURL;
    [SerializeField] private int currentTranslationVersion;
    [SerializeField] private UpdateFrequency remoteUpdateFrequency;

    private TranslationAsset[] languagesCache;
    private bool languagesCached = false;

    public TranslationAsset[] Languages
    {
        get
        {
            if (languagesCached)
                return languagesCache;

            bool containsDefaultLang = fallbackDefaultLanguage != null && languageMappings.ContainsValue(fallbackDefaultLanguage);
            var languages = new TranslationAsset[containsDefaultLang ? languageMappings.Count : languageMappings.Count + 1];
            int i = 0;
            foreach (var mapping in languageMappings)
            {
                languages[i] = mapping.Value;
                i++;
            }
            if (!containsDefaultLang)
            {
                languages[languages.Length - 1] = fallbackDefaultLanguage;
            }
            languagesCache = languages;
            languagesCached = true;
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
            if (Application.isPlaying && fallbackDefaultLanguage == null)
            {
                Debug.LogError("No default langauge is specified in TranslatorSettings", this);
            }
            return fallbackDefaultLanguage;
        }
    }

    public string RemoteManifestURL
    {
        get { return remoteManifestURL; }
        set { remoteManifestURL = value; }
    }

    public int CurrentTranslationVersion
    {
        get { return currentTranslationVersion; }
    }

    public UpdateFrequency RemoteUpdateFrequency
    {
        get { return remoteUpdateFrequency; }
    }

    public enum UpdateFrequency
    {
        EveryStart,
        Daily,
        Weekly,
        Monthly
    }
}
