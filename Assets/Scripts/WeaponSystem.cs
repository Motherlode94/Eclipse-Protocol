using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject equippedWeapon; // Arme actuellement équipée
    public Transform weaponHolderR, weaponHolderL, backHolder; // Supports pour les armes
    public List<GameObject> weaponInventory = new List<GameObject>(); // Inventaire des armes

    [Header("Offsets")]
    public Vector3 weaponOffsetR, weaponRotationR;
    public Vector3 weaponOffsetL, weaponRotationL;
    public Vector3 weaponOffsetBack, weaponRotationBack;

    private AudioSource audioSource;
    private float nextFireTime = 0f; // Gestion du cooldown global

    private void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["AttackPrimary"].performed += OnPrimaryAttack;
            playerInput.actions["SwitchWeapon"].performed += OnSwitchWeapon; // Gestion de la bascule entre armes
        }
    }

    private void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["AttackPrimary"].performed -= OnPrimaryAttack;
            playerInput.actions["SwitchWeapon"].performed -= OnSwitchWeapon;
        }
    }

    private void OnPrimaryAttack(InputAction.CallbackContext context)
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 0.5f; // Cooldown global (modifiable)
            UseWeapon();
        }
    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        if (weaponInventory.Count > 0)
        {
            // Passe à l'arme suivante dans l'inventaire
            int currentIndex = weaponInventory.IndexOf(equippedWeapon);
            int nextIndex = (currentIndex + 1) % weaponInventory.Count;
            EquipWeapon(weaponInventory[nextIndex]);
        }
    }

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

        equippedWeapon = Instantiate(newWeapon, selectedHolder);
        ConfigureWeaponPosition(selectedHolder, holderType);

        if (!weaponInventory.Contains(newWeapon))
        {
            weaponInventory.Add(newWeapon); // Ajoute à l'inventaire si ce n'est pas déjà fait
        }

        Debug.Log($"Arme équipée dans {holderType} : {newWeapon.name}");
    }

    private void UseWeapon()
    {
        if (equippedWeapon == null)
        {
            Debug.LogWarning("Aucune arme équipée !");
            return;
        }

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
}
