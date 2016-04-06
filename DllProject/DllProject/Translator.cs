using System;
using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif

//[AddComponentMenu("UniTranslate/Translator")]
[ExecuteInEditMode]
public class Translator : MonoBehaviour
{
    public const string version = "1.0";
    protected static Translator instance;

    public static Translator Instance
    {
        get
        {
            if (instance == null)
            {
                Initialize();
            }

            return instance;
        }
    }

    public static TranslatorSettings Settings { get; private set; }

    protected Translator() { }

    [SerializeField] private TranslationAsset translation;

    public TranslationAsset Translation
    {
        get { return translation; }
        set
        {
            translation = value;
            UpdateTranslations();
        }
    }

    public static void Initialize()
    {
        Settings = Resources.Load<TranslatorSettings>("TranslatorSettings");
        if (Settings == null)
        {
            Debug.LogError("No TranslatorSettings asset found!");
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = Resources.Load<Translator>("EditorTranslator");
            instance.Translation = Settings.StartupLanguage;
        }
        else
#endif
        {
            var obj = new GameObject("Runtime Translator");
            instance = obj.AddComponent<Translator>();
            instance.translation = Settings.StartupLanguage;
            //Debug.Log("UniTranslate " + version + " initialized!");
        }
    }

#if UNITY_EDITOR
    public static bool UpdateStartupLanguage()
    {
        var settings = Resources.Load<TranslatorSettings>("TranslatorSettings");
        if (settings == null)
        {
            Debug.LogError("Could not save startup language - no TranslatorSettings in resource folder found!");
            return false;
        }
        settings.StartupLanguage = instance.translation;
        EditorUtility.SetDirty(settings);
        return true;
    }
#endif

    public static void UpdateTranslations()
    {
        foreach (var localizedComponent in FindObjectsOfType<LocalizedComponent>())
        {
            localizedComponent.UpdateTranslation();
        }
    }

    private void Start()
	{
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateTranslations();
        }
        else
        {
            DontDestroyOnLoad(this);
        }
#else
        DontDestroyOnLoad(this);
#endif
    }

    public string TranslateKey(string key)
	{
	    if (translation == null)
	    {
            Debug.LogWarning("Translator: Translation asset is null", gameObject);
	        return key;
	    }
	    if (string.IsNullOrEmpty(key))
	        return key;

	    return translation.TranslationDictionary[key, key];
	}

    public string TranslateKey(string key, object replacementTokens)
    {
        string translationVal = TranslateKey(key);
        Type anonType = replacementTokens.GetType();
        foreach (var propertyInfo in anonType.GetProperties())
        {
            if (!propertyInfo.CanRead)
                continue;

            translationVal = translationVal.Replace("{" + propertyInfo.Name + "}", propertyInfo.GetValue(replacementTokens, null).ToString());
        }
        return translationVal;
    }

    public bool KeyExists(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        if (translation == null)
        {
            Debug.LogWarning("Translator: Translation asset is null", gameObject);
            return false;
        }
        return translation.TranslationDictionary.ContainsKey(key);
    }

    public static string Translate(string key)
    {
        return CheckInstance() ? Instance.TranslateKey(key) : key;
    }

    public static string Translate(string key, object replacementTokens)
    {
        return CheckInstance() ? Instance.TranslateKey(key, replacementTokens) : key;
    }

    private static bool CheckInstance()
    {
        if (Instance == null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                instance = FindObjectOfType<Translator>(); //Fix for missing instance references
            }
            else
#endif
            {
                Debug.LogError("Translator: TranslationAsset is null!");
            }
            return false;
        }
        return true;
    }

    public static bool TranslationExists(string key)
    {
        return Instance.KeyExists(key);
    }
}
