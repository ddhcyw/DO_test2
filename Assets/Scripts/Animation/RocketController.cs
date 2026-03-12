using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Animator Params")]
    public string igniteTrigger = "Ignite";
    public string idleStateName = "Idle";

    public void PlayIgnite()
    {
        if (!animator)
        {
            Debug.LogError("RocketController: animator 未指定");
            return;
        }

        Debug.Log("RocketController: PlayIgnite() 被呼叫");

        animator.ResetTrigger(igniteTrigger);
        animator.SetTrigger(igniteTrigger);
    }

    public void BackToIdle()
    {
        if (!animator) return;

        Debug.Log("RocketController: BackToIdle() 被呼叫");
        animator.Play(idleStateName, 0, 0f);
    }
}