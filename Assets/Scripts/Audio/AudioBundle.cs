using UnityEngine;

public class AudioBundle : Singleton<AudioBundle>
{
    [SerializeField] AudioClip[] pageFlipingStart;
    public AudioClip[] GetFlipingStartClips => pageFlipingStart;

    [SerializeField] AudioClip[] pageFlipingEnd;
    public AudioClip[] GetFlipingEndClips => pageFlipingEnd;
}
