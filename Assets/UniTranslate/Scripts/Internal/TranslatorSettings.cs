using UnityEngine;

/// <summary>
/// The internally used settings asset for the <see cref="Translator"/>. Must be called "TranslatorSettings" and placed in the Resources folder.
/// </summary>
[CreateAssetMenu(fileName = "TranslatorSettings", menuName = "Translator Settings (Resource)", order = 100001)]
public class TranslatorSettings : ScriptableObject
{
    [SerializeField] private TranslationAsset startupLanguage;

    public TranslationAsset StartupLanguage
    {
        get { return startupLanguage; }
        set { startupLanguage = value; }
    }
}
