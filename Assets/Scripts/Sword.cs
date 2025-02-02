using UnityEngine;

public class Sword : MonoBehaviour, IMeleeWeapon
{
    [Header("Sword Settings")]
    public int damage = 10; // Dégâts de l'arme
    public float attackSpeed = 1.2f; // Vitesse d'attaque (temps entre deux attaques)
    public float attackRange = 2f; // Portée de l'attaque
    public LayerMask targetLayer; // Couches cibles
    public ParticleSystem attackEffect; // Effet visuel d'attaque
    public AudioClip[] attackSounds; // Sons pour différents combos

    private Animator animator;
    private AudioSource audioSource;
    private int comboStep = 0; // Étape actuelle du combo
    private float comboCooldown = 1f; // Temps limite pour enchaîner les combos
    private float lastAttackTime;
    private bool isEquipped = false; // Indique si l'arme est équipée

    // Implémentation des propriétés de l'interface
    public float Damage => damage; // Notez que `damage` est maintenant un int
    public float AttackSpeed => attackSpeed;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Equip()
    {
        isEquipped = true;
        Debug.Log("Épée équipée !");
        gameObject.SetActive(true); // Active l'arme dans la scène
    }

    public void Unequip()
    {
        isEquipped = false;
        Debug.Log("Épée déséquipée !");
        gameObject.SetActive(false); // Désactive l'arme dans la scène
    }

    public void Attack()
    {
        if (!isEquipped)
        {
            Debug.LogWarning("Impossible d'attaquer : l'épée n'est pas équipée !");
            return;
        }

        // Gestion du combo
        if (Time.time - lastAttackTime > comboCooldown)
        {
            comboStep = 0; // Réinitialise le combo
        }

        lastAttackTime = Time.time;
        comboStep = (comboStep + 1) % 3; // Passe au coup suivant (max 3 étapes)

        Debug.Log($"Attaque avec l'épée ! Combo étape : {comboStep + 1}");

        // Lancer l'animation du combo
        if (animator != null)
        {
            animator.SetTrigger($"Attack{comboStep + 1}"); // Animation : Attack1, Attack2, Attack3
        }

        // Jouer le son d'attaque
        if (attackSounds.Length > comboStep && audioSource != null)
        {
            audioSource.PlayOneShot(attackSounds[comboStep]);
        }

        // Jouer l'effet visuel
        if (attackEffect != null)
        {
            attackEffect.Play();
        }

        // Détecter les cibles et appliquer des dégâts
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, attackRange, targetLayer);
        foreach (Collider target in hitTargets)
        {
            var targetStats = target.GetComponent<PlayerStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage); // Passe directement l'int
                Debug.Log($"Infligé {damage} dégâts à {target.name}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
