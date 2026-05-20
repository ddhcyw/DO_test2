using UnityEngine;

public class ClickOpenPanelOnly : MonoBehaviour
{
    [Header("要開啟的 UI 面板")]
    public GameObject panelToOpen;

    // 當滑鼠點擊到這個物件的 Collider 時，無視一切 UI 阻擋強行觸發
    private void OnMouseDown()
    {
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
            Debug.Log($"<color=lime>【純代碼點擊成功】</color> 已強行開啟 UI 頁面：<b>{panelToOpen.name}</b>");
        }
        else
        {
            Debug.LogError($"<color=red>【錯誤】</color> 物件 <b>{gameObject.name}</b> 忘記拖入要開啟的 panelToOpen 物件了！");
        }
    }
}