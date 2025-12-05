using System.Collections;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public int activationInterval;
    public bool isPowered;
    [SerializeField]private bool isActivated;

    private bool isRunningActivationCycle;
    
    private void Start()
    {
        isActivated = true;
        isRunningActivationCycle = false;
    }
    private void Update()
    {
        if (isPowered && activationInterval > 0 && !isRunningActivationCycle)
        {
            StartCoroutine(ActivationCycle());
        }
    }

    IEnumerator ActivationCycle()
    {
        isRunningActivationCycle = true;
        isActivated = true;
        Debug.Log("Camera Activated");
        yield return new WaitForSeconds(activationInterval);
        isActivated = false;
        Debug.Log("Camera Deactivated");
        yield return new WaitForSeconds(activationInterval);
        isRunningActivationCycle = false;
    }

    public void OnPlayerDetected()
    {
        if (isActivated && isPowered)
        {
            Debug.Log("Player Detected by camera");
        } else
        {
            Debug.Log("Player in Camera FOV, but camera is deactivated/depowered");
        }
    }
}