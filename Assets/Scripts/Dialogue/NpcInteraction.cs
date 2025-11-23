// NpcInteraction.cs

using UnityEngine;

public class NpcInteraction : MonoBehaviour
{
    public string npcName = "Lia";

    // 引用您的 DialogueController (假設您有一個單例或可以透過 FindObject 找到)
    private DialogueController dialogueController;

    void Start()
    {
        dialogueController = FindObjectOfType<DialogueController>(); // 自動尋找
    }

    public void OnItemDropped(Item item)
    {
        if (item.name == "Flyer")
        {
            Debug.Log($"在 {npcName} 身上使用了 {item.name}！");

            if (dialogueController != null)
            {
                // 啟動特定的 Ink 對話節點
                // 對應 ImagePlaza.ink 中的 === plaza_leah_flyer ===
                dialogueController.StartInkDialogue("plaza_leah_flyer");
            }

            // 移除物品 (如果需要)
            InventoryManager.Instance.Remove(item);
        }
        else
        {
            Debug.Log($"{npcName} 對 {item.name} 沒有興趣。");
            // 可以播放一個通用的 "不需要" 對話
        }
    }
}