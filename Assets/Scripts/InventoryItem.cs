using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Item Properties")]
    public string itemName; // Nom de l'objet
    public Sprite icon; // Icône pour l'affichage
    public string description; // Description de l'objet
    public bool isStackable; // Si l'objet peut être empilé
    public int maxStack; // Quantité maximale empilable

    [Header("Additional Properties")]
    public float poids; // Poids de l'objet
    public string category; // Catégorie de l'objet, ex. "Weapon", "Potion"
    public GameObject prefab; // Prefab associé à l'objet pour l'apparition dans le monde

    [Header("Equippable Settings")]
    public bool isEquippable; // Si l'objet peut être équipé
}
