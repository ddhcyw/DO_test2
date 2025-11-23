using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class MaiIdleSpineAnimator : MonoBehaviour
{
    [Header("Spine 動畫名稱")]
    public string idleAnimationName = "idle";   // 等一下在 Inspector 改成真正名稱

    private SkeletonAnimation skeletonAnimation;
    private string currentAnimation = "";

    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        PlayIdle();
    }

    void OnEnable()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        PlayIdle();
    }

    public void PlayIdle()
    {
        if (skeletonAnimation == null) return;
        if (currentAnimation == idleAnimationName) return;

        var data = skeletonAnimation.Skeleton.Data;
        if (data.FindAnimation(idleAnimationName) != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
            currentAnimation = idleAnimationName;
        }
        else
        {
            Debug.LogWarning($"MAI 找不到 idle 動畫：{idleAnimationName}");
        }
    }
}
