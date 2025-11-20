using System.Collections;
using UnityEngine;

namespace Core
{
    public class TrainingBug : MonoBehaviour
    {
        [Header("生命值設定")]
        [Tooltip("這隻數據蟲的最大 HP")]
        public int maxHp = 3;

        int currentHp;

        [Header("被打到的視覺效果")]
        public SpriteRenderer spriteRenderer;      // 沒指定就自動抓子物件
        public Color hitFlashColor = Color.white; // 被打到時閃一下的顏色
        public float hitFlashTime = 0.1f;

        [HideInInspector]
        public TrainingManager manager;

        void Awake()
        {
            currentHp = maxHp;

            if (!spriteRenderer)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// 被玩家相機打到一次
        /// </summary>
        public void HitByCamera()
        {
            currentHp--;
            Debug.Log($"[TrainingBug] 被相機打到，剩餘 HP = {currentHp}");

            if (currentHp <= 0)
            {
                Purify();
            }
            else
            {
                // 只做受傷特效，不消失
                if (spriteRenderer)
                {
                    StopAllCoroutines();
                    StartCoroutine(HitFlash());
                }
            }
        }

        void Purify()
        {
            Debug.Log("[TrainingBug] 被淨化！");

            if (manager != null)
            {
                manager.OnBugPurified(this);
            }

            Destroy(gameObject);
        }

        IEnumerator HitFlash()
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashTime);
            spriteRenderer.color = original;
        }
    }
}
