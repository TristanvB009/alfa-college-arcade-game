using System.Collections;
using UnityEngine;

public class GrandGate : MonoBehaviour
{
    [Header("Gate Components")]
    [Tooltip("Main gate animator for GateOpen, GateOffIdle, GateOnIdle animations")]
    public Animator gateAnimator;
    
    [Header("Individual Color Lights")]
    [Tooltip("Purple light object (Terminal ID 0)")]
    public GameObject purpleLights;
    public Animator purpleAnimator;
    
    [Tooltip("DarkBlue light object (Terminal ID 1)")]
    public GameObject darkBlueLights;
    public Animator darkBlueAnimator;
    
    [Tooltip("Pink light object (Terminal ID 2)")]
    public GameObject pinkLights;
    public Animator pinkAnimator;
    
    [Tooltip("LightBlue light object (Terminal ID 3)")]
    public GameObject lightBlueLights;
    public Animator lightBlueAnimator;
    
    [Header("All Lights Object")]
    [Tooltip("Object with all colors for final gate opening")]
    public GameObject allLightsObject;
    public Animator allLightsAnimator;
    
    [Header("Gate Spotlights")]
    [Tooltip("Additional spotlights to deactivate when gate opens")]
    public GameObject[] gateSpotlights = new GameObject[4];
    
    [Header("Camera Pan System")]
    [Tooltip("Main camera to pan (leave empty to auto-find)")]
    public Camera mainCamera;
    
    [Tooltip("Transform to pan camera to when showing gate (position the camera should look at)")]
    public Transform gateCameraTarget;
    
    [Tooltip("Duration of camera pan animation in seconds")]
    public float panDuration = 2f;
    
    [Tooltip("How long to hold on gate view before panning back")]
    public float holdDuration = 1.5f;
    
    [Tooltip("Ease curve for camera movement")]
    public AnimationCurve panCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    // Track which terminals have been activated
    private bool[] terminalActivated = new bool[4]; // IDs 0, 1, 2, 3
    private bool[] lightsActivated = new bool[4]; // Track which lights are on
    private bool gateOpened = false;
    
    // Camera pan system
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isPanning = false;
    
    // Player control management
    private PlayerController playerController;
    private bool playerControlsWereDisabled = false;

    void Start()
    {
        // Initialize gate in off idle state
        if (gateAnimator != null)
        {
            gateAnimator.SetBool("GateOffIdle", true);
            gateAnimator.SetBool("GateOnIdle", false);
        }
        
        // Auto-find main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        // Store original camera position and rotation
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }
    }

    // Called when a terminal is activated
    public void OnTerminalActivated(int terminalId)
    {
        if (terminalId < 0 || terminalId > 3 || terminalActivated[terminalId])
            return;

        terminalActivated[terminalId] = true;
        Debug.Log($"Terminal {terminalId} activated");

        // Check if we have the first 3 required terminals (IDs 0, 1, 2)
        if (terminalActivated[0] && terminalActivated[1] && terminalActivated[2])
        {
            // Switch gate to "on" idle state after first 3 terminals
            if (gateAnimator != null)
            {
                gateAnimator.SetBool("GateOffIdle", false);
                gateAnimator.SetBool("GateOnIdle", true);
            }
        }

        // Activate corresponding light based on terminal ID
        StartCoroutine(ActivateLight(terminalId));
        
        // Pan camera to show the gate lighting up
        if (gateCameraTarget != null && !isPanning)
        {
            StartCoroutine(PanCameraToGate());
        }
    }

    /// Activate light for specific terminal ID with proper animation sequence
    private IEnumerator ActivateLight(int terminalId)
    {
        Animator lightAnimator = null;
        string colorName = "";

        // Map terminal ID to color and animator
        switch (terminalId)
        {
            case 0: // Purple
                lightAnimator = purpleAnimator;
                colorName = "Purple";
                break;
            case 1: // DarkBlue
                lightAnimator = darkBlueAnimator;
                colorName = "DarkBlue";
                break;
            case 2: // Pink
                lightAnimator = pinkAnimator;
                colorName = "Pink";
                break;
            case 3: // LightBlue
                lightAnimator = lightBlueAnimator;
                colorName = "LightBlue";
                break;
        }

        if (lightAnimator == null)
        {
            Debug.LogWarning($"No animator found for terminal ID {terminalId}");
            yield break;
        }

        Debug.Log($"Activating {colorName} light for terminal {terminalId}");

        // Wait 0.5 seconds to allow camera to pan to gate before lights activate
        yield return new WaitForSeconds(1.5f);

        // Step 1: Activate the *Color*LightsActivate bool
        lightAnimator.SetBool($"{colorName}LightsActivate", true);

        // Wait 0.3 seconds
        yield return new WaitForSeconds(0.3f);

        // Step 2: Disable activate bool and enable *Color*LightsOn
        lightAnimator.SetBool($"{colorName}LightsActivate", false);
        lightAnimator.SetBool($"{colorName}LightsOn", true);

        // Mark this light as activated
        lightsActivated[terminalId] = true;

        Debug.Log($"{colorName} light fully activated");

        // Check if all 4 lights are activated
        if (AllLightsActivated())
        {
            StartCoroutine(OpenGate());
        }
    }

    /// Check if all 4 lights have been activated
    private bool AllLightsActivated()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!lightsActivated[i])
                return false;
        }
        return true;
    }

    /// Open the gate after all lights are activated
    private IEnumerator OpenGate()
    {
        if (gateOpened)
            yield break;

        gateOpened = true;
        Debug.Log("All lights activated - opening gate!");

        // Disable all individual color light objects
        if (purpleLights != null) purpleLights.SetActive(false);
        if (darkBlueLights != null) darkBlueLights.SetActive(false);
        if (pinkLights != null) pinkLights.SetActive(false);
        if (lightBlueLights != null) lightBlueLights.SetActive(false);

        // Activate AllLightsIdle on the AllLights object
        if (allLightsAnimator != null)
        {
            allLightsAnimator.SetBool("AllLightsIdle", true);
        }

        // Wait 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        // Activate AllLightsOpen and GateOpen
        if (allLightsAnimator != null)
        {
            allLightsAnimator.SetBool("AllLightsOpen", true);
        }

        if (gateAnimator != null)
        {
            gateAnimator.SetBool("GateOpen", true);
        }

        yield return new WaitForSeconds(0.29f);
        
        if (allLightsObject != null)
        {
            allLightsObject.SetActive(false);
        }
        
        // Deactivate all gate spotlights
        for (int i = 0; i < gateSpotlights.Length; i++)
        {
            if (gateSpotlights[i] != null)
            {
                gateSpotlights[i].SetActive(false);
                Debug.Log($"Deactivated gate spotlight {i + 1}");
            }
        }

        Debug.Log("Gate opened successfully, AllLights object and spotlights deactivated!");
        
        // Ensure player controls are enabled after gate opening (fixes softlock)
        EnablePlayerControls();
    }
    
    /// <summary>
    /// Pan camera to show the gate, then back to original position
    /// </summary>
    private IEnumerator PanCameraToGate()
    {
        if (mainCamera == null || gateCameraTarget == null || isPanning)
            yield break;
            
        isPanning = true;
        
        // Store current camera position (in case player moved)
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        // Calculate target camera position and rotation
        Vector3 targetPos = gateCameraTarget.position;
        Quaternion targetRot = gateCameraTarget.rotation;
        
        // Pan to gate
        yield return StartCoroutine(PanCamera(startPos, startRot, targetPos, targetRot, panDuration));
        
        // Hold on gate view
        yield return new WaitForSeconds(holdDuration);
        
        // Pan back to original position
        yield return StartCoroutine(PanCamera(targetPos, targetRot, startPos, startRot, panDuration));
        
        isPanning = false;
        
        // Re-enable player controls after camera pan if they were disabled
        if (playerControlsWereDisabled)
        {
            EnablePlayerControls();
            playerControlsWereDisabled = false;
        }
    }
    
    /// <summary>
    /// Disable player controls during camera pan
    /// </summary>
    private void DisablePlayerControls()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (playerController != null)
        {
            var inputEnabledField = typeof(PlayerController).GetField("InputEnabled", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inputEnabledField != null)
            {
                inputEnabledField.SetValue(playerController, false);
                playerControlsWereDisabled = true;
                Debug.Log("Player controls disabled for camera pan");
            }
        }
    }
    
    /// <summary>
    /// Enable player controls after camera pan
    /// </summary>
    private void EnablePlayerControls()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (playerController != null)
        {
            var inputEnabledField = typeof(PlayerController).GetField("InputEnabled", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inputEnabledField != null)
            {
                inputEnabledField.SetValue(playerController, true);
                Debug.Log("Player controls enabled after camera pan");
            }
        }
    }
    
    /// <summary>
    /// Smooth camera pan between two positions
    /// </summary>
    private IEnumerator PanCamera(Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float easedT = panCurve.Evaluate(t);
            
            mainCamera.transform.position = Vector3.Lerp(fromPos, toPos, easedT);
            mainCamera.transform.rotation = Quaternion.Lerp(fromRot, toRot, easedT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position is exact
        mainCamera.transform.position = toPos;
        mainCamera.transform.rotation = toRot;
    }

    // For debugging - can be called from inspector or other scripts
    [ContextMenu("Test Terminal 0")]
    public void TestTerminal0() { OnTerminalActivated(0); }
    
    [ContextMenu("Test Terminal 1")]
    public void TestTerminal1() { OnTerminalActivated(1); }
    
    [ContextMenu("Test Terminal 2")]
    public void TestTerminal2() { OnTerminalActivated(2); }
    
    [ContextMenu("Test Terminal 3")]
    public void TestTerminal3() { OnTerminalActivated(3); }
}
