using UnityEditor.UI;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Transform RespawnPoint;
    public int maxHealth = 10;
    [SerializeField] private int currentHealth;

    private bool isInvincible = false;
    public float invincibilityDuration = 1f;
    private float invincibilityTimer;
    
    // Animation
    private Animator animator;
    
    // Knockback
    [Header("Knockback")]
    public float knockbackForce;
    public float knockbackDuration;
    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    public bool isInvincibleStatus()
    {
        return isInvincible;
    }

    public void TakeDamage(int damage)
    {
        // Default knockback direction (backward/left)
        Vector2 defaultKnockbackDirection = Vector2.left;
        TakeDamage(damage, defaultKnockbackDirection);
    }
    
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        
        // Trigger damage animation
        if (animator != null)
        {
            animator.SetTrigger("takeDamage");
        }
        
        ApplyKnockback(knockbackDirection);
        
        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(BecomeTemporarilyInvincible());
    }
    
    public void TakeDamage(int damage, Transform damageSource)
    {
        if (damageSource != null)
        {
            // Check if damage source is to the left or right of the player
            float horizontalDirection = transform.position.x - damageSource.position.x;
            
            // Determine knockback direction (away from damage source)
            Vector2 knockbackDirection;
            if (horizontalDirection > 0)
            {
                // Damage source is to the left, so knock player to the right and up
                knockbackDirection = new Vector2(1f, 100f);
            }
            else
            {
                // Damage source is to the right, so knock player to the left and up
                knockbackDirection = new Vector2(-1f, 100f);
            }
            
            TakeDamage(damage, knockbackDirection);
        }
        else
        {
            TakeDamage(damage);
        }
    }
    
    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        if (rb != null)
        {
            isKnockedBack = true;
            
            // Temporarily disable PlayerController to prevent interference
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Calculate knockback force
            Vector2 knockback = new Vector2(
                knockbackDirection.x * knockbackForce, 
                knockbackDirection.y * knockbackForce
            );
            
            // Clear existing velocity first to ensure knockback takes effect
            rb.linearVelocity = Vector2.zero;
            
            // Apply knockback using both AddForce and direct velocity for more reliable effect
            rb.AddForce(knockback, ForceMode2D.Impulse);
            
            // Also set velocity directly to ensure immediate movement
            rb.linearVelocity = new Vector2(knockback.x * 0.5f, knockback.y * 0.5f);
            
            Debug.Log($"Knockback applied - Force: {knockback}, Final Velocity: {rb.linearVelocity}, Direction: {knockbackDirection}");
            
            StartCoroutine(EndKnockback(playerController));
        }
        else
        {
            Debug.LogError("Rigidbody2D not found! Knockback cannot be applied.");
        }
    }
    
    private System.Collections.IEnumerator EndKnockback(PlayerController playerController)
    {
        yield return new WaitForSeconds(knockbackDuration);
        
        // Re-enable player control
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        isKnockedBack = false;
        Debug.Log("Knockback ended, player control restored");
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    // On death, run Die()
    private void Die()
    {
        if (gameObject.CompareTag("Player"))
        {
            //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); // On death, reset scene
            //If the player dies, set the player gameObject to the RespawnPoint location
            transform.position = RespawnPoint.position;
            return;
        }
    }

    private System.Collections.IEnumerator BecomeTemporarilyInvincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        UnityEngine.Debug.Log("Player is no longer invincible.");
    }
}
