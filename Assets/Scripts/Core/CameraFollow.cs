using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    [Header("追蹤目標")]
    public Transform target;           
    [Header("平滑設定")]
    public float smoothSpeed = 5f;     
    public Vector3 offset = new Vector3(0, 0, -10); // 攝影機的偏移量（Z 要保持 -10）

    [Header("邊界設定 (可選)")]
    public bool useBounds = false;
    public Vector2 minBounds;          // 左下邊界
    public Vector2 maxBounds;          // 右上邊界

    void LateUpdate()
    {
        if (!target) return;

        //只在探索或戰鬥狀態下跟隨
        if (GameFlow.CurrentState == GameFlow.GameState.Talking)
            return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = smoothedPosition;
    }
}
