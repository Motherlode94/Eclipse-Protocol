using UnityEngine;

public class CoverSystem : MonoBehaviour
{
    [Header("Cover Settings")]
    public LayerMask coverLayer;
    public Transform playerTransform;
    public Transform droneTransform;
    public float detectionRadius = 2f;

    private CharacterController characterController;
    private Animator animator;

    private bool isCrouching = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckForPlayerCover();
    }

    private void CheckForPlayerCover()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, coverLayer);

        if (hits.Length > 0)
        {
            foreach (Collider hit in hits)
            {
                // Vérifie si le joueur est proche d'une zone de couverture accroupie
                if (hit.CompareTag("CoverCrouchZone"))
                {
                    EnterCrouchCover();
                    return;
                }
                // Vérifie si le joueur est proche d'une zone de couverture debout
                else if (hit.CompareTag("CoverStandZone"))
                {
                    EnterStandCover();
                    return;
                }
            }
        }

        // Si aucune couverture n'est détectée
        ExitCover();
    }

    private void EnterCrouchCover()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            animator.SetBool("IsCrouching", true);
            characterController.height = 1f; // Réduit la hauteur pour s'accroupir
            Debug.Log("Player entered crouch cover!");
        }
    }

    private void EnterStandCover()
    {
        if (isCrouching)
        {
            isCrouching = false;
            animator.SetBool("IsCrouching", false);
            characterController.height = 2f; // Hauteur normale pour debout
            Debug.Log("Player entered stand cover!");
        }
    }

    private void ExitCover()
    {
        if (isCrouching)
        {
            isCrouching = false;
            animator.SetBool("IsCrouching", false);
            characterController.height = 2f; // Restaure la hauteur normale
            Debug.Log("Player exited cover!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
