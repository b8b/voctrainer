using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class WebDeploymentWindow : EditorWindow
{
    internal enum DeploymentPlatform
    {
        Dropbox,
        Custom
    }

    private const string dataFileName = "translations_v{ver}.dat";
    private const string manifestFileName = "unitranslate.manifest";

    public string DeploymentPath { get; set; }
    private DeploymentPlatform platform;
    private bool[] assetsEnabled;
    private TranslationAsset[] translationAssets;
    private int version;
    private string remoteURL;
    private Vector2 scrollPos;
    private bool authorizing;
    private bool authSuccessful;
    private DropboxIntegration dropbox;

    private void OnEnable()
    {
        translationAssets = TranslationKeyDrawer.GetTranslationAssets();
        assetsEnabled = new bool[translationAssets.Length];
        dropbox = new DropboxIntegration();
        for (int i = 0; i < assetsEnabled.Length; i++)
        {
            assetsEnabled[i] = true;
        }
        this.position = new Rect(position.x, position.y, 600f, 400f);

        if (Translator.Settings != null)
        {
            version = Translator.Settings.CurrentTranslationVersion + 1;
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            fixedHeight = 20f
        };

        GUI.enabled = !authorizing;
        EditorGUILayout.HelpBox("This assistant will help you upload translation data to a web service, so you can update your string translations " +
                                "without updating the entire application. It will generate two files: a " + manifestFileName + " file, where the current version " +
                                "of your localizations is saved and a " + dataFileName.Replace("{ver}", "(version number)") + " file, where the actual data is stored. The manifest also " +
                                "stores the URL to the data file. Translation files are only downloaded if their version number is greater than " +
                                "the minimum version number defined in the Translator Settings and greater the version number of the previously downloaded translation file.", MessageType.Info);

        EditorGUILayout.LabelField("1. Choose a platform for deployment:", headingStyle);
        platform = (DeploymentPlatform) GUILayout.Toolbar((int) platform, new string[] {"Dropbox", "Custom web service (local deployment)"});
        if (platform == DeploymentPlatform.Dropbox)
        {
            if (authSuccessful)
            {
                string message = dropbox.UserMail != null
                    ? "Authentication successful. Logged in as " + dropbox.UserName + " (" + dropbox.UserMail + ")."
                    : "Authentication successful. Fetching account information...";
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Please log in with your Dropbox account. You will get a code which you need to paste in the field below.", MessageType.Info);
                if (GUILayout.Button("Sign in"))
                {
                    dropbox.AuthorizeInBrowser();
                }
                EditorGUILayout.BeginHorizontal();
                dropbox.AuthCode = EditorGUILayout.TextField("Authorization code: ", dropbox.AuthCode);
                if (GUILayout.Button(!authorizing ? "Authorize" : "Please wait...", GUILayout.ExpandWidth(false)))
                {
                    authorizing = true;
                    dropbox.FetchAuthorizationToken(success =>
                    {
                        //Debug.Log(success ? "Dropbox authorization successful" : "Dropbox authorization failed.");
                        authorizing = false;
                        authSuccessful = success;
                        Repaint();
                    }, Repaint);
                    GUIUtility.keyboardControl = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            
        }
        else if (platform == DeploymentPlatform.Custom)
        {
            EditorGUILayout.HelpBox("Please upload the manifest and data files to your webspace after saving them locally. Enter the URL where the manifest is reachable in step 4. " +
                                    "If the manifest and data files are not located in the same directory, you will have to change the path in the manifest file.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            DeploymentPath = EditorGUILayout.TextField("Deployment path:", DeploymentPath);
            if (GUILayout.Button("Browse", GUILayout.Width(100f)))
            {
                GUIUtility.keyboardControl = 0;
                string savePath = ShowSaveDialog();
                if (!string.IsNullOrEmpty(savePath))
                    DeploymentPath = savePath;
            }
            EditorGUILayout.EndHorizontal();
        }

        CustomGUI.Splitter();
        EditorGUILayout.LabelField("2. Choose which languages you want to deploy:", headingStyle);
        for (int i = 0; i < translationAssets.Length; i++)
        {
            assetsEnabled[i] = EditorGUILayout.ToggleLeft(translationAssets[i].LanguageName + " (" + translationAssets[i].LanguageCode + ")",
                assetsEnabled[i]);
        }

        CustomGUI.Splitter();
        EditorGUILayout.LabelField("3. Deploy", headingStyle);

        if (platform == DeploymentPlatform.Dropbox)
        {
            GUI.enabled = authSuccessful;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Version number:", GUILayout.ExpandWidth(false));
        version = EditorGUILayout.IntField(version, GUILayout.ExpandWidth(false));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Deploy"))
        {
            try
            {
                Export();
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Deployment failed", "An error occured while exporting: " + e.Message +
                                                                 "\nPlease verify that the file is not already used by another program and try again.", "OK");
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Deployment failed", "An error occured while exporting: " + e.Message, "OK");
                Debug.LogException(e);
            }
        }
        GUI.enabled = true;

        CustomGUI.Splitter();
        EditorGUILayout.LabelField("4: " + (platform == DeploymentPlatform.Custom ? "Enter" : "Verify") + " the remote URL to the manifest file.", headingStyle);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        remoteURL = EditorGUILayout.TextField(remoteURL);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateTranslatorSettings();
        }
        GUI.enabled = !string.IsNullOrEmpty(remoteURL) && remoteURL.StartsWith("http");
        if (GUILayout.Button("Preview in browser", GUILayout.ExpandWidth(false)))
        {
            Application.OpenURL(remoteURL);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void UpdateTranslatorSettings()
    {
        if (Translator.Settings == null)
        {
            EditorUtility.DisplayDialog("Translator settings invalid",
                "Error: Cannot access translator settings. Please verify that the settings are accessible.", "OK");
            return;
        }

        Translator.Settings.RemoteManifestURL = remoteURL;
        TranslationKeyDrawer.SetScriptableObjectDirty(Translator.Settings);
    }

    public static string ShowSaveDialog()
    {
        string savePath = EditorUtility.OpenFolderPanel("Export to CSV", null, "deploy");
        return savePath;
    }

    private void Export()
    {
        if (!AnyEnabled())
        {
            EditorUtility.DisplayDialog("Deployment failed", "Please select one or more languages.", "OK");
            return;
        }

        if (platform == DeploymentPlatform.Dropbox)
        {
            StartDropboxDeployment();
        }
        else if (platform == DeploymentPlatform.Custom)
        {
            StartCustomDeployment();
        }
    }

    private void StartDropboxDeployment()
    {
        EditorUtility.DisplayProgressBar("Dropbox deployment", "Uploading data file...", 0f);
        var allDicts = GatherTranslationDicts();
        byte[] dataFile = ObjectToByteArray(allDicts);
        dropbox.UploadFile(dataFile, dataFileName.Replace("{ver}", version.ToString()),
            (uploadSuccess, filePath) =>
            {
                if (!uploadSuccess)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                EditorUtility.DisplayProgressBar("Dropbox deployment", "Creating shared link for data file...", 0.25f);
                dropbox.ShareFile(filePath, raw: true, callback: (shareSuccess, dataFileUrl) =>
                {
                    if (!shareSuccess)
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                    DeployManifestFile(dataFileUrl);
                });
            });
    }

    private void DeployManifestFile(string dataFileUrl)
    {
        EditorUtility.DisplayProgressBar("Dropbox deployment", "Uploading manifest file...", 0.5f);
        string manifest = CreateManifest(version, dataFileUrl);
        dropbox.UploadFile(System.Text.Encoding.UTF8.GetBytes(manifest), manifestFileName,
            (uploadSuccess, filePath) =>
            {
                if (!uploadSuccess)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                EditorUtility.DisplayProgressBar("Dropbox deployment", "Creating shared link for manifest file...", 0.75f);
                dropbox.ShareFile(filePath, raw: true, callback: (shareSuccess, manifestFileUrl) =>
                {
                    EditorUtility.ClearProgressBar();
                    if (!shareSuccess)
                        return;
                    FinishDeployment(manifestFileUrl, () => {});
                });
            });
    }

    private void StartCustomDeployment()
    {
        var allDicts = GatherTranslationDicts();
        try
        {
            string newDataFileName = dataFileName.Replace("{ver}", version.ToString());
            string dataFilePath = DeploymentPath + Path.DirectorySeparatorChar + newDataFileName;
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream stream = new FileStream(dataFilePath, FileMode.Create, FileAccess.Write))
            {
                bf.Serialize(stream, allDicts);
                stream.Flush();
            }
            
            using (StreamWriter writer = new StreamWriter(new FileStream(DeploymentPath + Path.DirectorySeparatorChar + manifestFileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(CreateManifest(version, newDataFileName));
                writer.Flush();
            }

            FinishDeployment("", () => EditorUtility.RevealInFinder(DeploymentPath));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void FinishDeployment(string manifestFileUrl, Action dialogAction)
    {
        remoteURL = manifestFileUrl;
        UpdateTranslatorSettings();
        if (Translator.Settings != null)
        {
            Selection.activeObject = Translator.Settings;
        }
        if (EditorUtility.DisplayDialog("Deployment successful", "Deployment has been finished successfully.", "OK"))
        {
            dialogAction();
        }
        
        GUIUtility.keyboardControl = 0;
        Repaint();
    }

    private string CreateManifest(int ver, string dataFileUrl)
    {
        return string.Format("version {0}\n" +
                             "data_file {1}", ver, dataFileUrl);
    }

    private SerializableDictionary<string, TranslationAsset.StringDictionaryType> GatherTranslationDicts()
    {
        var dict = new SerializableDictionary<string, TranslationAsset.StringDictionaryType>();
        for (int i = 0; i < translationAssets.Length; i++)
        {
            if (assetsEnabled[i])
                dict.Add(translationAssets[i].LanguageCode, translationAssets[i].TranslationDictionary);
        }
        return dict;
    }

    private byte[] ObjectToByteArray(object obj)
    {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    private bool AnyEnabled()
    {
        return assetsEnabled.Any(enabled => enabled);
    }
}
