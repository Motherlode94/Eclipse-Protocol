using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject equippedWeapon; // Arme actuellement équipée
    public Transform weaponHolder; // Position où l'arme est tenue
    public Vector3 weaponOffset = Vector3.zero; // Décalage de position de l'arme
    public Vector3 weaponRotation = Vector3.zero; // Décalage de rotation de l'arme
    public AudioClip shootSound; // Son de tir ou d'attaque

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (equippedWeapon != null)
        {
            if (Input.GetButtonDown("Fire1")) // Bouton gauche de la souris
            {
                UseWeapon();
            }
        }
    }

    public void EquipWeapon(GameObject newWeapon)
    {
        // Si une arme est déjà équipée, détruisez-la
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
        }

        // Instancie la nouvelle arme et configure sa position
        equippedWeapon = Instantiate(newWeapon, weaponHolder);
        equippedWeapon.transform.localPosition = weaponOffset; // Applique le décalage de position
        equippedWeapon.transform.localEulerAngles = weaponRotation; // Applique le décalage de rotation
        equippedWeapon.transform.localScale = Vector3.one; // Réinitialise la taille de l'arme si nécessaire

        Debug.Log("Arme équipée : " + newWeapon.name);
    }

    private void UseWeapon()
    {
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound); // Joue le son de tir
        }

        // Ajoutez ici les fonctionnalités spécifiques de l'arme (exemple : tirer, frapper, etc.)
        Debug.Log("Utilisation de l'arme : " + equippedWeapon.name);
    }
}
