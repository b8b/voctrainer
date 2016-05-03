using UnityEngine;

/// <summary>
/// The base class for sprite based localizable components.
/// </summary>
public abstract class LocalizedTextureComponent : LocalizedComponent
{
    [SerializeField] [TranslationKey(typeof(Texture), AlwaysFoldout = true)] protected string key;

    /// <summary>
    /// The currently used translation key.
    /// </summary>
    /// <value>The currently used translation key.</value>
    public override string Key
    {
        get { return key; }
        set
        {
            key = value;
            UpdateTranslation();
        }
    }
}
