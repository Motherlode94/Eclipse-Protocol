using UnityEngine;

public class DynamicEvent : MonoBehaviour
{
    [Header("Trigger Settings")]
    public float triggerRadius = 10f; // Rayon de détection pour déclencher l'événement
    public GameObject eventPrefab; // Préfabriqué de l'événement à instancier
    public bool oneTimeTrigger = true; // Empêche plusieurs déclenchements si activé
    public float eventDelay = 0f; // Délai avant le déclenchement de l'événement

    [Header("Debug Settings")]
    public bool showTriggerRadius = true; // Affiche le rayon dans l'inspecteur

    private bool hasTriggered = false; // Empêche les multiples déclenchements

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!oneTimeTrigger || !hasTriggered))
        {
            hasTriggered = true; // Marque l'événement comme déclenché
            StartCoroutine(TriggerEventWithDelay());
        }
    }

    /// <summary>
    /// Gère le déclenchement de l'événement avec un délai optionnel.
    /// </summary>
    private System.Collections.IEnumerator TriggerEventWithDelay()
    {
        if (eventDelay > 0)
        {
            yield return new WaitForSeconds(eventDelay);
        }

        TriggerEvent();
    }

    /// <summary>
    /// Déclenche l'événement principal.
    /// </summary>
    private void TriggerEvent()
    {
        if (eventPrefab != null)
        {
            Instantiate(eventPrefab, transform.position, Quaternion.identity);
            Debug.Log("Événement déclenché !");
        }
        else
        {
            Debug.LogWarning("Aucun prefab d'événement assigné !");
        }
    }

    /// <summary>
    /// Affiche le rayon de détection dans l'éditeur.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (showTriggerRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
