using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public int maxSlots = 20;

    public event Action OnInventoryUpdated;
    public event Action<InventorySlot> OnItemUsed;
    public event Action<InventorySlot> OnItemRemoved;
    public float poids; // Poids de l'objet
    public bool isEquippable; // Indique si l'objet peut être équipé

    private void Awake()
    {
        // Implémentation du Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Plusieurs instances de InventoryManager détectées ! Destruction de la copie.");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Facultatif : garder l'instance entre les scènes

        // Initialisation des slots
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(new InventorySlot(null, 0)); // Slots vides
        }
    }

    public int GetItemCount(InventoryItem item)
{
    int count = 0;
    foreach (var slot in inventorySlots)
    {
        if (slot.item == item)
        {
            count += slot.amount;
        }
    }
    return count;
}

public InventoryItem GetItemByName(string itemName)
{
    foreach (var slot in inventorySlots)
    {
        if (slot.item != null && slot.item.itemName == itemName)
        {
            return slot.item;
        }
    }
    return null; // Retourne null si l'objet n'est pas trouvé
}

public void SwapItems(int slotIndex1, int slotIndex2)
{
    if (slotIndex1 < 0 || slotIndex1 >= inventorySlots.Count || slotIndex2 < 0 || slotIndex2 >= inventorySlots.Count)
    {
        Debug.LogError("Index de slot invalide pour l'échange !");
        return;
    }

    // Échange les slots
    InventorySlot temp = inventorySlots[slotIndex1];
    inventorySlots[slotIndex1] = inventorySlots[slotIndex2];
    inventorySlots[slotIndex2] = temp;

    NotifyInventoryUpdated();
    Debug.Log($"Objets échangés entre les slots {slotIndex1} et {slotIndex2}.");
}

public void SortInventory()
{
    inventorySlots.Sort((slot1, slot2) =>
    {
        if (slot1.item == null && slot2.item == null) return 0;
        if (slot1.item == null) return 1;
        if (slot2.item == null) return -1;
        return slot1.item.itemName.CompareTo(slot2.item.itemName);
    });

    NotifyInventoryUpdated();
    Debug.Log("Inventaire trié !");
}

public float GetTotalWeight()
{
    float totalWeight = 0f;
    foreach (var slot in inventorySlots)
    {
        if (slot.item != null)
        {
            totalWeight += slot.item.poids * slot.amount;
        }
    }
    return totalWeight;
}

public System.Collections.IEnumerator UseItemWithProgress(InventoryItem item, float useTime)
{
    if (HasItem(item))
    {
        Debug.Log($"Utilisation de {item.itemName} en cours...");
        float elapsedTime = 0f;

        while (elapsedTime < useTime)
        {
            elapsedTime += Time.deltaTime;
            // Ajoutez ici une mise à jour de la barre de progression dans l'UI
            yield return null;
        }

        UseItem(item);
        Debug.Log($"{item.itemName} utilisé avec succès !");
    }
    else
    {
        Debug.LogWarning("Objet non disponible pour l'utilisation.");
    }
}


public bool AddItem(InventoryItem item)
{
    if (item == null)
    {
        Debug.LogError("Tentative d'ajouter un item null dans l'inventaire !");
        return false;
    }

    // Vérifie si le poids total dépasse la limite
    float totalWeight = GetTotalWeight() + item.poids;
    if (totalWeight > maxSlots * 10) // Exemple : limite de poids par slot
    {
        Debug.LogWarning("Poids maximum atteint. Impossible d'ajouter cet objet !");
        return false;
    }

    // Recherche un slot existant avec cet item
    foreach (var slot in inventorySlots)
    {
        if (slot.item == item && slot.amount < item.maxStack)
        {
            slot.amount++;
            NotifyInventoryUpdated();
            Debug.Log($"{item.itemName} ajouté dans un slot existant.");
            return true;
        }
    }

    // Recherche un slot vide
    foreach (var slot in inventorySlots)
    {
        if (slot.item == null)
        {
            slot.item = item;
            slot.amount = 1;
            NotifyInventoryUpdated();
            Debug.Log($"{item.itemName} ajouté dans un nouveau slot.");
            return true;
        }
    }

    // Si aucun slot n'est disponible
    Debug.LogWarning("L'inventaire est plein !");
    NotifyInventoryFull();
    return false;
}
public bool SplitStack(int slotIndex, int amountToSplit)
{
    if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
    {
        Debug.LogError("Index de slot invalide !");
        return false;
    }

    InventorySlot slot = inventorySlots[slotIndex];
    if (slot.item == null || slot.amount <= amountToSplit)
    {
        Debug.LogWarning("Impossible de diviser cette pile.");
        return false;
    }

    // Trouve un slot vide pour placer la nouvelle pile
    foreach (var emptySlot in inventorySlots)
    {
        if (emptySlot.item == null)
        {
            emptySlot.item = slot.item;
            emptySlot.amount = amountToSplit;

            slot.amount -= amountToSplit;
            NotifyInventoryUpdated();
            return true;
        }
    }

    Debug.LogWarning("Pas de slot vide disponible pour diviser la pile.");
    return false;
}

public bool AutoStackItem(InventoryItem item, int amountToAdd)
{
    if (item == null)
    {
        Debug.LogError("Tentative d'ajouter un item null dans l'inventaire !");
        return false;
    }

    // Recherche de slots existants pour empiler
    foreach (var slot in inventorySlots)
    {
        if (slot.item == item && slot.amount < item.maxStack)
        {
            int availableSpace = item.maxStack - slot.amount;
            int toAdd = Mathf.Min(availableSpace, amountToAdd);

            slot.amount += toAdd;
            amountToAdd -= toAdd;

            if (amountToAdd <= 0)
            {
                NotifyInventoryUpdated();
                return true;
            }
        }
    }

    // S'il reste des quantités à ajouter, recherche un slot vide
    foreach (var slot in inventorySlots)
    {
        if (slot.item == null)
        {
            slot.item = item;
            slot.amount = Mathf.Min(amountToAdd, item.maxStack);
            amountToAdd -= slot.amount;

            if (amountToAdd <= 0)
            {
                NotifyInventoryUpdated();
                return true;
            }
        }
    }

    // Si l'inventaire est plein et qu'il reste des quantités
    Debug.LogWarning("L'inventaire est plein ou ne peut pas contenir tous les objets !");
    NotifyInventoryFull();
    return false;
}

public List<InventorySlot> GetSlotsByCategory(string category)
{
    List<InventorySlot> filteredSlots = new List<InventorySlot>();
    foreach (var slot in inventorySlots)
    {
        if (slot.item != null && slot.item.category == category)
        {
            filteredSlots.Add(slot);
        }
    }
    return filteredSlots;
}
    public bool RemoveItem(InventoryItem item, int amount)
    {
    if (item == null)
    {
        Debug.LogError("Tentative de retirer un item null !");
        return false;
    }

    foreach (var slot in inventorySlots)
    {
        if (slot.item == item)
        {
            if (slot.amount >= amount)
            {
                slot.amount -= amount;

                if (slot.amount <= 0)
                {
                    Debug.Log($"{item.itemName} retiré complètement de l'inventaire.");
                    slot.item = null;
                    slot.amount = 0;
                    OnItemRemoved?.Invoke(slot);
                }

                NotifyInventoryUpdated();
                return true;
            }
            else
            {
                Debug.LogWarning($"Pas assez d'exemplaires de {item.itemName} pour retirer.");
                return false;
            }
        }
    }

    Debug.LogWarning($"L'objet {item.itemName} n'est pas dans l'inventaire.");
    return false;
    }

    public void UseItem(InventoryItem item)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item && slot.amount > 0)
            {
                slot.amount--;
                OnItemUsed?.Invoke(slot);

                if (slot.amount <= 0)
                {
                    slot.item = null;
                    slot.amount = 0;
                    OnItemRemoved?.Invoke(slot);
                }

                NotifyInventoryUpdated();
                return;
            }
        }

        Debug.LogWarning("Objet non disponible pour l'utilisation.");
    }

    public bool HasItem(InventoryItem item)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item && slot.amount > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void ClearInventory()
    {
        foreach (var slot in inventorySlots)
        {
            slot.item = null;
            slot.amount = 0;
        }

        NotifyInventoryUpdated();
        Debug.Log("Inventaire vidé !");
    }

    private void NotifyInventoryUpdated()
    {
        Debug.Log("Inventory mis à jour, appel de l'événement.");
        OnInventoryUpdated?.Invoke();
    }

    private void NotifyInventoryFull()
    {
        Debug.Log("Inventaire plein !");
        // Si vous avez une UI, vous pouvez déclencher une notification ici
        // Exemple : UIManager.ShowAlert("Inventory Full!", 2f);
    }
}

[System.Serializable]
public class InventorySlot
{
    public InventoryItem item;
    public int amount;

    public InventorySlot(InventoryItem item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}
