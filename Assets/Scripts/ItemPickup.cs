using UnityEngine;


public class ItemPickup : MonoBehaviour
{
    public Item item;
   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }
   

    private void Pickup()
    {
        if (item == null)
        {
            Debug.LogWarning("地上的物品 " + gameObject.name + " 沒有設定 Item 資料！");
            return;
        }

        //呼叫 InventoryManager 的 Add 方法
        bool wasAdded = InventoryManager.Instance.Add(item);

        if (wasAdded)
        {
            Destroy(gameObject);
        }
        
    }
}