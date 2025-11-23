using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class NpcSpineIdleAnimator : MonoBehaviour
{
    [Header("Spine 動畫名稱")]
    public string idleAnimationName = "idle";

    private SkeletonAnimation skeletonAnimation;
    private string currentAnimation = "";

    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        PlayIdle();
    }

    void OnEnable()
    {
        if (!skeletonAnimation)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        PlayIdle();
    }

    public void PlayIdle()
    {
        if (!skeletonAnimation) return;
        if (string.IsNullOrEmpty(idleAnimationName)) return;
        if (currentAnimation == idleAnimationName) return;

        var data = skeletonAnimation.Skeleton.Data;
        if (data.FindAnimation(idleAnimationName) != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
            currentAnimation = idleAnimationName;
        }
        else
        {
            Debug.LogWarning($"NpcSpineIdleAnimator: 找不到 idle 動畫 '{idleAnimationName}'");
        }
    }
}
