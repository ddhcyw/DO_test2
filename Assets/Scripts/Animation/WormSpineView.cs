using UnityEngine;
using Spine.Unity;
public class WormSpineView : MonoBehaviour
{
    [SerializeField] SkeletonAnimation skeleton;

    void Awake()
    {
        if (skeleton == null)
            skeleton = GetComponent<SkeletonAnimation>();
    }

    public void PlayIdle()
    {
        skeleton.AnimationState.SetAnimation(0, "idle", true);
    }

    public void PlayMove()
    {
        skeleton.AnimationState.SetAnimation(0, "move", true);
    }

    public void PlayAttack()
    {
        skeleton.AnimationState.SetAnimation(0, "attack", false);
        skeleton.AnimationState.AddAnimation(0, "idle", true, 0);
    }

    public void PlayHit()
    {
        skeleton.AnimationState.SetAnimation(0, "hit", false);
        skeleton.AnimationState.AddAnimation(0, "idle", true, 0);
    }

    public void PlayDead()
    {
        skeleton.AnimationState.SetAnimation(0, "dead", false);
    }
}
