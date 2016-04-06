using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TranslatorSettings", menuName = "Translator Settings (Resource)", order = 100001)]
public class TranslatorSettings : ScriptableObject
{
    [SerializeField] private TranslationAsset startupLanguage;

    public TranslationAsset StartupLanguage
    {
        get { return startupLanguage; }
        set { startupLanguage = value; }
    }
}
