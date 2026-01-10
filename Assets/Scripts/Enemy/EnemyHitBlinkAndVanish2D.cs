using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class EnemyHitBlinkAndVanish2D : MonoBehaviour
{
    [Header("Blink")]
    public int blinkTimes = 2;
    public float blinkInterval = 0.08f;

    [Header("Disappear")]
    public bool setInactive = true; // true: SetActive(false), false: Destroy

    SkeletonAnimation sa;
    Collider2D col2D;
    Coroutine co;
    bool isDying;

    void Awake()
    {
        sa = GetComponent<SkeletonAnimation>();
        col2D = GetComponent<Collider2D>();
    }

    // 命中後呼叫
    public void TriggerVanishSequence()
    {
        if (isDying) return;
        isDying = true;

        // 1) 防止連續再被打到
        if (col2D) col2D.enabled = false;

        // 如果你有敵人 AI/移動，也可以在這裡停掉腳本
        // GetComponent<EnemyAI>()?.enabled = false;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(BlinkThenVanish());
    }

    IEnumerator BlinkThenVanish()
    {
        Skeleton sk = sa.Skeleton;
        if (sk == null)
        {
            Vanish();
            yield break;
        }

        SetAlpha(1f);

        for (int i = 0; i < blinkTimes; i++)
        {
            SetAlpha(0f);
            yield return new WaitForSeconds(blinkInterval);

            SetAlpha(1f);
            yield return new WaitForSeconds(blinkInterval);
        }

        Vanish();
    }

    void SetAlpha(float a)
    {
        var c = sa.Skeleton.GetColor();
        c.a = a;
        sa.Skeleton.SetColor(c);

        // 立刻刷新，避免延一幀才看到
        sa.LateUpdate();
    }

    void Vanish()
    {
        if (setInactive) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
