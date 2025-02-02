using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log("Le joueur est caché !");
                playerStats.ModifyWantedLevel(-10); // Réduit temporairement le niveau de recherche
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Le joueur quitte la cachette !");
        }
    }
}
