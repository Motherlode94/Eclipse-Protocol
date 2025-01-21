using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    public int damage = 20; // Dégâts infligés par le piège
    public float activationCooldown = 2f; // Temps entre deux activations
    public GameObject activationEffect; // Effet visuel à l'activation

    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        var targetStats = other.GetComponent<PlayerStats>();
        if (targetStats != null)
        {
            ActivateTrap(targetStats);
        }
    }

    private void ActivateTrap(PlayerStats target)
    {
        target.TakeDamage(damage);
        Debug.Log($"{target.name} a reçu {damage} dégâts du piège.");

        // Effet visuel
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }

        isActivated = true;
        Invoke(nameof(ResetTrap), activationCooldown);
    }

    private void ResetTrap()
    {
        isActivated = false;
    }
}
