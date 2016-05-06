using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UniTranslate/Localized Font")]
[ExecuteInEditMode]
public class LocalizedFont : LocalizedComponent
{
    [SerializeField] [TranslationKey(typeof(Font), AlwaysFoldout = true)]
    private string key;

    public override string Key
    {
        get { return key; }
        set
        {
            key = value;
            UpdateTranslation();
        }
    }

    private Text text;
    private TextMesh textMesh;
    private GUIText legacyGUIText;

    private void Awake()
    {
        InitializeFields();
    }

    private void Start()
    {
        UpdateTranslation();
    }

    private void InitializeFields()
    {
        text = GetComponent<Text>();
        textMesh = GetComponent<TextMesh>();
        legacyGUIText = GetComponent<GUIText>();
    }

    public override void UpdateTranslation()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            InitializeFields(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.FontExists(key))
                return;
        }
#endif
        Font font = Translator.TranslateFont(key);

        if (text != null)
            text.font = font;

        if (textMesh != null)
            textMesh.font = font;

        if (legacyGUIText != null)
            legacyGUIText.font = font;
    }
}
