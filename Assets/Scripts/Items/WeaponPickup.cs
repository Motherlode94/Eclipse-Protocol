using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weaponPrefab; // L'arme à ramasser
    public AudioClip pickupSound; // Son de ramassage

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();
            if (weaponSystem != null)
            {
                weaponSystem.EquipWeapon(weaponPrefab); // Équipe l'arme au joueur

                // Joue un son de ramassage
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                Debug.Log("Arme ramassée : " + weaponPrefab.name);
                Destroy(gameObject); // Détruit l'objet après ramassage
            }
        }
    }
}
