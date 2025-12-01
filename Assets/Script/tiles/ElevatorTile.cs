using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ElevatorEntry
{
    public int id;
    public Transform transform;
    public Collider2D collider;
}

public class ElevatorTile : MonoBehaviour
{
    public List<ElevatorEntry> elevatorEntries = new List<ElevatorEntry>();
    public int index;
    public Transform platform;
    public float speed;
    public float waitBeforeMove = 1.5f; // seconds to wait before elevator starts moving
    
    [Header("Sidegate Objects")]
    [Tooltip("Sidegate objects that will open when player enters and close when elevator reaches destination")]
    public Animator[] sidegateAnimators = new Animator[2];
    
    [Header("Button System")]
    [Tooltip("Button animator to control button press animations")]
    public Animator buttonAnimator;
    
    [Tooltip("The actual button GameObject/Collider that player must stand on")]
    public Collider2D buttonCollider;
    
    [Tooltip("Time in seconds the player must stand on button to activate elevator")]
    public float buttonPressTime = 2f;

    private Vector2 previousPosition;
    private Rigidbody2D rb;
    private Transform nextPoint;
    private bool isPlayerOnButton = false;
    private float buttonTimer = 0f;
    private bool elevatorActivated = false;
    private Transform currentPlayerTransform;
    private bool requirePlayerToLeaveButton = false; // Prevents immediate re-activation after elevator movement
    private bool waitingForPlayerToLeaveZone = false; // Waiting for player to leave elevator zone before resetting button

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (elevatorEntries.Count <= 1)
        {
            throw new Exception("Needs at least 2 points!");
        }

        rb = platform.GetComponent<Rigidbody2D>();
        previousPosition = rb.position;
    }
    
    void Update()
    {
        // Check if player is standing on the button
        CheckPlayerOnButton();
        
        // Check if we're waiting for player to leave the elevator zone
        if (waitingForPlayerToLeaveZone)
        {
            CheckPlayerInElevatorZone();
        }
    }
    
    /// <summary>
    /// Check if player is currently standing on the button
    /// </summary>
    private void CheckPlayerOnButton()
    {
        if (buttonCollider == null || elevatorActivated) return;
        
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Check if player's collider overlaps with button collider
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null && buttonCollider.bounds.Intersects(playerCollider.bounds))
        {
            // Player is on button
            if (!isPlayerOnButton)
            {
                // Player just stepped on button
                isPlayerOnButton = true;
                currentPlayerTransform = player.transform;
                
                // Check if we need to wait for player to leave first
                if (requirePlayerToLeaveButton)
                {
                    Debug.Log("Player on button but must leave first after elevator movement");
                    return; // Don't start activation yet
                }
                
                Debug.Log("Player stepped on button - starting activation timer");
                
                // Start button activation for current zone
                int currentZoneId = index; // Use current elevator position as zone ID
                StartCoroutine(ButtonActivationProcess(currentZoneId));
            }
        }
        else
        {
            // Player left button
            if (isPlayerOnButton)
            {
                // Player left button - reset state
                isPlayerOnButton = false;
                buttonTimer = 0f;
                requirePlayerToLeaveButton = false; // Player has left, can now reactivate when they return
                
                Debug.Log("Player left button - resetting button state");
                
                // No need to reset button animation since it never went down during activation
            }
        }
    }
    
    /// <summary>
    /// Check if player is still in the elevator zone - used after elevator movement
    /// </summary>
    private void CheckPlayerInElevatorZone()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Check if player is still in the current elevator zone
        Collider2D currentZoneCollider = elevatorEntries[index].collider;
        if (currentZoneCollider != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null && !currentZoneCollider.bounds.Intersects(playerCollider.bounds))
            {
                // Player has left the elevator zone - now we can reset the button
                Debug.Log("Player left elevator zone - resetting button");
                
                if (buttonAnimator != null)
                {
                    buttonAnimator.SetBool("ButtonUp", true);
                    buttonAnimator.SetBool("ButtonDown", false);
                }
                
                waitingForPlayerToLeaveZone = false;
                requirePlayerToLeaveButton = true; // Still require them to leave button area before next activation
                
                // Reset ButtonUp after a short delay
                StartCoroutine(ResetButtonUpAnimation());
            }
        }
    }

    /// <summary>
    /// Legacy method - now button detection is handled in Update()
    /// Keep this for compatibility but it no longer triggers elevator movement
    /// </summary>
    public void OnPlayerEnteredZone(int zoneId, Transform playerTransform)
    {
        Debug.Log($"Player entered elevator zone {zoneId} - use button to activate elevator");
    }
    
    /// <summary>
    /// Legacy method - now handled in Update()
    /// </summary>
    public void OnPlayerExitedZone(int zoneId, Transform playerTransform)
    {
        Debug.Log($"Player exited elevator zone {zoneId}");
    }
    
    /// <summary>
    /// Handle button activation process - player must stand on button for specified time
    /// </summary>
    private IEnumerator ButtonActivationProcess(int zoneId)
    {
        buttonTimer = 0f;
        
        while (isPlayerOnButton && buttonTimer < buttonPressTime && !elevatorActivated)
        {
            buttonTimer += Time.deltaTime;
            
            Debug.Log($"Button timer: {buttonTimer:F1}/{buttonPressTime}");
            yield return null;
        }
        
        // Check if button was held long enough
        if (isPlayerOnButton && buttonTimer >= buttonPressTime && !elevatorActivated)
        {
            Debug.Log("Button activated! Pressing button and starting elevator movement.");
            elevatorActivated = true;
            
            // NOW animate button down when timer completes
            if (buttonAnimator != null)
            {
                buttonAnimator.SetBool("ButtonDown", true);
            }
            
            // Start elevator movement
            StartCoroutine(GoToNextPoint(zoneId));
        }
        else
        {
            Debug.Log("Button activation cancelled - player left too early");
        }
    }

    public IEnumerator GoToNextPoint(int zoneId)
    {
        // Validate zoneId
        if (zoneId < 0 || zoneId >= elevatorEntries.Count)
        {
            Debug.LogWarning($"Invalid zoneId: {zoneId}");
            yield break;
        }

        int targetIndex;

        if (index == zoneId)
        {

            // Already at the requested zone, go to the next one
            targetIndex = (index + 1) % elevatorEntries.Count;

            // Open sidegates since we're about to move
            OpenSidegates();

            // wait so the player has a chance to get on before it starts moving
            yield return new WaitForSeconds(waitBeforeMove);
        }
        else
        {
            // Go to the requested zone
            targetIndex = zoneId;
            
            // Open sidegates since we're about to move
            OpenSidegates();
        }

        nextPoint = elevatorEntries[targetIndex].transform;

        // Move platform toward nextPoint
        while (Vector2.Distance(platform.position, nextPoint.position) > 0.1f)
        {
            Vector2 newPosition = Vector2.MoveTowards(platform.position, nextPoint.position, Time.fixedDeltaTime * speed);
            rb.MovePosition(newPosition);


            // Calculate and apply linear velocity
            // this doesn't effect the movement of t
            rb.linearVelocity = (newPosition - previousPosition) / Time.fixedDeltaTime;
            previousPosition = newPosition;

            yield return new WaitForFixedUpdate();
        }

        // Snap to final position and update index
        rb.MovePosition(nextPoint.position);
        rb.linearVelocity = Vector2.zero; // Stop the elevator completely
        index = targetIndex;
        
        // Close sidegates when elevator reaches destination
        CloseSidegates();
        
        // Don't reset button immediately - wait for player to leave elevator zone
        elevatorActivated = false;
        buttonTimer = 0f;
        waitingForPlayerToLeaveZone = true; // Wait for player to leave zone before resetting button
        
        // Don't reset isPlayerOnButton here - let CheckPlayerOnButton handle player state naturally
        // Button will be reset when player leaves the elevator zone
    }
    
    /// <summary>
    /// Reset ButtonUp animation after a short delay
    /// </summary>
    private IEnumerator ResetButtonUpAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("ButtonUp", false);
        }
    }
    
    /// <summary>
    /// Open sidegates when player enters elevator zone
    /// </summary>
    private void OpenSidegates()
    {
        if (sidegateAnimators != null && sidegateAnimators.Length > 0)
        {
            foreach (Animator sidegateAnimator in sidegateAnimators)
            {
                if (sidegateAnimator != null)
                {
                    sidegateAnimator.SetBool("Open", true);
                    sidegateAnimator.SetBool("Close", false);
                    Debug.Log($"Set Open=true, Close=false on sidegate: {sidegateAnimator.gameObject.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Close sidegates when elevator reaches destination
    /// </summary>
    private void CloseSidegates()
    {
        if (sidegateAnimators != null && sidegateAnimators.Length > 0)
        {
            foreach (Animator sidegateAnimator in sidegateAnimators)
            {
                if (sidegateAnimator != null)
                {
                    sidegateAnimator.SetBool("Open", false);
                    sidegateAnimator.SetBool("Close", true);
                    Debug.Log($"Set Open=false, Close=true on sidegate: {sidegateAnimator.gameObject.name}");
                    
                    // Reset Close to false after a short delay to allow the animation to play
                    StartCoroutine(ResetCloseBool(sidegateAnimator));
                }
            }
        }
    }
    
    /// <summary>
    /// Reset the Close bool to false after the closing animation has time to play
    /// </summary>
    private System.Collections.IEnumerator ResetCloseBool(Animator animator)
    {
        yield return new WaitForSeconds(0.3f); // Wait for animation to start
        if (animator != null)
        {
            animator.SetBool("Close", false);
            Debug.Log($"Reset Close=false on sidegate: {animator.gameObject.name}");
        }
    }



    private void OnDrawGizmos()
    {
        // Draw lines between waypoints for visualization in the editor
        if (elevatorEntries == null || elevatorEntries.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < elevatorEntries.Count; i++)
        {
            Vector3 current = elevatorEntries[i].transform.position;
            Vector3 next = elevatorEntries[(i + 1) % elevatorEntries.Count].transform.position;
            Gizmos.DrawLine(current, next);
        }
    }
}
