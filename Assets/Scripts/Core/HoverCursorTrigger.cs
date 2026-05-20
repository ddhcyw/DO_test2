using UnityEngine;

public class HoverCursorTrigger : MonoBehaviour
{
    [Header("游標設定")]
    [Tooltip("當滑鼠移到這個物件上時要顯示的自訂 Icon")]
    public Texture2D hoverCursorIcon;

    [Tooltip("Icon 的對齊中心點 (一般左上角為 Vector2.zero，若要圖案正中心對準滑鼠可以設成圖片寬高的一半)")]
    public Vector2 hotSpot = Vector2.zero;

    // 當對話中、或是打開書本、小遊戲時，不應該觸發懸停圖示
    // 這裡可以直接拉妳現有的 GameFlow 來做狀態防呆
    private bool CanChangeCursor()
    {
        if (GameFlow.Instance != null)
        {
            // 只有在探索狀態下，滑鼠指到物件才換 icon
            return GameFlow.Instance.CurrentState == GameFlow.GameState.Exploring;
        }
        return true;
    }

    // 1. 滑鼠移入物件時觸發
    void OnMouseEnter()
    {
        Debug.Log($"目前狀態: {GameFlow.Instance.CurrentState}");
        if (!CanChangeCursor()) return;

        if (hoverCursorIcon != null)
        {
            // 換成自訂的 Icon (CursorMode.Auto 讓系統自動處理硬體渲染，最流暢)
            Cursor.SetCursor(hoverCursorIcon, hotSpot, CursorMode.Auto);
        }
    }

    // 2. 滑鼠移出物件時觸發
    void OnMouseExit()
    {
        // 無條件恢復成 Unity 預設的作業系統游標
        ResetCursor();
    }

    // 防呆：如果物件突然被隱藏(SetActive(false))或銷毀，也要強迫把游標還原，否則圖示會卡住
    void OnDisable()
    {
        ResetCursor();
    }

    private void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}