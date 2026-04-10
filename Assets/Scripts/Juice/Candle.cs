using UnityEngine;

public class Candle : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] Light LightSource;

    [Header("Flicker Settings")]
    [SerializeField] float BaseIntensity = 1.2f;
    [SerializeField] float FlickerAmount = 0.3f;
    [SerializeField] float FlickerSpeed = 8f;

    [Header("Movement Flicker")]
    [SerializeField] float PositionJitter = 0.02f;
    [SerializeField] bool IsLit = true;

    private Vector3 _initialLightPosition;
    private float _noiseSeed;

    private void Awake()
    {
        _initialLightPosition = LightSource.transform.localPosition;
        _noiseSeed = Random.Range(0f, 100f);

        SetLit(IsLit);
    }

    private void Update()
    {
        if (!IsLit) return;

        HandleFlicker();
    }

    private void HandleFlicker()
    {
        float time = Time.time * FlickerSpeed;
        float noise = Mathf.PerlinNoise(_noiseSeed, time);

        LightSource.intensity = BaseIntensity + (noise - 0.5f) * FlickerAmount;

        Vector3 offset = new Vector3(
            (Mathf.PerlinNoise(time, _noiseSeed) - 0.5f) * PositionJitter,
            (Mathf.PerlinNoise(_noiseSeed, time * 0.5f) - 0.5f) * PositionJitter, 0f);

        LightSource.transform.localPosition = _initialLightPosition + offset;
    }

    public void SetLit(bool value)
    {
        IsLit = value;

        LightSource.enabled = value;
    }

    public void Toggle()
    {
        SetLit(!IsLit);
    }

    public void Extinguish()
    {
        SetLit(false);
    }

    public void Ignite()
    {
        SetLit(true);
    }
}