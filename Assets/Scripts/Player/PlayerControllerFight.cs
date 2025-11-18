using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerControllerFight : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockMask;

    Rigidbody2D rb;
    Vector2 input;
    ContactFilter2D filter;
    RaycastHit2D[] hits = new RaycastHit2D[4];

    private bool canMove = true;  // ★新增

    public void EnableMovement(bool active)
    {
        canMove = active;
        if (!active) rb.velocity = Vector2.zero;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = blockMask,
            useTriggers = false
        };
    }

    void Update()
    {
        if (!canMove)
        {
            input = Vector2.zero;
            return;
        }

        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 delta = input * moveSpeed * Time.fixedDeltaTime;

        if (delta.sqrMagnitude <= 0f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        int hitCount = rb.Cast(delta.normalized, filter, hits, delta.magnitude);
        if (hitCount > 0)
        {
            float allowed = hits[0].fraction * delta.magnitude;
            delta = delta.normalized * Mathf.Max(0f, allowed - 0.01f);
        }

        rb.MovePosition(rb.position + delta);
    }
}
