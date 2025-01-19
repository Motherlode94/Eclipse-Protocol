using UnityEngine;

public class DroneBehaviour : MonoBehaviour
{
    [Header("Reinforcement Settings")]
    public GameObject reinforcementPrefab;
    public Transform spawnPoint;
    public float reinforcementCooldown = 10f;

    [Header("Player Interaction")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 15f; // Distance maximale pour attaquer

    [Header("Effects")]
    public AudioClip reinforcementSound;
    public AudioClip fireSound;
    public ParticleSystem fireEffect;

    private float reinforcementTimer = 0f;
    private float attackTimer = 0f;

    private DroneAI droneAI;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private AudioSource audioSource;

    private void Start()
    {
        droneAI = GetComponent<DroneAI>();
        audioSource = GetComponent<AudioSource>();

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
            reinforcementTimer = reinforcementCooldown;
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

            if (reinforcementSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(reinforcementSound);
            }

            Debug.Log("Reinforcements called!");
        }
        else
        {
            Debug.LogWarning("Reinforcement prefab or spawn point is not assigned!");
        }
    }

    private void HandleAttack()
    {
        if (droneAI.isChasing && droneAI.Player != null && attackTimer <= 0f)
        {
            float distanceToPlayer = Vector3.Distance(droneAI.Player.position, transform.position);

            if (distanceToPlayer <= attackRange)
            {
                FireProjectile();
                attackTimer = attackCooldown;
            }
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
            Vector3 directionToPlayer = (droneAI.Player.position - firePoint.position).normalized;
            Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotationToPlayer);

            if (fireSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(fireSound);
            }

            if (fireEffect != null)
            {
                fireEffect.Play();
            }

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
