using UnityEngine;

/// <summary>
/// The base class for sprite based localizable components.
/// </summary>
public abstract class LocalizedStringComponent : LocalizedComponent
{
    [SerializeField] [TranslationKey(typeof(string), AlwaysFoldout = true)] protected string key;

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

#if UNITY_EDITOR
    public abstract string TextValue { get; set; }
#endif
}
