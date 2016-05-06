using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;

public class DropboxIntegration
{
    private const string appKey = "ddzk67lodhfagw9";
    private const string appSecret = "lud19htdv3d1for"; //TODO: encrypt?
    public string AuthCode { get; set; }
    public string AccessToken { get; set; }
    public string UID { get; set; }
    public string UserName { get; set; }
    public string UserMail { get; set; }

    public void AuthorizeInBrowser()
    {
        Application.OpenURL("https://www.dropbox.com/oauth2/authorize?response_type=code&client_id=" + appKey);
    }

    public void FetchAuthorizationToken(Action<bool> callback, Action userDataCallback)
    {
        if (string.IsNullOrEmpty(AuthCode))
        {
            EditorUtility.DisplayDialog("Empty auth code", "Please enter your auth code.", "OK");
            callback(false);
            return;
        }
        WWWForm form = new WWWForm();
        form.AddField("code", AuthCode);
        form.AddField("grant_type", "authorization_code");
        form.AddField("client_id", appKey);
        form.AddField("client_secret", appSecret);
        AuthCode = null;
        WWW req = new WWW("https://api.dropboxapi.com/oauth2/token", form);
        ContinuationManager.Add(() => req.isDone, () =>
        {
            if (!string.IsNullOrEmpty(req.error))
            {
                string info = !string.IsNullOrEmpty(req.text) ? "\nAdditional info: " + req.text : "";
                EditorUtility.DisplayDialog("Dropbox authorization failed", "An error occured: " + req.error + info + "\nPlease check your internet connection and your access code and try again.", "OK");
                callback(false);
            }
            else
            {
                try
                {
                    var obj = JObject.Parse(req.text);
                    AccessToken = obj["access_token"].ToString();
                    UID = obj["uid"].ToString();
                    GetMailAddress(userDataCallback);
                    callback(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("UniTranslate Dropbox integration error: " + e.Message);
                    callback(false);
                }
            }
        });
    }

    public void GetMailAddress(Action userDataCallback)
    {
        WWW req = new WWW("https://api.dropboxapi.com/2/users/get_current_account", System.Text.Encoding.UTF8.GetBytes("null"), new Dictionary<string, string>
        {
            {"Content-Type", "application/json"},
            {"Authorization", "Bearer " + AccessToken}
        });
        ContinuationManager.Add(() => req.isDone, () =>
        {
            if (!string.IsNullOrEmpty(req.error))
            {
                string info = !string.IsNullOrEmpty(req.text) ? "\nAdditional info: " + req.text : "";
                EditorUtility.DisplayDialog("Fetching email address failed", "An error occured: " + req.error + info + "\nPlease check your internet connection and your access code and try again.", "OK");
                userDataCallback();
            }
            else
            {
                try
                {
                    var obj = JObject.Parse(req.text);
                    UserMail = obj["email"].ToString();
                    UserName = obj["name"]["display_name"].ToString();
                    userDataCallback();
                }
                catch (Exception e)
                {
                    Debug.LogError("UniTranslate Dropbox integration error: " + e.Message);
                    userDataCallback();
                }
            }
        });
    }

    public void UploadFile(byte[] file, string name, Action<bool, string> callback)
    {
        WWW req = new WWW("https://content.dropboxapi.com/2/files/upload", file, new Dictionary<string, string>
        {
            {"Content-Type", "application/octet-stream"},
            {"Authorization", "Bearer " + AccessToken},
            {"Dropbox-API-Arg", "{\"path\": \"/" + name + "\",\"mode\": \"add\",\"autorename\": true,\"mute\": true}"}
        });
        ContinuationManager.Add(() => req.isDone, () =>
        {
            if (!string.IsNullOrEmpty(req.error))
            {
                string info = !string.IsNullOrEmpty(req.text) ? "\nAdditional info: " + req.text : "";
                EditorUtility.DisplayDialog("Uploading the file failed", "An error occured: " + req.error + info + "\nPlease check your internet connection and your access code and try again.", "OK");
                callback(false, null);
            }
            else
            {
                try
                {
                    var obj = JObject.Parse(req.text);
                    //Debug.Log("Upload successful. File path: " + obj["path_lower"]);
                    callback(true, obj["path_lower"].ToString());
                }
                catch (Exception e)
                {
                    Debug.LogError("UniTranslate Dropbox integration error: " + e.Message);
                    callback(true, null);
                }
            }
        });
    }

    public void ShareFile(string path, bool raw, Action<bool, string> callback)
    {
        WWW req = new WWW("https://api.dropboxapi.com/2/sharing/create_shared_link_with_settings",
            System.Text.Encoding.UTF8.GetBytes("{\"path\": \"" + path + "\",\"settings\": {\"requested_visibility\": \"public\"}}"), new Dictionary<string, string>
        {
            {"Content-Type", "application/json"},
            {"Authorization", "Bearer " + AccessToken}
        });
        ContinuationManager.Add(() => req.isDone, () =>
        {
            if (!string.IsNullOrEmpty(req.error))
            {
                string info = !string.IsNullOrEmpty(req.text) ? "\nAdditional info: " + req.text : "";
                EditorUtility.DisplayDialog("Sharing a file failed", "An error occured: " + req.error + info + "\nPlease check your internet connection and your access code and try again.", "OK");
                callback(false, null);
            }
            else
            {
                try
                {
                    var obj = JObject.Parse(req.text);
                    string url = obj["url"].ToString();
                    if (raw)
                    {
                        url = url.Replace("?dl=0", "");
                        url = url + "?raw=1";
                    }
                    //Debug.Log("Sharing successful. URL: " + url);
                    callback(true, url);
                }
                catch (Exception e)
                {
                    Debug.LogError("UniTranslate Dropbox integration error: " + e.Message);
                    callback(true, null);
                }
            }
        });
    }
}
