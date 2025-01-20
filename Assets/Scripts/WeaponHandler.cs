using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject rifle; // L'arme actuelle (par exemple, un fusil)
    public Transform firePoint; // Point d'origine du tir
    public GameObject muzzleFlash; // Effet visuel pour le tir
    public AudioClip fireSound; // Son joué lors du tir
    public float fireRate = 0.5f; // Temps minimum entre deux tirs
    public int maxAmmo = 30; // Nombre maximum de munitions
    public int currentAmmo; // Munitions actuelles

    private float nextFireTime = 0f; // Temps du prochain tir possible
    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialisation des munitions
        currentAmmo = maxAmmo;
    }

    public void Fire()
    {
        // Vérifie si le tir est possible
        if (Time.time < nextFireTime)
        {
            Debug.Log("Tir impossible : cooldown en cours.");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Pas de munitions !");
            return;
        }

        nextFireTime = Time.time + fireRate;

        // Décrémente les munitions
        currentAmmo--;

        // Déclenche l'animation
        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }

        // Joue le son de tir
        if (fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Affiche l'effet visuel (par exemple, un flash de canon)
        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.5f); // Détruit le flash après un court instant
        }

        Debug.Log("Tir effectué. Munitions restantes : " + currentAmmo);
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Arme rechargée. Munitions : " + currentAmmo);

        // Ajout d'une animation de rechargement (facultatif)
        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }
    }

    public void ChangeWeapon(GameObject newWeapon)
    {
        if (rifle != null)
        {
            rifle.SetActive(false);
        }

        rifle = newWeapon;

        if (rifle != null)
        {
            rifle.SetActive(true);
        }

        Debug.Log("Arme changée : " + rifle.name);
    }
}
