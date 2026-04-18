using UnityEngine;
using UnityEngine.UI;

public class MainMenuSliders : MonoBehaviour // a veryyyyy bad script but I'm super lazyy at this point
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider soundSlider;
    [SerializeField] Slider musicSlider;

    private void Update()
    {
        masterSlider.SetValueWithoutNotify(MixerManager.Instance.GetMasterVolume);
        soundSlider.SetValueWithoutNotify(MixerManager.Instance.GetSoundVolume);
        musicSlider.SetValueWithoutNotify(MixerManager.Instance.GetMusicVolume);
    }
}
