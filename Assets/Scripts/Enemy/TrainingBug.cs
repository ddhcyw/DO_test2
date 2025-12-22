using System.Collections;
using UnityEngine;
using Spine.Unity;

namespace Core
{
    public class TrainingBug : MonoBehaviour
    {
        [Header("生命值設定")]
        public int maxHp = 3;
        int currentHp;

        [Header("Spine")]
        public SkeletonAnimation skeletonAnimation; // 沒指定就自動抓
        [Tooltip("這隻蟲要用的 skin 名稱（例如 blueworm / greenworm / redworm）")]
        public string skinName = "blueworm";

        [Tooltip("受傷動畫名稱（沒有就留空）")]
        public string hitAnim = "hit";
        [Tooltip("死亡動畫名稱（沒有就留空）")]
        public string deadAnim = "dead";
        [Tooltip("待機動畫名稱（建議要有）")]
        public string idleAnim = "idle";

        [Header("被打到的視覺效果（Spine 變色閃白）")]
        public Color hitFlashColor = Color.white;
        public float hitFlashTime = 0.1f;

        [HideInInspector]
        public TrainingManager manager;

        bool isDead;

        void Awake()
        {
            currentHp = maxHp;

            if (!skeletonAnimation)
                skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            // 套用 skin
            if (skeletonAnimation && skeletonAnimation.Skeleton != null && !string.IsNullOrEmpty(skinName))
            {
                skeletonAnimation.Skeleton.SetSkin(skinName);
                skeletonAnimation.Skeleton.SetSlotsToSetupPose();
                skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
            }

            // 播 idle
            if (skeletonAnimation && skeletonAnimation.AnimationState != null && !string.IsNullOrEmpty(idleAnim))
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
        }

        public void HitByCamera()
        {
            if (isDead) return;

            currentHp--;
            Debug.Log($"[TrainingBug] 被相機打到，剩餘 HP = {currentHp}");

            if (currentHp <= 0)
            {
                Purify();
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(HitFeedback());
            }
        }

        void Purify()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log("[TrainingBug] 被淨化！");

            if (manager != null)
                manager.OnBugPurified(this);

            // 有 dead 動畫就播完再銷毀，沒有就直接銷毀
            if (skeletonAnimation && skeletonAnimation.AnimationState != null && !string.IsNullOrEmpty(deadAnim))
            {
                var entry = skeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);
                float t = (entry != null && entry.Animation != null) ? entry.Animation.Duration : 0f;
                StartCoroutine(DestroyAfter(t));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        IEnumerator HitFeedback()
        {
            // 播 hit 動畫（可選）
            if (skeletonAnimation && skeletonAnimation.AnimationState != null && !string.IsNullOrEmpty(hitAnim))
            {
                skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
                if (!string.IsNullOrEmpty(idleAnim))
                    skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0f);
            }

            // 閃白（改整體 skeleton 顏色）
            if (skeletonAnimation && skeletonAnimation.Skeleton != null)
            {
                var sk = skeletonAnimation.Skeleton;
                float or = sk.R, og = sk.G, ob = sk.B, oa = sk.A;

                sk.R = hitFlashColor.r; sk.G = hitFlashColor.g; sk.B = hitFlashColor.b; sk.A = hitFlashColor.a;
                yield return new WaitForSeconds(hitFlashTime);
                sk.R = or; sk.G = og; sk.B = ob; sk.A = oa;
            }
        }

        IEnumerator DestroyAfter(float seconds)
        {
            if (seconds > 0f)
                yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }
    }
}
