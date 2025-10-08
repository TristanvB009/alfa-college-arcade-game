using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        Health health = trigger.gameObject.GetComponent<Health>();
        Debug.Log("Collided with " + trigger.gameObject.name);
        if (health != null && health.isInvincibleStatus() == false)
        {
            health.TakeDamage(damageAmount);
        }
    }
}
