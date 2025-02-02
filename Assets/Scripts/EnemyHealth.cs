using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    private MissionManager missionManager;

    private void Start()
    {
        currentHealth = maxHealth;

        // Récupère le MissionManager dans la scène
        missionManager = FindObjectOfType<MissionManager>();
        if (missionManager == null)
        {
            Debug.LogError("MissionManager introuvable dans la scène !");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Notifie la mission active si elle existe
        if (missionManager != null)
        {
            Mission activeMission = missionManager.GetActiveMission();
            if (activeMission != null)
            {
                activeMission.RegisterKill(); // Appelle la méthode pour enregistrer une élimination
            }
        }

        Destroy(gameObject); // Détruit l'ennemi
    }
}
