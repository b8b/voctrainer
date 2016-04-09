using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;

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
    private int lastFocusedTranslation;
    private Vector2 scrollPos = Vector2.zero;
    private ReorderableList list;
    private string filter = "";
    private string[] searchResults;

    private bool[] autoTranslating;

    private const float leftHeaderMargin = 12f;
    private const float twoColumnMinWidth = 600f;
    public const int maxShownAutoCompleteButtons = 15;
    
    static TranslationWindow()
    {
        CheckMissingKeys = EditorPrefs.GetBool("TranslationWindow_CheckMissingKeys", true);
        AutoTranslateOnAdd = EditorPrefs.GetBool("TranslationWindow_AutoTranslateOnAdd", true);
    }

    private void OnEnable()
    {
        this.titleContent = new GUIContent("Translations");
        autoTranslating = new bool[2];

        //Load asset GUIDs from prefs
        try
        {
            string firstGuid = EditorPrefs.GetString("TranslationWindow_FirstAssetGuid", null);
            string secondGuid = EditorPrefs.GetString("TranslationWindow_SecondAssetGuid", null);
            if (firstGuid != null)
                firstAsset = (TranslationAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(firstGuid), typeof (TranslationAsset));
            if (secondGuid != null)
                secondAsset = (TranslationAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(secondGuid), typeof (TranslationAsset));
        }
        catch (Exception e)
        {
            Debug.LogError("Error when loading the last used translation assets: " + e.Message);
        }

        list = new ReorderableList(new List<string>(), typeof(string), draggable: true,
            displayHeader: true, displayAddButton: true, displayRemoveButton: true);
        list.drawHeaderCallback = rect =>
        {

            rect.x += leftHeaderMargin;
            rect.width -= leftHeaderMargin;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.2f, rect.height), "Key");
            if (firstAsset != secondAsset && secondAsset != null)
            {
                EditorGUI.LabelField(new Rect(rect.x + rect.width*0.2f, rect.y, rect.width*0.4f, rect.height),
                    firstAsset.LanguageName);
                EditorGUI.LabelField(new Rect(rect.x + rect.width*0.6f, rect.y, rect.width*0.4f, rect.height),
                    secondAsset.LanguageName);
            }
            else
            {
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.2f, rect.y, rect.width * 0.8f, rect.height),
                    firstAsset.LanguageName);
            }
        };
        

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            try
            {
                float height = rect.height - 3;
                string currentKey = (string) list.list[index];

                EditorGUI.BeginChangeCheck();
                string newKey = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width*0.2f, height), currentKey);
                if (EditorGUI.EndChangeCheck())
                {
                    if (firstAsset.TranslationDictionary.ContainsKey(currentKey))
                    {
                        string oldValue1 = firstAsset.TranslationDictionary[currentKey];
                        firstAsset.TranslationDictionary.Remove(currentKey);
                        firstAsset.TranslationDictionary[newKey] = oldValue1;
                    }

                    if (secondAsset != null && secondAsset.TranslationDictionary.ContainsKey(currentKey))
                    {
                        string oldValue2 = secondAsset.TranslationDictionary[currentKey];
                        secondAsset.TranslationDictionary.Remove(currentKey);
                        secondAsset.TranslationDictionary[newKey] = oldValue2;
                    }

                    UpdateList();
                    TranslationDictionaryDrawer.SetScriptableObjectDirty(firstAsset);
                    TranslationDictionaryDrawer.SetScriptableObjectDirty(secondAsset);
                }
                else
                {
                    newKey = currentKey;
                }

                EditorGUI.BeginChangeCheck();

                if (secondAsset != null && secondAsset != firstAsset)
                {
                    firstAsset.TranslationDictionary[newKey] =
                        EditorGUI.TextField(new Rect(rect.x + rect.width*0.2f, rect.y, rect.width*0.4f, height),
                            firstAsset.TranslationDictionary[newKey]);

                    if (secondAsset != null && secondAsset.TranslationDictionary.ContainsKey(newKey))
                    {
                        secondAsset.TranslationDictionary[newKey] =
                            EditorGUI.TextField(new Rect(rect.x + rect.width*0.6f, rect.y, rect.width*0.4f, height),
                                secondAsset.TranslationDictionary[newKey]);
                    }
                    else
                    {
                        GUIStyle boldTextFieldStyle = new GUIStyle(EditorStyles.textField) {fontStyle = FontStyle.Bold};
                        bool button = GUI.Button(new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, height),
                                "Add missing key", boldTextFieldStyle);

                        if (button)
                        {
                            secondAsset.TranslationDictionary.Add(newKey, "");
                        }
                    }
                }
                else
                {
                    firstAsset.TranslationDictionary[newKey] =
                        EditorGUI.TextField(new Rect(rect.x + rect.width * 0.2f, rect.y, rect.width * 0.8f, height),
                            firstAsset.TranslationDictionary[newKey]);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    UpdateList();
                    TranslationDictionaryDrawer.SetScriptableObjectDirty(firstAsset);
                    TranslationDictionaryDrawer.SetScriptableObjectDirty(secondAsset);
                }
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
                firstAsset.TranslationDictionary.Add("", "");
                TranslationDictionaryDrawer.SetScriptableObjectDirty(firstAsset);
                
                if (secondAsset != firstAsset)
                {
                    secondAsset.TranslationDictionary.Add("", "");
                    TranslationDictionaryDrawer.SetScriptableObjectDirty(secondAsset);
                }
                UpdateList();
                Repaint();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
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
            var firstNewDict = new TranslationAsset.TranslationDictionaryType();
            foreach (string key in list.list)
            {
                firstNewDict.Add(key, firstAsset.TranslationDictionary[key]);
            }
            firstAsset.TranslationDictionary = firstNewDict;
            TranslationDictionaryDrawer.SetScriptableObjectDirty(firstAsset);

            if (secondAsset == null || secondAsset == firstAsset)
                return;
            
            var secondNewDict = new TranslationAsset.TranslationDictionaryType();
            foreach (string key in list.list)
            {
                if (!secondAsset.TranslationDictionary.ContainsKey(key))
                    continue;
                secondNewDict.Add(key, secondAsset.TranslationDictionary[key]);
            }
            secondAsset.TranslationDictionary = secondNewDict;
            TranslationDictionaryDrawer.SetScriptableObjectDirty(secondAsset);
        };

        if (firstAsset != null)
            TranslationDictionaryDrawer.DoSearch("", firstAsset, out searchResults);
    }

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
            if (list.index >= 0 && list.index < list.list.Count && firstAsset != null)
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

        DrawTranslationList();

        if (position.width >= twoColumnMinWidth)
        {
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (list.index >= 0 && list.index < list.list.Count && firstAsset != null)
            {
                DrawEditUI((string)list.list[list.index], headingStyle, firstAsset,
                    secondAsset);
                EditorGUILayout.Space();
            }
        }
    }

    private static void DrawToolbar()
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
        GUILayout.Box(GUIContent.none, EditorStyles.toolbarButton, GUILayout.Width(10f));
        EditorGUI.BeginChangeCheck();
        CheckMissingKeys = GUILayout.Toggle(CheckMissingKeys, new GUIContent("Enable Translation Key Postprocessor",
            "If enabled, UniTranslate will search for missing keys after a scene is loaded in the editor or processed in a build. " +
            "The post processor might affect performance in editor, but is not included in builds. "),
            EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool("TranslationWindow_CheckMissingKeys", CheckMissingKeys);
        }

        GUILayout.Box("", EditorStyles.toolbarButton);
        EditorGUILayout.EndHorizontal();
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
            TranslationDictionaryDrawer.DoSearch(filter, firstAsset, out searchResults);
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
        list.list = firstAsset.TranslationDictionary.Select(pair => pair.Key)
            .Where(key => key.ToLower().StartsWith(filter != null ? filter.ToLower() : ""))
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

        if (GUILayout.Button("Remove Translation"))
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
        if (GUILayout.Button(!translating ? "Translate with Google" : "Downloading translation...", GUILayout.Width(200f)))
        {
            string sourceLang = translationAsset == currentLang ? TranslationService.autoLangCode : currentLang.LanguageCode;
            autoTranslating[assetIndex] = true;
            GUIUtility.keyboardControl = 0;
            TranslationService.Translate(currentLang.TranslationDictionary[key, ""],
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
        if (firstAsset.TranslationDictionary.ContainsKey(selectedKey))
        {
            firstAsset.TranslationDictionary.Remove(selectedKey);
        }
        if (secondAsset != null && secondAsset.TranslationDictionary.ContainsKey(selectedKey))
        {
            secondAsset.TranslationDictionary.Remove(selectedKey);
        }
        TranslationDictionaryDrawer.SetScriptableObjectDirty(firstAsset);
        TranslationDictionaryDrawer.SetScriptableObjectDirty(secondAsset);
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
                TranslationDictionaryDrawer.DoSearch(selectedKey, firstAsset, out searchResults);
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
