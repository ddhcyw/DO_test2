using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    public int Current { get; private set; }

    [Header("事件（Inspector 拖入對應方法）")]
    public UnityEvent onHit;   // 受傷但未死亡時觸發
    public UnityEvent onDie;   // HP 歸零時觸發

    void Awake() { Current = maxHP; }

    public void TakeDamage(int amount)
    {
        if (Current <= 0) return;
        Current = Mathf.Max(Current - amount, 0);
        if (Current == 0) Die();
        else onHit?.Invoke();
    }

    void Die()
    {
        Debug.Log($"{name} died");
        onDie?.Invoke();
        // 若沒有掛載任何 onDie 事件（例如 EnemyHitBlinkAndVanish2D），直接銷毀
        if (onDie == null || onDie.GetPersistentEventCount() == 0)
            Destroy(gameObject);
    }
}
