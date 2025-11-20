using UnityEngine;
using Spine.Unity; // 1. 引入 Spine 命名空間

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerControllerAni : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public LayerMask blockMask; // 請在 Inspector 勾選 Obstacle、Hurtbox 以及 "Default" (或牆壁所在圖層)

    [Header("Spine 動畫設定")]
    public string idleAnimationName = "idle"; // 在 Inspector 中確認您的動畫名稱
    public string walkAnimationName = "walk";

    private Rigidbody2D rb;
    private Vector2 input;
    private ContactFilter2D filter;
    private RaycastHit2D[] hits = new RaycastHit2D[4];

    private bool canMove = true;

    // 2. Spine 動畫元件
    private SkeletonAnimation skeletonAnimation;
    private string currentAnimation = ""; // 記錄當前動畫，避免重複播放

    public void EnableMovement(bool active)
    {
        canMove = active;
        if (!active)
        {
            rb.velocity = Vector2.zero;
            // 停止移動時，強制播放待機動畫
            SetAnimation(idleAnimationName, true);
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 3. 取得 Spine 動畫元件 (如果掛在子物件上，請改用 GetComponentInChildren)
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }

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

        // 4. 根據移動狀態切換動畫
        HandleAnimation();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 delta = input * moveSpeed * Time.fixedDeltaTime;

        // 如果沒有輸入，停止移動
        if (delta.sqrMagnitude <= 0f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 射線投射 (防穿牆邏輯)
        int hitCount = rb.Cast(delta.normalized, filter, hits, delta.magnitude);
        if (hitCount > 0)
        {
            // 如果撞到牆，計算允許移動的距離
            float allowed = hits[0].fraction * delta.magnitude;
            delta = delta.normalized * Mathf.Max(0f, allowed - 0.01f);
        }

        rb.MovePosition(rb.position + delta);

        // 5. 處理角色翻轉 (讓角色面向移動方向)
        if (input.x != 0)
        {
            // 如果往右(x>0)不翻轉，往左(x<0)翻轉
            // 注意：Spine 通常透過 Skeleton 的 ScaleX 來翻轉
            if (skeletonAnimation != null)
            {
                skeletonAnimation.Skeleton.ScaleX = input.x > 0 ? 1f : -1f;
            }
        }
    }

    // 處理動畫切換的邏輯
    void HandleAnimation()
    {
        if (skeletonAnimation == null) return;

        // 有輸入訊號 => 走路
        if (input.sqrMagnitude > 0.01f)
        {
            SetAnimation(walkAnimationName, true);
        }
        // 沒有輸入訊號 => 待機
        else
        {
            SetAnimation(idleAnimationName, true);
        }
    }

    // 設定動畫 (封裝方法)
    void SetAnimation(string animationName, bool loop)
    {
        if (animationName == currentAnimation) return; // 如果已經是這個動畫，就不重播

        // 安全檢查：確認 Spine 資料中真的有這個動畫名稱
        if (skeletonAnimation.Skeleton.Data.FindAnimation(animationName) != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
            currentAnimation = animationName;
        }
        else
        {
            Debug.LogWarning($"找不到名為 '{animationName}' 的動畫，請檢查 Spine 檔案或 Inspector 設定。");
        }
    }
}