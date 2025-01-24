using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PoliceBehavior : MonoBehaviour
{
    private Police police;
    private int currentPatrolIndex;
    private bool isWaiting;

    private void Start()
    {
        police = GetComponent<Police>();

        // Démarrer la patrouille si des points sont assignés
        if (police.patrolPoints.Length > 0)
        {
            StartPatrol();
        }
        else
        {
            Debug.LogWarning("Aucun point de patrouille assigné !");
        }
    }

    private void Update()
    {
        if (police.playerStats == null) return;

        // Détermine le comportement selon le niveau de "wanted"
        if (police.playerStats.WantedLevel >= police.wantedThreshold)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        // Mise à jour des paramètres pour le Blend Tree
        police.UpdateAnimatorParameters();
    }

    private void StartPatrol()
    {
        if (police.patrolPoints.Length > 0)
        {
            police.agent.speed = 2f; // Vitesse de marche
            police.agent.SetDestination(police.patrolPoints[currentPatrolIndex].position);
        }
    }

    private void Patrol()
    {
        if (isWaiting || police.patrolPoints.Length == 0) return;

        if (!police.agent.pathPending && police.agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }

        // Met à jour les paramètres Blend Tree pour marcher
        UpdateBlendTreeParameters(police.agent.velocity);
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        police.posX = 0;
        police.posY = 0;
        yield return new WaitForSeconds(police.patrolWaitTime);

        currentPatrolIndex = (currentPatrolIndex + 1) % police.patrolPoints.Length;
        police.agent.SetDestination(police.patrolPoints[currentPatrolIndex].position);
        isWaiting = false;
    }

    private void ChasePlayer()
    {
        police.agent.speed = 5f; // Vitesse de course
        police.agent.SetDestination(police.player.position);

        // Met à jour les paramètres Blend Tree pour courir
        UpdateBlendTreeParameters(police.agent.velocity);

        if (Vector3.Distance(transform.position, police.player.position) <= police.arrestRange)
        {
            ArrestPlayer();
        }
    }

    private void ArrestPlayer()
    {
        Debug.Log("Le joueur est arrêté !");
        police.animator.SetTrigger("Arrest");

        // Réinitialise le niveau de "wanted"
        if (police.playerStats != null)
        {
            police.playerStats.WantedLevel = 0;
        }

        police.agent.ResetPath();
    }

    private void UpdateBlendTreeParameters(Vector3 velocity)
    {
        // Convertir la vélocité en espace local pour le Blend Tree
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        police.posX = Mathf.Lerp(police.posX, localVelocity.x, Time.deltaTime * 10f);
        police.posY = Mathf.Lerp(police.posY, localVelocity.z, Time.deltaTime * 10f);
    }
}
