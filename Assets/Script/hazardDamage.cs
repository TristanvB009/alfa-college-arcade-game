using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        Debug.Log("Hazard collision with " + trigger.gameObject.name);
        Health health = trigger.gameObject.GetComponent<Health>();
        
        if (health != null)
        {
            Debug.Log("Health component found on " + trigger.gameObject.name);
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
        }
    }
}
