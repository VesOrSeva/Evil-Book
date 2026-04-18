using UnityEngine;

public class FinalSequence : Singleton<FinalSequence>
{
    [SerializeField] Book book;
    [SerializeField] GameObject sequence;

    public void StartTheFinally()
    {
        book.ForceLock();
        sequence.SetActive(true);
        MusicManager.Instance.SetVolume(SourceType.Main, 0f, 3f);
        MusicManager.Instance.SetVolume(SourceType.Finally, 0.8f, 15f);
    }
}
