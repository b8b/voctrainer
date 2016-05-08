using System;
using UnityEngine;

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

    private readonly Type translationType;

    public Type TranslationType
    {
        get { return translationType; }
    }

    /// <summary>
    /// Default constructor for string keys.
    /// </summary>
    public TranslationKeyAttribute()
    {
        this.translationType = typeof (string);
    }

    /// <summary>
    /// Alternate constructor which allows editing keys for different types than strings, like sprites.
    /// </summary>
    /// <param name="translationType">The type of object you want to translate with the specified key. Can be typeof(string) or typeof(Sprite).</param>
    public TranslationKeyAttribute(Type translationType)
    {
        this.translationType = translationType;
    }
}
