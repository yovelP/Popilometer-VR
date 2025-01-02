using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class newStimu : MonoBehaviour
{
    private GameObject stimulusCanvas;
    private GameObject stimulus;
    private Vector2[] vfLocations;

    void Start()
    {
        // Create Stimulus Canvas
        CreateStimulusCanvas();

        // Define Visual Field Locations (~4° and ~21° eccentricities)
        vfLocations = new Vector2[]
        {
            new Vector2(0, 0),      // Central (~4°)
            new Vector2(-200, 0),  // Nasal (~21° left)
            new Vector2(200, 0),   // Temporal (~21° right)
            new Vector2(0, 200),   // Superior (~21° up)
            new Vector2(0, -200)   // Inferior (~21° down)
        };

        // Start Stimuli Presentation
        StartCoroutine(PresentStimuli());
    }

    void CreateStimulusCanvas()
    {
        // Creates a new Canvas and set it to ScreenSpaceOverlay mode, above the background
        stimulusCanvas = new GameObject("StimulusCanvas");
        Canvas canvas = stimulusCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler canvasScaler = stimulusCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        stimulusCanvas.AddComponent<GraphicRaycaster>();

        // Ensure Stimulus Canvas Renders Above Background
        canvas.sortingOrder = 1;
    }

    IEnumerator PresentStimuli()
    {
        // Regular red and blue stimuli sequence
        for(int i = 0; i < 1; i++)
//        while (true)
        {
            // For each VF location, Displays a red stimulus for 0.5 sec,
            // and wait for 3.5 sec (0.5 duration + 3 in between intervals)
            foreach (var location in vfLocations)
            {
                // Red Light Stimuli
                ShowStimulus(Color.red, location, 1000, 0.5f); // Cone stimulation
//                ShowStimulus(WavelengthToRGB(624), location, 1000, 0.5f);
                yield return new WaitForSeconds(3.5f);

                // Blue Light Stimuli
                ShowStimulus(Color.blue, location, 170, 0.5f); // Rod stimulation
//                ShowStimulus(WavelengthToRGB(485), location, 170, 0.5f);
                yield return new WaitForSeconds(3.5f);
            }

            // After the Stimulus Presentation ends, start Melanopsin Ganglion Cells Testing
            yield return StartCoroutine(PresentMelanopsinStimuli());
        }
    }

    void ShowStimulus(Color color, Vector2 position, float luminance, float duration)
    {
        if (stimulus == null)
        {
            // Create Stimulus Object
            stimulus = new GameObject("Stimulus");
            stimulus.transform.SetParent(stimulusCanvas.transform, false);

            RectTransform rect = stimulus.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(50, 50); // Stimulus size
            stimulus.AddComponent<Image>();
        }

        // Position Stimulus at the Visual Field Location
        RectTransform rectTransform = stimulus.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        // Set Stimulus Color and Luminance
        Image image = stimulus.GetComponent<Image>();
        image.color = AdjustBrightness(color, luminance);

        // Hide Stimulus After Duration
        StartCoroutine(HideStimulusAfter(duration));

        Debug.Log($"Stimulus created at {position} with color {color}");
    }

    // Waits for the stimulus duration, then hides the stimulus by setting its color to fully transparent
    IEnumerator HideStimulusAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        stimulus.GetComponent<Image>().color = new Color(0, 0, 0, 0); // Make it invisible
    }

    private Color AdjustBrightness(Color baseColor, float luminance)
    {
        return new Color(baseColor.r * luminance, baseColor.g * luminance, baseColor.b * luminance, baseColor.a);
    }

    IEnumerator PresentMelanopsinStimuli()
    {
        // Set a dim blue background
        SetBackground(Color.blue, 0.04f);

//        for (int i = 0; i < vfLocations.Length; i++) // do we need only focal?
        for (int i = 0; i < 1; i++)
        {
            // Focal Blue Light Stimuli for melanopsin
            ShowStimulus(Color.blue, vfLocations[i], 6000, 8.0f); // 8 seconds duration
            yield return new WaitForSeconds(16.0f);               // 8 seconds stimulus + 8 seconds interval
        }

//        // Reset background after melanopsin testing - do we need to?
//        SetBackground(Color.white, 0.04f); // Restore the regular dim white background
    }

    void SetBackground(Color color, float luminance)
    {
        GameObject background = GameObject.Find("BackgroundPanel");
        if (background != null)
        {
            Image bgImage = background.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = AdjustBrightness(color, luminance);
            }
        }
    }

    // Calculates RGB values for a given wavelength within the visible spectrum
    private Color WavelengthToRGB(float wavelength)
    {
        float gamma = 0.8f; // Gamma correction
        float r = 0, g = 0, b = 0;

        if (wavelength >= 380 && wavelength < 440)
        {
            r = -(wavelength - 440) / (440 - 380);
            g = 0.0f;
            b = 1.0f;
        }
        else if (wavelength >= 440 && wavelength < 490)
        {
            r = 0.0f;
            g = (wavelength - 440) / (490 - 440);
            b = 1.0f;
        }
        else if (wavelength >= 490 && wavelength < 510)
        {
            r = 0.0f;
            g = 1.0f;
            b = -(wavelength - 510) / (510 - 490);
        }
        else if (wavelength >= 510 && wavelength < 580)
        {
            r = (wavelength - 510) / (580 - 510);
            g = 1.0f;
            b = 0.0f;
        }
        else if (wavelength >= 580 && wavelength < 645)
        {
            r = 1.0f;
            g = -(wavelength - 645) / (645 - 580);
            b = 0.0f;
        }
        else if (wavelength >= 645 && wavelength <= 780)
        {
            r = 1.0f;
            g = 0.0f;
            b = 0.0f;
        }

        // Intensity correction
        float factor = 0.0f;
        if (wavelength >= 380 && wavelength < 420)
            factor = 0.3f + 0.7f * (wavelength - 380) / (420 - 380);
        else if (wavelength >= 420 && wavelength < 645)
            factor = 1.0f;
        else if (wavelength >= 645 && wavelength <= 780)
            factor = 0.3f + 0.7f * (780 - wavelength) / (780 - 645);

        r = Mathf.Pow(r * factor, gamma);
        g = Mathf.Pow(g * factor, gamma);
        b = Mathf.Pow(b * factor, gamma);

        return new Color(r, g, b);
    }
}
