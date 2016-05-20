using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The internally used settings asset for the <see cref="Translator"/>. Must be called "TranslatorSettings" and placed in the Resources folder.
/// </summary>
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
[CreateAssetMenu(fileName = "TranslatorSettings", menuName = "Translator Settings (Resource)", order = 100001)]
#endif
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

    /// <summary>
    /// An array of languages configured in the translator settings. It contains all the languages from the language mappings plus the fallback language.
    /// </summary>
    public TranslationAsset[] Languages
    {
        get
        {
            if (languagesCached)
                return languagesCache;

            bool containsDefaultLang = fallbackDefaultLanguage != null && languageMappings.ContainsValue(fallbackDefaultLanguage);
            var languages = new List<TranslationAsset>();

            foreach (var mapping in languageMappings)
            {
                if (mapping.Value != null)
                    languages.Add(mapping.Value);
            }
            if (!containsDefaultLang)
            {
                languages.Add(fallbackDefaultLanguage);
            }
            languagesCache = languages.ToArray();
#if !UNITY_EDITOR
            languagesCached = true; //Never cache languages in the editor
#endif
            return languagesCache;
        }
    }

    /// <summary>
    /// The <see cref="TranslationAsset"/> used at startup. Either one of the mappings or the fallback language if mapping failed.
    /// </summary>
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

    /// <summary>
    /// The URL where the manifest (a file containing the version and path of the translation data) is located.
    /// If set to null, only translation assets included in the build are used (no downloads and the cache file will be also unused).
    /// </summary>
    public string RemoteManifestURL
    {
        get { return remoteManifestURL; }
        set { remoteManifestURL = value; }
    }

    /// <summary>
    /// The translation version of the translations contained in the build. Compared against remote versions.
    /// </summary>
    public int CurrentTranslationVersion
    {
        get { return currentTranslationVersion; }
    }

    /// <summary>
    /// Defines how often UniTranslate will check for updated translations. See also <seealso cref="UpdateFrequency"/>
    /// </summary>
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
