using UnityEngine;

public class ProjectileWeapon : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Préfabriqué du projectile
    public Transform firePoint; // Point d'origine du tir (par exemple : canon de l'arme)
    public float projectileSpeed = 20f; // Vitesse du projectile

    [Header("Cooldown")]
    public float fireRate = 0.5f; // Temps entre deux tirs
    private float nextFireTime = 0f;

    public void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            // Instancie le projectile au point de tir
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Applique une force au projectile
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * projectileSpeed;
            }

            Debug.Log("Projectile tiré !");
        }
    }
}
