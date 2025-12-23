using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Animator Params")]
    public string igniteTrigger = "Ignite";   // 你 Animator 裡的 Trigger 名稱
    public string idleStateName = "Idle";     // 可選：用來回到待機

    public void PlayIgnite()
    {
        if (!animator)
        {
            Debug.LogError("RocketController: animator 未指定");
            return;
        }

        animator.ResetTrigger(igniteTrigger);
        animator.SetTrigger(igniteTrigger);
    }

    // 可選：如果你想強制回 Idle（不用也可以）
    public void BackToIdle()
    {
        if (!animator) return;
        animator.Play(idleStateName, 0, 0f);
    }
}
