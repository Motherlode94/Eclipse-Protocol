using UnityEngine;

public class DestructibleCrate : MonoBehaviour
{
    [Header("Crate Settings")]
    public int health = 30; // Points de santé de la caisse
    public GameObject destructionEffect; // Effet visuel de destruction
    public AudioClip destructionSound; // Son joué lors de la destruction

    [Header("Loot Settings")]
    public GameObject[] lootItems; // Objets à lâcher
    public int minLootCount = 1; // Nombre minimum d'objets à lâcher
    public int maxLootCount = 3; // Nombre maximum d'objets à lâcher
    public Vector3 lootSpawnOffset = Vector3.up; // Décalage pour le spawn des loot

    [Header("Physics Settings")]
    public bool enableRigidbodyOnDestruction = false; // Active un Rigidbody pour les morceaux
    public float explosionForce = 5f; // Force appliquée sur les morceaux

    private AudioSource audioSource;

    private void Start()
    {
        // Ajout automatique d'une AudioSource si destructionSound est assigné
        if (destructionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = destructionSound;
        }
    }

    /// <summary>
    /// Inflige des dégâts à la caisse.
    /// </summary>
    /// <param name="damage">Quantité de dégâts reçus.</param>
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DestroyCrate();
        }
    }

    /// <summary>
    /// Détruit la caisse et génère les effets, loot, et débris.
    /// </summary>
    private void DestroyCrate()
    {
        Debug.Log("Caisse détruite !");

        // Joue l'effet de destruction visuelle
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }

        // Joue le son de destruction
        if (audioSource != null && destructionSound != null)
        {
            audioSource.Play();
        }

        // Génère les loot
        SpawnLoot();

        // Active la physique des débris
        if (enableRigidbodyOnDestruction)
        {
            AddRigidbodyToDebris();
        }

        // Détruit l'objet après avoir joué l'effet
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// Génère des objets aléatoires depuis la caisse.
    /// </summary>
    private void SpawnLoot()
    {
        int lootCount = Random.Range(minLootCount, maxLootCount + 1);

        for (int i = 0; i < lootCount; i++)
        {
            if (lootItems.Length > 0)
            {
                int randomIndex = Random.Range(0, lootItems.Length);
                GameObject loot = lootItems[randomIndex];

                Vector3 spawnPosition = transform.position + lootSpawnOffset + Random.insideUnitSphere * 0.5f;
                Instantiate(loot, spawnPosition, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Ajoute des Rigidbody aux morceaux de la caisse pour simuler une explosion.
    /// </summary>
    private void AddRigidbodyToDebris()
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, transform.position, 2f);
        }
    }
}
