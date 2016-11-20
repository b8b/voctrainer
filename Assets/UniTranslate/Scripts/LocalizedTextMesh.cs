﻿using UnityEngine;

/// <summary>
/// Localizes the text of 3D <see cref="TextMesh"/> components.
/// </summary>
[RequireComponent(typeof(TextMesh))]
[AddComponentMenu("UniTranslate/Localized TextMesh")]
[ExecuteInEditMode]
public class LocalizedTextMesh : LocalizedStringComponent
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
                || !Translator.StringExists(key))
                return;
        }
#endif
        textMesh.text = Translator.Translate(key);
    }
    
    //used for internal purposes
    public override string TextValue
    {
        get
        {
            textMesh = GetComponent<TextMesh>(); //Null reference fix
            return textMesh.text;
        }
        set
        {
            textMesh = GetComponent<TextMesh>(); //Null reference fix
            textMesh.text = value;
        }
    }
}