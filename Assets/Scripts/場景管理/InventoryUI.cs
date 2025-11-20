using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;    
    public Transform inventoryToolbarParent;  
    public Transform mainToolbarParent;
    public GameObject inventorySlotPrefab;

    private InventoryManager inventoryManager;
    private InventorySlot[] slots;
    private InventorySlot[] invToolbarSlots; // 選單工具列格子
    private InventorySlot[] mainToolbarSlots; // 主畫面工具列格子

    void Start()
    {
        inventoryManager = InventoryManager.Instance;
        inventoryManager.OnInventoryChanged += UpdateUI; // 訂閱事件

        // 產生一般物品欄格子
        slots = new InventorySlot[inventoryManager.inventorySpace];
        for (int i = 0; i < inventoryManager.inventorySpace; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, itemsParent);
            slots[i] = slotGO.GetComponent<InventorySlot>();
            slots[i].slotIndex = i;
            slots[i].slotType = SlotType.Inventory; // *** 指派類型 ***
        }

        // 產生「選單工具列」格子
        invToolbarSlots = new InventorySlot[inventoryManager.toolbarSpace];
        for (int i = 0; i < inventoryManager.toolbarSpace; i++)
        {
            // *** 修正：補上 Instantiate 程式碼 ***
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventoryToolbarParent);
            invToolbarSlots[i] = slotGO.GetComponent<InventorySlot>();
            invToolbarSlots[i].slotIndex = i;
            invToolbarSlots[i].slotType = SlotType.MenuToolbar; // 指派為 MenuToolbar
        }

        // 產生「主畫面工具列」格子
        mainToolbarSlots = new InventorySlot[inventoryManager.toolbarSpace];
        for (int i = 0; i < inventoryManager.toolbarSpace; i++)
        {
            // *** 修正：補上 Instantiate 程式碼 ***
            GameObject slotGO = Instantiate(inventorySlotPrefab, mainToolbarParent);
            mainToolbarSlots[i] = slotGO.GetComponent<InventorySlot>();
            mainToolbarSlots[i].slotIndex = i;
            mainToolbarSlots[i].slotType = SlotType.HudToolbar; // 指派為 HudToolbar
        }
        UpdateUI();
        //inventoryPanel.SetActive(false); // 預設關閉
    }


    void UpdateUI()
    {
        // 更新「主背包」 (讀取 inventoryItems)
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventoryManager.inventoryItems.Length && inventoryManager.inventoryItems[i] != null)
            {
                slots[i].AddItem(inventoryManager.inventoryItems[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        // 更新「選單工具列」 (讀取 toolbarItems)
        for (int i = 0; i < invToolbarSlots.Length; i++)
        {
            if (i < inventoryManager.toolbarItems.Length && inventoryManager.toolbarItems[i] != null)
            {
                invToolbarSlots[i].AddItem(inventoryManager.toolbarItems[i]);
            }
            else
            {
                invToolbarSlots[i].ClearSlot();
            }
        }

        // 更新「主畫面工具列」 (也讀取 toolbarItems)
        for (int i = 0; i < mainToolbarSlots.Length; i++)
        {
            if (i < inventoryManager.toolbarItems.Length && inventoryManager.toolbarItems[i] != null)
            {
                mainToolbarSlots[i].AddItem(inventoryManager.toolbarItems[i]);
            }
            else
            {
                mainToolbarSlots[i].ClearSlot();
            }
        }
    }
}