using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UniTranslateEditor;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(TranslationKeyAttribute))]
public class TranslationKeyDrawer : PropertyDrawer
{
    private bool foldOut;

    public const float fieldHeight = 16f;
    private const float buttonHeight = 18f;
    private const float headerHeight = 20f;
    private const float helpBoxHeight = 45f;
    private const float bottomMargin = 5f;
    private const float searchLeftMargin = 20f;
    private const float space = 5f;
    private const int maxShownAutoCompleteButtons = 15;
    private const int maxRecommendedKeyLength = 18;

    private float currentHeight = fieldHeight;
    private float yPos;
    private TranslationAsset[] translationAssets;
    private object[] savedTranslationValues;
    private bool[] autoTranslating;
    private Texture2D translateServiceImage;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        UpdateUI(new Rect(0, 0, 0, 0), property, label, false);
        return currentHeight - yPos;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        UpdateUI(position, property, label, true);
    }

    private void UpdateUI(Rect position, SerializedProperty property, GUIContent label, bool drawing)
    {
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };
        if (translateServiceImage == null)
            translateServiceImage = UIDataHolder.GoogleTranslateIcon;

        translationAssets = GetTranslationAssets();

        if (translationAssets != null)
        {

            if (savedTranslationValues == null || savedTranslationValues.Length != translationAssets.Length)
                savedTranslationValues = new object[translationAssets.Length];

            if (autoTranslating == null || autoTranslating.Length != translationAssets.Length)
                autoTranslating = new bool[translationAssets.Length];
        }

        yPos = position.y;
        currentHeight = yPos;
        var keyAttribute = (TranslationKeyAttribute) attribute;

        if (drawing)
        {
            if (keyAttribute.AlwaysFoldout)
            {
                foldOut = true;
            }
            else
            {
                foldOut = EditorGUI.Foldout(new Rect(position.x, currentHeight, 130, fieldHeight), foldOut,
                    GUIContent.none);
            }
        }

        bool notFound = Translator.Instance == null || Translator.Instance.Translation == null ||
                            translationAssets == null || !Translator.Instance.Translation.KeyExists(keyAttribute.TranslationType, property.stringValue);

        EditorGUI.BeginChangeCheck();
        GUI.SetNextControlName("keyField");
        string key = property.stringValue;
        if (drawing) { 
            key = EditorGUI.TextField(new Rect(position.x, currentHeight, position.width, fieldHeight), notFound ? label.text + " (Missing!)" : label.text, property.stringValue) ?? "";
        }
        currentHeight += fieldHeight;

        key = MigrateTextToKey(property, key);
        property.stringValue = key;

        if (!foldOut)
            return;

        if (DrawErrorMessages(position, drawing)) return;

        if (Translator.Instance.Translation != null) //Translation exists
        {
            currentHeight += space;
            
            if (Translator.Instance.Translation.KeyExists(keyAttribute.TranslationType, key))
            {
                if (drawing)
                {
                    GUI.Label(new Rect(position.x, currentHeight, position.width, headerHeight),
                        "Edit Translations for key " + key, headingStyle);
                }
                currentHeight += headerHeight;

                DrawEditUI(position, key, keyAttribute.TranslationType, drawing);
                DrawDeleteUI(position, key, keyAttribute.TranslationType, drawing);
            }
            else //Translation does not exist
            {
                string result = DrawSearchList(position, key, keyAttribute.TranslationType, drawing);
                if (result != null)
                {
                    property.stringValue = result;
                }

                if (!key.EndsWith(".") && key != String.Empty)
                {
                    if (drawing)
                    {
                        GUI.Label(new Rect(position.x, currentHeight, position.width, headerHeight),
                            "New translation for key " + key, headingStyle);
                    }
                    currentHeight += headerHeight;
                    DrawAddUI(position, key, drawing);
                }
            }
        }
        currentHeight += space;

        DrawFooter(position, drawing);
        UpdateLocalizedComponent(property.serializedObject.targetObject);
    }

    private string MigrateTextToKey(SerializedProperty property, string key)
    {
        var localizedComponent = property.serializedObject.targetObject as LocalizedStringComponent;
        if (localizedComponent != null)
        {
            string text = localizedComponent.TextValue ?? "";
            
            if (key != text && String.IsNullOrEmpty(key))
            {
                if (GUI.GetNameOfFocusedControl() == "keyField") //Don't change it if the key field is selected
                {
                    return "";
                }

                for (int i = 0; i < savedTranslationValues.Length; i++)
                {
                    savedTranslationValues[i] = text;
                    //Auto-translate
                    if (translationAssets[i] != null)
                    {
                        var index = i;
                        EditorTranslationService.Translate(text, silently: true, sourceLang: EditorTranslationService.autoLangCode,
                            targetLang: translationAssets[i].LanguageCode,
                            callback: result =>
                            {
                                if (result.Error)
                                    return;
                                savedTranslationValues[index] = result.TranslatedText;
                            });
                    }
                }
                key = TranslationKeyDrawer.GenerateRecommendedKey(text);
            }
        }
        return key;
    }

    private bool DrawErrorMessages(Rect position, bool drawing)
    {
        if (Translator.Instance == null)
        {
            if (drawing)
            {
                EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight),
                    "No translator instance was found. Please create a GameObject with a Translator component attached to it.",
                    MessageType.Warning);
            }
            currentHeight += helpBoxHeight;
            return true;
        }
        if (translationAssets == null)
        {
            if (drawing)
            {
                EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight),
                    "No translation assets were found. Please create at least one translation asset in your project assets folder.",
                    MessageType.Warning);
            }
            currentHeight += helpBoxHeight;
            return true;
        }
        return false;
    }

    private string DrawSearchList(Rect position, string query, Type searchType, bool drawing)
    {
        string[] foundKeys = DoSearch(query, Translator.Instance.Translation, searchType);

        string selectedKey = null;
        foreach (var key in foundKeys.Take(maxShownAutoCompleteButtons))
        {
            bool button = false;
            if (drawing) { 
                button = GUI.Button(new Rect(position.x + searchLeftMargin, currentHeight, position.width - searchLeftMargin, fieldHeight), key, EditorStyles.textField);
            }
            currentHeight += fieldHeight;
            if (button)
            {
                selectedKey = key;
                GUIUtility.keyboardControl = 0;
            }
        }
        if (foundKeys.Length > maxShownAutoCompleteButtons)
        {
            if (drawing) { 
                GUI.Button(new Rect(position.x + searchLeftMargin, currentHeight, position.width - searchLeftMargin, fieldHeight), "...", EditorStyles.textField);
            }
            currentHeight += fieldHeight;
        }
        return selectedKey;
    }

    private void DrawAddUI(Rect position, string key, bool drawing)
    {
        var keyAttribute = (TranslationKeyAttribute) attribute;
        for (int i = 0; i < translationAssets.Length; i++)
        {
            var translationAsset = translationAssets[i];
            if (string.IsNullOrEmpty(translationAsset.LanguageName))
            {
                if (drawing)
                {
                    EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight),
                        "No name is set for this language. Please select that file and add a name.", MessageType.Warning);
                }
                currentHeight += helpBoxHeight;
            }
            if (string.IsNullOrEmpty(translationAsset.LanguageCode))
            {
                if (drawing)
                {
                    EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight),
                        "No language code is set for this language. Please select that file and add a code.",
                        MessageType.Warning);
                }
                currentHeight += helpBoxHeight;
            }

            if (drawing)
            {
                savedTranslationValues[i] = TranslationAssetEditorExtensions.DrawTypedField(
                    keyAttribute.TranslationType,
                    new Rect(position.x, currentHeight, position.width, fieldHeight), savedTranslationValues[i],
                    translationAsset.LanguageName);
            }
            currentHeight += fieldHeight;
        }

        bool button = false;
        if (drawing)
        {
            button = GUI.Button(new Rect(position.x, currentHeight, position.width, buttonHeight), "Add");
        }
        currentHeight += buttonHeight;
        if (button || (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return))
        {
            for (int i = 0; i < savedTranslationValues.Length; i++)
            {
                var translationAsset = translationAssets[i];
                translationAsset.Add(keyAttribute.TranslationType, key, savedTranslationValues[i]);
                EditorUtility.SetDirty(translationAsset);
            }
        }
    }

    private void DrawEditUI(Rect position, string key, Type editingType, bool drawing)
    {
        for (int i = 0; i < translationAssets.Length; i++)
        {
            var translationAsset = translationAssets[i];
            if (editingType == typeof(string))
            {
                if (drawing)
                {
                    DrawAutoTranslateButton(key, translationAsset, i, position);
                }
            }
            if (editingType == typeof (string) || !translationAsset.KeyExists(editingType, key))
            {
                if (drawing)
                {
                    EditorGUI.PrefixLabel(new Rect(position.x, currentHeight, position.width, fieldHeight),
                        new GUIContent(translationAsset.LanguageName));
                }
                currentHeight += fieldHeight;
            }

            if (!translationAsset.KeyExists(editingType, key))
            {
                GUIStyle boldTextFieldStyle = new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };
                bool button = false;
                if (drawing)
                {
                    button = GUI.Button(new Rect(position.x, currentHeight, position.width, fieldHeight),
                        "Add missing key", boldTextFieldStyle);
                }
                if (button)
                {
                    translationAsset.CreateNewEmptyKey(editingType);
                }
                currentHeight += fieldHeight;
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                float editorHeight = editingType == typeof (string) ? fieldHeight*3 : fieldHeight;
                if (drawing)
                {
                    translationAsset.DrawFieldForKey(editingType,
                        new Rect(position.x, currentHeight, position.width, editorHeight),
                        key, translationAsset.LanguageName);
                }
                currentHeight += editorHeight;

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(translationAsset);
                }
            }
            GUI.enabled = true; //Reset disabled UI because of auto translation
        }
    }

    private void DrawAutoTranslateButton(string key, TranslationAsset translationAsset, int assetIndex, Rect position)
    {
        if (Translator.Settings == null)
            return;
        var currentLang = Translator.Instance.Translation;
        if (currentLang == null)
            return;
        bool translating = autoTranslating[assetIndex];
        if (translating)
            GUI.enabled = false;
        if (GUI.Button(new Rect(position.width - 15f, currentHeight, 30f, 20f), new GUIContent(translateServiceImage)))
        {
            string sourceLang = translationAsset == currentLang ? EditorTranslationService.autoLangCode : currentLang.LanguageCode;
            autoTranslating[assetIndex] = true;
            GUIUtility.keyboardControl = 0;
            EditorTranslationService.Translate(currentLang.TranslationDictionary[key, ""],
                sourceLang: sourceLang,
                targetLang: translationAsset.LanguageCode,
                callback: result =>
                {
                    autoTranslating[assetIndex] = false;
                    if (!result.Error)
                        translationAsset.TranslationDictionary[key] = result.TranslatedText;
                });
        }
    }

    private void DrawDeleteUI(Rect position, string key, Type editingType, bool drawing)
    {
        if (drawing)
        {
            if (GUI.Button(new Rect(position.x, currentHeight, position.width, buttonHeight), "Remove Key"))
            {
                for (int i = 0; i < translationAssets.Length; i++)
                {
                    var translationAsset = translationAssets[i];
                    translationAsset.RemoveIfKeyExists(editingType, key);
                    EditorUtility.SetDirty(translationAsset);
                }
            }
        }
        currentHeight += buttonHeight;
    }

    private void DrawFooter(Rect position, bool drawing)
    {
        EditorGUI.BeginChangeCheck();
        var serializedTranslator = new SerializedObject(Translator.Instance);
        if (drawing)
        {
            EditorGUI.PropertyField(new Rect(position.x, currentHeight, position.width, fieldHeight),
                serializedTranslator.FindProperty("translation"), new GUIContent("Preview language"));
        }
        currentHeight += fieldHeight;
        if (EditorGUI.EndChangeCheck())
        {
            serializedTranslator.ApplyModifiedProperties();
            Translator.UpdateTranslations();
        }

        if (Translator.Instance.Translation != null)
        {
            if (drawing)
            {
                EditorGUI.LabelField(new Rect(position.x, currentHeight, position.width, fieldHeight),
                    "Preview language name:",
                    Translator.Instance.Translation.ToString());
            }
            currentHeight += fieldHeight;
        }

        if (drawing)
        {
            if (GUI.Button(new Rect(position.x, currentHeight, position.width, buttonHeight), "Edit all translations"))
            {
                TranslationWindow.ShowWindow();
            }
        }
        currentHeight += buttonHeight + bottomMargin;
    }

    private void UpdateLocalizedComponent(Object targetObject)
    {
        EditorUtility.SetDirty(targetObject);
        var localizedComponent = targetObject as LocalizedComponent;
        if (localizedComponent != null)
        {
            localizedComponent.UpdateTranslation();
        }
    }

    public static TranslationAsset[] GetTranslationAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:TranslationAsset");
        if (guids.Length == 0)
            return null;
        TranslationAsset[] assets = new TranslationAsset[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TranslationAsset>(path);
            assets[i] = asset;
        }
        return assets;
    }

    public static string GenerateRecommendedKey(string textFieldValue)
    {
        string filtered = Regex.Replace(textFieldValue, "[^a-zA-Z0-9 ]", "");
        string pascalCase = ToPascalCase(filtered);
        if (pascalCase.Length > maxRecommendedKeyLength)
        {
            pascalCase = pascalCase.Substring(0, maxRecommendedKeyLength);
        }
        string[] scenePathSplit = EditorSceneManager.GetActiveScene().path.Split('/');
        string sceneName = scenePathSplit[scenePathSplit.Length - 1].Replace(".unity", "");
        string name = sceneName + "." + pascalCase;
        if (Translator.Instance == null || Translator.Instance.Translation == null)
            return name;

        if (Translator.StringExists(name))
        {
            return name + "_1";
        }
        return name;
    }

    public static string ToPascalCase(string str)
    {
        // If there are 0 or 1 characters, just return the string.
        if (str == null) return str;
        if (str.Length < 2) return str.ToUpper();

        // Split the string into words.
        string[] words = str.Split(
            new char[] { },
            StringSplitOptions.RemoveEmptyEntries);

        // Combine the words.
        string result = "";
        foreach (string word in words)
        {
            result +=
                word.Substring(0, 1).ToUpper() +
                word.Substring(1);
        }

        return result;
    }

    public class DotFirstComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            bool xEndsWithDot = x.EndsWith(".");
            bool yEndsWithDot = y.EndsWith(".");

            if (xEndsWithDot && yEndsWithDot) //both end with .
            {
                return String.CompareOrdinal(x, y);
            }
            else if (xEndsWithDot) //only x ends with .
            {
                return -1;
            }
            else //neither of them ends with .
            {
                return String.CompareOrdinal(x, y);
            }
        }
    }

    public static string[] DoSearch(string query, TranslationAsset asset, Type searchType)
    {
        string[] querySplit = query.Split('.');

        string[] searchResults = asset.KeysForType(searchType)
            .Where(key =>
            {
                if (String.IsNullOrEmpty(key))
                    return false;
                string[] keySplit = key.Split('.');
                if (keySplit.Length < querySplit.Length)
                    return false;
                string currentSegment = keySplit[querySplit.Length - 1];
                return key.StartsWith(query, StringComparison.CurrentCultureIgnoreCase) && currentSegment != query;
            })
            .Select(key =>
            {
                string[] keySplit = key.Split('.');
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < querySplit.Length; i++)
                {
                    builder.Append(keySplit[i]);
                    if (i < querySplit.Length - 1)
                    {
                        builder.Append(".");
                    }
                }
                return querySplit.Length == keySplit.Length ? builder.ToString() : builder.Append('.').ToString();
            })
            .OrderBy(key => key, new TranslationKeyDrawer.DotFirstComparer())
            .Distinct()
            .Take(TranslationWindow.maxShownAutoCompleteButtons)
            .ToArray();
        return searchResults;
    }

    public static void SetScriptableObjectDirty(Object targetObject)
    {
        var serializedObject = targetObject as ScriptableObject;
        if (serializedObject != null)
        {
            EditorUtility.SetDirty(serializedObject);
        }
    }
}
