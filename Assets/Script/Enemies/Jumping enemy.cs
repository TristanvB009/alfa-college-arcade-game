using UnityEngine;

public class Jumpingenemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 7f;

    [Header("Jump Timing")]
    public float minJumpDelay = 1.5f;
    public float maxJumpDelay = 3.5f;

    [Header("Turn Timing")]
    public float minTurnDelay = 1f;
    public float maxTurnDelay = 3f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float jumpTimer;
    private float turnTimer;
    private int direction = 1; // 1 = rechts, -1 = links

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetJumpTimer();
        ResetTurnTimer();
    }

    // Update is called once per frame
    void Update()
    {
        jumpTimer -= Time.deltaTime;
        turnTimer -= Time.deltaTime;

        if (turnTimer <= 0f)
        {
            direction *= -1;
            ResetTurnTimer();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        
        if (jumpTimer <= 0f && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            ResetJumpTimer();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void ResetJumpTimer()
    {
        jumpTimer = Random.Range(minJumpDelay, maxJumpDelay);
    }

    void ResetTurnTimer()
    {
        turnTimer = Random.Range(minTurnDelay, maxTurnDelay);
    }
}
