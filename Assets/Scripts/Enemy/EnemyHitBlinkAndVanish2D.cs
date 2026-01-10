using System.Collections;
using UnityEngine;
using Spine.Unity;

public class EnemyHitBlinkAndVanish2D : MonoBehaviour
{
    public int blinkCount = 2;
    public float blinkInterval = 0.08f;

    public bool disableColliderOnHit = true;

    SkeletonAnimation skeletonAnimation;
    Renderer[] renderers;

    bool isVanishing;

    void Awake()
    {
        skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void TriggerVanishSequence()
    {
        if (isVanishing) return;
        isVanishing = true;

        Debug.Log($"[Vanish] Triggered on {name}");

        if (disableColliderOnHit)
        {
            foreach (var c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;
        }

        StartCoroutine(BlinkAndVanish());
    }

    IEnumerator BlinkAndVanish()
    {
        // 確保 Spine 已初始化（避免 renderer 還沒 ready）
        if (skeletonAnimation != null) skeletonAnimation.Initialize(true);

        for (int i = 0; i < blinkCount; i++)
        {
            SetRenderers(false);
            yield return new WaitForSeconds(blinkInterval);
            SetRenderers(true);
            yield return new WaitForSeconds(blinkInterval);
        }

        Destroy(gameObject);
    }

    void SetRenderers(bool on)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].enabled = on;
    }
}
