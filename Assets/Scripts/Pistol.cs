using System.Collections; // Pour IEnumerator
using UnityEngine;

public class Pistol : MonoBehaviour, IRangedWeapon
{
    [Header("Pistol Settings")]
    public int damage = 15;
    public int maxAmmo = 12;
    public float fireRate = 0.3f; // Temps entre les tirs
    public float reloadTime = 1.5f; // Temps pour recharger
    public float range = 25f; // Portée maximale du pistolet
    public Transform firePoint; // Point de tir
    public GameObject bulletPrefab; // Projectile (balle)
    public ParticleSystem muzzleFlash; // Effet visuel du canon
    public AudioClip fireSound, reloadSound; // Sons de tir et rechargement

    private int currentAmmo;
    private float nextFireTime = 0f;
    private AudioSource audioSource;

    public float Damage => damage; // Implémentation de l'interface
    public float Range => range; // Implémentation de l'interface

    private void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Equip()
    {
        Debug.Log("Pistolet équipé !");
        gameObject.SetActive(true);
    }

    public void Unequip()
    {
        Debug.Log("Pistolet déséquipé !");
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

        Debug.Log("Tir effectué ! Dégâts : " + damage + ", Portée : " + range);
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Pistolet rechargé. Munitions : " + currentAmmo);
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
}
