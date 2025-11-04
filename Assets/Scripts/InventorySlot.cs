using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public Image icon;
    public Item item; // 當前格子內的物品
    public int slotIndex;
    public SlotType slotType;

    private Transform originalParent;
    private static GameObject dragIcon; // 靜態變數，確保同一時間只有一個拖曳圖示

    // 關鍵修正 1: 新增一個靜態旗標，追蹤拖放是否成功
    private static bool dropSuccessful;

    // 修正 2: 包含 "null 圖示變透明" 的 AddItem 邏輯
    public void AddItem(Item newItem)
    {
        item = newItem;

        if (newItem != null && newItem.icon != null)
        {
            // 如果物品有效，且有圖示
            icon.sprite = newItem.icon;
            icon.enabled = true;
            icon.color = Color.white;
        }
        else
        {
            // 如果物品是 null，或是物品沒有圖示 (icon 是 null)
            ClearSlot(); // 直接呼叫 ClearSlot 保持邏輯一致
        }
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false; // 隱藏 Image 元件
    }

    // --- Drag and Drop ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;

        // 關鍵修正 3: 每次開始拖曳時，重設旗標
        dropSuccessful = false;

        // 建立一個暫時的拖曳圖示
        dragIcon = new GameObject("DragIcon");
        Image newImage = dragIcon.AddComponent<Image>();
        newImage.sprite = icon.sprite;
        newImage.raycastTarget = false; // 讓滑鼠射線可以穿透它

        dragIcon.transform.SetParent(transform.root, false);
        dragIcon.transform.SetAsLastSibling();

        originalParent = transform;
        icon.enabled = false; // 隱藏原始圖示
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        // 關鍵修正 4: 
        // 只有在拖放「失敗」時，才恢復原始圖示
        if (!dropSuccessful)
        {
            icon.enabled = (item != null && item.icon != null);
        }

        dragIcon = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        InventorySlot sourceSlot = draggedObject.GetComponent<InventorySlot>();

        if (sourceSlot != null && sourceSlot != this)
        {
            dropSuccessful = true;

            // 4. (修改) 呼叫 Manager 的新方法
            InventoryManager.Instance.MoveItem(
                sourceSlot.slotType,  // 來源類型
                sourceSlot.slotIndex, // 來源索引
                this.slotType,        // 目標類型
                this.slotIndex        // 目標索引
            );
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. (修改) 只有在是左鍵點擊、有物品、且插槽類型是「HudToolbar」時
        if (eventData.button == PointerEventData.InputButton.Left && item != null && slotType == SlotType.HudToolbar)
        {
            // 呼叫物品自己的 "UseItem" 方法！
            item.UseItem();
        }
        else if (item != null && slotType == SlotType.MenuToolbar)
        {
            // (可選) 玩家點擊了背包選單中的工具列，可以給個提示
            Debug.Log("請將道具拖曳到主畫面工具列來使用。");
        }
    }
}
