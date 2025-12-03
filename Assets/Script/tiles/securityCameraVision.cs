using System;
using System.Collections;
using UnityEngine;

public class securityCameraVision : MonoBehaviour
{
    public SecurityCamera securityCamera;
    public string playerTag = "Player";
    public float rotationSpeed;
    public int rotationPause;
    public float rotationAngleStart;
    public float rotationAngleEnd;
    private bool isRotationPaused;

    private float targetAngle;

    private bool playerDetected;

    private void Start()
    {
        targetAngle = rotationAngleEnd;
        playerDetected = false;
        isRotationPaused = false;
    } 

    private void Update()
    {
        if (!isRotationPaused)
        {
            float step = rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, step);

            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle)) < Math.Abs(step) + 0.1f)
            {
                Debug.Log("Reached target rotation");
                StartCoroutine(PauseAndSwitchTarget());
            }   
        }
    }

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

    IEnumerator PauseAndSwitchTarget()
    {
        isRotationPaused = true;
            yield return new WaitForSeconds(rotationPause);
            isRotationPaused = false;
            if (Mathf.Approximately(targetAngle, rotationAngleEnd))
            {
                targetAngle = rotationAngleStart;
                rotationSpeed = -rotationSpeed;
            }
            else
            {
                targetAngle = rotationAngleEnd;
                rotationSpeed = -rotationSpeed;
            }
            Debug.Log("New target angle: " + targetAngle);
    }
}