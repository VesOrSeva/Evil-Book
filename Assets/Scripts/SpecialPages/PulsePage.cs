using UnityEngine;
using UnityEngine.Rendering;

public class PulsePage : MonoBehaviour
{
    [SerializeField] Volume volume;
    [SerializeField] float minWeight = 0.5f;
    [SerializeField] float speed = 3f;

    private void Awake()
    {
        volume.weight = 0f;
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        volume.weight = Mathf.Lerp(minWeight, 1f, t);
    }
}