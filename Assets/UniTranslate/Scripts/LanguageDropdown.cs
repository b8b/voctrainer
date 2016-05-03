using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UniTranslate/Language Dropdown")]
[RequireComponent(typeof(Dropdown))]
public class LanguageDropdown : MonoBehaviour
{
    private Dropdown dropdown;
    private TranslationAsset[] languages;

    private void Awake()
    {
        dropdown = GetComponent<Dropdown>();
    }

    private void Start()
    {
        languages = Translator.Settings.Languages;
        for (int i = 0; i < languages.Length; i++)
        {
            TranslationAsset language = languages[i];
            if (language == null)
                continue;

            dropdown.options.Add(new Dropdown.OptionData(language.LanguageName));
            if (language == Translator.Instance.Translation)
            {
                dropdown.value = i;
            }
        }

        dropdown.onValueChanged.AddListener(index =>
        {
            TranslationAsset lang = languages[index];
            Translator.Instance.Translation = lang;
            PlayerPrefs.SetString("UniTranslate_Language", lang.LanguageCode);
        });

#if !UNITY_EDITOR
        LoadSavedLanguage();
#endif
        dropdown.RefreshShownValue();
    }
    
    private void LoadSavedLanguage()
    {
        string langCode = PlayerPrefs.GetString("UniTranslate_Language", "");
        if (string.IsNullOrEmpty(langCode))
            return;
        
        for (int i = 0; i < languages.Length; i++)
        {
            TranslationAsset language = languages[i];
            if (language.LanguageCode == langCode)
            {
                Translator.Instance.Translation = language;
                dropdown.value = i;
                return;
            }
        }
    }
}
