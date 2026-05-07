using UnityEngine;
using System.Collections.Generic;

public class PlayerCameraAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public KeyCode attackKey = KeyCode.F;
    public float attackRadius = 1.5f;
    public LayerMask bugLayerMask;
    public Transform attackCenter;

    [Header("方向過濾")]
    public PlayerController playerController;
    [Tooltip("0 = ±90°（正前方半圓）, 0.5 = ±60°, 負值=更寬")]
    public float facingDotThreshold = 0f;

    [Header("相機持有狀態")]
    public bool hasCamera = true;

    [Header("淨化目標")]
    public BlackLiaPurifyTarget purifyTarget;

    private PlayerSpineSwitcher spineSwitcher;
    private Vector2 lastFacingDir = Vector2.right;

    void Awake()
    {
        spineSwitcher = GetComponentInChildren<PlayerSpineSwitcher>();
        if (spineSwitcher == null)
            spineSwitcher = GetComponent<PlayerSpineSwitcher>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 持續記錄最後移動方向
        if (playerController != null && playerController.InputVector.sqrMagnitude > 0.01f)
            lastFacingDir = playerController.InputVector.normalized;

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

        if (spineSwitcher != null)
            spineSwitcher.PlayShot();

        if (!attackCenter) attackCenter = transform;

        Vector2 center = attackCenter.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, bugLayerMask);

        if (hits != null && hits.Length > 0)
        {
            HashSet<EnemyHitBlinkAndVanish2D> handled = new HashSet<EnemyHitBlinkAndVanish2D>();

            foreach (var hit in hits)
            {
                if (!hit) continue;

                // 方向過濾：只打面向方向那側的蟲
                Vector2 toEnemy = ((Vector2)hit.transform.position - center).normalized;
                if (Vector2.Dot(lastFacingDir, toEnemy) < facingDotThreshold)
                    continue;

                var trainingBug = hit.GetComponentInParent<Core.TrainingBug>();
                if (trainingBug != null)
                {
                    trainingBug.HitByCamera();
                    continue;
                }

                var enemy = hit.GetComponentInParent<EnemyHitBlinkAndVanish2D>();
                if (enemy != null && !handled.Contains(enemy))
                {
                    handled.Add(enemy);
                    enemy.TriggerVanishSequence();
                }
            }
        }
        else
        {
            Debug.Log("[CameraAttack] 沒有拍到任何蟲");
        }

        // 淨化黑色利亞（按 F 直接觸發，不判距離）
        if (purifyTarget != null && purifyTarget.IsActive)
            purifyTarget.TriggerPurify();
    }

    void OnDrawGizmosSelected()
    {
        if (!attackCenter) attackCenter = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackCenter.position, attackRadius);

        // 顯示面向方向
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(attackCenter.position, (Vector3)lastFacingDir * attackRadius);
    }
}
