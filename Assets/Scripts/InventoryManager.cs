using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public int maxSlots = 20;

    public event Action OnInventoryUpdated;
    public event Action<InventorySlot> OnItemUsed;
    public event Action<InventorySlot> OnItemRemoved;

    private void Awake()
    {
        if (inventorySlots == null)
        {
            inventorySlots = new List<InventorySlot>();
        }

        // Remplir la liste avec des slots vides
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(new InventorySlot(null, 0)); // Slot vide
        }
    }

    public bool AddItem(InventoryItem item)
    {
        if (item == null)
        {
            Debug.LogError("Tentative d'ajouter un item null dans l'inventaire !");
            return false;
        }

        // Recherche d'un slot existant avec l'item
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item && slot.amount < item.maxStack)
            {
                slot.amount++;
                NotifyInventoryUpdated();
                return true;
            }
        }

        // Recherche d'un slot vide
        foreach (var slot in inventorySlots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.amount = 1;
                NotifyInventoryUpdated();
                return true;
            }
        }

        // Si aucun slot disponible
        Debug.LogWarning("L'inventaire est plein !");
        NotifyInventoryFull();
        return false;
    }

    public bool RemoveItem(InventoryItem item, int amount)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item)
            {
                if (slot.amount >= amount)
                {
                    slot.amount -= amount;

                    if (slot.amount <= 0)
                    {
                        slot.item = null;
                        slot.amount = 0;
                        OnItemRemoved?.Invoke(slot);
                    }

                    NotifyInventoryUpdated();
                    return true;
                }
                else
                {
                    Debug.LogWarning("Pas assez d'objets dans le slot pour retirer.");
                    return false;
                }
            }
        }

        Debug.LogWarning("L'objet n'est pas dans l'inventaire.");
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
