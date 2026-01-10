using UnityEngine;
using System.Collections.Generic;

public class PlayerCameraAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public KeyCode attackKey = KeyCode.F;
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
            Debug.Log("[CameraAttack] 沒有相機，不能攻擊");
            return;
        }

        if (!attackCenter) attackCenter = transform;

        Vector2 center = attackCenter.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, bugLayerMask);

        if (hits == null || hits.Length == 0)
        {
            Debug.Log("[CameraAttack] 沒有拍到任何蟲");
            return;
        }

        // 避免同一隻蟲被多個 collider 重複處理
        HashSet<EnemyHitBlinkAndVanish2D> handled = new HashSet<EnemyHitBlinkAndVanish2D>();

        foreach (var hit in hits)
        {
            if (!hit) continue;

            // 從 collider 往上找 EnemyHitBlinkAndVanish2D
            var enemy = hit.GetComponentInParent<EnemyHitBlinkAndVanish2D>();

            Debug.Log(
                $"[CameraAttack] Hit: {hit.name}, " +
                $"Parent: {hit.transform.parent?.name}, " +
                $"HasVanishScript: {enemy != null}"
            );

            if (enemy == null) continue;
            if (handled.Contains(enemy)) continue;

            handled.Add(enemy);
            enemy.TriggerVanishSequence();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!attackCenter) attackCenter = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackCenter.position, attackRadius);
    }
}
