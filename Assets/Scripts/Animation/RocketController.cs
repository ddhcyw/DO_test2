using UnityEngine;
using System.Collections;

public class RocketController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Animator Params")]
    public string igniteTrigger = "Ignite";
    public string idleStateName = "Idle";

    [Header("Ignite Effect")]
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer openEyesSpriteRenderer;
    public Color glowColor = new Color(1f, 0.95f, 0.7f, 1f);
    public float glowInDuration = 0.5f;
    public float shakeDuration = 3.5f;
    public float glowOutDuration = 1f;

    private Color normalColor;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;

        if (openEyesSpriteRenderer != null)
            openEyesSpriteRenderer.gameObject.SetActive(false);
    }

    public void PlayIgnite()
    {
        if (!animator)
        {
            Debug.LogError("RocketController: animator 未指定");
            return;
        }

        Debug.Log("RocketController: PlayIgnite() 被呼叫");
        StartCoroutine(IgniteSequence());
    }

    IEnumerator IgniteSequence()
    {
        // Phase 1: 隱藏閉眼 SR，只顯示睜眼 SR + 發光
        Debug.Log($"[Rocket] Phase1 開始 | openEyesSR={(openEyesSpriteRenderer != null ? openEyesSpriteRenderer.gameObject.name : "NULL")} | sprite={(openEyesSpriteRenderer != null ? (openEyesSpriteRenderer.sprite != null ? openEyesSpriteRenderer.sprite.name : "無sprite") : "SR為null")}");
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (openEyesSpriteRenderer) openEyesSpriteRenderer.gameObject.SetActive(true);
        Debug.Log($"[Rocket] openEyesSR.activeSelf={openEyesSpriteRenderer?.gameObject.activeSelf}");

        float t = 0;
        while (t < glowInDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / glowInDuration);
            if (openEyesSpriteRenderer) openEyesSpriteRenderer.color = Color.Lerp(normalColor, glowColor, ratio);
            yield return null;
        }
        if (openEyesSpriteRenderer) openEyesSpriteRenderer.color = glowColor;
        Debug.Log("[Rocket] Phase1 結束，進入 Phase2");

        // Phase 2: 關閉睜眼 SR，恢復閉眼 SR，播放 shake
        if (openEyesSpriteRenderer) openEyesSpriteRenderer.gameObject.SetActive(false);
        if (spriteRenderer) { spriteRenderer.color = glowColor; spriteRenderer.enabled = true; }
        animator.ResetTrigger(igniteTrigger);
        animator.SetTrigger(igniteTrigger);
        Debug.Log("[Rocket] Phase2 shake 開始");
        yield return new WaitForSeconds(shakeDuration);

        // Phase 3: 暗掉
        animator.Play(idleStateName, 0, 0f);
        t = 0;
        while (t < glowOutDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer)
                spriteRenderer.color = Color.Lerp(glowColor, normalColor, Mathf.Clamp01(t / glowOutDuration));
            yield return null;
        }
        if (spriteRenderer) spriteRenderer.color = normalColor;
    }

    public void BackToIdle()
    {
        if (!animator) return;

        Debug.Log("RocketController: BackToIdle() 被呼叫");
        animator.Play(idleStateName, 0, 0f);
    }
}
