using UnityEngine;

public class ClueObjectInteract : MonoBehaviour
{
    [Header("要播放的 Ink 節點名稱")]
    public string inkKnotName;

    void OnMouseDown()
    {
        // 1. 如果已經在對話中，不要重複觸發
        if (GameFlow.Instance.CurrentState == GameFlow.GameState.Talking) return;

        Debug.Log($"點擊了線索物件，播放劇情: {inkKnotName}");

        // 2. 呼叫 GameFlow 開始對話
        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.StartDialogue(inkKnotName);
        }
    }
}