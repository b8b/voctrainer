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
    
    //used for internal purposes
    public override string TextValue
    {
        get
        {
            text = GetComponent<GUIText>(); //Null reference fix
            return text.text;
        }
        set
        {
            text = GetComponent<GUIText>(); //Null reference fix
            text.text = value;
        }
    }
}
