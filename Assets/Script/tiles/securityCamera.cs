using System.Collections;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public int activationInterval;
    public bool isPowered;
    [SerializeField]private bool isActivated;

    private bool isRunningActivationCycle;
    
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
}