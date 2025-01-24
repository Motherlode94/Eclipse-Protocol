using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MercenaryBehavior : MonoBehaviour
{
    private Mercenary mercenary;

    private int currentPatrolIndex;

    private void Start()
    {
        mercenary = GetComponent<Mercenary>();
    }

    private void Update()
    {
        if (mercenary.playerStats != null)
        {
            UpdateState(mercenary.playerStats.Reputation);
            HandleBehavior();
        }

        mercenary.UpdateAnimatorParameters();
    }

    private void UpdateState(int reputation)
    {
        if (reputation >= mercenary.allyThreshold)
        {
            mercenary.mercenaryState = Mercenary.State.Ally;
        }
        else if (reputation <= mercenary.enemyThreshold)
        {
            mercenary.mercenaryState = Mercenary.State.Enemy;
        }
        else
        {
            mercenary.mercenaryState = Mercenary.State.Neutral;
        }
    }

    private void HandleBehavior()
    {
        switch (mercenary.mercenaryState)
        {
            case Mercenary.State.Ally:
                FollowAndProtectPlayer();
                break;
            case Mercenary.State.Enemy:
                AttackPlayer();
                break;
            case Mercenary.State.Neutral:
                Patrol();
                break;
        }
    }

    private void FollowAndProtectPlayer()
    {
        mercenary.agent.SetDestination(mercenary.player.position);
        mercenary.animator.SetBool("isWalking", true);
    }

    private void AttackPlayer()
    {
        if (mercenary.player == null) return;

        mercenary.agent.SetDestination(mercenary.player.position);
        mercenary.animator.SetBool("isRunning", true);
    }

    private void Patrol()
    {
        if (mercenary.patrolPoints.Length > 0)
        {
            if (!mercenary.agent.pathPending && mercenary.agent.remainingDistance < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % mercenary.patrolPoints.Length;
                mercenary.agent.SetDestination(mercenary.patrolPoints[currentPatrolIndex].position);
            }
        }
    }
}
