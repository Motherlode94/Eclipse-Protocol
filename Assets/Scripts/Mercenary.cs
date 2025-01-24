using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Mercenary : MonoBehaviour
{
    public enum State { Ally, Enemy, Neutral }
    public State mercenaryState = State.Neutral;

    [Header("Reputation Thresholds")]
    public int allyThreshold = 70;
    public int enemyThreshold = 30;

    [Header("Références")]
    public Transform[] patrolPoints;
    public Animator animator;
    public NavMeshAgent agent;
    public Transform player;
    public PlayerStats playerStats;

    // Animation parameters
    [HideInInspector] public float posX;
    [HideInInspector] public float posY;

    // Blend Tree velocity tracking
    public Vector3 LocalVelocity => transform.InverseTransformDirection(agent.velocity);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
        else
        {
            Debug.LogError("Le script PlayerStats n'est pas attaché au joueur !");
        }
    }

    public void UpdateAnimatorParameters()
    {
        posX = Mathf.Lerp(posX, LocalVelocity.x, Time.deltaTime * 10f);
        posY = Mathf.Lerp(posY, LocalVelocity.z, Time.deltaTime * 10f);

        animator.SetFloat("PosX", posX);
        animator.SetFloat("PosY", posY);
    }
}
