using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class ExportCsvWindow : EditorWindow
{
    public string FileName { get; set; }

    private string newLineSubstitude = "{newLine}";
    private bool[] assetsEnabled;
    private TranslationAsset[] translationAssets;
    private bool finished;
    private const char csvSeperator = ';';

    private void OnEnable()
    {
        translationAssets = TranslationKeyDrawer.GetTranslationAssets();
        assetsEnabled = new bool[translationAssets.Length];
        for (int i = 0; i < assetsEnabled.Length; i++)
        {
            assetsEnabled[i] = true;
        }
        this.position = new Rect(position.x, position.y, 600f, 400f);
    }

    private void OnGUI()
    {
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        EditorGUILayout.HelpBox("This wizard will help you convert your translations to CSV (comma-seperated values) format. " +
                                "Please note that the first line of the generated file contains the language code of the translation assets, " +
                                "which are used to identify them when re-importing.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        FileName = EditorGUILayout.TextField("File name:", FileName);
        if (GUILayout.Button("Browse", GUILayout.Width(100f)))
        {
            GUIUtility.keyboardControl = 0;
            string savePath = ShowSaveDialog();
            if (!string.IsNullOrEmpty(savePath))
                FileName = savePath;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Substitude new line characters with: ");
        newLineSubstitude = EditorGUILayout.TextField(" ", newLineSubstitude);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Exported languages:", headingStyle);
        for (int i = 0; i < translationAssets.Length; i++)
        {
            assetsEnabled[i] = EditorGUILayout.ToggleLeft(translationAssets[i].LanguageName + " (" + translationAssets[i].LanguageCode + ")",
                assetsEnabled[i]);
        }

        if (finished)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Export finished!", headingStyle);
        }
        
        if (GUI.Button(new Rect(position.width - 200, position.height - 40, 195, 25), "Export"))
        {
            try
            {
                Export();
                finished = true;
                if (!EditorUtility.DisplayDialog("Export successful", "The CSV export was finished successfully.", "OK", "Open")) //Selected "Open" option
                {
                    Process.Start(FileName);
                }
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Export failed", "An error occured while exporting: " + e.Message +
                    "\nPlease verify that the file is not already used by another program and try again.", "OK");
                UnityEngine.Debug.LogException(e);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Export failed", "An error occured while exporting: " + e.Message, "OK");
                UnityEngine.Debug.LogException(e);
            }
        }
    }

    public static string ShowSaveDialog()
    {
        string savePath = EditorUtility.SaveFilePanel("Export to CSV", null, "translations", "csv");
        return savePath;
    }

    private void Export()
    {
        if (!AnyEnabled())
        {
            EditorUtility.DisplayDialog("Export failed", "Please select one or more languages.", "OK");
            return;
        }

        using (StreamWriter writer = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)))
        {
            writer.Write("Key" + csvSeperator);
            for (int i = 0; i < translationAssets.Length; i++)
            {
                if (!assetsEnabled[i])
                    continue;

                writer.Write(translationAssets[i].LanguageCode);
                if (i != translationAssets.Length - 1)
                    writer.Write(csvSeperator);
            }
            writer.WriteLine();

            List<string> allKeys = CollectKeys();
            foreach (var key in allKeys)
            {
                writer.Write(key + csvSeperator);
                for (int i = 0; i < translationAssets.Length; i++)
                {
                    if (!assetsEnabled[i])
                        continue;

                    writer.Write(translationAssets[i].TranslationDictionary[key, ""].Replace("\n", newLineSubstitude));
                    if (i != translationAssets.Length - 1)
                        writer.Write(csvSeperator);
                }
                writer.WriteLine();
            }

            writer.Flush();
        }
    }

    private bool AnyEnabled()
    {
        return assetsEnabled.Any(enabled => enabled);
    }

    private List<string> CollectKeys()
    {
        List<string> allKeys = new List<string>();
        for (int i = 0; i < translationAssets.Length; i++)
        {
            if (!assetsEnabled[i])
                continue;

            var asset = translationAssets[i];
            allKeys.AddRange(asset.TranslationDictionary.Select(pair => pair.Key)
                    .Where(key => !allKeys.Contains(key))
                    .ToList());
        }
        return allKeys;
    }
}