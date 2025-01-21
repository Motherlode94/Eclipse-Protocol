using UnityEngine;
using UnityEngine.AI;

public class CompanionAI : MonoBehaviour
{
    public Transform player;
    public float followDistance = 5f;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, player.position) > followDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    public void AttackTarget(Transform target)
    {
        Debug.Log("Compagnon attaque " + target.name);
        // Ajoutez ici les actions d'attaque
    }
}
