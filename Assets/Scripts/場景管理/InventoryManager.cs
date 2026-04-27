using System.Collections.Generic;
using UnityEngine;
using System;

public enum SlotType { Inventory, MenuToolbar, HudToolbar }
public class InventoryManager : MonoBehaviour
{
    #region Singleton
    public static InventoryManager Instance { get; private set; }


    private void Awake()
    {
        // 這是 Singleton 單例模式的檢查
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // 如果 Instance 是空的，就把自己指派上去
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨場景保留

            // 處理主背包 (9格)
            if (inventoryItems == null || inventoryItems.Length == 0)
                inventoryItems = new Item[inventorySpace];
            else
                Array.Resize(ref inventoryItems, inventorySpace); // 保留原本塞的物品

            // 處理工具列 (3格)
            if (toolbarItems == null || toolbarItems.Length == 0)
                toolbarItems = new Item[toolbarSpace];
            else
                Array.Resize(ref toolbarItems, toolbarSpace); // 保留原本塞的物品
        }
    }
    #endregion
    public bool isUnlocked = false; // 按E能否打開背包
    public Item[] inventoryItems; // 主背包 (9格)
    public Item[] toolbarItems;   // 工具列 (3格)

    public int inventorySpace = 9;
    public int toolbarSpace = 3;

    // 定義一個事件，當物品欄變更時觸發
    public event Action OnInventoryChanged;

    public bool Add(Item item)
    {
        for (int i = 0; i < inventoryItems.Length; i++) 
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = item;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        Debug.Log("背包已滿。");
        return false;
    }

    public void Remove(Item item)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == item)
            {
                inventoryItems[i] = null; // 清空格子
                OnInventoryChanged?.Invoke(); // 通知 UI
                return; // 只移除第一個找到的
            }
        }
    }
    private Item[] GetArrayFromType(SlotType type)
    {
        if (type == SlotType.Inventory)
        {
            return inventoryItems;
        }
        else
        {
            // MenuToolbar 和 HudToolbar 都使用同一個 toolbarItems 數據
            return toolbarItems;
        }
    }

    public void MoveItem(SlotType fromType, int fromIndex, SlotType toType, int toIndex)
    {
        // 決定要操作哪個陣列
        Item[] fromArray = GetArrayFromType(fromType);
        Item[] toArray = GetArrayFromType(toType);

        // 取得索引邊界
        if (fromIndex < 0 || fromIndex >= fromArray.Length || toIndex < 0 || toIndex >= toArray.Length)
        {
            Debug.LogError("無效的索引");
            return;
        }

        // 取得兩個位置上的物品
        Item fromItem = fromArray[fromIndex];
        Item toItem = toArray[toIndex];

        // 交換它們
        fromArray[fromIndex] = toItem;
        toArray[toIndex] = fromItem;

        // 通知所有 UI 更新！
        OnInventoryChanged?.Invoke();
    }
}