using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerControllerFight : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockMask; // 勾 Obstacle + Hurtbox
    Rigidbody2D rb;
    Vector2 input;
    ContactFilter2D filter;
    RaycastHit2D[] hits = new RaycastHit2D[4];

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
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        Vector2 delta = input * moveSpeed * Time.fixedDeltaTime;
        if (delta.sqrMagnitude <= 0f) { rb.velocity = Vector2.zero; return; }

        // 以移動方向做形體投射，預測會不會撞到（Obstacle/Hurtbox）
        int hitCount = rb.Cast(delta.normalized, filter, hits, delta.magnitude);
        if (hitCount > 0)
        {
            // 只走到碰撞點前（保留一點安全距離）
            float allowed = hits[0].fraction * delta.magnitude;
            delta = delta.normalized * Mathf.Max(0f, allowed - 0.01f);
        }

        rb.MovePosition(rb.position + delta);
    }
}
