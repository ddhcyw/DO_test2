using System.Collections;
using UnityEngine;
using Spine.Unity;

public class BlackLiaPurifyTarget : MonoBehaviour
{
    [Header("Spine 元件")]
    public SkeletonAnimation skeleton;

    [Header("淡出設定")]
    public float fadeTime = 1.5f;

    public bool IsActive { get; private set; }

    private bool triggered = false;

    void Reset()
    {
        if (skeleton == null)
            skeleton = GetComponentInChildren<SkeletonAnimation>();
    }

    public void Activate()
    {
        triggered = false;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void TriggerPurify()
    {
        if (triggered || !IsActive) return;
        triggered = true;
        IsActive = false;

        StartCoroutine(FadeOutAndFinish());
    }

    IEnumerator FadeOutAndFinish()
    {
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);

            if (skeleton != null)
                skeleton.skeleton.A = alpha;

            yield return null;
        }

        if (skeleton != null)
            skeleton.skeleton.A = 0f;

        if (GameFlow.Instance != null)
            GameFlow.Instance.OnPurifyComplete();

        gameObject.SetActive(false);
    }
}
