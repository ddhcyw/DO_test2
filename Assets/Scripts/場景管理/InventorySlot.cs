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
        if (dragIcon != null) Destroy(dragIcon);

        // --- 強制偵測：不論如何都會印出這行，用來確認有沒有跑這段 ---
        Debug.Log($"【拖曳結束】物品: {(item != null ? item.name : "空")}, 來源格子類型: {slotType}");

        // 修改條件：讓大背包 (Inventory) 和 快捷列 (HudToolbar) 拖出來都有效
        if (!dropSuccessful && item != null && (slotType == SlotType.HudToolbar || slotType == SlotType.Inventory))
        {
            // --- 1. 處理相機功能 ---
            if (item is CameraItem cameraItem)
            {
                cameraItem.UseItemAtPosition(Input.mousePosition);
            }

            // --- 2. 物理偵測 (針對場景中的 2D NPC) ---
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("【物理碰撞】碰到了物體: " + hit.collider.gameObject.name);
                if (hit.collider.TryGetComponent<NpcInteraction>(out NpcInteraction npcWorld))
                {
                    Debug.Log("【成功】交給了場景 NPC: " + npcWorld.name);
                    npcWorld.OnItemDropped(item);
                    ResetSlotVisuals();
                    return;
                }
            }

            // --- 3. UI 偵測 (針對 UI 上的 NPC，原本的邏輯保留備用) ---
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent<NpcInteraction>(out NpcInteraction npcUI))
                {
                    Debug.Log("【成功】交給了 UI NPC: " + npcUI.name);
                    npcUI.OnItemDropped(item);
                    break;
                }
            }
        }

        ResetSlotVisuals();
    }

    private void ResetSlotVisuals()
    {
        if (this.item != null) icon.enabled = (item.icon != null);
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
        if (TutorialManager.Instance == null || !TutorialManager.Instance.IsTutorialActive) return;
        
        if (TutorialManager.Instance.CurrentStepIndex == TutorialManager.Instance.dragStepIndex)
        {
            // 檢查是否放入工具列 (三個鑰匙框)
            if (slotType == SlotType.MenuToolbar)
            {
                Debug.Log("教學：成功將道具拖入工具列！準備進入最後一步");
                TutorialManager.Instance.NextStep();
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 左鍵點擊 + 有物品
        if (eventData.button == PointerEventData.InputButton.Left && item != null)
        {
            // 檢查是否正在進行教學，且剛好是「點擊道具」的那一步
            CheckTutorialClick();

            // 情況 1: 在主畫面工具列 -> 使用道具
            if (slotType == SlotType.HudToolbar)
            {
                item.UseItem();
            }
            // 情況 2: 在背包選單內 -> 顯示道具資訊
            else if (slotType == SlotType.Inventory || slotType == SlotType.MenuToolbar)
            {
                if (ItemDescriptionUI.Instance != null)
                {
                    ItemDescriptionUI.Instance.ShowDescription(item);
                }
            }
        }
    }

    private void CheckTutorialClick()
    {
        if (TutorialManager.Instance == null || !TutorialManager.Instance.IsTutorialActive) return;

        if (TutorialManager.Instance.CurrentStepIndex == TutorialManager.Instance.clickStepIndex)
        {
            Debug.Log("教學：成功點擊道具！準備進入下一步");
            TutorialManager.Instance.NextStep();
        }
    }
}
