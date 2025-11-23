using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class PlayerSpineAnimator : MonoBehaviour
{
    [Header("Spine 動畫名稱")]
    public string idleAnimationName = "idle";
    public string walkAnimationName = "walk";

    [Header("朝向設定")]
    [Tooltip("如果原始動畫裡，角色【面向右邊】時是 X 負數，請勾選")]
    public bool faceRightIsNegativeX = true;

    private SkeletonAnimation skeletonAnimation;
    private PlayerController playerController;

    private string currentAnimation = "";
    private bool isTalking = false;
    private float baseScaleX = 1f;

    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();

        // 從自己或父物件抓 PlayerController
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        if (skeletonAnimation != null)
        {
            baseScaleX = skeletonAnimation.Skeleton.ScaleX;
            if (Mathf.Approximately(baseScaleX, 0f))
                baseScaleX = 1f;
        }
    }

    void Update()
    {
        if (skeletonAnimation == null) return;

        // 只要不能移動(對話、事件鎖定)、或 isTalking 被外部設定，就全部 idle
        bool movementLocked =
            playerController != null &&
            (!playerController.enabled || !playerController.CanMove);

        if (movementLocked || isTalking)
        {
            SetAnimation(idleAnimationName, true);
            return;
        }

        // 正常情況：依照移動輸入切換 idle / walk
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
            SetAnimation(walkAnimationName, true);
        else
            SetAnimation(idleAnimationName, true);

        // 左右翻面
        if (input.x != 0)
        {
            float sign = input.x > 0 ? 1f : -1f;   // 按右 = +1, 按左 = -1
            if (faceRightIsNegativeX)
                sign *= -1f;

            skeletonAnimation.Skeleton.ScaleX = Mathf.Abs(baseScaleX) * sign;
        }
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

    // 如果之後你還是想從對話系統手動控制，也可以用這個
    public void SetTalking(bool talking)
    {
        isTalking = talking;
        if (talking)
        {
            SetAnimation(idleAnimationName, true);
        }
    }
}
