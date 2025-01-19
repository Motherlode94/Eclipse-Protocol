using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public InventoryItem item;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager inventory = other.GetComponent<InventoryManager>();
            if (inventory.AddItem(item))
            {
                Destroy(gameObject);
            }
        }
    }
}
