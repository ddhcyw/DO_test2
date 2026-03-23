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

    private PlayerSpineSwitcher spineSwitcher;

    void Awake()
    {
        spineSwitcher = GetComponentInChildren<PlayerSpineSwitcher>();
        if (spineSwitcher == null)
            spineSwitcher = GetComponent<PlayerSpineSwitcher>();
    }

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

        if (spineSwitcher != null)
            spineSwitcher.PlayShot();

        if (!attackCenter) attackCenter = transform;

        Vector2 center = attackCenter.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, bugLayerMask);

        if (hits == null || hits.Length == 0)
        {
            Debug.Log("[CameraAttack] 沒有拍到任何蟲");
            return;
        }

        HashSet<EnemyHitBlinkAndVanish2D> handled = new HashSet<EnemyHitBlinkAndVanish2D>();

        foreach (var hit in hits)
        {
            if (!hit) continue;

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

    void OnDrawGizmosSelected()
    {
        if (!attackCenter) attackCenter = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackCenter.position, attackRadius);
    }
}