using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Paramètres de l'arme")]
    public GameObject weaponPrefab; // L'arme à ramasser
    public AudioClip pickupSound; // Son de ramassage
    public ParticleSystem pickupEffect; // Effet visuel de ramassage (facultatif)

    private Collider pickupCollider;

    private void Start()
    {
        // Récupérer le collider pour le désactiver après ramassage
        pickupCollider = GetComponent<Collider>();
        if (pickupCollider == null)
        {
            Debug.LogError("Aucun Collider trouvé sur " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();
            if (weaponSystem != null)
            {
                // Équipe l'arme au joueur
                weaponSystem.EquipWeapon(weaponPrefab);

                // Joue un son de ramassage
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Joue un effet visuel de ramassage
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                Debug.Log("Arme ramassée : " + weaponPrefab.name);

                // Désactiver le collider pour éviter les collisions multiples
                if (pickupCollider != null)
                {
                    pickupCollider.enabled = false;
                }

                // Détruire l'objet après ramassage
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Le joueur n'a pas de composant WeaponSystem !");
            }
        }
    }
}
