using UnityEngine;

public class Sword : MonoBehaviour, IMeleeWeapon
{
    [Header("Sword Settings")]
    public int damage = 10; // Dégâts infligés par l'épée
    public float attackRange = 2f; // Portée de l'attaque
    public LayerMask targetLayer; // Couches ciblées (par exemple, ennemis)
    public ParticleSystem attackEffect; // Effet visuel lors de l'attaque
    public AudioClip attackSound; // Son de l'attaque

    private Animator animator; // Pour jouer les animations
    private AudioSource audioSource;

    private void Start()
    {
        // Récupère les composants nécessaires
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Attack()
    {
        Debug.Log("Attaque avec l'épée !");
        
        // Lancer l'animation d'attaque si disponible
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Jouer le son d'attaque
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        // Jouer l'effet visuel
        if (attackEffect != null)
        {
            attackEffect.Play();
        }

        // Détecter les cibles dans la portée d'attaque
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, attackRange, targetLayer);
        foreach (Collider target in hitTargets)
        {
            // Appliquer des dégâts si la cible possède un script compatible (par exemple, PlayerStats)
            var targetStats = target.GetComponent<PlayerStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage);
                Debug.Log($"Infligé {damage} dégâts à {target.name}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Affiche la portée d'attaque dans l'éditeur Unity
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
