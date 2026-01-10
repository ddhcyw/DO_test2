using UnityEngine;

public class PlayerCameraAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public KeyCode attackKey = KeyCode.K;
    public float attackRadius = 1.5f;
    public LayerMask bugLayerMask;
    public Transform attackCenter;

    [Header("相機持有狀態")]
    public bool hasCamera = true;

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
            TryShoot();
    }

    void TryShoot()
    {
        if (!hasCamera)
        {
            Debug.Log("PlayerCameraAttack: 沒有相機，不能攻擊");
            return;
        }

        if (!attackCenter) attackCenter = transform;

        Vector2 center = attackCenter.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, bugLayerMask);

        if (hits == null || hits.Length == 0)
        {
            Debug.Log("PlayerCameraAttack: 這一圈沒有蟲蟲被拍到");
            return;
        }

        foreach (var hit in hits)
        {
            if (!hit) continue;

            // 可選：用 Tag 再篩一次
            if (!hit.CompareTag("Enemy"))
                continue;

            // 觸發「閃兩次後消失」
            var blink = hit.GetComponent<EnemyHitBlinkAndVanish2D>();
            if (blink != null)
            {
                Debug.Log($"PlayerCameraAttack: 淨化(閃爍) {hit.name}");
                blink.TriggerVanishSequence();
            }
            else
            {
                // 保底：如果敵人沒掛閃爍腳本，就直接刪掉避免卡住
                Debug.LogWarning($"PlayerCameraAttack: {hit.name} 沒有 EnemyHitBlinkAndVanish2D，改用 Destroy");
                Destroy(hit.gameObject);
            }
        }

        // TODO: 在這裡播放拍照動畫 / 相機閃光特效 / 音效
    }

    void OnDrawGizmosSelected()
    {
        if (!attackCenter) attackCenter = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackCenter.position, attackRadius);
    }
}
