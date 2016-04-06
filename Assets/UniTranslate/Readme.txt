Thanks for choosing UniTranslate!
This Readme file will guide you through all the steps needed to set up UniTranslate for your project.

1.  Add at least one translation asset to your project. It is available in the menu via
    Assets - Create - Translation.
2.  You can open the Edit Translations window via Tools - Translations - Edit Translations
3.  You can now add a LocalizedText component to your UI Text objects. To use an existing
    key, enter it in the Key field. If the key is recognized, you can directly edit all translations
	for the entered key. If the entered key does not exist, you can assign translations and press "Add"
	to add the key and its corresponding translations to all translation assets.
4.  To use UniTranslate from your code, call the Translator.Translate(string key) function. To make sure
    that an entered key exists, add a member variable for the key to your script and add the
	[TranslationKey] attribute, for example:

	[TranslationKey] public string myKey;

	It will show a UI similar to that of the LocalizedText component.

-- Replacing tokens --
  If you want to replace tokens in your translation string dynamically, you can put those values in
  {curly brackets} and use the Translator.Translate(string key, object replacementTokens) function
  and pass an anonymous object with the desired values:
  
  Translator.Translate("Test.XYZ", new {val1 = "Hello", val2 = "World"});
  turns "{val1}, {val2}!" into "Hello, World!" if "{val1}, {val2}!" is assigned to the key "Text.XYZ".

-- Changing the active language --
  The language can be changed by assigning a different TranslationAsset to the Translator, e.g. with
  the following code: Translator.Instance.Translation = myTranslationAsset;