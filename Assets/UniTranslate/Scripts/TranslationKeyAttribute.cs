using UnityEngine;
using System.Collections;

/// <summary>
/// Enables in-component validation and editing in the Unity editor
/// </summary>
public class TranslationKeyAttribute : PropertyAttribute
{
    /// <summary>
    /// If set to true, the entire key editing UI will always be visible and cannot be
    /// hidden in the inspector.
    /// </summary>
    public bool AlwaysFoldout { get; set; }
}
