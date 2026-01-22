using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public Image icon;
    public Item item; // 當前格子內的物品
    public int slotIndex;
    public SlotType slotType;

    private Transform originalParent;
    private static GameObject dragIcon; // 靜態變數，確保同一時間只有一個拖曳圖示

    // 新增一個靜態旗標，追蹤拖放是否成功
    private static bool dropSuccessful;

    // 包含 "null 圖示變透明" 的 AddItem 邏輯
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
        // 銷毀拖曳時的暫存圖示
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        // 檢查條件：(1) 沒有放入其他格子 (2) 手上有物品
        if (!dropSuccessful && item != null)
        {
            // =============================================================
            // 限制：只有從「主畫面工具列 (HudToolbar)」拖曳才有效
            // =============================================================
            if (slotType == SlotType.HudToolbar)
            {
                // --- 檢測是否對準場景中的可拍攝物體 (物理射線) ---
                if (item is CameraItem cameraItem)
                {
                    // 將滑鼠位置轉為世界座標
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    // 發射射線偵測 Collider
                    RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

                    // 檢查是否打到有 Photographable 腳本的物體
                    if (hit.collider != null && hit.collider.TryGetComponent<Photographable>(out Photographable target))
                    {
                        Debug.Log("在工具列拖曳相機，拍到了：" + target.name);
                        // 在滑鼠位置打開相機觀景窗
                        cameraItem.UseItemAtPosition(Input.mousePosition);
                    }
                }

                // --- 檢測是否對準 NPC (UI 射線) ---
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (RaycastResult result in results)
                {
                    NpcInteraction npc = result.gameObject.GetComponent<NpcInteraction>();
                    if (npc != null)
                    {
                        npc.OnItemDropped(item);
                        // 如果是一次性道具，可以在這裡移除：
                        // InventoryManager.Instance.Remove(item); 
                        break;
                    }
                }
            }
        }

        // 恢復原本格子的圖示顯示
        if (this.item != null)
        {
            icon.enabled = (item.icon != null);
        }

        dragIcon = null;
        dropSuccessful = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        InventorySlot sourceSlot = draggedObject.GetComponent<InventorySlot>();

        if (sourceSlot != null && sourceSlot != this)
        {
            dropSuccessful = true;

            // 執行原本的移動物品邏輯
            InventoryManager.Instance.MoveItem(
                sourceSlot.slotType,
                sourceSlot.slotIndex,
                this.slotType,
                this.slotIndex
            );

            // 通知教學系統
            CheckTutorialProgress();
        }
    }
    private void CheckTutorialProgress()
    {
        // 1. 檢查 TutorialManager 是否存在
        if (TutorialManager.Instance == null) return;

        // 2. 只有在「教學正在進行中」才做動作 (避免遊戲後期整理背包時誤觸)
        if (!TutorialManager.Instance.IsTutorialActive) return;

        // 3. 檢查條件：如果這個格子是「工具列」
        if (slotType == SlotType.HudToolbar || slotType == SlotType.MenuToolbar)
        {
            // 4. (選填) 也可以再嚴格一點：檢查放進來的東西是不是相機
            // if (item != null && item.itemName == "Camera") ...

            Debug.Log("物品放入工具列，觸發教學下一步！");
            TutorialManager.Instance.NextStep();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 左鍵點擊 + 有物品
        if (eventData.button == PointerEventData.InputButton.Left && item != null)
        {
            // 情況 1: 在主畫面工具列 -> 使用道具
            if (slotType == SlotType.HudToolbar)
            {
                item.UseItem();
            }
            // 情況 2: 在背包選單內 -> 顯示道具資訊 (*** 新增這段 ***)
            else if (slotType == SlotType.Inventory || slotType == SlotType.MenuToolbar)
            {
                // 呼叫我們剛剛寫的 UI 腳本來更新畫面
                if (ItemDescriptionUI.Instance != null)
                {
                    ItemDescriptionUI.Instance.ShowDescription(item);
                }
            }
        }
    }
}
