using UnityEngine;
using System.Collections;

/// <summary>
/// The base class for sprite based localizable components.
/// </summary>
public abstract class LocalizedSpriteComponent : LocalizedComponent
{
    [SerializeField] [TranslationKey(typeof(Sprite), AlwaysFoldout = true)] protected new string key;

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
