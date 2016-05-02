using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UniTranslateEditor;

[InitializeOnLoad]
public class TranslationWindow : EditorWindow
{
    [MenuItem("Window/Translation Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<TranslationWindow>();
    }

    public static bool CheckMissingKeys { get; private set; }
    public static bool AutoTranslateOnAdd { get; private set; }

    private TranslationAsset firstAsset;
    private TranslationAsset secondAsset;
    private Vector2 scrollPos = Vector2.zero;
    private ReorderableList list;
    private Texture2D translateServiceImage;

    private string filter = "";
    private string[] searchResults;
    private bool[] autoTranslating;
    
    private static readonly Type[] translationTypes = new Type[] {typeof(string), typeof(Sprite), typeof(Texture), typeof(AudioClip), typeof(Font), typeof(ScriptableObject)};
    private static readonly string[] translationTypeToolbarStrings = new string[] {"Strings", "Sprites", "Textures", "Audio", "Fonts", "Scriptable Objects"};
    private static Type currentTranslationType = translationTypes[0];
    private static int currentTranslationTypeIndex = 0;
    
    private const float leftHeaderMargin = 13f;
    private const float twoColumnMinWidth = 600f;
    public const int maxShownAutoCompleteButtons = 15;
    public const string googleTranslateLogoPath = "Assets/UniTranslate/Editor/Images/GoogleTranslate.png";

    #region Initialization
    static TranslationWindow()
    {
        CheckMissingKeys = EditorPrefs.GetBool("TranslationWindow_CheckMissingKeys", true);
        AutoTranslateOnAdd = EditorPrefs.GetBool("TranslationWindow_AutoTranslateOnAdd", false);
        currentTranslationTypeIndex = EditorPrefs.GetInt("TranslationWindow_CurrentTranslationTypeIndex", 0);
        if (currentTranslationTypeIndex > translationTypes.Length - 1 || currentTranslationTypeIndex < 0)
            currentTranslationTypeIndex = 0;
    }

    private void OnEnable()
    {
        this.titleContent = new GUIContent("Translations");
        translateServiceImage = AssetDatabase.LoadAssetAtPath<Texture2D>(googleTranslateLogoPath);

        InitializeData();
        InitializeList();
    }

    private void InitializeData()
    {
        autoTranslating = new bool[2];
 
        //Load asset GUIDs from prefs
        try
        {
            string firstGuid = EditorPrefs.GetString("TranslationWindow_FirstAssetGuid", null);
            string secondGuid = EditorPrefs.GetString("TranslationWindow_SecondAssetGuid", null);
            if (firstGuid != null) { 
                firstAsset = (TranslationAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(firstGuid), typeof(TranslationAsset));
            }
            if (secondGuid != null) { 
                secondAsset = (TranslationAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(secondGuid), typeof(TranslationAsset));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error when loading the last used translation assets: " + e.Message);
        }
    }
#endregion

    #region List UI Events
    private void InitializeList()
    {
        list = new ReorderableList(new List<string>(), typeof (string), draggable: true,
            displayHeader: true, displayAddButton: true, displayRemoveButton: true);

        list.drawHeaderCallback = rect =>
        {
            if (string.IsNullOrEmpty(filter))
            {
                rect.x += leftHeaderMargin;
                rect.width -= leftHeaderMargin;
            }

            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width*0.2f, rect.height), "Key");
            if (firstAsset != secondAsset && secondAsset != null)
            {
                EditorGUI.LabelField(new Rect(rect.x + rect.width*0.2f, rect.y, rect.width*0.4f, rect.height),
                    firstAsset.LanguageName);
                EditorGUI.LabelField(new Rect(rect.x + rect.width*0.6f, rect.y, rect.width*0.4f, rect.height),
                    secondAsset.LanguageName);
            }
            else
            {
                EditorGUI.LabelField(new Rect(rect.x + rect.width*0.2f, rect.y, rect.width*0.8f, rect.height),
                    firstAsset.LanguageName);
            }
        };

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            try
            {
                float height = rect.height - 3;
                string currentKey = (string) list.list[index];
                string key = DrawKeyField(rect, height, currentKey);

                EditorGUI.BeginChangeCheck();

                DrawValueFields(key, rect);

                if (EditorGUI.EndChangeCheck())
                {
                    UpdateList();
                    TranslationKeyDrawer.SetScriptableObjectDirty(firstAsset);
                    TranslationKeyDrawer.SetScriptableObjectDirty(secondAsset);
                }
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        };

        list.onAddCallback = reorderableList =>
        {
            try
            {
                TranslationKeyDrawer.SetScriptableObjectDirty(firstAsset);
                firstAsset.CreateNewEmptyKey(currentTranslationType);
                if (secondAsset != firstAsset)
                {
                    secondAsset.CreateNewEmptyKey(currentTranslationType);
                    TranslationKeyDrawer.SetScriptableObjectDirty(secondAsset);
                }
                
                UpdateList();
                Repaint();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        };

        list.onRemoveCallback = reorderableList =>
        {
            RemoveSelected();
            UpdateList();
            Repaint();
        };

        list.onReorderCallback = reorderableList =>
        {
            firstAsset.ReorderDictionary(currentTranslationType, list.list);
            TranslationKeyDrawer.SetScriptableObjectDirty(firstAsset);

            if (secondAsset == null || secondAsset == firstAsset)
                return;

            secondAsset.ReorderDictionary(currentTranslationType, list.list);
            TranslationKeyDrawer.SetScriptableObjectDirty(secondAsset);
        };

        if (firstAsset != null)
            searchResults = TranslationKeyDrawer.DoSearch("", firstAsset, currentTranslationType);
    }

    private string DrawKeyField(Rect rect, float height, string oldKey)
    {
        EditorGUI.BeginChangeCheck();
        string newKey = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width*0.2f, height), oldKey);
        if (EditorGUI.EndChangeCheck())
        {
            firstAsset.ChangeKey(currentTranslationType, oldKey, newKey);
            TranslationKeyDrawer.SetScriptableObjectDirty(firstAsset);
            if (secondAsset != null)
            {
                secondAsset.ChangeKey(currentTranslationType, oldKey, newKey);
                TranslationKeyDrawer.SetScriptableObjectDirty(secondAsset);
            }
            UpdateList();
        }
        else
        {
            newKey = oldKey;
        }
        return newKey;
    }

    private void DrawValueFields(string key, Rect rect)
    {
        float y = currentTranslationType == typeof (string) ? rect.y : rect.y + 1;
        float height = currentTranslationType == typeof(string) ? rect.height - 3 : TranslationKeyDrawer.fieldHeight;
        var firstRect = new Rect(rect.x + rect.width*0.2f, y,
            firstAsset != secondAsset ? rect.width*0.4f : rect.width*0.8f, height);
        
        firstAsset.DrawFieldForKey(currentTranslationType, firstRect, key);
        if (secondAsset != null && secondAsset != firstAsset)
        {
            var secondRect = new Rect(rect.x + rect.width*0.6f, y, rect.width*0.4f, height);
            if (secondAsset.KeyExists(currentTranslationType, key))
            {
                secondAsset.DrawFieldForKey(currentTranslationType, secondRect, key);
            }
            else
            {
                DrawAddMissingButton(key, rect, height);
            }
        }
    }

    

    private void DrawAddMissingButton(string key, Rect rect, float height)
    {
        GUIStyle boldTextFieldStyle = new GUIStyle(EditorStyles.textField) {fontStyle = FontStyle.Bold};
        bool button = GUI.Button(new Rect(rect.x + rect.width*0.6f, rect.y, rect.width*0.4f, height),
            "Add missing key", boldTextFieldStyle);

        if (button)
        {
            secondAsset.Add(currentTranslationType, key);
            if (!AutoTranslateOnAdd || firstAsset == null || firstAsset.TranslationDictionary == null ||
                Translator.Settings == null || currentTranslationType != typeof(string))
                return;

            EditorTranslationService.Translate(firstAsset.TranslationDictionary[key, ""],
                firstAsset.LanguageCode, secondAsset.LanguageCode,
                silently: true, callback: result =>
                {
                    if (!result.Error)
                    {
                        secondAsset.TranslationDictionary[key] = result.TranslatedText;
                    }
                });
        }
    }
    #endregion

    private void OnGUI()
    {
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        DrawToolbar();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete && list.index >= 0)
        {
            RemoveSelected();
            Repaint();
        }

        if (position.width >= twoColumnMinWidth)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 4), GUILayout.MaxWidth(position.width / 4));
        }
        else
        {
            EditorGUILayout.BeginVertical();
        }
        EditorGUILayout.Space();
        DrawOptions();

        if (position.width >= twoColumnMinWidth)
        {
            if (list.index >= 0 && list.index < list.list.Count && firstAsset != null && currentTranslationType == typeof(string))
            {
                DrawEditUI((string)list.list[list.index], headingStyle, firstAsset,
                    secondAsset);
            }
            DrawDefaultLanguageField();
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            CustomGUI.Splitter();
        }

        EditorGUILayout.BeginVertical();
        DrawTranslationList();
        EditorGUILayout.EndVertical();

        if (position.width >= twoColumnMinWidth)
        {
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (list.index >= 0 && list.index < list.list.Count && firstAsset != null && currentTranslationType == typeof(string))
            {
                DrawEditUI((string)list.list[list.index], headingStyle, firstAsset,
                    secondAsset);
                EditorGUILayout.Space();
            }
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Import from CSV", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            string loadPath = ImportCsvWindow.ShowLoadDialog();
            if (!string.IsNullOrEmpty(loadPath))
            {
                EditorWindow.GetWindow<ImportCsvWindow>(true, "CSV Import Wizard").FileName = loadPath;
            }
        }
        if (GUILayout.Button("Export to CSV", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            string savePath = ExportCsvWindow.ShowSaveDialog();
            if (!string.IsNullOrEmpty(savePath))
            {
                EditorWindow.GetWindow<ExportCsvWindow>(true, "CSV Export Wizard").FileName = savePath;
            }
        }
        
        if (position.width < twoColumnMinWidth)
        {
            GUILayout.Box("", EditorStyles.toolbarButton);
            DrawTranslationTypeSelect();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
        }
        else
        {
            GUILayout.Box("", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        }

        EditorGUI.BeginChangeCheck();
        CheckMissingKeys = GUILayout.Toggle(CheckMissingKeys, new GUIContent(position.width >= twoColumnMinWidth ? "Enable Translation Key Postprocessor" : "Enable Postprocessor",
            "If enabled, UniTranslate will search for missing keys after a scene is loaded in the editor or processed in a build. " +
            "The post processor might affect performance in editor, but is not included in builds. "),
            EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        AutoTranslateOnAdd = GUILayout.Toggle(AutoTranslateOnAdd, new GUIContent(position.width >= twoColumnMinWidth ? "Use Google Translator to Suggest Translations" : "Suggest Translations",
            "If enabled, UniTranslate will use Google Translator to suggest translations whenever a new key is added on a text component " +
            "based on its content. Ensure that your language codes are right, so that it is recognized by Google Translator."),
            EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool("TranslationWindow_CheckMissingKeys", CheckMissingKeys);
            EditorPrefs.SetBool("TranslationWindow_AutoTranslateOnAdd", AutoTranslateOnAdd);
        }

        GUILayout.Box("", EditorStyles.toolbarButton);
        if (position.width < twoColumnMinWidth)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
        }
        else
        {
            DrawTranslationTypeSelect();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTranslationTypeSelect()
    {
        EditorGUI.BeginChangeCheck();
        currentTranslationTypeIndex = EditorGUILayout.Popup(currentTranslationTypeIndex, translationTypeToolbarStrings,
            EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
        currentTranslationType = translationTypes[currentTranslationTypeIndex];
        if (EditorGUI.EndChangeCheck())
        {
            searchResults = TranslationKeyDrawer.DoSearch("", firstAsset, currentTranslationType);
            EditorPrefs.SetInt("TranslationWindow_CurrentTranslationTypeIndex", currentTranslationTypeIndex);
        }
    }

    private void DrawOptions()
    {
        EditorGUI.BeginChangeCheck();
        firstAsset = (TranslationAsset) EditorGUILayout.ObjectField("Primary language:", firstAsset, typeof(TranslationAsset), false);
        secondAsset = (TranslationAsset) EditorGUILayout.ObjectField("Secondary language:", secondAsset, typeof(TranslationAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            try
            {
                string firstGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(firstAsset));
                string secondGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(secondAsset));
                EditorPrefs.SetString("TranslationWindow_FirstAssetGuid", firstGuid);
                EditorPrefs.SetString("TranslationWindow_SecondAssetGuid", secondGuid);
                if (firstAsset != null)
                    searchResults = TranslationKeyDrawer.DoSearch("", firstAsset, currentTranslationType);
            }
            catch (Exception e)
            {
                Debug.LogError("Error when saving the current selection in EditorPrefs: " + e.Message);
            }
        }

        if (firstAsset == null)
            return;
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        filter = EditorGUILayout.TextField("Filter", filter) ?? "";
        if (EditorGUI.EndChangeCheck())
        {
            searchResults = TranslationKeyDrawer.DoSearch(filter, firstAsset, currentTranslationType);
        }
        
        var result = DrawSearchList(filter, firstAsset);
        if (result != null)
        {
            filter = result;
        }
        
        list.draggable = string.IsNullOrEmpty(filter);
    }

    private void DrawTranslationList()
    {
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (firstAsset == null)
        {
            EditorGUILayout.HelpBox("Please select the translation asset you want to edit.", MessageType.Info);
        }
        else
        {
            if (Event.current.type != EventType.Layout)
            {
               UpdateList();
            }
            list.DoLayoutList();
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void UpdateList()
    {
        var keys = firstAsset.KeysForType(currentTranslationType);
        list.list = keys.Where(key => key.ToLower().StartsWith(filter != null ? filter.ToLower() : ""))
            .ToList();
    }

    private void DrawEditUI(string key, GUIStyle headingStyle, params TranslationAsset[] assets)
    {
        GUI.skin.label.wordWrap = true;
        EditorGUILayout.Space();
        CustomGUI.Splitter();
        
        GUILayout.Label("Edit Translations for key " + key, headingStyle, GUILayout.ExpandWidth(false));
        
        TranslationAsset previousAsset = null;
        for (int i = 0; i < assets.Length; i++)
        {
            var translationAsset = assets[i];
            if (translationAsset == previousAsset || translationAsset == null)
                continue;

            if (!translationAsset.TranslationDictionary.ContainsKey(key))
                return;
            previousAsset = translationAsset;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(translationAsset.LanguageName);
            GUILayout.FlexibleSpace();
            DrawAutoTranslateButton(key, translationAsset, i);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            if (position.width >= twoColumnMinWidth)
            {
                translationAsset.TranslationDictionary[key] =
                    EditorGUILayout.TextArea(translationAsset.TranslationDictionary[key],
                        GUILayout.ExpandHeight(true), GUILayout.MaxHeight(300f));
            }
            else
            {
                translationAsset.TranslationDictionary[key] = EditorGUILayout.TextArea(translationAsset.TranslationDictionary[key], GUILayout.MaxHeight(100f));
            }
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(translationAsset);
            }
        }

        if (GUILayout.Button("Remove Key"))
            RemoveSelected();
    }

    private void DrawAutoTranslateButton(string key, TranslationAsset translationAsset, int assetIndex)
    {
        if (Translator.Settings == null)
            return;
        var currentLang = Translator.Instance.Translation;
        if (currentLang == null)
            return;
        bool translating = autoTranslating[assetIndex];
        if (translating)
            GUI.enabled = false;
        if (GUILayout.Button(new GUIContent(translateServiceImage), GUILayout.Width(30f), GUILayout.Height(20f)))
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
                    this.Repaint();
                });
        }
    }

    private void RemoveSelected()
    {
        if (list.list.Count - 1 < list.index)
            return;
        string selectedKey = (string)list.list[list.index] ?? "";

        firstAsset.RemoveIfKeyExists(currentTranslationType, selectedKey);
        if (secondAsset != null)
        {
            secondAsset.RemoveIfKeyExists(currentTranslationType, selectedKey);
        }

        TranslationKeyDrawer.SetScriptableObjectDirty(firstAsset);
        TranslationKeyDrawer.SetScriptableObjectDirty(secondAsset);
    }

    public string DrawSearchList(string query, TranslationAsset asset)
    {
        EditorGUILayout.Separator();
        GUIStyle textFieldLeftMarginStyle = new GUIStyle(EditorStyles.textField)
        {
            margin = new RectOffset(30, 0, 0, 0)
        };

        string selectedKey = null;
        foreach (var key in searchResults)
        {
            if (GUILayout.Button(key, textFieldLeftMarginStyle))
            {
                selectedKey = key;
                searchResults = TranslationKeyDrawer.DoSearch(selectedKey, firstAsset, currentTranslationType);
                GUIUtility.keyboardControl = 0;
            }
        }
        if (searchResults.Length > maxShownAutoCompleteButtons)
        {
            GUILayout.Button("...", textFieldLeftMarginStyle);
        }
        return selectedKey;
    }

    private void DrawDefaultLanguageField()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.Space();
        CustomGUI.Splitter();
        EditorGUILayout.Space();

        var serializedTranslator = new SerializedObject(Translator.Instance);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedTranslator.FindProperty("translation"), new GUIContent("Preview language"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedTranslator.ApplyModifiedProperties();
            Translator.UpdateTranslations();
        }

        if (Translator.Instance.Translation != null)
        {
            EditorGUILayout.LabelField("Preview language name:", Translator.Instance.Translation.ToString());
            if (GUILayout.Button("Set as startup language"))
            {
                Translator.UpdateStartupLanguage();
            }
        }

        if (Translator.Settings != null)
        {
            if (Translator.Settings.StartupLanguage != null)
            {
                EditorGUILayout.LabelField("Startup language: ", Translator.Settings.StartupLanguage.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("Startup language: ", "No startup language set!");
            }
        }
        EditorGUILayout.Space();
    }
}
