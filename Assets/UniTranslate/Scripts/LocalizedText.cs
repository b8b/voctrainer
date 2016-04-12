using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Localizes the text of NGUI <see cref="Text"/> components.
/// </summary>
[RequireComponent(typeof(Text))]
[AddComponentMenu("UniTranslate/Localized Text")]
[ExecuteInEditMode]
public class LocalizedText : LocalizedStringComponent
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
                || !Translator.StringExists(key))
                return;
        }
#endif
        text.text = Translator.Translate(key);
    }
}
