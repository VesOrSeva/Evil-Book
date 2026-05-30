using UnityEngine;
using UnityEngine.UI;

public class FinalSequence : Singleton<FinalSequence>
{
    [SerializeField] Book book;
    [SerializeField] GameObject sequence;
    [SerializeField] Button Ves;
    [SerializeField] Button AA;
    [SerializeField] Button closeButton;

    private void Start()
    {
        Ves.onClick.AddListener(VesProtfolio);
        AA.onClick.AddListener(AAProtfolio);
        closeButton.onClick.AddListener(ExitGame);
    }
    public void StartTheFinally()
    {
        book.ForceLock();
        sequence.SetActive(true);
        MusicManager.Instance.SetVolume(SourceType.Main, 0f, 3f);
        MusicManager.Instance.SetVolume(SourceType.Finally, 0.8f, 15f);
    }
    private void VesProtfolio()
    {
        Application.OpenURL("https://famona.itch.io/");
    }
    private void AAProtfolio()
    {
        Application.OpenURL("https://3threeteacups.itch.io/");
    }

    public void ExitGame()
    {
        Debug.Log("Closing the application");
        Application.Quit();
    }
}
