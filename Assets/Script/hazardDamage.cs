using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        Debug.Log("Collided with " + collision.gameObject.name);
        if (health != null && health.isInvincibleStatus() == false)
        {
            health.TakeDamage(damageAmount);
        }
    }
}
