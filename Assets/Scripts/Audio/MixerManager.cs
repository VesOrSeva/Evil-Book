using UnityEngine;
using UnityEngine.Audio;

public class MixerManager : Singleton<MixerManager>
{
    [SerializeField] AudioMixer mixer;
    private float previousMasterVolume = 0.8f;
    private float previousSoundVolume = 0.8f;
    private float previousMusicVolume = 0.8f;
    public float GetMasterVolume => previousMasterVolume;
    public float GetSoundVolume => previousSoundVolume;
    public float GetMusicVolume => previousMusicVolume;

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
        previousMasterVolume = volume;
    }
    public void SetSoundVolume(float volume)
    {
        mixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20f);
        previousSoundVolume = volume;
    }
    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
        previousMusicVolume = volume;
    }
}
