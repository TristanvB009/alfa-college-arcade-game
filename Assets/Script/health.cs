using UnityEditor.UI;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Transform RespawnPoint;
    public int maxHealth = 10;
    [SerializeField] private int currentHealth;

    private bool isInvincible = false;
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private Coroutine invincibilityCoroutine;
    private Knockback knockback;
    
    // Animation
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        knockback = GetComponent<Knockback>();
        
        // Debug check for missing knockback component
        if (knockback == null)
        {
            Debug.LogError("Knockback component not found on " + gameObject.name + "! Make sure to add the Knockback script to this GameObject.");
        }
        else
        {
            Debug.Log("Knockback component found on " + gameObject.name);
        }
    }
    public bool isInvincibleStatus()
    {
        return isInvincible;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        
        // Trigger damage animation
        if (animator != null)
        {
            animator.SetTrigger("takeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        // Store the coroutine reference to manage it properly
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(BecomeTemporarilyInvincible());
    }

    public void TakeDamage(int damage, Transform damageSource)
    {
        Debug.Log($"TakeDamage called with damage: {damage}, source: {(damageSource != null ? damageSource.name : "null")}");
        
        if (isInvincible) 
        {
            Debug.Log("Player is invincible, damage blocked");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Health reduced to: {currentHealth}");
        
        // Trigger damage animation
        if (animator != null)
        {
            animator.SetTrigger("takeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        Vector2 hitDirection = CalculateHitDirection(damageSource);
        Debug.Log($"Hit direction calculated: {hitDirection}");
        
        if (knockback != null)
        {
            Debug.Log("Calling knockback...");
            knockback.CallKnockback(hitDirection, Vector2.up, Input.GetAxisRaw("Horizontal"));
        }
        else
        {
            Debug.LogError("Knockback component is null! Cannot apply knockback.");
        }

        // Store the coroutine reference to manage it properly
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(BecomeTemporarilyInvincible());
    }

    // Calculates the hit direction from a damage source to the player.
    // Returns a normalized Vector2 indicating the direction the player should be knocked back.
    private Vector2 CalculateHitDirection(Transform damageSource)
    {
        if (damageSource == null)
        {
            // Default knockback direction (left) if no source is provided
            return Vector2.left;
        }

        // Calculate direction from damage source to player
        Vector2 directionToPlayer = (transform.position - damageSource.position).normalized;
        
        // Add a slight upward component to make knockback feel better
        Vector2 hitDirection = new Vector2(directionToPlayer.x, Mathf.Abs(directionToPlayer.y) + 0.5f);
        
        // Normalize the result
        return hitDirection.normalized;
    }

    // Alternative hit direction calculation with custom upward force.
    private Vector2 CalculateHitDirection(Transform damageSource, float upwardForce)
    {
        if (damageSource == null)
        {
            return new Vector2(-1f, upwardForce).normalized;
        }

        // Calculate horizontal direction from damage source to player
        float horizontalDirection = transform.position.x - damageSource.position.x;
        
        // Determine knockback direction (away from damage source)
        Vector2 hitDirection = new Vector2(
            horizontalDirection > 0 ? 1f : -1f, // Push right if source is left, push left if source is right
            upwardForce
        );
        
        return hitDirection.normalized;
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
        UnityEngine.Debug.Log($"Player became invincible for {invincibilityDuration} seconds");
        
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvincible = false;
        invincibilityCoroutine = null; // Clear the reference when done
        UnityEngine.Debug.Log("Player is no longer invincible.");
    }
}
