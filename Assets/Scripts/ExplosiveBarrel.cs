using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Barrel Settings")]
    public int health = 50; // Points de vie du baril
    public float explosionRadius = 5f; // Rayon d'explosion
    public int damage = 40; // Dégâts infligés par l'explosion
    public bool applyForce = true; // Applique une force d'explosion
    public float explosionForce = 500f; // Force de l'explosion
    public LayerMask affectedLayers; // Couches affectées par l'explosion

    [Header("Visual & Audio Effects")]
    public GameObject explosionEffect; // Effet visuel de l'explosion
    public AudioClip explosionSound; // Son d'explosion
    public float soundVolume = 1f; // Volume du son

    private bool hasExploded = false;

    public void TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        Debug.Log($"Baril touché. Santé restante : {health}");

        if (health <= 0 && !hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // Effet visuel
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Son d'explosion
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, soundVolume);
        }

        // Détecter les objets dans le rayon d'explosion
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayers);
        foreach (Collider hit in hitColliders)
        {
            // Appliquer des dégâts si la cible implémente IDamageable
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {hit.name}");
            }

            // Appliquer une force physique si applicable
            if (applyForce)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
        }

        // Détruire le baril
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualisation du rayon d'explosion
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
