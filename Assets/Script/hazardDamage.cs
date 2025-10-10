using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        Debug.Log("Hazard collision with " + trigger.gameObject.name);
        Health health = trigger.gameObject.GetComponent<Health>();
        PlayerController playerController = trigger.gameObject.GetComponent<PlayerController>();

        
        if (health != null)
        {
            Debug.Log("Health component found on " + trigger.gameObject.name);
        //If the object has a health component, apply damage
            if (health.isInvincibleStatus() == false)
            {
                Debug.Log($"Calling TakeDamage({damageAmount}, {this.transform.name})");
                health.TakeDamage(damageAmount, this.transform);
            }
            else
            {
                Debug.Log("Player is invincible, hazard damage blocked");
            }
        }
        else
        {
            Debug.Log("No Health component found on " + trigger.gameObject.name);

            // If the object is the player and has health over 0, respawn at last grounded
            if (trigger.gameObject.CompareTag("Player") && health.currentHealth > 0 && playerController != null)
            {
                playerController.LastGroundedRespawn();
            }
        }
    }
}
