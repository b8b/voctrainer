public static class UniTranslateExtensions
{
    public static string TranslateKey(this string key)
    {
        return Translator.Translate(key);
    }

    public static string TranslateKey(this string key, object replacementValues)
    {
        return Translator.Translate(key, replacementValues);
    }
}
