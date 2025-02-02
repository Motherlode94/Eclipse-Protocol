using System.Collections;
using UnityEngine;

public class Shotgun : MonoBehaviour, IRangedWeapon
{
    [Header("Shotgun Settings")]
    public int damage = 25; // Dégâts par projectile
    public int pellets = 8; // Nombre de projectiles par tir
    public int maxAmmo = 8; // Munitions maximales
    public float reloadTime = 2.5f; // Temps de rechargement
    public float spreadAngle = 15f; // Angle de dispersion des projectiles
    public float range = 10f; // Portée des projectiles
    public float fireRate = 1f; // Temps minimum entre deux tirs
    public Transform firePoint; // Point d'origine des projectiles
    public GameObject pelletPrefab; // Projectile (fragment)
    public ParticleSystem muzzleFlash; // Effet visuel de tir
    public AudioClip fireSound; // Son du tir
    public AudioClip reloadSound; // Son du rechargement

    private int currentAmmo; // Munitions actuelles
    private AudioSource audioSource; // Source audio
    private float nextFireTime = 0f; // Temps du prochain tir possible

    // Propriétés implémentées de l'interface IRangedWeapon
    public float Damage => damage; 
    public float Range => range;

    private void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Equip()
    {
        Debug.Log("Fusil à pompe équipé !");
        gameObject.SetActive(true);
    }

    public void Unequip()
    {
        Debug.Log("Fusil à pompe déséquipé !");
        gameObject.SetActive(false);
    }

    public void Fire()
    {
        // Vérifie si le tir est possible
        if (Time.time < nextFireTime)
        {
            Debug.Log("Cooldown en cours, tir impossible.");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Pas de munitions !");
            return;
        }

        currentAmmo--; // Réduit les munitions
        nextFireTime = Time.time + fireRate; // Définit le cooldown

        // Effet visuel
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Effet sonore
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Debug.Log("Tir avec fusil à pompe !");

        // Lancer les projectiles
        for (int i = 0; i < pellets; i++)
        {
            FirePellet();
        }
    }

    private void FirePellet()
    {
        // Calcul de la direction avec dispersion
        Vector3 direction = firePoint.forward;
        direction.x += Random.Range(-spreadAngle, spreadAngle) * 0.01f;
        direction.y += Random.Range(-spreadAngle, spreadAngle) * 0.01f;

        if (pelletPrefab != null)
        {
            GameObject pellet = Instantiate(pelletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            Destroy(pellet, range / 10f); // Détruit le projectile après un temps proportionnel à sa portée
        }
    }

    public void Reload()
    {
        if (currentAmmo == maxAmmo)
        {
            Debug.Log("Le fusil à pompe est déjà chargé !");
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        Debug.Log("Rechargement en cours...");

        // Joue le son de rechargement
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo; // Recharge complètement
        Debug.Log("Rechargement terminé !");
    }
}
