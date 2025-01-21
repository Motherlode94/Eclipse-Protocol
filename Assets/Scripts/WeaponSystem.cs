using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject equippedWeapon; // Arme actuellement équipée
    public Transform weaponHolderR; // Position où l'arme est tenue (main droite)
    public Transform weaponHolderL; // Position où l'arme est tenue (main gauche)
    public Transform backHolder; // Position pour stocker une arme dans le dos

    public Vector3 weaponOffsetR = Vector3.zero; // Décalage pour la main droite
    public Vector3 weaponRotationR = Vector3.zero; // Rotation pour la main droite
    public Vector3 weaponOffsetL = Vector3.zero; // Décalage pour la main gauche
    public Vector3 weaponRotationL = Vector3.zero; // Rotation pour la main gauche
    public Vector3 weaponOffsetBack = Vector3.zero; // Décalage pour le dos
    public Vector3 weaponRotationBack = Vector3.zero; // Rotation pour le dos

    private AudioSource audioSource;

    private void Start()
    {
        // Ajoute une AudioSource si elle n'existe pas
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Enregistre les actions d'entrée via PlayerInput
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["AttackPrimary"].performed += OnPrimaryAttack;
        }
    }

    private void OnDisable()
    {
        // Déconnecte les actions d'entrée
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["AttackPrimary"].performed -= OnPrimaryAttack;
        }
    }

    /// <summary>
    /// Gestion de l'attaque primaire
    /// </summary>
    private void OnPrimaryAttack(InputAction.CallbackContext context)
    {
        UseWeapon();
    }

    /// <summary>
    /// Équipe une arme dans une main ou sur le dos.
    /// </summary>
    /// <param name="newWeapon">L'arme à équiper</param>
    /// <param name="holderType">Support d'équipement : "right", "left", ou "back"</param>
    public void EquipWeapon(GameObject newWeapon, string holderType = "right")
    {
        Transform selectedHolder = GetWeaponHolder(holderType);

        if (selectedHolder == null)
        {
            Debug.LogError($"Type de support invalide : {holderType}");
            return;
        }

        // Supprime l'arme déjà équipée dans ce support
        if (selectedHolder.childCount > 0)
        {
            Destroy(selectedHolder.GetChild(0).gameObject);
        }

        // Instancie et configure la nouvelle arme
        equippedWeapon = Instantiate(newWeapon, selectedHolder);
        ConfigureWeaponPosition(selectedHolder, holderType);
        Debug.Log($"Arme équipée dans {holderType} : {newWeapon.name}");
    }

    /// <summary>
    /// Utilise l'arme équipée (tir ou attaque).
    /// </summary>
    private void UseWeapon()
    {
        if (equippedWeapon == null)
        {
            Debug.LogWarning("Aucune arme équipée !");
            return;
        }

        // Vérifie si l'arme est de type mêlée ou distance
        var meleeWeapon = equippedWeapon.GetComponent<IMeleeWeapon>();
        var rangedWeapon = equippedWeapon.GetComponent<IRangedWeapon>();

        if (meleeWeapon != null)
        {
            meleeWeapon.Attack();
        }
        else if (rangedWeapon != null)
        {
            rangedWeapon.Fire();
        }
        else
        {
            Debug.LogWarning("L'arme équipée n'a pas de script implémentant IMeleeWeapon ou IRangedWeapon.");
        }
    }

    /// <summary>
    /// Retourne le Transform correspondant au support sélectionné.
    /// </summary>
    private Transform GetWeaponHolder(string holderType)
    {
        return holderType.ToLower() switch
        {
            "right" => weaponHolderR,
            "left" => weaponHolderL,
            "back" => backHolder,
            _ => null,
        };
    }

    /// <summary>
    /// Configure la position et la rotation de l'arme équipée.
    /// </summary>
    private void ConfigureWeaponPosition(Transform holder, string holderType)
    {
        Vector3 offset, rotation;

        switch (holderType.ToLower())
        {
            case "right":
                offset = weaponOffsetR;
                rotation = weaponRotationR;
                break;
            case "left":
                offset = weaponOffsetL;
                rotation = weaponRotationL;
                break;
            case "back":
                offset = weaponOffsetBack;
                rotation = weaponRotationBack;
                break;
            default:
                Debug.LogError("Type de support invalide pour configurer la position !");
                return;
        }

        equippedWeapon.transform.localPosition = offset;
        equippedWeapon.transform.localEulerAngles = rotation;
        equippedWeapon.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Déséquipe l'arme actuellement dans le support.
    /// </summary>
    /// <param name="holderType">Support à déséquiper : "right", "left", ou "back"</param>
    public void UnequipWeapon(string holderType = "right")
    {
        Transform selectedHolder = GetWeaponHolder(holderType);

        if (selectedHolder != null && selectedHolder.childCount > 0)
        {
            Destroy(selectedHolder.GetChild(0).gameObject);
            Debug.Log($"Arme déséquipée de {holderType}");
        }
    }
}

#region Interfaces

/// <summary>
/// Interface pour les armes de mêlée.
/// </summary>
public interface IMeleeWeapon
{
    void Attack(); // Effectuer une attaque de mêlée
}

/// <summary>
/// Interface pour les armes à distance.
/// </summary>
public interface IRangedWeapon
{
    void Fire(); // Effectuer un tir
}

#endregion
