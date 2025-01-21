using UnityEngine;

public class Gun : MonoBehaviour, IRangedWeapon
{
    [Header("Gun Settings")]
    public int damage = 20; // Dégâts infligés par le pistolet
    public float fireRate = 0.5f; // Temps entre deux tirs
    public int maxAmmo = 10; // Munitions maximum
    public int currentAmmo; // Munitions actuelles
    public float reloadTime = 2f; // Temps pour recharger
    public Transform firePoint; // Point de sortie des projectiles
    public GameObject bulletPrefab; // Prefab du projectile
    public LayerMask targetLayer; // Couches ciblées

    [Header("Effects")]
    public ParticleSystem muzzleFlash; // Effet visuel du tir
    public AudioClip fireSound; // Son du tir
    public AudioClip reloadSound; // Son de rechargement

    private AudioSource audioSource;
    private Animator animator;
    private float nextFireTime; // Temps du prochain tir autorisé
    private bool isReloading;

    private void Start()
    {
        // Initialisation
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        // Gestion du rechargement
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }
    }

    public void Fire()
    {
        // Vérifie si le pistolet peut tirer
        if (isReloading || Time.time < nextFireTime)
        {
            Debug.Log("Impossible de tirer pour l'instant !");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Plus de munitions !");
            return;
        }

        // Tirs et gestion des munitions
        currentAmmo--;
        nextFireTime = Time.time + fireRate;

        Debug.Log($"Tir avec le pistolet. Munitions restantes : {currentAmmo}");

        // Jouer les effets visuels
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Jouer le son
        if (fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Créer le projectile
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * 1000f); // Ajuster la force selon vos besoins
            }
        }

        // Animation de tir
        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }
    }

    private void Reload()
    {
        if (isReloading) return;

        Debug.Log("Rechargement...");
        isReloading = true;

        // Jouer le son de rechargement
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        // Animation de rechargement
        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }

        // Attendre le rechargement
        Invoke(nameof(FinishReload), reloadTime);
    }

    private void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Rechargement terminé !");
    }
}
