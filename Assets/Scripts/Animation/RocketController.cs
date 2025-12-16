using UnityEngine;

public class RocketController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string igniteTrigger = "Ignite";

    public void PlayIgnite()
    {
        if (!animator)
        {
            Debug.LogError("RocketController: Animator 沒有指定");
            return;
        }
        animator.SetTrigger(igniteTrigger);
    }
}
