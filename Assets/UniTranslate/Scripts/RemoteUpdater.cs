using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RemoteUpdater : MonoBehaviour
{
    private const char manifestSeperator = ' ';
    private TranslatorSettings settings;
    private string dataFileUrl;

    public void RunWithSettings(TranslatorSettings settings)
    {
        this.settings = settings;
        //TODO: last update check
        StartCoroutine(FetchManifest());
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

        if (!CheckVersion(version))
        {
            Debug.Log("UniTranslate remote updater: No updates found.", gameObject);
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

        yield return StartCoroutine(RunUpdate(dataFilePath, version));
    }

    private bool CheckVersion(int remoteVersion)
    {
        int localVersion = PlayerPrefs.GetInt("UniTranslate_LocalVersion", 0);
        return remoteVersion > Mathf.Max(localVersion, settings.CurrentTranslationVersion);
    }

    private IEnumerator RunUpdate(string dataFilePath, int version)
    {
        yield return null;
        //PlayerPrefs.SetInt("UniTranslate_LocalVersion", version);
        //PlayerPrefs.Save();
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
        return Path.GetDirectoryName(settings.RemoteManifestURL) + dataFilePath;
    }
}