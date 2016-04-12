using UnityEngine;
using System.Collections;

/// <summary>
/// The base class for all localizable components, like <see cref="LocalizedText"/> or <see cref="LocalizedTextMesh"/>.
/// </summary>
public abstract class LocalizedComponent : MonoBehaviour
{

    /// <summary>
    /// The currently used translation key.
    /// </summary>
    /// <value>The currently used translation key.</value>
    public abstract string Key { get; set; }

    /// <summary>
    /// Updates the visual representation of the value of the translation key.
    /// </summary>
    public abstract void UpdateTranslation();
}
