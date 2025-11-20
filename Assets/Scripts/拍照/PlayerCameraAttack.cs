using UnityEngine;
using Core;

public class PlayerCameraAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackRange = 2f;
    public LayerMask bugLayerMask;        // 只打得到 Databug 的 Layer
    public Transform cameraOrigin;        // 發射射線的位置（角色中間或相機位置）

    [Header("相機持有狀態")]
    public bool hasCamera = true;         // 如果是由 Inventory 控制，就讓別的系統來改這個值

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (!hasCamera)
        {
            Debug.Log("PlayerCameraAttack: 攻擊時沒有相機");
            return;
        }

        if (!cameraOrigin)
            cameraOrigin = transform;

        // 這裡簡單用面朝右/左來決定方向，如果你有自己的 facingDir 就改成那個
        Vector2 dir = transform.right; // 假設角色向右是正方向

        RaycastHit2D hit = Physics2D.Raycast(
            cameraOrigin.position,
            dir,
            attackRange,
            bugLayerMask
        );

        if (hit.collider != null)
        {
            // 敵人可能掛在子物件 Collider 上，所以用 GetComponentInParent
            TrainingBug bug = hit.collider.GetComponentInParent<TrainingBug>();
            if (bug != null)
            {
                bug.HitByCamera();
                Debug.Log("PlayerCameraAttack: 成功拍到數據蟲");
            }
            else
            {
                Debug.Log("PlayerCameraAttack: 打到的東西沒有 TrainingBug 組件");
            }
        }
        else
        {
            Debug.Log("PlayerCameraAttack: 目前沒有蟲蟲被拍到");
        }
    }
}
