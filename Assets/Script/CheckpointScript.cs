using UnityEngine;


public class CheckpointScript : MonoBehaviour
{

    [Header("Checkpoint Sprites")]
    [SerializeField] private Sprite[] checkpointSprites = new Sprite[3];
    private SpriteRenderer spriteRenderer;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private float standStillTime = 1f;
    [SerializeField] private float movementThreshold = 0.1f; // How much movement is considered "standing still"
    
    private Transform playerTransform;
    private Vector3 lastPlayerPosition;
    private float standStillTimer = 0f;
    private bool isPlayerInRange = false;
    
    // Animation functionality
    private Animator playerAnimator;
    private bool isPlayerSitting = false;

    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D gameObject)
    {
        if (gameObject.CompareTag("Player"))
        {
            // Get the Health components
            Health playerHealth = gameObject.GetComponent<Health>();
            Health currentHealth = gameObject.GetComponent<Health>();
            Health maxHealth = gameObject.GetComponent<Health>();

            if (playerHealth != null)
            {
                // Set this checkpoint as new respawn point
                playerHealth.RespawnPoint = this.transform;
                playerHealth.currentHealth = playerHealth.maxHealth;
                Debug.Log("Checkpoint reached, Respawn updated");
            }
        }
    }
}