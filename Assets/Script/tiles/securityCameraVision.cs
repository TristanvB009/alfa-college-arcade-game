using System.Collections.Generic;
using UnityEngine;

public class securityCameraVision : MonoBehaviour
{
    public SecurityCamera securityCamera;
    public string playerTag = "Player";

    private bool playerDetected;

    private void OnTriggerEnter2D(Collider2D vision)
    {
        if (vision.CompareTag(playerTag))
        {
            playerDetected = true;
            Debug.Log("player in vision");
        }
    }

    private void OnTriggerExit2D(Collider2D vision)
    {
        if (vision.CompareTag(playerTag))
        {
            playerDetected = false;
            Debug.Log("player out of vision");
       }
    }
}
// TODO
// - Improve Player Tag getting
// - Activation Timer
// - Configurable Camera Rotation