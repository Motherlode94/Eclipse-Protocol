using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float delay = 3f; // Temps avant l'explosion
    public float explosionRadius = 5f; // Rayon d'explosion
    public int damage = 50; // Dégâts infligés
    public LayerMask targetLayer; // Cibles affectées par l'explosion

    [Header("Effects")]
    public GameObject explosionEffect; // Effet visuel d'explosion
    public AudioClip explosionSound; // Son d'explosion

    private float countdown;
    private bool hasExploded = false;

    private void Start()
    {
        countdown = delay;
    }

    private void Update()
    {
        countdown -= Time.deltaTime;

        if (countdown <= 0f && !hasExploded)
        {
            Explode();
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

        // Détecte les cibles dans le rayon d'explosion
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);
        foreach (Collider target in hitTargets)
        {
            var targetStats = target.GetComponent<PlayerStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {target.name}");
            }
        }

        // Détruit la grenade après l'explosion
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
