using UnityEngine;
using UnityEngine.UI;

public class newBG : MonoBehaviour
{
    private GameObject canvasGO;
    private GameObject fixationLight;

    // when the scene stars it initializes the background and the fixation light
    void Start()
    {
        // Create Background Canvas
        CreateBackground();

        // Create Fixation Light
        CreateFixationLight();
    }

    void CreateBackground()
    {
        // Creates a new GameObject named BackgroundCanvas
        canvasGO = new GameObject("BackgroundCanvas");
        // Adds a Canvas component, which acts as a container for UI elements
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        // Sets the canvas to always overlays the screen
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Scale correctly the canvas across different screen sizes
        CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Background Panel
        // Creates a child GameObject to "BackgroundCanvas" to hold the background color
        GameObject backgroundPanelGO = new GameObject("BackgroundPanel");
        backgroundPanelGO.transform.SetParent(canvasGO.transform, false);

        // Configures the RectTransform to stretch and cover the entire canvas
        RectTransform backgroundRect = backgroundPanelGO.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        // Adds an Image component to display the background
        Image backgroundImage = backgroundPanelGO.AddComponent<Image>();
        // Sets the color to a dim white (0.04 cd/m²)
        backgroundImage.color = AdjustBrightness(Color.white, 0.04f);
        // Ensure Background is Rendered Below Everything
        canvas.sortingOrder = 0;
    }

    void CreateFixationLight()
    {
        // Creates a new GameObject named FixationLight and attaches it to the "BackgroundCanvas"
        fixationLight = new GameObject("FixationLight");
        fixationLight.transform.SetParent(canvasGO.transform, false);

        // Adds a RectTransform to position and size the fixation light at the center of the screen
        RectTransform fixationRect = fixationLight.AddComponent<RectTransform>();
        fixationRect.sizeDelta = new Vector2(10, 10); // Fixation size
        fixationRect.anchoredPosition = Vector2.zero; // Center position

        // Adds an Image component to display the fixation light
        Image fixationImage = fixationLight.AddComponent<Image>();
        // Sets its color to 6 cd/m²
        fixationImage.color = AdjustBrightness(Color.white, 6.0f); // Fixation light
    }

    // Multiplies the RGB channels of a color by the luminance value to simulate brightness adjustment
    private Color AdjustBrightness(Color baseColor, float luminance)
    {
        return new Color(baseColor.r * luminance, baseColor.g * luminance, baseColor.b * luminance, baseColor.a);
    }
}
