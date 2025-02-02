using UnityEngine;

public class Bow : MonoBehaviour, IRangedWeapon
{
    public float Damage { get; private set; } = 25f;
    public float Range { get; private set; } = 50f;
    public AudioClip FireSound { get; private set; }
    public GameObject MuzzleFlash { get; private set; }

    public void Equip()
    {
        Debug.Log("Arc équipé !");
    }

    public void Fire()
    {
        Debug.Log("Flèche tirée avec un arc !");
    }

    public void Reload()
    {
        Debug.Log("Les flèches sont réapprovisionnées !");
    }

    public void Unequip()
    {
        Debug.Log("Arc déséquipé !");
    }
}
