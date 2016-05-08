using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Used to download updated translation data from a server. Automatically instanciated if 
/// a manifest URL is set in <see cref="TranslatorSettings"/>.<para/>
/// 
/// Please note that newly downloaded translations are not displayed immediately by default, 
/// but whenever a translation is loaded by a <see cref="LocalizedComponent"/> (e.g. when the 
/// displayed language is changed or a new scene is loaded). 
/// </summary>
public class RemoteUpdate : MonoBehaviour
{
    public const string cacheFileName = "localization_cache.dat";
    private const char manifestSeperator = ' ';
    private TranslatorSettings settings;
    private string dataFileUrl;
    private string cacheFilePath;

    /// <summary>
    /// Performs the remote update.<para/>
    /// 
    /// Please note that newly downloaded translations are not displayed immediately by default, 
    /// but whenever a translation is loaded by a <see cref="LocalizedComponent"/> (e.g. when the 
    /// displayed language is changed or a new scene is loaded). 
    /// </summary>
    /// <param name="settings">The settings used by to update</param>
    public void RunWithSettings(TranslatorSettings settings)
    {
        this.settings = settings;

        if (UpdateCheckAllowed())
        {
            StartCoroutine(FetchManifest());
        }

        cacheFilePath = Application.persistentDataPath + "/" + cacheFileName;
        LoadTranslationCache();
    }


    public void LoadTranslationCache()
    {
        if (!File.Exists(cacheFilePath) || PlayerPrefs.GetInt("UniTranslate_LocalVersion", 0) < settings.CurrentTranslationVersion)
            return;

        Debug.Log("UniTranslate remote updater: Loading translations from cache file: " + cacheFilePath);
        var formatter = new BinaryFormatter();
        using (var stream = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read))
        {
            var languageMappings = (SerializableDictionary<string, TranslationAsset.StringDictionaryType>) formatter.Deserialize(stream);
            foreach (var translationAsset in settings.Languages)
            {
                foreach (var mapping in languageMappings)
                {
                    if (translationAsset.LanguageCode == mapping.Key)
                    {
                        translationAsset.CachedTranslationDictionary = mapping.Value;
                    }
                }
            }
        }
    }

    private bool UpdateCheckAllowed()
    {
        if (settings.RemoteUpdateFrequency == TranslatorSettings.UpdateFrequency.EveryStart)
            return true;

        string lastCheckStr = PlayerPrefs.GetString("UniTranslate_LastUpdateCheck", "0");

        long ticks;
        if (!long.TryParse(lastCheckStr, out ticks))
        {
            Debug.LogError("PlayerPrefs UniTranslate_LastUpdateCheck is malformed!", gameObject);
        }

        DateTime lastUpdateCheck = new DateTime(ticks);
        var timeSpan = DateTime.Now - lastUpdateCheck;

        switch (settings.RemoteUpdateFrequency)
        {
            case TranslatorSettings.UpdateFrequency.Daily:
                return timeSpan.Days >= 1;
            case TranslatorSettings.UpdateFrequency.Weekly:
                return timeSpan.Days >= 7;
            case TranslatorSettings.UpdateFrequency.Monthly:
                return timeSpan.Days >= 30;
            default:
                return true;
        }
    }

    private IEnumerator FetchManifest()
    {
        var request = new WWW(settings.RemoteManifestURL);
        yield return request;
        if (request.error != null || request.text == null)
        {
            Debug.LogError("UniTranslate remote updater error: " + request.error, gameObject);
            yield break;
        }

        var manifest = ParseManifest(request.text);
        if (!manifest.ContainsKey("data_file") || !manifest.ContainsKey("version"))
        {
            Debug.LogError("Invalid manifest file. Please ensure that it contains at least the 'version' and 'data_file' keys.", gameObject);
            yield break;
        }

        int version;
        if (!int.TryParse(manifest["version"], out version))
        {
            Debug.LogError("Invalid version in manifest", gameObject);
            yield break;
        }

        PlayerPrefs.SetString("UniTranslate_LastUpdateCheck", DateTime.Now.Ticks.ToString());
        if (!CheckVersion(version))
        {
            Debug.Log("UniTranslate remote updater: No updates found. (remote version: " + version + ")", gameObject);
            yield break;
        }

        string dataFilePath = manifest["data_file"];
        if (string.IsNullOrEmpty(dataFilePath))
        {
            Debug.LogError("Invalid data file path in manifest", gameObject);
            yield break;
        }

        dataFileUrl = ParsePath(dataFilePath);
        Debug.Log("UniTranslate remote updater: Update found. Version: " + version + ", data file url:" + dataFileUrl, gameObject);

        yield return StartCoroutine(RunUpdate(dataFileUrl, version));
    }

    public bool CheckVersion(int remoteVersion)
    {
        int localVersion = PlayerPrefs.GetInt("UniTranslate_LocalVersion", 0);
        return remoteVersion > Mathf.Max(localVersion, settings.CurrentTranslationVersion);
    }

    private IEnumerator RunUpdate(string dataFileURL, int version)
    {
        var request = new WWW(dataFileURL);
        yield return request;

        byte[] bytes = request.bytes;
        if (request.error != null || bytes == null)
        {
            Debug.LogError("UniTranslate remote updater error: " + request.error, gameObject);
            yield break;
        }

        Debug.Log("UniTranslate remote updater: Download finished. Saving to " + cacheFileName);

        using (var stream = new FileStream(Application.persistentDataPath + "/" + cacheFileName, FileMode.Create, FileAccess.Write))
        {
            stream.Write(bytes, 0, request.bytesDownloaded);
            stream.Flush();
        }

        PlayerPrefs.SetInt("UniTranslate_LocalVersion", version);
        PlayerPrefs.Save();
        LoadTranslationCache();
        Debug.Log("Update finished. Updated translations will normally not show up immediately, but whenever localized components are " +
                  "updated (e.g. when a scene is loaded or languages are swapped).\nCall Translator.UpdateTranslations() manually " +
                         "if you want an immediate update. Double-click on this message to edit the script.", gameObject);
        //Translator.UpdateTranslations();
    }

    public static Dictionary<string, string> ParseManifest(string text)
    {
        var dict = new Dictionary<string, string>();
        string[] lines = text.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 1)
            return null;

        foreach (string line in lines)
        {
            if (!line.Contains(manifestSeperator.ToString()))
                continue;

            string[] split = line.Split(manifestSeperator);
            if (split.Length != 2 || string.IsNullOrEmpty(split[0]))
                continue;

            dict.Add(split[0], split[1]);
        }
        return dict;
    }

    private string ParsePath(string dataFilePath)
    {
        if (dataFilePath.StartsWith("http://") || dataFilePath.StartsWith("https://") || dataFilePath.StartsWith("ftp://")) //Path is absolute
            return dataFilePath;

        //Path is relative
        int lastSlash = settings.RemoteManifestURL.LastIndexOf('/');
        string withoutLastComponent = (lastSlash > -1) ? settings.RemoteManifestURL.Substring(0, lastSlash) : settings.RemoteManifestURL;
        return withoutLastComponent + "/" + dataFilePath;
        
    }
}