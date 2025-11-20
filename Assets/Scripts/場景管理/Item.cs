using UnityEngine;

// [CreateAssetMenu] 可以直接在 Project 視窗中右鍵 -> Create -> Inventory -> Item 來建立新的物品資料
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public Sprite icon = null;
    public string description = "Item Description";
    public virtual void UseItem()
    {
        // 預設行為 (例如一個石頭、一個鑰匙)
        Debug.Log("使用了 " + name);
    }
    //public int maxStack = 64; // 最大堆疊數量

    // 添加更多屬性
    // public bool isConsumable = false;
    // public int damage = 0;
    // public float durability = 100f;
}