using UnityEngine;
using System.Collections;
using Spine.Unity;

public class PlayerSpineSwitcher : MonoBehaviour
{
    [Header("主 Spine / 拍照 Spine")]
    public GameObject mainSpineObject;
    public GameObject shotSpineObject;

    [Header("拍照 Spine 元件")]
    public SkeletonAnimation shotSkeletonAnimation;
    public string shotAnimationName = "animation";

    [Header("朝向設定")]
    [Tooltip("如果角色面向右時 ScaleX 應該是負的，請勾選")]
    public bool faceRightIsNegativeX = true;

    private float lastFacingX = 1f;
    private bool isPlayingShot = false;

    void Awake()
    {
        if (shotSpineObject != null)
            shotSpineObject.SetActive(false);

        // 從 MainSpine 目前實際朝向初始化
        if (mainSpineObject != null)
        {
            var mainAnim = mainSpineObject.GetComponent<SkeletonAnimation>();
            if (mainAnim != null && mainAnim.Skeleton != null)
            {
                float currentScaleX = mainAnim.Skeleton.ScaleX;

                // 依照 faceRightIsNegativeX 推回「目前是朝左還朝右」
                if (faceRightIsNegativeX)
                    lastFacingX = currentScaleX < 0 ? 1f : -1f;
                else
                    lastFacingX = currentScaleX > 0 ? 1f : -1f;
            }
        }

        ApplyFacingToBoth();
    }

    public void SetFacing(float x)
    {
        if (Mathf.Abs(x) > 0.01f)
            lastFacingX = x > 0 ? 1f : -1f;

        ApplyFacingToBoth();
    }

    public float GetFacing()
    {
        return lastFacingX;
    }

    void ApplyFacingToBoth()
    {
        float sign = lastFacingX > 0 ? 1f : -1f;
        if (faceRightIsNegativeX)
            sign *= -1f;

        ApplyFacing(mainSpineObject, sign);
        ApplyFacing(shotSpineObject, sign);
    }

    void ApplyFacing(GameObject obj, float sign)
    {
        if (obj == null) return;

        var skeletonAnim = obj.GetComponent<SkeletonAnimation>();
        if (skeletonAnim == null || skeletonAnim.Skeleton == null) return;

        float absScaleX = Mathf.Abs(skeletonAnim.Skeleton.ScaleX);
        if (absScaleX < 0.0001f) absScaleX = 1f;

        skeletonAnim.Skeleton.ScaleX = absScaleX * Mathf.Sign(sign);
    }

    public void PlayShot()
    {
        if (isPlayingShot) return;
        if (mainSpineObject == null || shotSpineObject == null || shotSkeletonAnimation == null)
        {
            Debug.LogWarning("[PlayerSpineSwitcher] 參考未指定完整");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PlayShotRoutine());
    }

    IEnumerator PlayShotRoutine()
    {
        isPlayingShot = true;

        ApplyFacingToBoth();

        mainSpineObject.SetActive(false);
        shotSpineObject.SetActive(true);

        var anim = shotSkeletonAnimation.Skeleton.Data.FindAnimation(shotAnimationName);
        if (anim == null)
        {
            Debug.LogWarning($"[PlayerSpineSwitcher] 找不到拍照動畫：{shotAnimationName}");
            shotSpineObject.SetActive(false);
            mainSpineObject.SetActive(true);
            isPlayingShot = false;
            yield break;
        }

        var entry = shotSkeletonAnimation.AnimationState.SetAnimation(0, shotAnimationName, false);
        float duration = anim.Duration;
        if (entry != null && entry.Animation != null)
            duration = entry.Animation.Duration;

        yield return new WaitForSeconds(duration);

        shotSpineObject.SetActive(false);
        mainSpineObject.SetActive(true);

        isPlayingShot = false;
    }

    public bool IsPlayingShot()
    {
        return isPlayingShot;
    }
}