using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Barrel Settings")]
    public int health = 50; // Points de vie du baril
    public float explosionRadius = 5f; // Rayon d'explosion
    public int damage = 40; // Dégâts infligés par l'explosion
    public GameObject explosionEffect; // Effet visuel de l'explosion

    public void TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        Debug.Log($"Baril touché. Santé restante : {health}");

        if (health <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Effet visuel de l'explosion
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Dégâts aux cibles proches
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            var targetStats = hit.GetComponent<PlayerStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {hit.name}");
            }
        }

        Destroy(gameObject); // Détruit le baril
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
