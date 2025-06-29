using UnityEngine;
using UnityEngine.UI;
using Fove.Unity;
using System.Collections;
using System.Collections.Generic;
using LSL;

public class StimuTest : MonoBehaviour
{
    private GameObject redSphere;
    private GameObject blueSphere;
    private GameObject canvas;
    private FoveInterface foveInterface;
    private GameObject fixationLight;

    public float stimulationDurationRedBlue = 0.5f; // Duration for each square (0.5 sec)
    public float intervalDurationRedBlue = 3.0f; // Time between stimuli (3 sec)
    public float stimulationDurationLongBlue = 8f; // Duration long blue square (8 sec)
    public float intervalDurationLongBlue = 8f; // Time between stimuli (8 sec)
    public float blueCircleSize = 0.1f;
    public float redCircleSize = 0.1f;
    public float blueLuminance = 0.3f;
    public float redLuminance = 0.3f;
    public float LongBlueLuminance = 6000.0f;
    public float fixationLightSize = 0.09f;
    public float fixationLightLuminance = 0.5f;
    public float WaitForSec = 30;
    public Vector3[] vectorPositions = new Vector3[]
    {
        new Vector3(0, 0, 2.0f),     // Center
        new Vector3(-0.73f, 0, 2.0f), // Nasal (Left)
        new Vector3(0.73f, 0, 2.0f),  // Temporal (Right)
        new Vector3(0, 0.73f, 2.0f),  // Superior (Up)
        new Vector3(0, -0.73f, 2.0f)  // Inferior (Down)
    };

   // For futer use, test only one eye at each time 
    private bool leftEye = true;
    private bool rightEye = true;

    // Controls the streams for the LabRecorder.exe
    private LSL.StreamOutlet pupilOutlet; // LSL outlet for pupil data
    private LSL.StreamOutlet eventOutlet; // LSL outlet for event markers

    void Start()
    {
        // Get the FoveInterface component
        foveInterface = FindObjectOfType<FoveInterface>();
        if (foveInterface == null)
        {
            Debug.LogError("FoveInterface not found in the scene. Please add it to ensure proper functionality.");
            return;
        }

        // Create a Canvas
        canvas = new GameObject("BlinkingCanvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.WorldSpace;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000, 1000);
        canvas.transform.localScale = Vector3.one * 0.001f;

        // Create the spheres;
        redSphere = CreateSphereStimulus("RedSphere", Color.red, redCircleSize, redLuminance);
        blueSphere = CreateSphereStimulus("BlueSphere", Color.blue, blueCircleSize, blueLuminance);

        redSphere.SetActive(false);
        blueSphere.SetActive(false);

        // Call fove SDK for PupilRadius
        FoveManager.RegisterCapabilities(Fove.ClientCapabilities.PupilRadius);

        // Create the fixation light
        CreateFixationLight();

        // Initialize LSL streams
        InitializeLSL();

        // For futer use, test only one eye at each time 
        //ControlEyesRendering();

        // Start the stimulus presentation coroutine
        StartCoroutine(ShowStimuliSequence());
    }

    void ControlEyesRendering() 
    {
        Camera foveCamera = foveInterface.GetComponent<Camera>();

        if (leftEye && rightEye)
        {
            foveCamera.stereoTargetEye = StereoTargetEyeMask.Both;
        }
        else if (leftEye && !rightEye)
        {
            foveCamera.stereoTargetEye = StereoTargetEyeMask.Left;
        }
        else if (!leftEye && rightEye)
        {
            foveCamera.stereoTargetEye = StereoTargetEyeMask.Right;
        }
        else 
        {
            Debug.LogError("ERROR: no eye was selected.");
            return;
        }
    }

    GameObject CreateSphereStimulus(string name, Color color, float size, float luminance)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(transform, false);

        // Adjust size
        sphere.transform.localScale = Vector3.one * size;  
        sphere.SetActive(false);

        AdjustMaterial(sphere, color, luminance);

        return sphere;
    }

    void AdjustMaterial(GameObject sphere, Color color, float luminance) 
    {
        Material sphereMaterial = new Material(Shader.Find("Unlit/Color"));
        sphereMaterial.color = AdjustColorLuminance(color, luminance);
        sphere.GetComponent<Renderer>().material = sphereMaterial;
    }

    void CreateFixationLight()
    {
        fixationLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fixationLight.name = "FixationLight";
        fixationLight.transform.SetParent(transform, false);
        fixationLight.layer = LayerMask.NameToLayer("Default");

        // Make the fixation light smaller than the spheres
        fixationLight.transform.localScale = Vector3.one * fixationLightSize; // Smaller size than the squares

        // Add material and set luminance
        AdjustMaterial(fixationLight, Color.white, fixationLightLuminance);
    }

    void InitializeLSL()
    {
        // Pupil data stream
        var pupilStreamInfo = new LSL.StreamInfo("UnityPupilData", "PupilData", 2, 100, LSL.channel_format_t.cf_float32, "pupil_stream");
        pupilOutlet = new LSL.StreamOutlet(pupilStreamInfo);

        // Event markers stream
        var eventStreamInfo = new StreamInfo("UnityEventStream", "Markers", 1, 0, LSL.channel_format_t.cf_string, "event_stream");
        eventOutlet = new LSL.StreamOutlet(eventStreamInfo);

        Debug.Log("LSL streams initialized.");
    }

    Color AdjustColorLuminance(Color color, float luminance)
    {
        // Normalize luminance
        float intensity = luminance / 1.0f; 
        return color * intensity;
    }

    IEnumerator ShowStimuliSequence()
    {
        // Create Focal Chromatic Light Stimuli
        string eventName;
        bool isRed = true;

        // Wait for ${WaitForSec} before starting the stimulation
        yield return new WaitForSeconds(WaitForSec);

        // Iterate 2 times, one for all red lights and one for all blue lights
        for (int j = 0; j < 2; j++)
        {

            for (int i = 0; i < vectorPositions.Length; i++)
            {
                // Check if it's the red or blue iteration
                GameObject currentsphere = isRed ? redSphere : blueSphere;
                currentsphere.SetActive(true);

                Vector3 offsetPosition = vectorPositions[i];
                StartCoroutine(UpdateSpherePosition(currentsphere, offsetPosition));

                eventName = "Start " + (isRed ? "Red" : "Blue") + $" sphere for {stimulationDurationRedBlue} seconds in {vectorPositions[i]}.";
                SendEvent(eventName);

                yield return new WaitForSeconds(stimulationDurationRedBlue);

                currentsphere.SetActive(false);
                StopCoroutine(UpdateSpherePosition(currentsphere, offsetPosition));

                eventName = "Stop " + (isRed ? "Red" : "Blue") + $" sphere in {vectorPositions[i]}.";
                SendEvent(eventName);

                yield return new WaitForSeconds(intervalDurationRedBlue);

            }

            // Set red spherer off when ending the fist iteration
            isRed = false;
        }

        // Start long blue light stimulation

        // Adjust blue sphere color for high intensity (6000 cd/m²)
        AdjustMaterial(blueSphere, Color.blue, LongBlueLuminance);

        for (int j = 0; j < vectorPositions.Length; j++)
        {
            Vector3 offsetPosition = vectorPositions[j];

            GameObject currentBlueSphere = blueSphere;
            currentBlueSphere.SetActive(true);

            StartCoroutine(UpdateSpherePosition(currentBlueSphere, offsetPosition));

            eventName = $"Start focal blue light Stimulation in {offsetPosition} for {stimulationDurationLongBlue} seconds.";
            SendEvent(eventName);

            yield return new WaitForSeconds(stimulationDurationLongBlue);

            currentBlueSphere.SetActive(false);
            StopCoroutine(UpdateSpherePosition(blueSphere, offsetPosition));

            eventName = $"Stop focal blue light Stimulation in {offsetPosition}.";
            SendEvent(eventName);

            yield return new WaitForSeconds(intervalDurationLongBlue);
        }

        EndRun();
    }

    IEnumerator UpdateSpherePosition(GameObject sphere, Vector3 offsetPosition)
    {
        while (sphere.activeSelf)
        {
            Vector3 targetPosition = foveInterface.transform.position + foveInterface.transform.rotation * offsetPosition;
            sphere.transform.position = targetPosition;
            yield return null;
        }
    }

    void SendPupilData()
    {
        var leftPupil = FoveManager.GetPupilRadius(Fove.Eye.Left);
        var rightPupil = FoveManager.GetPupilRadius(Fove.Eye.Right);

        float left = leftPupil.IsValid ? leftPupil.value * 1000f : -1f;
        float right = rightPupil.IsValid ? rightPupil.value * 1000f : -1f;

        pupilOutlet.push_sample(new float[] { left, right });
        Debug.Log($"Sent Pupil Data: Left={left}, Right={right}");
    }

    void SendEvent(string eventName)
    {
        eventOutlet.push_sample(new string[] { eventName });
        Debug.Log($"Sent Event: {eventName}");
    }

    void Update()
    {
        // Log eye data
        SendPupilData();

        // Update Fixation Light's position and ensure it's active every frame
        if (fixationLight != null)
        {
            UpdateFixationLight();
        }
    }

    void UpdateFixationLight()
    {
        // Ensure the fixation light is active (in case LSL deactivates it)
        if (!fixationLight.activeSelf)
        {
            fixationLight.SetActive(true);
        }

        // Reposition it in case something is affecting its placement
        fixationLight.transform.position = foveInterface.transform.position + foveInterface.transform.forward * 8.5f;
    }

    void EndRun()
    {
        Debug.Log("Stimuli sequence finished. Quitting application.");
        UnityEditor.EditorApplication.isPlaying = false;
    }

}
