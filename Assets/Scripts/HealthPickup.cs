using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int healthAmount = 50; // Points de santé restaurés
    public AudioClip pickupSound; // Son lors du ramassage

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.RegenerateHealth(healthAmount);
                Debug.Log($"Santé restaurée : {healthAmount}");

                // Joue le son de ramassage
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Détruit l'objet après le ramassage
                Destroy(gameObject);
            }
        }
    }
}
