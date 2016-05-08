using UnityEngine;

/// <summary>
/// Localizes the text of <see cref="AudioSource"/> components.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("UniTranslate/Localized Audio")]
[ExecuteInEditMode]
public class LocalizedAudio : LocalizedAudioComponent
{
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        UpdateTranslation();
    }
    
    public override void UpdateTranslation()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            audioSource = GetComponent<AudioSource>(); //Null reference fix
            if (Translator.Instance == null || Translator.Instance.Translation == null
                || !Translator.AudioExists(key))
                return;
        }
#endif
        audioSource.clip = Translator.TranslateAudio(key);
    }
}
