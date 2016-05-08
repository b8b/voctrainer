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
        //Get an array of languages from the translator
        languages = Translator.Settings.Languages;

        //Add all available languages to the dropdown
        for (int i = 0; i < languages.Length; i++)
        {
            TranslationAsset language = languages[i];
            if (language == null)
                continue;

            dropdown.options.Add(new Dropdown.OptionData(language.LanguageName));
            if (language == Translator.Instance.Translation)
            {
                //Display the currently used translation asset
                dropdown.value = i;
            }
        }

        dropdown.onValueChanged.AddListener(index =>
        {
            //Change the current language of the translator and save the code in PlayerPrefs
            TranslationAsset lang = languages[index];
            Translator.Instance.Translation = lang;
            PlayerPrefs.SetString("UniTranslate_Language", lang.LanguageCode);
            PlayerPrefs.Save();
        });

#if !UNITY_EDITOR
        LoadSavedLanguage();
#endif
        dropdown.RefreshShownValue();
    }
    
    private void LoadSavedLanguage()
    {
        //Try to find a language preset in PlayerPrefs
        string langCode = PlayerPrefs.GetString("UniTranslate_Language", "");
        if (string.IsNullOrEmpty(langCode))
            return;
        
        //Search for a translation asset with this language code
        for (int i = 0; i < languages.Length; i++)
        {
            TranslationAsset language = languages[i];
            if (language.LanguageCode == langCode)
            {
                //Change the current language of the translator to the found asset.
                Translator.Instance.Translation = language;
                dropdown.value = i;
                return;
            }
        }
    }
}
