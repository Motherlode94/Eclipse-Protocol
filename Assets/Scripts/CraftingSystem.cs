using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
    public string itemName; // Nom de l'objet à fabriquer
    public List<MaterialRequirement> requiredMaterials; // Matériaux nécessaires
    public GameObject craftedItem; // Objet résultant de la fabrication
}

[System.Serializable]
public class MaterialRequirement
{
    public InventoryItem material; // Matériel requis (objet d'inventaire)
    public int amount; // Quantité nécessaire
}

public class CraftingSystem : MonoBehaviour
{
    public List<Recipe> recipes; // Liste des recettes disponibles

    public bool CraftItem(string itemName, InventoryManager inventory)
    {
        // Trouve la recette correspondant à l'itemName
        Recipe recipe = recipes.Find(r => r.itemName == itemName);
        if (recipe == null)
        {
            Debug.LogWarning($"Recette non trouvée pour l'objet : {itemName}");
            return false;
        }

        // Vérifie si les matériaux nécessaires sont disponibles
        foreach (var material in recipe.requiredMaterials)
        {
            int availableAmount = inventory.GetItemCount(material.material);
            if (availableAmount < material.amount)
            {
                Debug.LogWarning($"Matériaux insuffisants pour {itemName} (Manque de {material.material.itemName})");
                return false;
            }
        }

        // Retire les matériaux et ajoute l'objet créé
        foreach (var material in recipe.requiredMaterials)
        {
            inventory.RemoveItem(material.material, material.amount);
        }

        // Ajoute l'objet fabriqué à l'inventaire
        InventoryItem craftedItem = recipe.craftedItem.GetComponent<InventoryItem>();
        if (craftedItem != null)
        {
            inventory.AddItem(craftedItem);
            Debug.Log($"Objet fabriqué : {itemName}");
            return true;
        }
        else
        {
            Debug.LogError($"L'objet fabriqué {itemName} ne contient pas de composant InventoryItem !");
            return false;
        }
    }
}
