using UnityEngine;

public class Projectile : MonoBehaviour
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
        // Déplacement du projectile
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet touché est un joueur ou un ennemi
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Gère les dégâts
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Joue l'effet visuel et sonore
            HandleImpactEffects();

            // Détruit le projectile
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            // Si c'est un mur, on détruit simplement le projectile
            HandleImpactEffects();
            Destroy(gameObject);
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
