using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    public int Current { get; private set; }

    void Awake() { Current = maxHP; }

    public void TakeDamage(int amount)
    {
        Current = Mathf.Max(Current - amount, 0);
        if (Current == 0) Die();
    }

    void Die()
    {
        Destroy(gameObject); // 之後可換死亡特效
    }
}
