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
    
    // Track which terminals have been activated
    private bool[] terminalActivated = new bool[4]; // IDs 0, 1, 2, 3
    private bool[] lightsActivated = new bool[4]; // Track which lights are on
    private bool gateOpened = false;

    void Start()
    {
        // Initialize gate in off idle state
        if (gateAnimator != null)
        {
            gateAnimator.SetBool("GateOffIdle", true);
            gateAnimator.SetBool("GateOnIdle", false);
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

        Debug.Log("Gate opened successfully and AllLights object deactivated!");
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
