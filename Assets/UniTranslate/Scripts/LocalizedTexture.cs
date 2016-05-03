#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Localizes the main texture of <see cref="Renderer"/> components.
/// </summary>
[RequireComponent(typeof(Renderer))]
[AddComponentMenu("UniTranslate/Localized Texture")]
[ExecuteInEditMode]
public class LocalizedTexture : LocalizedTextureComponent
{
    private Renderer rendererRef;

    private void Awake()
    {
        rendererRef = GetComponent<Renderer>();
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
            rendererRef = GetComponent<Renderer>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.TextureExists(key))
                return;

            if (rendererRef.sharedMaterial != null)
                rendererRef.sharedMaterial.mainTexture = Translator.TranslateTexture(key);
        }
        else if (Application.isPlaying)
#endif
        {
            //Automatically create a material at runtime if no material is assigned to the renderer
            rendererRef.material.mainTexture = Translator.TranslateTexture(key);
        }
    }
}
