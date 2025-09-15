using UnityEngine;

public class MeleeSmash : MonoBehaviour
{
    public Transform attackPoint;     // 放在玩家前方的空物件
    public float range = 0.7f;
    public int damage = 1;            // 砸一下扣 1，怪 maxHP=3 → 三下陣亡
    public float cooldown = 0.35f;
    public LayerMask hurtboxMask;     // 指向 Hurtbox 層
    float cd;

    void Update()
    {
        cd -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.J) && cd <= 0f) // 你可改成滑鼠或其它鍵
        {
            cd = cooldown;
            DoSmash();
        }
    }

    void DoSmash()
    {
        var hits = Physics2D.OverlapCircleAll(attackPoint.position, range, hurtboxMask);
        int count = 0;
        foreach (var h in hits)
            if (h.TryGetComponent<Health>(out var hp))
                hp.TakeDamage(damage);
        // 之後可在此觸發揮擊 VFX/音效/相機抖動
        Debug.Log($"MeleeSmash hit {count} target(s).");
  
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint) Gizmos.DrawWireSphere(attackPoint.position, range);
    }
}
