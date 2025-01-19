using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public GameObject rifle; // Example for a rifle weapon
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Fire()
    {
        // Handle firing logic
        Debug.Log("Firing weapon");
        animator.SetTrigger("Fire");
    }
}
