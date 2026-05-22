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

    [Header("攻擊時鎖定移動")]
    public float attackLockTime = 0.4f;   // 攻擊動作持續鎖定秒數

    [Header("相機持有狀態")]
    [System.NonSerialized] public bool hasCamera = false;

    [Header("淨化目標")]
    public BlackLiaPurifyTarget purifyTarget;

    [Header("攻擊範圍指示器")]
    [SerializeField] Transform rangeIndicatorPivot;
    [SerializeField] SpriteRenderer rangeIndicator;
    [SerializeField] Sprite facingLeftSprite;   // 面朝左的扇形素材（目前已正確的那張）
    [SerializeField] Sprite facingRightSprite;  // 面朝右的扇形素材（Ellipse 24）
    [Tooltip("面朝右時的扇形角度")]
    [SerializeField] float facingRightAngle = 0f;
    [Tooltip("面朝左時的扇形角度")]
    [SerializeField] float facingLeftAngle = 0f;

    private PlayerSpineSwitcher spineSwitcher;
    private Vector2 lastFacingDir = Vector2.right;

    void Awake()
    {
        hasCamera = PlayerPrefs.GetInt("HasCamera", 0) == 1;

        spineSwitcher = GetComponentInChildren<PlayerSpineSwitcher>();
        if (spineSwitcher == null)
            spineSwitcher = GetComponent<PlayerSpineSwitcher>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (rangeIndicator != null)
            rangeIndicator.gameObject.SetActive(false);
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

        if (playerController != null)
            StartCoroutine(LockMovement());

        ShowRangeIndicator();

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

    void ShowRangeIndicator()
    {
        if (rangeIndicator == null) return;

        float facing = spineSwitcher != null ? spineSwitcher.GetFacing() : (lastFacingDir.x >= 0 ? 1f : -1f);

        if (facing > 0 && facingRightSprite != null)
            rangeIndicator.sprite = facingRightSprite;
        else if (facing <= 0 && facingLeftSprite != null)
            rangeIndicator.sprite = facingLeftSprite;

        float angle = facing > 0 ? facingRightAngle : facingLeftAngle;
        Transform pivot = rangeIndicatorPivot != null ? rangeIndicatorPivot : rangeIndicator.transform;
        pivot.localRotation = Quaternion.Euler(0f, 0f, angle);

        rangeIndicator.gameObject.SetActive(true);
        StartCoroutine(HideRangeAfter(attackLockTime));
    }

    System.Collections.IEnumerator HideRangeAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rangeIndicator != null)
            rangeIndicator.gameObject.SetActive(false);
    }

    System.Collections.IEnumerator LockMovement()
    {
        playerController.EnableMovement(false);
        yield return new WaitForSeconds(attackLockTime);
        playerController.EnableMovement(true);
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
