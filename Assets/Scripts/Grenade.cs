using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float delay = 3f; // Temps avant l'explosion
    public float explosionRadius = 5f; // Rayon d'explosion
    public int damage = 50; // Dégâts infligés
    public LayerMask targetLayer; // Cibles affectées par l'explosion
    public bool applyForce = true; // Applique une force d'explosion
    public float explosionForce = 700f; // Force de l'explosion

    [Header("Effects")]
    public GameObject explosionEffect; // Effet visuel d'explosion
    public AudioClip explosionSound; // Son d'explosion
    public float soundVolume = 1f; // Volume du son d'explosion

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

        // Détecte les cibles dans le rayon d'explosion
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);
        foreach (Collider target in hitTargets)
        {
            // Appliquer des dégâts si la cible implémente IDamageable
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                int adjustedDamage = CalculateDamage(distance);
                damageable.TakeDamage(adjustedDamage);
                Debug.Log($"Infligé {adjustedDamage} dégâts à {target.name}");
            }

            // Appliquer une force d'explosion
            if (applyForce)
            {
                Rigidbody rb = target.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
        }

        // Détruire la grenade après l'explosion
        Destroy(gameObject);
    }

    private int CalculateDamage(float distance)
    {
        // Réduit les dégâts en fonction de la distance (dégâts maximaux au centre)
        float damageFactor = Mathf.Clamp01(1 - (distance / explosionRadius));
        return Mathf.RoundToInt(damage * damageFactor);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
