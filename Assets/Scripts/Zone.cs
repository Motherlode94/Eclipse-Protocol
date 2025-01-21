using UnityEngine;

public class Zone : MonoBehaviour
{
    public enum ZoneType { Secure, Hostile, Neutral }
    public ZoneType zoneType;

    private Renderer zoneRenderer;

    [Header("Caractéristiques")]
    [TextArea] public string description; // Description de la zone
    public string[] personnages; // Types de personnages présents
    public string[] activites; // Activités principales
    public AudioClip ambianceSonore; // Son spécifique à la zone

    private AudioSource audioSource;

    void Start()
    {
        zoneRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateZone();
    }

    void UpdateZone()
    {
        UpdateZoneColor();
        UpdateAmbianceSonore();
    }

    void UpdateZoneColor()
    {
        if (zoneRenderer == null)
        {
            Debug.LogWarning("Renderer non trouvé sur la zone !");
            return;
        }

        switch (zoneType)
        {
            case ZoneType.Secure:
                zoneRenderer.material.color = Color.green;
                break;
            case ZoneType.Hostile:
                zoneRenderer.material.color = Color.red;
                break;
            case ZoneType.Neutral:
                zoneRenderer.material.color = Color.white;
                break;
        }
    }

    void UpdateAmbianceSonore()
    {
        if (ambianceSonore != null)
        {
            audioSource.clip = ambianceSonore;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered a {zoneType} zone.");
            DisplayZoneInfo();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited a {zoneType} zone.");
        }
    }

    void DisplayZoneInfo()
    {
        string info = $"Zone : {zoneType}\nDescription : {description}\n";
        info += $"Personnages présents : {string.Join(", ", personnages)}\n";
        info += $"Activités : {string.Join(", ", activites)}";

        Debug.Log(info);
        // Vous pouvez intégrer cette information dans l'UI du jeu ici.
    }

    public void ApplyWorldRules(string targetRules)
    {
        Debug.Log($"Application des règles reçues par le portail : {targetRules}");

        // Exemple : modifier la zone en fonction des règles
        if (targetRules == "HostileMode")
        {
            zoneType = ZoneType.Hostile;
            UpdateZone();
            Debug.Log("La zone est maintenant hostile !");
        }
        else if (targetRules == "NeutralMode")
        {
            zoneType = ZoneType.Neutral;
            UpdateZone();
            Debug.Log("La zone est maintenant neutre.");
        }
        else if (targetRules == "SecureMode")
        {
            zoneType = ZoneType.Secure;
            UpdateZone();
            Debug.Log("La zone est maintenant sécurisée.");
        }
        else
        {
            Debug.LogWarning("Aucune règle valide appliquée.");
        }
    }
}
