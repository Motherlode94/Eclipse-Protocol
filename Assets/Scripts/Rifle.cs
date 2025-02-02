using System.Collections;
using UnityEngine;

public class Rifle : MonoBehaviour, IRangedWeapon
{
    [Header("Rifle Settings")]
    public int damage = 8;
    public int maxAmmo = 30;
    public float fireRate = 0.1f; // Très rapide (10 tirs/sec)
    public float reloadTime = 2f;
    public float range = 50f; // Portée maximale des balles
    public Transform firePoint; // Point de tir
    public GameObject bulletPrefab; // Projectile
    public ParticleSystem muzzleFlash;
    public AudioClip fireSound, reloadSound;

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isFiring = false;
    private AudioSource audioSource;

    // Implémentation de l'interface
    public float Damage => damage; 
    public float Range => range; 

    private void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (isFiring && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Fire();
        }
    }

    public void Equip()
    {
        Debug.Log("Fusil équipé !");
        gameObject.SetActive(true);
    }

    public void Unequip()
    {
        Debug.Log("Fusil déséquipé !");
        gameObject.SetActive(false);
    }

    public void Fire()
    {
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

        currentAmmo--;
        nextFireTime = Time.time + fireRate;

        // Joue le son
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Affiche le flash du canon
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        Debug.Log($"Tir avec fusil ! Dégâts : {damage}, Portée : {range}");
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Fusil rechargé. Munitions : " + currentAmmo);
    }

    private IEnumerator ReloadRoutine()
    {
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;

        Debug.Log("Rechargement terminé !");
    }

    public void StartFiring()
    {
        isFiring = true;
    }

    public void StopFiring()
    {
        isFiring = false;
    }
}
