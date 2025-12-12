using System.Collections;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public int activationInterval;
    public bool isPowered;
    [SerializeField]private bool isActivated;
    private float TimePlayerInVision = 0;

    private bool isRunningActivationCycle;

    public securityCameraVision securityCameraVision;
    
    public hazardDamage hazardDamage;
    public float DetectDelay;

    GameObject player;
    
    private void Start()
    {
        isActivated = true;
        isRunningActivationCycle = false;
        securityCameraVision = GetComponentInChildren<securityCameraVision>();
        hazardDamage = GetComponentInChildren<hazardDamage>();
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void Update()
    {
        if (isPowered)
        {
            if (activationInterval > 0 && !isRunningActivationCycle)
            {
                StartCoroutine(ActivationCycle());
            }

            if (securityCameraVision.PlayerInVision() && isActivated)
            {
                TimePlayerInVision+= 1 * Time.deltaTime;
            }
            else
            {
                TimePlayerInVision = 0;
            }

            if (TimePlayerInVision > DetectDelay)
            {
                OnPlayerDetected();
            }
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
        // TODO: put stuff here
        Debug.Log("Player Detected");
        StartCoroutine(hazardDamage.WaitForKnockbackThenRespawn
        (
            player.GetComponent<PlayerController>(),
            player
        ));
        TimePlayerInVision = 0;
    }
}