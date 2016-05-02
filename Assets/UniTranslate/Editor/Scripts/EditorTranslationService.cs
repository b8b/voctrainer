using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.Events;

public class EditorTranslationService
{
    public const string autoLangCode = "auto";

    //[MenuItem("Tools/TestTranslationService")]
    public static void TestTranslationService()
    {
        Translate("Hallo, Welt", "de-de", "en", Debug.Log);
    }

    public static void Translate(string text, string targetLang, UnityAction<TranslationResult> callback)
    {
        Translate(text, autoLangCode, targetLang, false, callback);
    }

    public static void Translate(string text, string sourceLang, string targetLang, UnityAction<TranslationResult> callback)
    {
        Translate(text, sourceLang, targetLang, false, callback);
    }

    public static void Translate(string text, string sourceLang, string targetLang, bool silently, UnityAction<TranslationResult> callback)
    {
        string escapedText = WWW.EscapeURL(text);
        WWW req = new WWW(BuildURI(escapedText, sourceLang, targetLang));
        ContinuationManager.Add(() => req.isDone, () =>
        {
            if (!string.IsNullOrEmpty(req.error))
            {
                if (!silently)
                {
                    EditorUtility.DisplayDialog("Auto-translation failed", "An error occured: " + req.error, "OK");
                }
            }
            else
            {
                try
                {
                    var arr = JArray.Parse(req.text);
                    if (!arr[0].HasValues)
                    {
                        callback(TranslationResult.ErrorResult);
                        if (!silently)
                            Debug.LogError("Auto translator: The language code '" + sourceLang +
                                           "' of the source language is not supported by Google Translator. Please change your language code and try again. ");
                    }
                    else
                    {
                        string translated = arr[0][0][0].ToString();
                        string source = arr[0][0][1].ToString();
                        string sourceLangApi = arr[2].ToString();
                        callback(new TranslationResult(source, translated, sourceLangApi, targetLang));
                    }
                }
                catch (Exception e)
                {
                    callback(TranslationResult.ErrorResult);
                    if (!silently)
                        Debug.LogError("Auto translator exception: " + e.Message);
                }
            }
        });
    }

    private static string BuildURI(string escapedText, string sourceLang, string targetLang)
    {
        //sourcelang unknown: empty result
        //targetlang unknown: translation to english
        return "https://translate.googleapis.com/translate_a/single?client=gtx&sl=" + sourceLang + "&tl=" + targetLang + "&dt=t&q=" + escapedText;
    }

    public class TranslationResult
    {
        public static TranslationResult ErrorResult
        {
            get
            {
                return new TranslationResult(error: true);
            }
        }

        public string SourceText { get; private set; }
        public string TranslatedText { get; private set; }
        public string SourceLang { get; private set; }
        public string TargetLang { get; private set; }
        public bool Error { get; set; }

        public TranslationResult(bool error)
        {
            Error = true;
        }

        public TranslationResult(string sourceText, string translatedText, string sourceLang, string targetLang)
        {
            SourceText = sourceText;
            TranslatedText = translatedText;
            SourceLang = sourceLang;
            TargetLang = targetLang;
        }

        public override string ToString()
        {
            return TranslatedText;
        }
    }
}
