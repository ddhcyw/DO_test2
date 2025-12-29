using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 3f;

    [Header("加速度設定")]
    [SerializeField] float accel = 20f;   // 加速
    [SerializeField] float decel = 60f;   // 減速（通常比 accel 大）

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 currentVel;

    private bool canMove = true;
    public bool CanMove => canMove;

    public Vector2 InputVector => input;

    public void EnableMovement(bool active)
    {
        canMove = active;
        if (!active)
        {
            input = Vector2.zero;
            currentVel = Vector2.zero;
            rb.velocity = Vector2.zero;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 路線 A 建議這兩個在 Inspector 也設一次（防止被 prefab 覆蓋）
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
            currentVel = Vector2.zero;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 targetVel = input * moveSpeed;
        float a = (input.sqrMagnitude > 0f) ? accel : decel;

        currentVel = Vector2.MoveTowards(currentVel, targetVel, a * Time.fixedDeltaTime);

        // 很小的殘速直接歸零，避免尾巴滑行
        if (input.sqrMagnitude == 0f && currentVel.magnitude < 0.05f)
            currentVel = Vector2.zero;

        rb.velocity = currentVel;
    }
}
