using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("追蹤目標")]
    public Transform target;

    [Header("平滑設定")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10); // 攝影機的偏移量（Z 要保持 -10）

    [Header("邊界設定 (可選)")]
    public bool useBounds = false;
    public Vector2 minBounds;   // 左下邊界
    public Vector2 maxBounds;   // 右上邊界

    private bool isPanning = false;

    void LateUpdate()
    {
        if (!target) return;

        // 對話中鏡頭不跟玩家，但 cinematic pan 期間需要繼續跟蹤
        if (!isPanning && GameFlow.Instance != null &&
            GameFlow.Instance.CurrentState == GameFlow.GameState.Talking)
        {
            return;
        }

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

    public void PanToTarget(Transform panTarget, float travelTime, float stayTime)
    {
        StartCoroutine(PanCoroutine(panTarget, travelTime, stayTime));
    }

    IEnumerator PanCoroutine(Transform panTarget, float travelTime, float stayTime)
    {
        isPanning = true;
        Transform savedTarget = target;
        target = panTarget;
        yield return new WaitForSeconds(travelTime + stayTime);
        target = savedTarget;
        isPanning = false;
    }
}
