using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 3f;

    [Header("加速度設定")]
    [SerializeField] float accel = 20f;
    [SerializeField] float decel = 60f;

    private Rigidbody rb;
    private Vector3 input;      
    private Vector3 currentVel;

    private bool canMove = true;
    public bool CanMove => canMove;

    public Vector3 InputVector => input;

    public void EnableMovement(bool active)
    {
        canMove = active;
        if (!active)
        {
            input = Vector3.zero;
            currentVel = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.drag = 0f;

        // 鎖定旋轉，防止人倒下或亂轉
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        if (!canMove)
        {
            input = Vector3.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        input = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            currentVel = Vector3.zero;
            rb.velocity = Vector3.zero;
            return;
        }

        Vector3 targetVel = input * moveSpeed;
        float a = (input.sqrMagnitude > 0f) ? accel : decel;

        currentVel = Vector3.MoveTowards(currentVel, targetVel, a * Time.fixedDeltaTime);

        if (input.sqrMagnitude == 0f && currentVel.magnitude < 0.05f)
            currentVel = Vector3.zero;

        rb.velocity = new Vector3(currentVel.x, rb.velocity.y, currentVel.z);
    }
}