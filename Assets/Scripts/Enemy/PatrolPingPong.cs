using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolPingPong : MonoBehaviour
{
    public Vector2 direction = Vector2.right; // 來回方向：水平用 (1,0)，垂直用 (0,1)
    public float distance = 3f;               // 來回半徑（中心左右各 3 單位）
    public float speed = 2f;

    Rigidbody2D rb; Vector2 start; float t;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.freezeRotation = true;
        start = rb.position;
        direction = direction.normalized;
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime * speed;
        float offset = Mathf.PingPong(t, distance * 2f) - distance; // [-distance, +distance]
        Vector2 target = start + direction * offset;
        rb.MovePosition(target);
    }
}
