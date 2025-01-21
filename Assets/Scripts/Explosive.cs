using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Explosive Settings")]
    public float explosionRadius = 5f; // Rayon de l'explosion
    public int damage = 50; // Dégâts infligés
    public float delay = 2f; // Délai avant l'explosion
    public bool applyForce = true; // Applique une force d'explosion
    public float explosionForce = 500f; // Force de l'explosion
    public LayerMask affectedLayers; // Couches affectées par l'explosion

    [Header("Visual & Audio Effects")]
    public GameObject explosionEffect; // Effet visuel d'explosion
    public AudioClip explosionSound; // Son d'explosion
    public float soundVolume = 1f; // Volume du son

    private bool hasExploded = false;

    private void Start()
    {
        // Déclenche l'explosion après un délai
        Invoke(nameof(Explode), delay);
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // Affiche l'effet visuel
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Joue le son d'explosion
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, soundVolume);
        }

        // Détecte les objets affectés dans le rayon d'explosion
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayers);
        foreach (Collider hit in hitColliders)
        {
            // Applique des dégâts si la cible a un script de type IDamageable
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {hit.name}");
            }

            // Applique une force d'explosion si applicable
            if (applyForce)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
        }

        // Détruit l'objet explosif
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Affiche le rayon d'explosion dans l'éditeur
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
