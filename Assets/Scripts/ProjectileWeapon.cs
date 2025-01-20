using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileWeapon : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Préfabriqué du projectile à tirer.")]
    public GameObject projectilePrefab;
    [Tooltip("Point d'origine du tir (par exemple : canon de l'arme).")]
    public Transform firePoint;
    [Tooltip("Vitesse initiale du projectile.")]
    public float projectileSpeed = 20f;

    [Header("Cooldown")]
    [Tooltip("Temps minimum entre deux tirs.")]
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    [Header("Effects")]
    [Tooltip("Effet visuel au moment du tir.")]
    public ParticleSystem muzzleFlash;
    [Tooltip("Son joué lors du tir.")]
    public AudioClip fireSound;

    private AudioSource audioSource;

    private void Start()
    {
        // Ajoute un AudioSource si nécessaire
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        // Vérifie que l'action est déclenchée
        if (context.performed)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        // Vérifie le cooldown
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;

        // Vérifie si le projectile et le point de tir sont assignés
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("ProjectilePrefab ou FirePoint n'est pas assigné !");
            return;
        }

        // Instancie le projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Applique une vitesse au projectile
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * projectileSpeed;
        }
        else
        {
            Debug.LogWarning("Le projectile n'a pas de Rigidbody !");
        }

        // Détruit le projectile après 5 secondes pour éviter les fuites de mémoire
        Destroy(projectile, 5f);

        // Joue l'effet visuel
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Joue le son de tir
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Debug.Log("Projectile tiré !");
    }
}
