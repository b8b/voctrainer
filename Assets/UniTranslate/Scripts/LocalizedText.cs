using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[AddComponentMenu("UniTranslate/Localized Text")]
[ExecuteInEditMode]
public class LocalizedText : LocalizedComponent
{
    private Text text;
    
    private void Awake()
    {
        text = GetComponent<Text>();
    }
    
    private void Start()
    {
        UpdateTranslation();
    }

    public override void UpdateTranslation()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            text = GetComponent<Text>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.TranslationExists(key))
                return;
        }
#endif
        text.text = Translator.Translate(key);
    }
}
