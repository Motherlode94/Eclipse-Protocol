using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName; // Nom de l'objet
    public Sprite icon; // Icône pour l'affichage
    public string description; // Description de l'objet
    public bool isStackable; // Si l'objet peut être empilé
    public int maxStack;
}
