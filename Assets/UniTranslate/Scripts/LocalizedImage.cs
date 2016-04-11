using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Localizes the sprite of NGUI <see cref="Image"/> components.
/// </summary>
[RequireComponent(typeof(Image))]
[AddComponentMenu("UniTranslate/Localized Image")]
[ExecuteInEditMode]
public class LocalizedImage : LocalizedSpriteComponent
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
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
            image = GetComponent<Image>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.TranslationExists(key))
                return;
        }
#endif
        image.sprite = Translator.Translate<Sprite>(key);
    }
}
