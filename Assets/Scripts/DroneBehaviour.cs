using UnityEngine;

public class DroneBehaviour : MonoBehaviour
{
    [Header("Reinforcement Settings")]
    public GameObject reinforcementPrefab; // Préfab des renforts
    public Transform spawnPoint; // Position où le renfort apparaît
    public float reinforcementCooldown = 10f; // Temps entre deux appels de renfort

    [Header("Player Interaction")]
    public float attackDamage = 10f; // Dégâts infligés par attaque
    public float attackCooldown = 1f; // Temps entre deux attaques

    private float reinforcementTimer = 0f;
    private float attackTimer = 0f;

    private DroneAI droneAI;
    public GameObject projectilePrefab; // Préfab du projectile
    public Transform firePoint; // Point de tir du projectile

    private void Start()
    {
        droneAI = GetComponent<DroneAI>();
        if (droneAI == null)
        {
            Debug.LogError("DroneAI script is missing!");
        }
    }

    private void Update()
    {
        HandleReinforcements();
        HandleAttack();
    }

    private void HandleReinforcements()
    {
        if (droneAI.isChasing && reinforcementTimer <= 0f)
        {
            CallReinforcements();
            reinforcementTimer = reinforcementCooldown; // Réinitialise le cooldown
        }
        else
        {
            reinforcementTimer -= Time.deltaTime;
        }
    }

    private void CallReinforcements()
    {
        if (reinforcementPrefab != null && spawnPoint != null)
        {
            Instantiate(reinforcementPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("Reinforcements called!");
        }
        else
        {
            Debug.LogWarning("Reinforcement prefab or spawn point is not assigned!");
        }
    }

    public void HandleAttack()
    {
        if (droneAI.isChasing && droneAI.Player != null && attackTimer <= 0f)
        {
            // Tire un projectile
            FireProjectile();

            attackTimer = attackCooldown; // Réinitialise le cooldown d'attaque
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab != null && firePoint != null && droneAI.Player != null)
        {
            // Calcule la direction du joueur
            Vector3 directionToPlayer = (droneAI.Player.position - firePoint.position).normalized;

            // Crée une rotation pour pointer vers le joueur
            Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);

            // Instancie le projectile avec la bonne orientation
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotationToPlayer);

            // Configure les dégâts du projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = attackDamage;
            }

            Debug.Log("Projectile fired towards the player!");
        }
        else
        {
            Debug.LogWarning("Projectile prefab, fire point, or player is not assigned!");
        }
    }
}
