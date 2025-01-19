using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectName; // Nom de l'objet
    public int missionIndex;  // Index de la mission à compléter dans MissionManager

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Interaction avec : {objectName}");
            MissionManager missionManager = FindObjectOfType<MissionManager>();

            if (missionManager != null)
            {
                missionManager.CompleteMission(missionIndex);
            }
            else
            {
                Debug.LogWarning("MissionManager introuvable !");
            }
        }
    }
}
