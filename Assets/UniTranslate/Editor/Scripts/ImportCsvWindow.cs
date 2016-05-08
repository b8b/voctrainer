using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class ImportCsvWindow : EditorWindow
{
    public string FileName
    {
        get { return fileName; }
        set
        {
            fileName = value;
            ReadFirstLine();
            DoInitialAssignments();
        }
    }

    private string newLineSubstitude = "{newLine}";
    private string[] options;
    private TranslationAsset[] translationAssets;
    private int keyColumn;
    private int[] assignedColums;
    private bool skipFirstLine = true;
    private bool finished;
    private string fileName;
    private const char csvSeperator = ';';

    private void OnEnable()
    {
        translationAssets = TranslationKeyDrawer.GetTranslationAssets();
        assignedColums = new int[translationAssets.Length];
        options = new string[0];

        this.position = new Rect(position.x, position.y, 600f, 400f);
    }

    private void OnGUI()
    {
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        EditorGUILayout.HelpBox("This wizard will help you import translations from a CSV (comma-seperated values) file. " +
                                "The first row of the CSV file is used to assign the columns to their corresponding translation assets.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        string fileName = EditorGUILayout.TextField("File name:", FileName);
        if (EditorGUI.EndChangeCheck())
        {
            FileName = fileName;
            finished = false;
        }

        if (GUILayout.Button("Browse", GUILayout.Width(100f)))
        {
            GUIUtility.keyboardControl = 0;
            string loadPath = ShowLoadDialog();
            if (!string.IsNullOrEmpty(loadPath))
            {
                FileName = loadPath;
                finished = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Substitude this string with new line characters: ");
        newLineSubstitude = EditorGUILayout.TextField(" ", newLineSubstitude);
        skipFirstLine = EditorGUILayout.ToggleLeft("Skip the first row", skipFirstLine);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Language mappings:", headingStyle);
        DrawAssignUI();

        if (finished)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Import finished!", headingStyle);
        }

        if (options == null)
            return;

        if (GUI.Button(new Rect(position.width - 200, position.height - 40, 195, 25), "Import"))
        {
            try
            {
                Import();
                if (!EditorUtility.DisplayDialog("Import successful", "The CSV import was finished successfully.", "OK", "Show")) //Selected "Show" option
                {
                    this.Close();
                    EditorWindow.GetWindow<TranslationWindow>().Repaint();
                }
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Import failed", "An error occured while importing: " + e.Message +
                    "\nPlease verify that the file is not already used by another program and try again.", "OK");
                UnityEngine.Debug.LogException(e);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Import failed", "An error occured while importing: " + e.Message, "OK");
                UnityEngine.Debug.LogException(e);
            }
        }
    }

    public static string ShowLoadDialog()
    {
         return EditorUtility.OpenFilePanel("Import from CSV", "translations", "csv");
    }

    private void ReadFirstLine()
    {
        try
        {
            using (StreamReader reader = new StreamReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
            {
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line) || !line.Contains(csvSeperator))
                    throw new Exception("Invalid csv file!");

                options = line.Split(csvSeperator);
            }
        }
        catch
        {
            options = null;
        }
    }

    private void DrawAssignUI()
    {
        if (options == null)
        {
            EditorGUILayout.HelpBox("Cannot read selected file.", MessageType.Error);
            return;
        }
        var optionContents = OptionsArray();
        keyColumn = EditorGUILayout.Popup(new GUIContent("Keys"), keyColumn,
                optionContents.Skip(1).ToArray(), EditorStyles.popup, null);
        for (int i = 0; i < translationAssets.Length; i++)
        {
            var translationAsset = translationAssets[i];
            EditorGUILayout.BeginHorizontal();
            assignedColums[i] = EditorGUILayout.Popup(new GUIContent(translationAsset.ToString()), assignedColums[i] + 1,
                optionContents, EditorStyles.popup, null) - 1; //Subtract 1 because of empty value
            EditorGUILayout.EndHorizontal();
        }
    }

    private GUIContent[] OptionsArray()
    {
        string[] fixedValues = new string[] {"---"};
        return fixedValues.Concat(options).Select(label => new GUIContent(label)).ToArray();
    }

    private void Import()
    {
        using (StreamReader reader = new StreamReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
        {
            if (skipFirstLine)
                reader.ReadLine();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line) || !line.Contains(csvSeperator))
                    continue;

                string[] split = line.Split(csvSeperator);
                if (split.Length - 1 < keyColumn)
                {
                    throw new Exception("Invalid csv file: Key missing in column.");
                }

                string key = split[keyColumn];
                for (int i = 0; i < translationAssets.Length; i++)
                {
                    if (assignedColums[i] == -1)
                        continue;
                    var asset = translationAssets[i];
                    if (split.Length - 1 < assignedColums[i])
                    {
                        throw new Exception("Invalid csv file: Value missing in column.");
                    }

                    string value = split[assignedColums[i]];
                    if (asset.TranslationDictionary.ContainsKey(key))
                    {
                        asset.TranslationDictionary[key] = value;
                    }
                    else
                    {
                        asset.TranslationDictionary.Add(key, value);
                    }
                    EditorUtility.SetDirty(asset);
                }
            }
        }
        finished = true;
    }

    private void DoInitialAssignments()
    {
        if (options == null)
            return;

        for (int i = 0; i < assignedColums.Length; i++)
        {
            assignedColums[i] = -1;
        }

        for (int i = 0; i < translationAssets.Length; i++)
        {
            var translationAsset = translationAssets[i];
            for (int j = 0; j < options.Length; j++)
            {
                var option = options[j];
                if (string.Equals(option, "Key", StringComparison.CurrentCultureIgnoreCase))
                {
                    keyColumn = j;
                }
                else if (string.Equals(option, translationAsset.LanguageCode, StringComparison.CurrentCultureIgnoreCase))
                {
                    assignedColums[i] = j;
                }
                else if (string.Equals(option, translationAsset.LanguageName, StringComparison.CurrentCultureIgnoreCase))
                {
                    assignedColums[i] = j;
                }
            }
        }
    }
}
