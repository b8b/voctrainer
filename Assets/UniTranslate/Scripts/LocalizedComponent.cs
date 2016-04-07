using UnityEngine;
using System.Collections;

/// <summary>
/// The base class for all localizable components, like LocalizedText or LocalizedTextMesh.
/// </summary>
public abstract class LocalizedComponent : MonoBehaviour
{
    [SerializeField] [TranslationKey(AlwaysFoldout = true)] protected string key;

    /// <summary>
    /// The currently used translation key.
    /// </summary>
    /// <value>The currently used translation key.</value>
    public string Key
    {
        get { return key; }
        set
        {
            key = value;
            UpdateTranslation();
        }
    }

    /// <summary>
    /// Updates the visual representation of the value of the translation key.
    /// </summary>
    public abstract void UpdateTranslation();
}
