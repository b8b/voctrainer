using UnityEngine;
using System.Collections;

public abstract class LocalizedComponent : MonoBehaviour
{
    [SerializeField] [TranslationKey(AlwaysFoldout = true)] protected string key;
    public string Key
    {
        get { return key; }
        set
        {
            key = value;
            UpdateTranslation();
        }
    }
    public abstract void UpdateTranslation();
}
