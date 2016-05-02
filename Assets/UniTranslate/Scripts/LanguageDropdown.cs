using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UniTranslate/Language Dropdown")]
[RequireComponent(typeof(Dropdown))]
public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TranslationAsset[] translationAssets;

    private Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<Dropdown>();
    }

    private void Start()
    {
        foreach (var translationAsset in translationAssets)
        {
            dropdown.options.Add(new Dropdown.OptionData(translationAsset.LanguageName));
        }
        dropdown.captionText.text = translationAssets[0].LanguageName;

        dropdown.onValueChanged.AddListener(index =>
        {
            Translator.Instance.Translation = translationAssets[index];
        });
    }
}
