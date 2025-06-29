using Fove.Unity;
using UnityEngine;

public class FoveBackgroundController : MonoBehaviour
{
    [SerializeField] public Color backgroundColor = Color.blue;
    [SerializeField] public float luminance = 0.3f;

    void Start()
    {
        // Get the FOVE Interface camera
        FoveInterface foveInterface = GetComponentInChildren<FoveInterface>();
        if (foveInterface != null)
        {
            Camera foveCamera = foveInterface.gameObject.GetComponent<Camera>();
            if (foveCamera != null)
            {
                // Set the camera's background color based on luminance
                foveCamera.clearFlags = CameraClearFlags.SolidColor;
                foveCamera.backgroundColor = backgroundColor * luminance;
            }
        }
        else
        {
            Debug.LogError("Fove Interface not found under this GameObject.");
        }
    }

    private Color AdjustColorLuminance(Color color, float luminance)
    {
        // Normalize luminance to approximate the intended brightness
        float intensity = luminance / 1.0f; 
        return color * intensity;
    }
}
