using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject equippedWeapon; // Arme actuellement équipée
    public Transform weaponHolderR; // Position où l'arme est tenue (main droite)
    public Transform weaponHolderL; // Position où l'arme est tenue (main gauche)
    public Transform backHolder; // Position pour stocker une arme dans le dos
    public Vector3 weaponOffset = Vector3.zero; // Décalage de position de l'arme
    public Vector3 weaponRotation = Vector3.zero; // Décalage de rotation de l'arme
    public AudioClip shootSound; // Son de tir ou d'attaque
    public bool isDualWielding; // Si le joueur utilise deux armes

    private AudioSource audioSource;

    private void Start()
    {
        // Ajoute une AudioSource si elle n'existe pas déjà
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        // Vérifie si une arme est équipée
        if (equippedWeapon != null)
        {
            // Bouton gauche de la souris pour attaquer ou tirer
            if (Input.GetButtonDown("Fire1"))
            {
                UseWeapon();
            }
        }
    }

    /// <summary>
    /// Équipe une arme dans une main ou dans le dos.
    /// </summary>
    /// <param name="newWeapon">La nouvelle arme à équiper</param>
    /// <param name="holderType">Type de support : "right", "left", ou "back"</param>
    public void EquipWeapon(GameObject newWeapon, string holderType = "right")
    {
        Transform selectedWeaponHolder;

        // Détermine le support sélectionné
        switch (holderType.ToLower())
        {
            case "right":
                selectedWeaponHolder = weaponHolderR;
                break;
            case "left":
                selectedWeaponHolder = weaponHolderL;
                break;
            case "back":
                selectedWeaponHolder = backHolder;
                break;
            default:
                Debug.LogError("Type de support d'arme invalide !");
                return;
        }

        // Si une arme est déjà équipée dans le support sélectionné, détruit-la
        if (selectedWeaponHolder.childCount > 0)
        {
            Destroy(selectedWeaponHolder.GetChild(0).gameObject);
        }

        // Instancie la nouvelle arme et configure sa position
        equippedWeapon = Instantiate(newWeapon, selectedWeaponHolder);
        equippedWeapon.transform.localPosition = weaponOffset; // Applique le décalage de position
        equippedWeapon.transform.localEulerAngles = weaponRotation; // Applique le décalage de rotation
        equippedWeapon.transform.localScale = Vector3.one; // Réinitialise la taille de l'arme

        Debug.Log($"Arme équipée dans {holderType} : {newWeapon.name}");
    }

    /// <summary>
    /// Transfère une arme entre le dos et une main.
    /// </summary>
    /// <param name="toHand">Si true, transfère l'arme du dos à une main droite ; sinon, vers le dos</param>
    public void TransferWeapon(bool toHand = true)
    {
        if (toHand)
        {
            if (backHolder.childCount > 0)
            {
                GameObject weaponOnBack = backHolder.GetChild(0).gameObject;
                EquipWeapon(weaponOnBack, "right");
            }
            else
            {
                Debug.LogWarning("Aucune arme à transférer depuis le dos !");
            }
        }
        else
        {
            if (weaponHolderR.childCount > 0)
            {
                GameObject weaponInHand = weaponHolderR.GetChild(0).gameObject;
                EquipWeapon(weaponInHand, "back");
            }
            else
            {
                Debug.LogWarning("Aucune arme à transférer depuis la main !");
            }
        }
    }

    /// <summary>
    /// Utilise l'arme équipée (tirer, attaquer, etc.).
    /// </summary>
    private void UseWeapon()
    {
        // Joue un son de tir ou d'attaque
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Ajoutez ici les fonctionnalités spécifiques de l'arme
        Debug.Log($"Utilisation de l'arme : {equippedWeapon.name}");

        // Exemple : Si l'arme a un script propre (exemple : Gun), on peut appeler une méthode
        var weaponScript = equippedWeapon.GetComponent<IWeapon>();
        if (weaponScript != null)
        {
            weaponScript.Fire();
        }
    }

    /// <summary>
    /// Retire l'arme équipée d'un support (main ou dos).
    /// </summary>
    /// <param name="holderType">Type de support : "right", "left", ou "back"</param>
    public void UnequipWeapon(string holderType = "right")
    {
        Transform selectedWeaponHolder;

        switch (holderType.ToLower())
        {
            case "right":
                selectedWeaponHolder = weaponHolderR;
                break;
            case "left":
                selectedWeaponHolder = weaponHolderL;
                break;
            case "back":
                selectedWeaponHolder = backHolder;
                break;
            default:
                Debug.LogError("Type de support d'arme invalide !");
                return;
        }

        if (selectedWeaponHolder.childCount > 0)
        {
            Destroy(selectedWeaponHolder.GetChild(0).gameObject);
            Debug.Log($"Arme déséquipée de {holderType}");
        }
    }
}

/// <summary>
/// Interface pour les armes afin d'ajouter des fonctionnalités spécifiques.
/// </summary>
public interface IWeapon
{
    void Fire(); // Méthode pour tirer ou attaquer
}
