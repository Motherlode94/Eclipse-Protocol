using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Explosive Settings")]
    public float explosionRadius = 5f; // Rayon d'explosion
    public int damage = 50; // Dégâts infligés
    public float delay = 2f; // Délai avant l'explosion
    public GameObject explosionEffect; // Effet visuel d'explosion
    public AudioClip explosionSound; // Son d'explosion

    private bool hasExploded = false;

    private void Start()
    {
        Invoke(nameof(Explode), delay); // Déclenche l'explosion après un délai
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // Effet visuel
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Son d'explosion
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Détecter les cibles dans le rayon
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            var targetStats = hit.GetComponent<PlayerStats>(); // Exemple avec PlayerStats
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {hit.name}");
            }
        }

        // Détruire l'explosif
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Affiche le rayon d'explosion dans l'éditeur
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
