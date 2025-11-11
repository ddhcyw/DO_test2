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
            Debug.LogWarning("�a�W�����~ " + gameObject.name + " �S���]�w Item ��ơI");
            return;
        }

        
        bool wasAdded = InventoryManager.Instance.Add(item);

        if (wasAdded)
        {
            Destroy(gameObject);
        }
        
    }
}