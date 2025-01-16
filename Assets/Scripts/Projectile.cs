using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Vitesse du projectile
    public float damage = 10f; // Dégâts infligés
    public float lifetime = 5f; // Durée de vie du projectile

    private void Start()
    {
        Destroy(gameObject, lifetime); // Détruit le projectile après un certain temps
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime); // Déplacement du projectile
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(Mathf.RoundToInt(damage)); // Inflige des dégâts au joueur
            }
            Destroy(gameObject); // Détruit le projectile après l'impact
        }
    }
}
