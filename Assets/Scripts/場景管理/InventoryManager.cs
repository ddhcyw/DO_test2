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
        // �o�O Singleton ��ҼҦ����ˬd
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // �p�G Instance �O�Ū��A�N��ۤv�����W�h
            Instance = this;
            DontDestroyOnLoad(gameObject); // ������O�d

            // *** �b�o�̪�l�Ƨڭ̪����~�}�C ***
            // �إߤ@�Ӧ� inventorySpace (9) �ӪŮ� (null) ���}�C
            inventoryItems = new Item[inventorySpace];
            toolbarItems = new Item[toolbarSpace];
        }
    }
    #endregion
    public bool isUnlocked = false; // ��E��_���}�I�]
    public Item[] inventoryItems; // �D�I�] (9��)
    public Item[] toolbarItems;   // �u��C (3��)

    public int inventorySpace = 9;
    public int toolbarSpace = 3;

    // �w�q�@�Өƥ�A�����~���ܧ��Ĳ�o
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
        Debug.Log("�I�]�w���C");
        return false;
    }

    public void Remove(Item item)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == item)
            {
                inventoryItems[i] = null; // �M�Ů�l
                OnInventoryChanged?.Invoke(); // �q�� UI
                return; // �u�����Ĥ@�ӧ�쪺
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
            // MenuToolbar �M HudToolbar ���ϥΦP�@�� toolbarItems �ƾ�
            return toolbarItems;
        }
    }

    public void MoveItem(SlotType fromType, int fromIndex, SlotType toType, int toIndex)
    {
        // �M�w�n�ާ@���Ӱ}�C
        Item[] fromArray = GetArrayFromType(fromType);
        Item[] toArray = GetArrayFromType(toType);

        // ���o�������
        if (fromIndex < 0 || fromIndex >= fromArray.Length || toIndex < 0 || toIndex >= toArray.Length)
        {
            Debug.LogError("�L�Ī�����");
            return;
        }

        // ���o��Ӧ�m�W�����~
        Item fromItem = fromArray[fromIndex];
        Item toItem = toArray[toIndex];

        // �洫����
        fromArray[fromIndex] = toItem;
        toArray[toIndex] = fromItem;

        // �q���Ҧ� UI ��s�I
        OnInventoryChanged?.Invoke();
    }
    public void ClearAll()
    {
        for (int i = 0; i < inventoryItems.Length; i++) inventoryItems[i] = null;
        for (int i = 0; i < toolbarItems.Length; i++) toolbarItems[i] = null;
        isUnlocked = false;
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(Item item)
    {
        if (item == null) return false;

        // �ˬd�D�I�]�G��� itemID �r��
        foreach (Item i in inventoryItems)
        {
            // ��� i.itemID == item.itemID
            if (i != null && i.itemID == item.itemID) return true;
        }

        // �ˬd�u��C
        foreach (Item i in toolbarItems)
        {
            if (i != null && i.itemID == item.itemID) return true;
        }

        return false;
    }
}