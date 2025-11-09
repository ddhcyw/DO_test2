using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackRange = 1.2f;       // 攻擊距離
    public int attackDamage = 1;           // 每次傷害
    public float attackCooldown = 0.35f;   // 攻擊冷卻秒數
    public LayerMask enemyMask;            // 敵人層（請勾選 "Enemy" 或你的 DataBug Layer）

    private float lastAttackTime = 0f;
    private Transform attackPoint;

    void Start()
    {
        // 假設 AttackPoint 是 Player 的子物件（可設在角色前方）
        attackPoint = transform.Find("AttackPoint");
        if (!attackPoint)
        {
            Debug.LogWarning("Player 沒有 AttackPoint 子物件，請手動建立。");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // 攻擊動畫或特效可在這裡觸發
        Attack();
    }

    void Attack()
    {
        if (!attackPoint) return;

        // 檢查範圍內的敵人
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRange, enemyMask
        );

        foreach (var hit in hits)
        {
            var hp = hit.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(attackDamage);
                Debug.Log($"🗡️ 攻擊到 {hit.name}，造成 {attackDamage} 傷害！");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
