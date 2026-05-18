    using UnityEngine;
using Spine.Unity;

public class BlackLiaSpineController : MonoBehaviour
{
    [Header("Spine 元件")]
    public SkeletonAnimation skeleton;

    [Header("動畫名稱（對應 Spine 裡的 animation 名稱）")]
    public string idleAnim  = "blackLia_idle";
    public string idle2Anim = "blackLia_idle2";
    public string idle3Anim = "blackLia_idle3";

    public string winAnim   = "blackLia_win";

    public string loseAnim  = "blackLialose";
    public string lose2Anim = "blackLialose2";
    public string lose3Anim = "blackLialose3";

    void Reset()
    {
        if (skeleton == null)
            skeleton = GetComponent<SkeletonAnimation>();
    }

    void Awake()
    {
        if (skeleton == null)
            skeleton = GetComponent<SkeletonAnimation>();
    }

    void Start()
    {
        PlayIdle();
    }

    // 給外部直接呼叫的函式 ----------------------------------

    public void PlayIdle()  => PlayLoop(idleAnim);
    public void PlayIdle2() => PlayLoop(idle2Anim);
    public void PlayIdle3() => PlayLoop(idle3Anim);

    public void PlayWin()   => PlayLoop(winAnim);

    public void PlayLose()  => PlayOnce(loseAnim);
    public void PlayLose2() => PlayOnce(lose2Anim);
    public void PlayLose3() => PlayOnce(lose3Anim);

    // 方便用 tag 直接呼叫（之後給 DialogueController 用）
    public void PlayByTag(string tag)
    {
        switch (tag)
        {
            case "blackLiaidle":
                PlayIdle();
                break;
            case "blackLiaidle2":
                PlayIdle2();
                break;
            case "blackLiaidle3":
                PlayIdle3();
                break;
            case "blackLiawin":
                PlayWin();
                break;
            case "blackLialose":
                PlayLose();
                break;
            case "blackLialose2":
                PlayLose2();
                break;
            case "blackLialose3":
                PlayLose3();
                break;
        }
    }

    // 內部共用函式 -------------------------------------------

    void PlayLoop(string animName)
    {
        if (skeleton == null || string.IsNullOrEmpty(animName)) return;
        skeleton.state.SetAnimation(0, animName, true);
    }

    void PlayOnce(string animName)
    {
        if (skeleton == null || string.IsNullOrEmpty(animName)) return;
        skeleton.state.SetAnimation(0, animName, false);
    }
}
