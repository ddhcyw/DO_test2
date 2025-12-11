using UnityEngine;

public class CutsceneObjectAnimator : MonoBehaviour
{
    Animator anim;
    public string triggerName = "play";

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayOnce()
    {
        anim.SetTrigger(triggerName);
    }
}
