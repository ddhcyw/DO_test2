using UnityEngine;

public class PlayerCameraAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public KeyCode attackKey = KeyCode.K;
    public float attackRadius = 1.5f;       // 攻擊半徑
    public LayerMask bugLayerMask;          // 只打得到 Databug 的 Layer
    public Transform attackCenter;          // 攻擊中心點（可指定到玩家胸口位置）

    [Header("相機持有狀態")]
    public bool hasCamera = true;           // 如果之後有物品欄，就由那邊控制

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (!hasCamera)
        {
            Debug.Log("PlayerCameraAttack: 沒有相機，不能攻擊");
            return;
        }

        if (!attackCenter)
            attackCenter = transform;

        Vector2 center = attackCenter.position;

        // 範圍判定：以玩家附近畫一個圓，偵測所有敵人
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, bugLayerMask);

        if (hits.Length == 0)
        {
            Debug.Log("PlayerCameraAttack: 這一圈沒有蟲蟲被拍到");
            return;
        }

        // 先做最單純的版本：有打到就直接淨化（Destroy）
        foreach (var hit in hits)
        {
            // 如果怪是用 Tag 管理，也可以再加一層判斷
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"PlayerCameraAttack: 淨化 {hit.name}");
                Destroy(hit.gameObject);
            }
            else
            {
                // 如果之後有 Health / Photographable，可以在這裡改成扣血或觸發特效
                Debug.Log($"PlayerCameraAttack: 打到 {hit.name}，但沒有 Enemy Tag，先忽略");
            }
        }

        // TODO: 在這裡播放拍照動畫 / 相機特效
    }

    void OnDrawGizmosSelected()
    {
        if (!attackCenter) attackCenter = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackCenter.position, attackRadius);
    }
}
