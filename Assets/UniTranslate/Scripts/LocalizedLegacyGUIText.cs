using UnityEngine;

/// <summary>
/// Localizes the text of legacy <see cref="GUIText"/> components.
/// </summary>
[RequireComponent(typeof(GUIText))]
[AddComponentMenu("UniTranslate/Localized Legacy GUI Text")]
[ExecuteInEditMode]
public class LocalizedLegacyGUIText : LocalizedStringComponent
{
    private GUIText text;
    
    private void Awake()
    {
        text = GetComponent<GUIText>();
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
            text = GetComponent<GUIText>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.StringExists(key))
                return;
        }
#endif
        text.text = Translator.Translate(key);
    }

#if UNITY_EDITOR
    public override string TextValue
    {
        get { return text.text; }
        set { text.text = value; }
    }
#endif
}
