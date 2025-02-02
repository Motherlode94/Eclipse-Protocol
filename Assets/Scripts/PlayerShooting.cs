using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;

    [Header("Ammo")]
    public int maxAmmo = 10;
    private int currentAmmo;

    private float fireCooldown;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (fireCooldown > 0)
        {
            fireCooldown -= Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && fireCooldown <= 0 && currentAmmo > 0)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        fireCooldown = fireRate;
        currentAmmo--;
        Debug.Log("Tir effectué, munitions restantes : " + currentAmmo);
    }

    public void Reload(int ammo)
    {
        currentAmmo = Mathf.Min(currentAmmo + ammo, maxAmmo);
        Debug.Log("Recharge effectuée, munitions : " + currentAmmo);
    }
}
