using UnityEngine;

public class DestructibleCrate : MonoBehaviour
{
    [Header("Crate Settings")]
    public int health = 30; // Points de santé de la caisse
    public GameObject[] lootItems; // Objets à lâcher
    public int lootCount = 2; // Nombre d'objets à lâcher
    public GameObject destructionEffect; // Effet visuel de destruction

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DestroyCrate();
        }
    }

    private void DestroyCrate()
    {
        Debug.Log("Caisse détruite !");

        // Affiche l'effet de destruction
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }

        // Lâche des objets aléatoires
        for (int i = 0; i < lootCount; i++)
        {
            if (lootItems.Length > 0)
            {
                int randomIndex = Random.Range(0, lootItems.Length);
                Instantiate(lootItems[randomIndex], transform.position, Quaternion.identity);
            }
        }

        // Détruit la caisse
        Destroy(gameObject);
    }
}
