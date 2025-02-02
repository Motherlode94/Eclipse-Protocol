using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public InventoryItem item;

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Interact(); // Ramasse l'objet automatiquement si le joueur entre dans le trigger
    }
    Debug.Log("Méthode Interact appelée.");

}


    public void Interact()
    {
    InventoryManager inventory = FindObjectOfType<InventoryManager>();
    if (inventory != null && inventory.AddItem(item))
    {
        Debug.Log($"Item ramassé : {item.itemName}");
        MissionManager missionManager = FindObjectOfType<MissionManager>();
        if (missionManager != null)
        {
            missionManager.NotifyObjectiveCompleted(gameObject);
        }
        Destroy(gameObject);
    }
    else
    {
        Debug.Log("Impossible d'ajouter l'objet à l'inventaire.");
    }
    }
}
