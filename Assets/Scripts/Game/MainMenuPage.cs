using BookGraph.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPage : OnPageContent
{
    [SerializeField] Button exitButton;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider soundSlider;
    [SerializeField] Slider musicSlider;

    public override void Initialize(RuntimeNode node, GameObject originalPage)
    {
        this.originalPage = originalPage.GetComponent<B_SpecialPage>();

        exitButton.onClick.AddListener(ExitGame);
        masterSlider.SetValueWithoutNotify(MixerManager.Instance.GetMasterVolume);
        soundSlider.SetValueWithoutNotify(MixerManager.Instance.GetSoundVolume);
        musicSlider.SetValueWithoutNotify(MixerManager.Instance.GetMusicVolume);
    }

    public void SetMasterVolume(float volume)
    {
        MixerManager.Instance.SetMasterVolume(volume);
    }
    public void SetSoundVolume(float volume)
    {
        MixerManager.Instance.SetSoundVolume(volume);
    }
    public void SetMusicVolume(float volume)
    {
        MixerManager.Instance.SetMusicVolume(volume);
    }

    public void ExitGame()
    {
        Debug.Log("Closing the application");
        ApplicationManager.Instance.ExitGame();
    }
}