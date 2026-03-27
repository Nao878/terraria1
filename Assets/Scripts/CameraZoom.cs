using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minSize = 3f;
    public float maxSize = 20f;
    public float zoomSpeed = 0.01f; // Scroll value is different in New Input System
    
    private Camera cam;
    private MonoBehaviour vcam; // Cinemachine Camera Support

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Try to find Cinemachine Lens if attached to this object
        vcam = GetComponent("Unity.Cinemachine.CinemachineCamera") as MonoBehaviour;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            if (vcam != null)
            {
                // Adjust via Reflection for Cinemachine 3
                var type = vcam.GetType();
                var lensProp = type.GetProperty("Lens");
                if (lensProp != null)
                {
                    var lens = lensProp.GetValue(vcam);
                    var lensType = lens.GetType();
                    var orthoField = lensType.GetField("OrthographicSize");
                    if (orthoField != null)
                    {
                        float currentSize = (float)orthoField.GetValue(lens);
                        float nextSize = Mathf.Clamp(currentSize - scroll * zoomSpeed, minSize, maxSize);
                        orthoField.SetValue(lens, nextSize);
                        lensProp.SetValue(vcam, lens);
                    }
                }
            }
            else if (cam != null)
            {
                float newSize = cam.orthographicSize - scroll * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(newSize, minSize, maxSize);
            }
        }
    }
}
