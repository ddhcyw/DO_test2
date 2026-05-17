using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackRange = 1.2f;
    public int attackDamage = 1;
    public float attackCooldown = 0.35f;
    public LayerMask enemyMask;

    [Header("攻擊原點（拖入手部位置的空物件）")]
    [SerializeField] Transform handPoint;  // 手部攻擊點，未指定則自動找 HandPoint / AttackPoint

    private float lastAttackTime = 0f;

    void Start()
    {
        if (!handPoint) handPoint = transform.Find("HandPoint");
        if (!handPoint) handPoint = transform.Find("AttackPoint");
        if (!handPoint)
            Debug.LogWarning("[PlayerAttack] 找不到 HandPoint 或 AttackPoint 子物件，請在 Inspector 拖入手部空物件。");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) TryAttack();
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        Attack();
    }

    void Attack()
    {
        if (!handPoint) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(handPoint.position, attackRange, enemyMask);
        foreach (var hit in hits)
        {
            var hp = hit.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(attackDamage);
                Debug.Log($"[PlayerAttack] 攻擊到 {hit.name}，造成 {attackDamage} 傷害！");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (handPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(handPoint.position, attackRange);
        }
    }
}
