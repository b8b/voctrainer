using UnityEngine;

/// <summary>
/// Localizes the sprite of <see cref="SpriteRenderer"/> components.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[AddComponentMenu("UniTranslate/Localized Sprite")]
[ExecuteInEditMode]
public class LocalizedSprite : LocalizedSpriteComponent
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            spriteRenderer = GetComponent<SpriteRenderer>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.SpriteExists(key))
                return;
        }
#endif
        spriteRenderer.sprite = Translator.TranslateSprite(key);
    }
}
