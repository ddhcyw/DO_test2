using UnityEngine;

public class NpcInteraction : MonoBehaviour
{
    [Header("NPC 設定")]
    public string npcName = "Lia"; // 務必與場景中物件設定的名字一致

    private DialogueController dialogueController;

    void Start()
    {
        // 尋找場景中的對話控制器
        dialogueController = Object.FindFirstObjectByType<DialogueController>();
    }

    /// <summary>
    /// 當玩家將道具拖曳到此 NPC 身上時觸發
    /// </summary>
    public void OnItemDropped(Item item)
    {
        if (item == null) return;

        // 偵測 Debug 訊息，確認射線有成功打到 NPC
        Debug.Log($"【NPC 接收】{npcName} 偵測到道具: {item.name} (ID: {item.itemID})");

        // --- 根據 itemID 與 npcName 進行邏輯判定 ---

        // 1. 給 Lia 傳單
        if (item.itemID == "flyer_leah" && npcName == "Lia")
        {
            HandleSuccess("plaza_leah_flyer", item);
        }
        // 2. 給 Dandadan 作品集
        else if (item.itemID == "portfolio" && npcName == "Dandadan")
        {
            HandleSuccess("dandadan_portfolio", item);
            UpdateClickInteraction();
        }
        // 3. 給 good_fortune 作品集
        else if (item.itemID == "portfolio" && npcName == "good_fortune")
        {
            HandleSuccess("good_fortune_portfolio", item);
            UpdateClickInteraction();
        }
        // 4. 給 cheap_buyer 作品集
        else if (item.itemID == "portfolio" && npcName == "cheap_buyer")
        {
            HandleSuccess("cheap_buyer_portfolio", item);
            UpdateClickInteraction();
        }
        else
        {
            // 如果 NPC 不收這個道具
            Debug.Log($"{npcName} 對道具 {item.name} (ID: {item.itemID}) 沒有反應。");
        }
    }

    /// <summary>
    /// 處理交付成功的後續（觸發對話、移除道具）
    /// </summary>
    private void HandleSuccess(string inkKnot, Item item)
    {
        Debug.Log($"【成功】{npcName} 接受了 {item.name}，觸發劇情: {inkKnot}");

        if (dialogueController != null)
        {
            dialogueController.StartInkDialogue(inkKnot);
        }

        // 同時檢查大背包與快捷列並移除道具
        RemoveItemFromTotalInventory(item);
    }

    /// <summary>
    /// 更新點擊互動狀態
    /// </summary>
    private void UpdateClickInteraction()
    {
        if (TryGetComponent<NpcClickInteract>(out var clickInteract))
        {
            clickInteract.dialogueCompleted = true;
        }
    }

    /// <summary>
    /// 同時從主背包與快捷列中移除物品
    /// </summary>
    private void RemoveItemFromTotalInventory(Item item)
    {
        if (InventoryManager.Instance == null) return;

        // 1. 移除主背包中的物品 (9格)
        InventoryManager.Instance.Remove(item);

        // 2. 手動移除快捷列中的物品 (3格) 
        // 解決原本 InventoryManager.Remove 只清空主背包的問題
        for (int i = 0; i < InventoryManager.Instance.toolbarItems.Length; i++)
        {
            if (InventoryManager.Instance.toolbarItems[i] == item)
            {
                InventoryManager.Instance.toolbarItems[i] = null;
                Debug.Log($"已從快捷列第 {i} 格移除物品。");
                break;
            }
        }

        // 3. 通知 UI 刷新
        // 假設 InventoryManager 有一個公開的事件或方法可以觸發 UI 更新
        // 如果沒有，這裡可以加入通知邏輯
    }
}