using UnityEngine;

public class Landmine : MonoBehaviour
{
    [Header("Mine Settings")]
    public float detectionRadius = 3f; // Rayon de détection
    public int damage = 100; // Dégâts infligés
    public LayerMask targetLayer; // Couches des cibles
    public GameObject explosionEffect; // Effet visuel d'explosion
    public AudioClip explosionSound; // Son d'explosion

    private bool hasExploded = false;

    private void Update()
    {
        if (!hasExploded)
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
            if (targets.Length > 0)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // Affiche l'effet d'explosion
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Joue le son d'explosion
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Applique des dégâts aux cibles
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        foreach (Collider target in hitTargets)
        {
            var targetStats = target.GetComponent<PlayerStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {target.name}");
            }
        }

        // Détruit la mine après l'explosion
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
