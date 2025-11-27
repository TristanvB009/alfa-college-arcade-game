using UnityEngine;


public class CheckpointScript : MonoBehaviour
{
    [Header("Checkpoint Sprites")]
    [SerializeField] private Sprite[] checkpointSprites = new Sprite[3];
    private SpriteRenderer spriteRenderer;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private Vector3 playerSittingOffset = Vector3.up; // Offset from checkpoint position where player sits
    
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    
    // Animation functionality
    private Animator playerAnimator;
    private bool isPlayerSitting = false;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (checkpointSprites.Length > 0 && spriteRenderer != null)
        {
            int randomIndex = Random.Range(0, checkpointSprites.Length);
            spriteRenderer.sprite = checkpointSprites[randomIndex];
        }
    }

    void Update()
    {
        CheckForPlayerInRange();
        
        if (isPlayerInRange && playerTransform != null)
        {
            // If player is sitting, check for movement to stand up
            if (isPlayerSitting)
            {
                CheckForPlayerMovement();
            }
            else
            {
                // Check for W key input to sit at checkpoint
                CheckForSitInput();
            }
        }
    }

    private void CheckForPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance <= interactionRadius)
            {
                if (!isPlayerInRange)
                {
                    // Player just entered range
                    isPlayerInRange = true;
                    playerTransform = player.transform;
                }
            }
            else
            {
                if (isPlayerInRange)
                {
                    // Player left range
                    isPlayerInRange = false;
                    playerTransform = null;
                }
            }
        }
    }

    private void CheckForSitInput()
    {
        // Check for W key input only (no axis input to avoid conflicts)
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("W key pressed - attempting to sit at checkpoint");
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        // Don't activate if already sitting
        if (isPlayerSitting) return;
        
        Vector3 checkpointTop = transform.position + playerSittingOffset;
        playerTransform.position = checkpointTop;
        
        // Set player velocity to zero when sitting
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }
        
        playerAnimator = playerTransform.GetComponent<Animator>();
        
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("sittingDown");
            playerAnimator.SetBool("isSitting", true);
        }
        
        // Set sitting state
        isPlayerSitting = true;

        // Set respawn point
        Health playerHealth = playerTransform.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.RespawnPoint = this.transform;
            playerHealth.Heal(playerHealth.maxHealth); 
        }
    }
    
    private void CheckForPlayerMovement()
    {
        // Check for specific key inputs instead of axis values to avoid residual input issues
        bool leftInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightInput = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        bool downInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool jumpInput = Input.GetKeyDown(KeyCode.Space);
        
        // If player is pressing movement keys (but not W), stop sitting
        if (leftInput || rightInput || downInput || jumpInput)
        {
            Debug.Log("Player movement detected, stopping sitting");
            StopSitting();
        }
    }
    
    private void StopSitting()
    {
        if (playerAnimator != null)
        {
            // Set sitting state to false
            playerAnimator.SetBool("isSitting", false);
            
            // Add a standing up trigger to ensure proper transition
            playerAnimator.SetTrigger("standingUp");
            
            // Force update the animator to ensure immediate transition
            playerAnimator.Update(0f);
        }
        
        isPlayerSitting = false;
        Debug.Log("Player stopped sitting");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}