using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(TextMesh))]
[AddComponentMenu("UniTranslate/Localized TextMesh")]
[ExecuteInEditMode]
public class LocalizedTextMesh : LocalizedComponent
{
    private TextMesh textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
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
            textMesh = GetComponent<TextMesh>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.TranslationExists(key))
                return;
        }
#endif
        textMesh.text = Translator.Translate(key);
    }
}
