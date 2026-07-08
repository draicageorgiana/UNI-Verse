using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Audio layers and volume slider abstracting (§4.4.2 —
/// Assets/Project/Scripts/VolumeSettings.cs).
///
/// Keeps background music and sound cues responsive by separating volume
/// adjustments from individual scene files: structural user inputs from the
/// visual slider components are converted into real-time float metrics that
/// directly modify the global volume channels, ensuring a consistent auditory
/// experience across different game environments.
///
/// Works in two modes:
///  • AudioMixer assigned  → sliders drive exposed mixer parameters
///    (decibel conversion applied), giving independent Music/SFX channels.
///  • No mixer assigned    → the master slider drives AudioListener.volume,
///    so the component still functions before a mixer asset exists.
/// Preferences persist as settings values and are re-applied on every scene.
/// </summary>
public class VolumeSettings : MonoBehaviour
{
    [Header("Global volume channels")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterParameter = "MasterVolume";
    [SerializeField] private string musicParameter = "MusicVolume";
    [SerializeField] private string sfxParameter = "SFXVolume";

    [Header("Slider components")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MasterPrefKey = "Settings_MasterVolume";
    private const string MusicPrefKey = "Settings_MusicVolume";
    private const string SfxPrefKey = "Settings_SFXVolume";

    void Start()
    {
        float master = PlayerPrefs.GetFloat(MasterPrefKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicPrefKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxPrefKey, 1f);

        InitializeSlider(masterSlider, master, SetMasterVolume);
        InitializeSlider(musicSlider, music, SetMusicVolume);
        InitializeSlider(sfxSlider, sfx, SetSfxVolume);

        // Re-apply persisted values so every scene starts consistent (§4.4.2).
        ApplyMaster(master);
        ApplyChannel(musicParameter, music);
        ApplyChannel(sfxParameter, sfx);
    }

    private static void InitializeSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> handler)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(handler);
    }

    // ---- Real-time channel updates (wired to sliders in code) ----------------

    public void SetMasterVolume(float value)
    {
        ApplyMaster(value);
        PlayerPrefs.SetFloat(MasterPrefKey, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        ApplyChannel(musicParameter, value);
        PlayerPrefs.SetFloat(MusicPrefKey, value);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        ApplyChannel(sfxParameter, value);
        PlayerPrefs.SetFloat(SfxPrefKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyMaster(float value)
    {
        if (audioMixer != null)
        {
            ApplyChannel(masterParameter, value);
        }
        else
        {
            // Fallback global channel: keeps the slider functional before an
            // AudioMixer asset has been created and assigned.
            AudioListener.volume = Mathf.Clamp01(value);
        }
    }

    private void ApplyChannel(string parameterName, float value)
    {
        if (audioMixer == null || string.IsNullOrEmpty(parameterName)) return;

        audioMixer.SetFloat(parameterName, LinearToDecibels(value));
    }

    /// <summary>Converts a 0..1 slider metric into the mixer's decibel scale.</summary>
    private static float LinearToDecibels(float linear)
    {
        return linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
    }
}
