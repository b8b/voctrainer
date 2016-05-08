using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniTranslateEditor
{
    public static class TranslationAssetEditorExtensions
    {
        public static void CreateNewEmptyKey(this TranslationAsset asset, Type translationType)
        {
            if (translationType == typeof (string))
                asset.TranslationDictionary.Add("", "");
            else if (translationType == typeof (Sprite))
                asset.SpriteDictionary.Add("", null);
            else if (translationType == typeof (Texture))
                asset.TextureDictionary.Add("", null);
            else if (translationType == typeof (AudioClip))
                asset.AudioDictionary.Add("", null);
            else if (translationType == typeof (Font))
                asset.FontDictionary.Add("", null);
            else if (translationType == typeof (ScriptableObject))
                asset.ScriptableObjectDictionary.Add("", null);
        }

        public static void Add(this TranslationAsset asset, Type translationType, string key, object value = null)
        {
            if (translationType == typeof(string))
                asset.TranslationDictionary.Add(key, value as string ?? "");
            else if (translationType == typeof(Sprite))
                asset.SpriteDictionary.Add(key, value as Sprite);
            else if (translationType == typeof(Texture))
                asset.TextureDictionary.Add(key, value as Texture);
            else if (translationType == typeof(AudioClip))
                asset.AudioDictionary.Add(key, value as AudioClip);
            else if (translationType == typeof(Font))
                asset.FontDictionary.Add(key, value as Font);
            else if (translationType == typeof(ScriptableObject))
                asset.ScriptableObjectDictionary.Add(key, value as ScriptableObject);
        }

        public static bool KeyExists(this TranslationAsset asset, Type translationType, string key)
        {
            return ((translationType == typeof (string) && asset.TranslationDictionary.ContainsKey(key)) ||
                    (translationType == typeof (Sprite) && asset.SpriteDictionary.ContainsKey(key)) ||
                    (translationType == typeof (Texture) && asset.TextureDictionary.ContainsKey(key)) ||
                    (translationType == typeof (AudioClip) && asset.AudioDictionary.ContainsKey(key)) ||
                    (translationType == typeof (Font) && asset.FontDictionary.ContainsKey(key)) ||
                    (translationType == typeof (ScriptableObject) && asset.ScriptableObjectDictionary.ContainsKey(key)));
        }

        public static void RemoveIfKeyExists(this TranslationAsset asset, Type translationType, string key)
        {
            if (KeyExists(asset, translationType, key))
            {
                Remove(asset, translationType, key);
            }
        }

        public static void Remove(this TranslationAsset asset, Type translationType, string key)
        {
            if (translationType == typeof(string))
                asset.TranslationDictionary.Remove(key);
            else if (translationType == typeof(Sprite))
                asset.SpriteDictionary.Remove(key);
            else if (translationType == typeof(Texture))
                asset.TextureDictionary.Remove(key);
            else if (translationType == typeof(AudioClip))
                asset.AudioDictionary.Remove(key);
            else if (translationType == typeof(Font))
                asset.FontDictionary.Remove(key);
            else if (translationType == typeof(ScriptableObject))
                asset.ScriptableObjectDictionary.Remove(key);
        }

        public static void ChangeKey(this TranslationAsset asset, Type translationType, string oldKey, string newKey)
        {
            if (translationType == typeof(string))
            {
                if (asset.TranslationDictionary.ContainsKey(oldKey))
                {
                    string value = asset.TranslationDictionary[oldKey];
                    asset.TranslationDictionary.Remove(oldKey);
                    asset.TranslationDictionary[newKey] = value;
                }
            }
            else if (translationType == typeof(Sprite))
            {
                if (asset.SpriteDictionary.ContainsKey(oldKey))
                {
                    var value = asset.SpriteDictionary[oldKey];
                    asset.SpriteDictionary.Remove(oldKey);
                    asset.SpriteDictionary[newKey] = value;
                }
            }
            else if (translationType == typeof(Texture))
            {
                if (asset.TextureDictionary.ContainsKey(oldKey))
                {
                    var value = asset.TextureDictionary[oldKey];
                    asset.TextureDictionary.Remove(oldKey);
                    asset.TextureDictionary[newKey] = value;
                }
            }
            else if (translationType == typeof(AudioClip))
            {
                if (asset.AudioDictionary.ContainsKey(oldKey))
                {
                    var value = asset.AudioDictionary[oldKey];
                    asset.AudioDictionary.Remove(oldKey);
                    asset.AudioDictionary[newKey] = value;
                }
            }
            else if (translationType == typeof(Font))
            {
                if (asset.FontDictionary.ContainsKey(oldKey))
                {
                    var value = asset.FontDictionary[oldKey];
                    asset.FontDictionary.Remove(oldKey);
                    asset.FontDictionary[newKey] = value;
                }
            }
            else if (translationType == typeof(ScriptableObject))
            {
                if (asset.ScriptableObjectDictionary.ContainsKey(oldKey))
                {
                    var value = asset.ScriptableObjectDictionary[oldKey];
                    asset.ScriptableObjectDictionary.Remove(oldKey);
                    asset.ScriptableObjectDictionary[newKey] = value;
                }
            }
        }


        public static void ReorderDictionary(this TranslationAsset asset, Type translationType, IList keyList) 
        {
            if (translationType == typeof(string))
            {
                var secondNewDict = new TranslationAsset.StringDictionaryType();
                foreach (string key in keyList)
                {
                    if (!asset.TranslationDictionary.ContainsKey(key))
                        continue;
                    secondNewDict.Add(key, asset.TranslationDictionary[key]);
                }
                asset.TranslationDictionary = secondNewDict;
            }
            else if (translationType == typeof(Sprite))
            {
                var secondNewDict = new TranslationAsset.SpriteDictionaryType();
                foreach (string key in keyList)
                {
                    if (!asset.SpriteDictionary.ContainsKey(key))
                        continue;
                    secondNewDict.Add(key, asset.SpriteDictionary[key]);
                }
                asset.SpriteDictionary = secondNewDict;
            }
            else if (translationType == typeof(Texture))
            {
                var secondNewDict = new TranslationAsset.TextureDictionaryType();
                foreach (string key in keyList)
                {
                    if (!asset.TextureDictionary.ContainsKey(key))
                        continue;
                    secondNewDict.Add(key, asset.TextureDictionary[key]);
                }
                asset.TextureDictionary = secondNewDict;
            }
            else if (translationType == typeof(AudioClip))
            {
                var secondNewDict = new TranslationAsset.AudioDictionaryType();
                foreach (string key in keyList)
                {
                    if (!asset.AudioDictionary.ContainsKey(key))
                        continue;
                    secondNewDict.Add(key, asset.AudioDictionary[key]);
                }
                asset.AudioDictionary = secondNewDict;
            }
            else if (translationType == typeof(Font))
            {
                var secondNewDict = new TranslationAsset.FontDictionaryType();
                foreach (string key in keyList)
                {
                    if (!asset.FontDictionary.ContainsKey(key))
                        continue;
                    secondNewDict.Add(key, asset.FontDictionary[key]);
                }
                asset.FontDictionary = secondNewDict;
            }
            else if (translationType == typeof(ScriptableObject))
            {
                var secondNewDict = new TranslationAsset.ScriptableObjectDictionaryType();
                foreach (string key in keyList)
                {
                    if (!asset.ScriptableObjectDictionary.ContainsKey(key))
                        continue;
                    secondNewDict.Add(key, asset.ScriptableObjectDictionary[key]);
                }
                asset.ScriptableObjectDictionary = secondNewDict;
            }
        }

        public static IEnumerable<string> KeysForType(this TranslationAsset asset, Type translationType)
        {
            if (translationType == typeof (string))
                return asset.TranslationDictionary.Select(pair => pair.Key);
            if (translationType == typeof (Sprite))
                return asset.SpriteDictionary.Select(pair => pair.Key);
            if (translationType == typeof (Texture))
                return asset.TextureDictionary.Select(pair => pair.Key);
            if (translationType == typeof (AudioClip))
                return asset.AudioDictionary.Select(pair => pair.Key);
            if (translationType == typeof (Font))
                return asset.FontDictionary.Select(pair => pair.Key);
            if (translationType == typeof (ScriptableObject))
                return asset.ScriptableObjectDictionary.Select(pair => pair.Key);
            return null;
        }

        public static void DrawFieldForKey(this TranslationAsset asset, Type translationType, Rect rect, string key, GUIContent label = null)
        {
            if (translationType == typeof(string))
                asset.TranslationDictionary[key] = DrawTypedField(translationType, rect, asset.TranslationDictionary[key], label).ToString();
            else if (translationType == typeof(Sprite))
                asset.SpriteDictionary[key] = (Sprite) DrawTypedField(translationType, rect, asset.SpriteDictionary[key], label);
            else if (translationType == typeof(Texture))
                asset.TextureDictionary[key] = (Texture) DrawTypedField(translationType, rect, asset.TextureDictionary[key], label);
            else if (translationType == typeof(AudioClip))
                asset.AudioDictionary[key] = (AudioClip) DrawTypedField(translationType, rect, asset.AudioDictionary[key], label);
            else if (translationType == typeof(Font))
                asset.FontDictionary[key] = (Font) DrawTypedField(translationType, rect, asset.FontDictionary[key], label);
            if (translationType == typeof(ScriptableObject))
                asset.ScriptableObjectDictionary[key] = (ScriptableObject) DrawTypedField(translationType, rect, asset.ScriptableObjectDictionary[key], label);
        }

        public static void DrawFieldForKey(this TranslationAsset asset, Type translationType, Rect rect, string key, string label)
        {
            DrawFieldForKey(asset, translationType, rect, key, new GUIContent(label));
        }

        public static object DrawTypedField(Type type, Rect rect, object displayedObject, GUIContent label = null)
        {
            label = label ?? GUIContent.none;
            if (type == typeof(string))
            {
                return rect.height > 20 ? EditorGUI.TextArea(rect, displayedObject != null ? displayedObject.ToString() : "") : EditorGUI.TextField(rect, label, displayedObject != null ? displayedObject.ToString() : "");
            }
            return EditorGUI.ObjectField(rect, label, (UnityEngine.Object) displayedObject, type, true);
        }

        public static object DrawTypedField(Type type, Rect rect, object displayedObject, string label)
        {
            return DrawTypedField(type, rect, displayedObject, new GUIContent(label));
        }
    }
}