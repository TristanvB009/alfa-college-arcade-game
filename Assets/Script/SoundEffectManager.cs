using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    //play a sound effect
    // SoundEffectManager.Play("LIBRARY NAME");
    // Ex.: SoundEffectManager.Play("Footsteps");
    private static SoundEffectManager _instance;

    private static AudioSource _audioSource;
    private static SoundEffectLibrary _soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
            _soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // New: Play with per-call volume scale (0..1). This is multiplied by the AudioSource.volume.
    public static void Play(string soundName, float volumeScale)
    {
        if (_soundEffectLibrary == null || _audioSource == null) return;
        AudioClip audioClip = _soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
            _audioSource.PlayOneShot(audioClip, Mathf.Clamp01(volumeScale));
        }
    }

    // Backwards-compatible: plays at full per-clip scale (1.0)
    public static void Play(string soundName)
    {
        Play(soundName, 1f);
    }

    void Start()
    {
        // Preload SFX audio data to reduce first-play latency
        _soundEffectLibrary?.PreloadAll();

        if (sfxSlider != null)
        {
            // Wire slider directly to static SetVolume so we don't need an instance reference in the inspector.
            sfxSlider.onValueChanged.AddListener(SetVolume);

            // Initialize slider to current audio source volume if available
            if (_audioSource != null)
                sfxSlider.value = _audioSource.volume;
        }
    }

    public static void SetVolume(float volume)
    {
        if (_audioSource == null) return;
        _audioSource.volume = Mathf.Clamp01(volume);
    }

    // kept for compatibility with existing code patterns (optional)
    public void OnValueChanged()
    {
        if (sfxSlider != null)
            SetVolume(sfxSlider.value);
    }

    private void OnDestroy()
    {
        // Unsubscribe the slider listener if object is destroyed to avoid dangling listeners
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetVolume);

        if (_instance == this)
            _instance = null;
    }
}
