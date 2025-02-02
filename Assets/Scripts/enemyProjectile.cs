using UnityEngine;

public class enemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f; // Vitesse du projectile
    public float damage = 10f; // Dégâts infligés
    public float lifetime = 5f; // Durée de vie du projectile

    [Header("Effects")]
    public GameObject impactEffect; // Effet visuel lors de l'impact
    public AudioClip impactSound; // Son joué lors de l'impact

    private AudioSource audioSource;
    private void Start()
    {
        // Détruit le projectile après un certain temps
        Destroy(gameObject, lifetime);

        // Ajoute un AudioSource si nécessaire
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

private void OnTriggerEnter(Collider other)
{
    EnemyHealth enemy = other.GetComponent<EnemyHealth>();
    if (enemy != null)
    {
        enemy.TakeDamage((int)damage); // Conversion explicite de damage en int
        HandleImpactEffects(); // Gestion des effets d'impact
        Destroy(gameObject); // Détruit le projectile
    }
}


    private void HandleImpactEffects()
    {
        // Joue l'effet visuel
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Joue le son d'impact
        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }
    }
}
