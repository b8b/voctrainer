using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(TranslationKeyAttribute))]
public class TranslationKeyDrawer : PropertyDrawer
{
    private bool foldOut;

    private const float fieldHeight = 17f;
    private const float headerHeight = 20f;
    private const float helpBoxHeight = 45f;
    private const float bottomMargin = 5f;
    private const float searchLeftMargin = 20f;
    private const float space = 5f;
    private const int maxShownAutoCompleteButtons = 15;

    private float currentHeight = fieldHeight;
    private float yPos;
    private TranslationAsset[] translationAssets;
    private string[] savedTranslationValues;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return currentHeight - yPos;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };
        translationAssets = GetTranslationAssets();

        if (savedTranslationValues == null || savedTranslationValues.Length != translationAssets.Length)
            savedTranslationValues = new string[translationAssets.Length];

        yPos = position.y;
        currentHeight = yPos;
        var keyAttribute = attribute as TranslationKeyAttribute;

        if (keyAttribute.AlwaysFoldout)
        {
            foldOut = true;
        }
        else
        {
            foldOut = EditorGUI.Foldout(new Rect(position.x, currentHeight, 130, fieldHeight), foldOut, GUIContent.none);
        }

        bool notFound = Translator.Instance == null || Translator.Instance.Translation == null ||
                            translationAssets == null || !Translator.TranslationExists(property.stringValue);

        EditorGUI.BeginChangeCheck();
        GUI.SetNextControlName("keyField");
        string key = EditorGUI.TextField(new Rect(position.x, currentHeight, position.width, fieldHeight), notFound ? label.text + " (Missing!)" : label.text, property.stringValue) ?? "";
        currentHeight += fieldHeight;

        key = MigrateTextToKey(property, key);
        property.stringValue = key;

        if (!foldOut)
            return;

        if (DrawErrorMessages(position)) return;

        if (Translator.Instance.Translation != null) //Translation exists
        {
            currentHeight += space;
            if (Translator.TranslationExists(key))
            {
                GUI.Label(new Rect(position.x, currentHeight, position.width, headerHeight),
                    "Edit Translations for key " + key, headingStyle);
                currentHeight += headerHeight;

                DrawEditUI(position, key, property.serializedObject.targetObject);
                DrawDeleteUI(position, key);
            }
            else //Translation does not exist
            {
                string result = DrawSearchList(position, key);
                if (result != null)
                {
                    property.stringValue = result;
                }

                if (!key.EndsWith(".") && key != string.Empty)
                {
                    GUI.Label(new Rect(position.x, currentHeight, position.width, headerHeight),
                    "New translation for key " + key, headingStyle);
                    currentHeight += headerHeight;
                    DrawAddUI(position, key);
                }
            }
        }
        currentHeight += space;

        DrawFooter(position);
        UpdateLocalizedComponent(property.serializedObject.targetObject);
    }

    private string MigrateTextToKey(SerializedProperty property, string key)
    {
        var localizedComponent = property.serializedObject.targetObject as LocalizedComponent;
        if (localizedComponent != null)
        {
            string text = "";
            if (localizedComponent is LocalizedText)
            {
                text = localizedComponent.GetComponent<Text>().text;
            }
            else if (localizedComponent is LocalizedTextMesh)
            {
                text = localizedComponent.GetComponent<TextMesh>().text;
            }
            
            if (key != text && string.IsNullOrEmpty(key))
            {
                if (GUI.GetNameOfFocusedControl() == "keyField") //Don't change it if the key field is selected
                {
                    return "";
                }

                for (int i = 0; i < savedTranslationValues.Length; i++)
                {
                    savedTranslationValues[i] = text;
                }
                key = TranslationKeyDrawer.GenerateRecommendedKey(text);
            }
        }
        return key;
    }

    private bool DrawErrorMessages(Rect position)
    {
        if (Translator.Instance == null)
        {
            EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight),
                "No translator instance was found. Please create a GameObject with a Translator component attached to it.",
                MessageType.Warning);
            currentHeight += helpBoxHeight;
            return true;
        }
        if (translationAssets == null)
        {
            EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight),
                "No translation assets were found. Please create at least one translation asset in your project assets folder.",
                MessageType.Warning);
            currentHeight += helpBoxHeight;
            return true;
        }
        return false;
    }

    private string DrawSearchList(Rect position, string query)
    {
        string[] querySplit = query.Split('.');
        var foundKeys = Translator.Instance.Translation.TranslationDictionary.Keys
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
            .OrderBy<string, string>(key => key, new TranslationKeyDrawer.DotFirstComparer())
            .Distinct()
            .ToArray();

        string selectedKey = null;
        foreach (var key in foundKeys.Take(maxShownAutoCompleteButtons))
        {
            bool button = GUI.Button(new Rect(position.x + searchLeftMargin, currentHeight, position.width - searchLeftMargin, fieldHeight), key, EditorStyles.textField);
            currentHeight += fieldHeight;
            if (button)
            {
                selectedKey = key;
                GUIUtility.keyboardControl = 0;
            }
        }
        if (foundKeys.Length > maxShownAutoCompleteButtons)
        {
            GUI.Button(new Rect(position.x + searchLeftMargin, currentHeight, position.width - searchLeftMargin, fieldHeight), "...", EditorStyles.textField);
            currentHeight += fieldHeight;
        }
        return selectedKey;
    }

    private void DrawAddUI(Rect position, string key)
    {
        for (int i = 0; i < translationAssets.Length; i++)
        {
            var translationAsset = translationAssets[i];
            if (String.IsNullOrEmpty(translationAsset.LanguageName))
            {
                EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight), "No name is set for this language. Please select that file and add a name.", MessageType.Warning);
                currentHeight += helpBoxHeight;
            }
            if (String.IsNullOrEmpty(translationAsset.LanguageCode))
            {
                EditorGUI.HelpBox(new Rect(position.x, currentHeight, position.width, helpBoxHeight), "No language code is set for this language. Please select that file and add a code.", MessageType.Warning);
                currentHeight += helpBoxHeight;
            }
            
            savedTranslationValues[i] = EditorGUI.TextField(new Rect(position.x, currentHeight, position.width, fieldHeight), translationAsset.LanguageName,
                savedTranslationValues[i]);
            currentHeight += fieldHeight;
        }

        bool button = GUI.Button(new Rect(position.x, currentHeight, position.width, fieldHeight), "Add");
        currentHeight += fieldHeight;
        if (button || (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return))
        {
            for (int i = 0; i < savedTranslationValues.Length; i++)
            {
                var translationAsset = translationAssets[i];
                translationAsset.TranslationDictionary.Add(key, savedTranslationValues[i]);
                EditorUtility.SetDirty(translationAsset);
            }
        }
    }

    private void DrawEditUI(Rect position, string key, Object targetObject)
    {
        for (int i = 0; i < translationAssets.Length; i++)
        {
            var translationAsset = translationAssets[i];
            EditorGUI.PrefixLabel(new Rect(position.x, currentHeight, position.width, fieldHeight), new GUIContent(translationAsset.LanguageName));
            currentHeight += fieldHeight;

            if (!translationAsset.TranslationDictionary.ContainsKey(key))
            {
                GUIStyle boldTextFieldStyle = new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };
                bool button = GUI.Button(new Rect(position.x, currentHeight, position.width, fieldHeight),
                        "Add missing key", boldTextFieldStyle);
                if (button)
                {
                    translationAsset.TranslationDictionary.Add(key, "");
                }
                currentHeight += fieldHeight;
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                translationAsset.TranslationDictionary[key] =
                    EditorGUI.TextArea(new Rect(position.x, currentHeight, position.width, fieldHeight*3), translationAsset.TranslationDictionary[key]);
                currentHeight += fieldHeight*3;

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(translationAsset);
                }
            }
        }
    }

    private void DrawDeleteUI(Rect position, string key)
    {
        if (GUI.Button(new Rect(position.x, currentHeight, position.width, fieldHeight), "Delete Translation"))
        {
            for (int i = 0; i < translationAssets.Length; i++)
            {
                var translationAsset = translationAssets[i];
                translationAsset.TranslationDictionary.Remove(key);
                EditorUtility.SetDirty(translationAsset);
            }
        }
        currentHeight += fieldHeight;
    }

    private void DrawFooter(Rect position)
    {
        EditorGUI.BeginChangeCheck();
        var serializedTranslator = new SerializedObject(Translator.Instance);
        EditorGUI.PropertyField(new Rect(position.x, currentHeight, position.width, fieldHeight), serializedTranslator.FindProperty("translation"), new GUIContent("Current language"));
        currentHeight += fieldHeight;
        if (EditorGUI.EndChangeCheck())
        {
            serializedTranslator.ApplyModifiedProperties();
            Translator.UpdateTranslations();
        }

        if (Translator.Instance.Translation != null)
        {
            EditorGUI.LabelField(new Rect(position.x, currentHeight, position.width, fieldHeight),
                "Current language name:",
                Translator.Instance.Translation.ToString());
            currentHeight += fieldHeight;
        }

        if (GUI.Button(new Rect(position.x, currentHeight, position.width, fieldHeight), "Edit all translations"))
        {
            TranslationWindow.ShowWindow();
        }
        currentHeight += fieldHeight + bottomMargin;
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
        string[] scenePathSplit = EditorSceneManager.GetActiveScene().path.Split('/');
        string sceneName = scenePathSplit[scenePathSplit.Length - 1].Replace(".unity", "");
        string name = sceneName + "." + pascalCase;
        if (Translator.Instance == null || Translator.Instance.Translation == null)
            return name;

        if (Translator.TranslationExists(name))
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
}
