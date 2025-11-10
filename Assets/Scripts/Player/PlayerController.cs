using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public LayerMask blockMask; // 勾選 Obstacle、Hurtbox

    private Rigidbody2D rb;
    private Vector2 input;
    private ContactFilter2D filter;
    private RaycastHit2D[] hits = new RaycastHit2D[4];

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
        // 取得輸入（上下左右）
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        Vector2 delta = input * moveSpeed * Time.fixedDeltaTime;
        if (delta.sqrMagnitude <= 0f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 預測碰撞，防止穿牆
        int hitCount = rb.Cast(delta.normalized, filter, hits, delta.magnitude);
        if (hitCount > 0)
        {
            float allowed = hits[0].fraction * delta.magnitude;
            delta = delta.normalized * Mathf.Max(0f, allowed - 0.01f);
        }

        rb.MovePosition(rb.position + delta);
    }
}
