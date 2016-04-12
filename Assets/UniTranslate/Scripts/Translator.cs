﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Translates strings with lookups in the <see cref="TranslationAsset.TranslationDictionary"/> of the attached <see cref="TranslationAsset"/>.
/// </summary>
/// <remarks>
/// An object with a <see cref="Translator"/> component is automatically instantiated when needed and intended for internal use.
/// You can also initialize it manually using the <see cref="Translator.Translate(string)"/> method if you adjust the script execution order.
/// Otherwise, it will be created at the first translation request.
/// </remarks>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[ExecuteInEditMode]
public class Translator : MonoBehaviour
{
    public const string version = "1.0";
    protected static Translator instance;

    /// <summary>
    /// The current Translator instance.
    /// </summary>
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

    /// <summary>
    /// The current <see cref="TranslatorSettings"/> asset.
    /// </summary>
    public static TranslatorSettings Settings { get; private set; }

    protected Translator() { }

    [SerializeField] private TranslationAsset translation;

    /// <summary>
    /// The currently used <see cref="TranslationAsset"/>. If set, the UpdateTranslation method of all LocalizedComponents in the current scene is called.
    /// </summary>
    public TranslationAsset Translation
    {
        get { return translation; }
        set
        {
            translation = value;
            UpdateTranslations();
        }
    }

    /// <summary>
    /// Initializes the translator.<para /> Loads the <see cref="TranslatorSettings"/> from an asset in "Resources/TranslatorSettings" 
    /// and then uses the editor translator located in Editor/Resources/EditorTranslator if the application is not running in the editor 
    /// or initializes a new <see cref="GameObject"/> with a Translator at runtime.
    /// </summary>
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
            instance = AssetDatabase.LoadAssetAtPath<Translator>("Assets/UniTranslate/Editor/EditorTranslator.prefab");
            if (instance == null)
                Debug.LogError("Could not initialize editor translator because the asset at path 'UniTranslate/Editor/EditorTranslator.prefab' was not found!");
            else
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
    /// <summary>
    /// Sets the <see cref="TranslatorSettings.StartupLanguage"/> of the <see cref="TranslatorSettings"/> asset in "Resources/TranslatorSettings" 
    /// to the <see cref="TranslationAsset"/> of the editor translator.
    /// </summary>
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

    /// <summary>
    /// Calls <see cref="LocalizedComponent.UpdateTranslation"/> method of all <see cref="LocalizedComponent"/>s in the current scene.
    /// </summary>
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

    /// <summary>
    /// Translates the key by performing a lookup in the currently assigned <see cref="TranslationAsset"/>.
    /// Returns the key string itself if the key does not exist in the active <see cref="TranslationAsset"/>. 
    /// </summary>
    /// <param name="key">The translation key string.</param>
    /// <returns>The translated value assigned to the given key or the key string itself 
    /// if the key does not exist in the active <see cref="TranslationAsset"/>.</returns>
    public string TranslateKey(string key)
	{
	    if (translation == null)
	    {
            Debug.LogWarning("Translator: Translation asset is null", gameObject);
	        return key;
	    }
	    if (string.IsNullOrEmpty(key))
	        return key;

	    return translation.TranslationDictionary[key, defaultValue: key];
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

    public Sprite TranslateSpriteKey(string key)
    {
        if (translation == null)
        {
            Debug.LogWarning("Translator: Translation asset is null", gameObject);
            return null;
        }

        if (string.IsNullOrEmpty(key))
            return null;

        return translation.SpriteDictionary[key, defaultValue: null];
    }

    /// <summary>
    /// Checks if a specified key exists in the active <see cref="TranslationAsset"/>.
    /// </summary>
    /// <param name="key">The key to locate in the active <see cref="TranslationAsset"/>.</param>
    /// <returns>true if the active <see cref="TranslationAsset"/> contains an element with the specified key; otherwise, false.</returns>
    public bool StringKeyExists(string key)
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

    public bool SpriteKeyExists(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        if (translation == null)
        {
            Debug.LogWarning("Translator: Translation asset is null", gameObject);
            return false;
        }
        return translation.SpriteDictionary.ContainsKey(key);
    }

    /// <summary>
    /// Translates the key by performing a lookup in the currently assigned <see cref="TranslationAsset"/>.
    /// Returns the key string itself if the key does not exist in the active <see cref="TranslationAsset"/>. 
    /// </summary>
    /// <param name="key">The translation key string.</param>
    /// <returns>The translated value assigned to the given key or the key string itself 
    /// if the key does not exist in the active <see cref="TranslationAsset"/>.</returns>
    public static string Translate(string key)
    {
        return CheckInstance() ? Instance.TranslateKey(key) : key;
    }
    
    public static Sprite TranslateSprite(string key)
    {
        if (!CheckInstance())
            return null;

        return Instance.TranslateSpriteKey(key);
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

    /// <summary>
    /// Checks if a specified key exists in the active <see cref="TranslationAsset"/>.
    /// </summary>
    /// <param name="key">The key to locate in the active <see cref="TranslationAsset"/>.</param>
    /// <returns>true if the active <see cref="TranslationAsset"/> contains an element with the specified key; otherwise, false.</returns>
    public static bool StringExists(string key)
    {
        return Instance.StringKeyExists(key);
    }

    public static bool SpriteExists(string key)
    {
        return Instance.SpriteKeyExists(key);
    }
}
