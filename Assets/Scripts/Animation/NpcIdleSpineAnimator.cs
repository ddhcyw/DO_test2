using UnityEngine;
using Spine.Unity;

public class NpcIdleSpineAnimator : MonoBehaviour
{
    [Header("Spine")]
    public SkeletonAnimation skeletonAnimation;   // 要指定同一個物件上的 SkeletonAnimation
    public string idleAnimationName = "idle";     // 在 Inspector 填入真正的動畫名稱

    void Awake()
    {
        if (!skeletonAnimation)
            skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    void OnEnable()
    {
        PlayIdle();
    }

    public void PlayIdle()
    {
        if (!skeletonAnimation)
        {
            Debug.LogError("[NpcIdleSpineAnimator] SkeletonAnimation 還沒指定", this);
            return;
        }

        // 先確認動畫是不是存在
        var anim = skeletonAnimation.Skeleton.Data.FindAnimation(idleAnimationName);
        if (anim == null)
        {
            Debug.LogError($"[NpcIdleSpineAnimator] 找不到動畫 '{idleAnimationName}'，請確認名字有沒有打對。", this);
            return;
        }

        // 播放站姿動畫（第 0 軌、Loop = true）
        skeletonAnimation.state.SetAnimation(0, anim, true);
    }
}
