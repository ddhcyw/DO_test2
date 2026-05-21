using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class PlayerSpineAnimator : MonoBehaviour
{
    [Header("Spine 動畫名稱（有相機）")]
    public string idleAnimationName = "idle";
    public string walkAnimationName = "walk";

    [Header("Spine 動畫名稱（無相機）")]
    public string idleNoCameraName = "idle(nocamera)";
    public string walkNoCameraName = "walk(nocamera)";

    [Header("相機狀態")]
    public bool hasCamera = false;

    [Header("朝向設定")]
    [Tooltip("如果原始動畫裡，角色【面向右邊】時是 X 負數，請勾選")]
    public bool faceRightIsNegativeX = true;

    [Header("動畫速度設定")]
    [Tooltip("無相機走路動畫對應的移動速度")]
    public float referenceSpeed = 3f;
    [Tooltip("有相機走路動畫對應的移動速度（與 referenceSpeed 分開設定）")]
    public float referenceCameraSpeed = 3f;

    [Header("切換控制")]
    public PlayerSpineSwitcher spineSwitcher;

    private SkeletonAnimation skeletonAnimation;
    private PlayerController playerController;

    private string currentAnimation = "";
    private bool isTalking = false;
    private float baseScaleX = 1f;
    private float lastFacingX = 1f;

    void Start()
    {
        if (PlayerPrefs.GetInt("HasCamera", 0) == 1)
            hasCamera = true;
    }

    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();

        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        if (skeletonAnimation != null)
        {
            baseScaleX = skeletonAnimation.Skeleton.ScaleX;
            if (Mathf.Approximately(baseScaleX, 0f))
                baseScaleX = 1f;
        }

        if (spineSwitcher == null)
            spineSwitcher = GetComponentInParent<PlayerSpineSwitcher>();
        if (skeletonAnimation != null && skeletonAnimation.Skeleton != null)
        {
            float currentScaleX = skeletonAnimation.Skeleton.ScaleX;

            if (faceRightIsNegativeX)
                lastFacingX = currentScaleX < 0 ? 1f : -1f;
            else
                lastFacingX = currentScaleX > 0 ? 1f : -1f;
        }
    }

    void Update()
    {
        if (skeletonAnimation == null) return;

        if (spineSwitcher != null && spineSwitcher.IsPlayingShot())
            return;

        bool movementLocked =
            playerController != null &&
            (!playerController.enabled || !playerController.CanMove);

        if (movementLocked || isTalking)
        {
            SetAnimation(hasCamera ? idleAnimationName : idleNoCameraName, true);
            return;
        }

        Vector2 input = Vector2.zero;
        if (playerController != null)
        {
            input = playerController.InputVector;
        }
        else
        {
            input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
        }

        bool isMoving = input.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            float speed = playerController != null ? playerController.moveSpeed : referenceSpeed;
            float refSpeed = hasCamera ? referenceCameraSpeed : referenceSpeed;
            skeletonAnimation.AnimationState.TimeScale = speed / refSpeed;
            SetAnimation(hasCamera ? walkAnimationName : walkNoCameraName, true);
        }
        else
        {
            skeletonAnimation.AnimationState.TimeScale = 1f;
            SetAnimation(hasCamera ? idleAnimationName : idleNoCameraName, true);
        }

        if (input.x > 0.01f)
            lastFacingX = 1f;
        else if (input.x < -0.01f)
            lastFacingX = -1f;

        ApplyFacing(lastFacingX);

        if (spineSwitcher != null)
            spineSwitcher.SetFacing(lastFacingX);
    }

    void SetAnimation(string animationName, bool loop)
    {
        if (animationName == currentAnimation) return;

        if (skeletonAnimation.Skeleton.Data.FindAnimation(animationName) != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
            currentAnimation = animationName;
        }
        else
        {
            Debug.LogWarning($"找不到 Spine 動畫：{animationName}");
        }
    }

    void ApplyFacing(float facingX)
    {
        float sign = facingX > 0 ? 1f : -1f;

        if (faceRightIsNegativeX)
            sign *= -1f;

        skeletonAnimation.Skeleton.ScaleX = Mathf.Abs(baseScaleX) * sign;
    }

    public void SetTalking(bool talking)
    {
        isTalking = talking;
        if (talking)
        {
            SetAnimation(hasCamera ? idleAnimationName : idleNoCameraName, true);
        }
    }
}