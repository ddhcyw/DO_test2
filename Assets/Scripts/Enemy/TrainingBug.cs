using System.Collections;
using System.Text;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace Core
{
    public class TrainingBug : MonoBehaviour
    {
        public enum BugColor { Blue, Green, Red } // 你現在資料是 greenwarm，不是 yellow

        [Header("生命值設定")]
        public int maxHp = 3;
        int currentHp;

        [Header("Spine")]
        public SkeletonAnimation skeletonAnimation;

        [Tooltip("顏色（實際是切換不同 loop 動畫）")]
        public BugColor color = BugColor.Blue;

        [Tooltip("loop 動畫名稱格式：{0}warm，例如 bluewarm / greenwarm / redwarm")]
        public string loopAnimPattern = "{0}warm";

        [Tooltip("受傷動畫（如果你 Spine 沒有就留空）")]
        public string hitAnim = "";   // 先留空，避免找不到
        [Tooltip("死亡動畫（如果你 Spine 沒有就留空）")]
        public string deadAnim = "";  // 先留空，避免找不到

        [Header("被打到的視覺效果（Spine 變色閃白）")]
        public Color hitFlashColor = Color.white;
        public float hitFlashTime = 0.1f;

        [HideInInspector] public TrainingManager manager;

        bool isDead;

        void Awake()
        {
            currentHp = maxHp;

            if (!skeletonAnimation)
                skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            if (!skeletonAnimation)
            {
                Debug.LogError("[TrainingBug] SkeletonAnimation not found.");
                return;
            }

            skeletonAnimation.Initialize(true);

            // 依顏色播放對應 loop 動畫（bluewarm/greenwarm/redwarm）
            string loopAnim = GetLoopAnimName(color);
            SetAnimationSafe(0, loopAnim, true);
        }

        string GetLoopAnimName(BugColor c)
        {
            string prefix = c.ToString().ToLower(); // blue/green/red
            return string.Format(loopAnimPattern, prefix);
        }

        bool HasAnimation(string animName)
        {
            var data = skeletonAnimation.Skeleton?.Data;
            return data != null && data.FindAnimation(animName) != null;
        }

        void LogAvailableAnimations()
        {
            var data = skeletonAnimation.Skeleton?.Data;
            if (data == null) return;

            var sb = new StringBuilder();
            for (int i = 0; i < data.Animations.Count; i++)
            {
                sb.Append(data.Animations.Items[i].Name);
                if (i < data.Animations.Count - 1) sb.Append(", ");
            }
            Debug.LogError($"[TrainingBug] Available animations: {sb}");
        }

        void SetAnimationSafe(int trackIndex, string animName, bool loop)
        {
            if (string.IsNullOrEmpty(animName)) return;
            if (skeletonAnimation.AnimationState == null) return;

            if (!HasAnimation(animName))
            {
                Debug.LogError($"[TrainingBug] Animation not found: '{animName}'");
                LogAvailableAnimations();
                return;
            }

            skeletonAnimation.AnimationState.SetAnimation(trackIndex, animName, loop);
        }

        void AddAnimationSafe(int trackIndex, string animName, bool loop, float delay)
        {
            if (string.IsNullOrEmpty(animName)) return;
            if (skeletonAnimation.AnimationState == null) return;

            if (!HasAnimation(animName))
            {
                Debug.LogError($"[TrainingBug] Animation not found: '{animName}'");
                LogAvailableAnimations();
                return;
            }

            skeletonAnimation.AnimationState.AddAnimation(trackIndex, animName, loop, delay);
        }

        public void HitByCamera()
        {
            if (isDead) return;

            currentHp--;
            Debug.Log($"[TrainingBug] 被相機打到，剩餘 HP = {currentHp}");

            if (currentHp <= 0) Purify();
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

            // 如果你之後真的有 dead 動畫再填 deadAnim
            if (!string.IsNullOrEmpty(deadAnim) && HasAnimation(deadAnim))
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
            if (!string.IsNullOrEmpty(hitAnim) && HasAnimation(hitAnim))
            {
                SetAnimationSafe(0, hitAnim, false);
                string loopAnim = GetLoopAnimName(color);
                AddAnimationSafe(0, loopAnim, true, 0f);
            }

            if (skeletonAnimation && skeletonAnimation.Skeleton != null)
            {
                var sk = skeletonAnimation.Skeleton;
                float or = sk.R, og = sk.G, ob = sk.B, oa = sk.A;

                // 白色等同未設定，改用深紅確保看得見
                Color flash = (hitFlashColor == Color.white)
                    ? new Color(1f, 0.25f, 0.25f, 1f)
                    : hitFlashColor;

                sk.R = flash.r; sk.G = flash.g; sk.B = flash.b; sk.A = flash.a;
                yield return new WaitForSeconds(hitFlashTime);
                sk.R = or; sk.G = og; sk.B = ob; sk.A = oa;
            }
        }

        IEnumerator DestroyAfter(float seconds)
        {
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }
    }
}
