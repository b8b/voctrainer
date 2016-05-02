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
            {
                asset.TranslationDictionary.Add("", "");
            }
            else if (translationType == typeof (Sprite))
            {
                asset.SpriteDictionary.Add("", null);
            }
        }

        public static void Add(this TranslationAsset asset, Type translationType, string key, object value = null)
        {
            if (translationType == typeof(string))
            {
                asset.TranslationDictionary.Add(key, value as string ?? "");
            }
            else if (translationType == typeof(Sprite))
            {
                asset.SpriteDictionary.Add(key, value as Sprite);
            }
        }

        public static bool KeyExists(this TranslationAsset asset, Type translationType, string key)
        {
            return ((translationType == typeof (string) && asset.TranslationDictionary.ContainsKey(key)) ||
                    (translationType == typeof (Sprite) && asset.SpriteDictionary.ContainsKey(key))) ;
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
            {
                asset.TranslationDictionary.Remove(key);
            }
            else if (translationType == typeof(Sprite))
            {
                asset.SpriteDictionary.Remove(key);
            }
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
        }

        public static IEnumerable<string> KeysForType(this TranslationAsset asset, Type translationType)
        {
            if (translationType == typeof (string))
                return asset.TranslationDictionary.Select(pair => pair.Key);
            if (translationType == typeof (Sprite))
                return asset.SpriteDictionary.Select(pair => pair.Key);
            return null;
        }

        public static void DrawFieldForKey(this TranslationAsset asset, Type translationType, Rect rect, string key, GUIContent label = null)
        {
            if (translationType == typeof(string))
            {
                asset.TranslationDictionary[key] = DrawTypedField(translationType, rect, asset.TranslationDictionary[key], label).ToString();
            }
            else if (translationType == typeof(Sprite))
            {
                asset.SpriteDictionary[key] = (Sprite) DrawTypedField(translationType, rect, asset.SpriteDictionary[key], label);
            }
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
                return rect.height > 20 ? EditorGUI.TextArea(rect, displayedObject.ToString()) : EditorGUI.TextField(rect, label, displayedObject.ToString());
            }
            return EditorGUI.ObjectField(rect, label, (UnityEngine.Object) displayedObject, type, true);
        }

        public static object DrawTypedField(Type type, Rect rect, object displayedObject, string label)
        {
            return DrawTypedField(type, rect, displayedObject, new GUIContent(label));
        }
    }
}