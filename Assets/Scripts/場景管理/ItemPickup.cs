using UnityEngine;
using UnityEngine.Events;


public class ItemPickup : MonoBehaviour
{
    public Item item;
    public UnityEvent onPickup;

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
            Debug.LogWarning("場景中的道具 " + gameObject.name + " 沒有指派 Item 數據！");
            return;
        }

        bool wasAdded = InventoryManager.Instance.Add(item);

        if (wasAdded)
        {
            if (GameFlow.Instance != null)
            {
                GameFlow.Instance.ShowSpecialItemGotUI(item);
            }

            onPickup?.Invoke();

            // 讓地上的道具消失
            Destroy(gameObject);
        }
    }
}