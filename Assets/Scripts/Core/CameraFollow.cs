using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追蹤目標")]
    public Transform target;

    [Header("平滑設定")]
    public float smoothSpeed = 5f;

    // 2.5D 建議偏移量：例如 (0, 10, -10) 代表在玩家上方 10 單位、後方 10 單位
    public Vector3 offset = new Vector3(0, 10, -10);

    [Header("邊界設定 (XZ 平面)")]
    public bool useBounds = false;
    public Vector2 minBounds;   // X 最小值, Z 最小值
    public Vector2 maxBounds;   // X 最大值, Z 最大值

    void LateUpdate()
    {
        if (!target) return;

        if (GameFlow.Instance != null &&
            GameFlow.Instance.CurrentState == GameFlow.GameState.Talking)
        {
            return;
        }

        // 1. 計算理想位置 (目標位置 + 偏移量)
        Vector3 desiredPosition = target.position + offset;

        // 2. 平滑移動
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // 3. 邊界限制 (現在是限制在 3D 的地面平面 X 和 Z)
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
            // 注意：2D 的 y 邊界在這裡改為 z 邊界
            smoothedPosition.z = Mathf.Clamp(smoothedPosition.z, minBounds.y, maxBounds.y);
        }

        transform.position = smoothedPosition;

        // 提示：如果你的相機 Rotation 沒有在 Inspector 鎖死，
        // 可以在這裡加一行 transform.LookAt(target); 讓相機永遠盯著角色看
    }
}