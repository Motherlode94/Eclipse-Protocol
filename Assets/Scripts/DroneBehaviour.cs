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
    public float attackRange = 15f;

    [Header("Effects")]
    public AudioClip reinforcementSound;
    public AudioClip fireSound;
    public ParticleSystem fireEffect;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float reinforcementTimer = 0f;
    private float attackTimer = 0f;

    private DroneAI droneAI;
    private AudioSource audioSource;

    private void Start()
    {
        droneAI = GetComponent<DroneAI>();
        audioSource = GetComponent<AudioSource>();

        if (droneAI == null)
        {
            Debug.LogError($"[DroneBehaviour] DroneAI script is missing on {gameObject.name}!");
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

            PlaySound(reinforcementSound);

            Debug.Log($"[DroneBehaviour] Reinforcements called by {gameObject.name}!");
        }
        else
        {
            Debug.LogWarning($"[DroneBehaviour] ReinforcementPrefab or SpawnPoint is not assigned on {gameObject.name}.");
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
            // Calculate direction and rotation towards the player
            Vector3 directionToPlayer = (droneAI.Player.position - firePoint.position).normalized;
            Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);

            // Instantiate and configure projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotationToPlayer);

            PlaySound(fireSound);

            if (fireEffect != null)
            {
                fireEffect.Play();
            }

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = attackDamage;
            }

            Debug.Log($"[DroneBehaviour] {gameObject.name} fired a projectile at {droneAI.Player.name}!");
        }
        else
        {
            Debug.LogWarning($"[DroneBehaviour] Missing components for firing on {gameObject.name}.");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
