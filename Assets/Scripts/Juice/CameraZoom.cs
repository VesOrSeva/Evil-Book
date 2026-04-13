using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
    [SerializeField] float zoomSpeed = 10f;
    [SerializeField] float minFOV = 30f;
    [SerializeField] float maxFOV = 80f;
    [SerializeField] float smooth = 0.1f;
    [SerializeField] float zoomOffsetOnY = -0.1f;

    private CinemachineRotationComposer composer;
    private float targetFOV;
    private float currentVelocity;

    private void Awake()
    {
        if (cam == null)
        {
            enabled = false;
            return;
        }

        targetFOV = cam.Lens.FieldOfView;
        composer = cam.GetComponent<CinemachineRotationComposer>();
    }

    private void Update()
    {
        HandleZoomInput();
        ApplyZoom();
        ApplyComposerOffset();
    }

    private void HandleZoomInput()
    {
        float scrollInput = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollInput) < 0.01f) return;

        targetFOV -= scrollInput * zoomSpeed;
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
    }

    private void ApplyZoom()
    {
        float currentFOV = cam.Lens.FieldOfView;
        float smoothedFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref currentVelocity, smooth);

        cam.Lens.FieldOfView = smoothedFOV;
    }

    private void ApplyComposerOffset()
    {
        float t = Mathf.InverseLerp(maxFOV, minFOV, cam.Lens.FieldOfView);
        float targetY = Mathf.Lerp(0f, zoomOffsetOnY, t);
        float smoothedY = Mathf.SmoothDamp(composer.Composition.ScreenPosition.y, targetY, ref currentVelocity, smooth);

        var composition = composer.Composition;
        composition.ScreenPosition.y = smoothedY;
        composer.Composition = composition;
    }
}