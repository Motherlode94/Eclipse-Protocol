using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [Header("Panneaux à gérer")]
    public GameObject playerPanel; // Référence au PlayerPanel
    public GameObject minimapPanel;
    public GameObject inventoryPanel; // Référence à l'InventoryPanel

    private bool isGamePaused = false; // État pour suivre si le jeu est en pause

    private void OnEnable()
    {
        // Active InventoryPanel et désactive PlayerPanel
        if (inventoryPanel != null && playerPanel != null)
        {
            playerPanel.SetActive(false);
            minimapPanel.SetActive(false);
            PauseGame(); // Met le jeu en pause
        }
        else
        {
            Debug.LogWarning("Références manquantes dans PanelManager. Vérifiez l'Inspector.");
        }
    }

    private void OnDisable()
    {
        // Réactive PlayerPanel lorsque InventoryPanel est désactivé
        if (inventoryPanel != null && playerPanel != null)
        {
            playerPanel.SetActive(true);
            minimapPanel.SetActive(true);
            ResumeGame(); // Reprend le jeu
        }
    }

    private void PauseGame()
    {
        if (!isGamePaused)
        {
            Time.timeScale = 0f; // Arrête le temps
            isGamePaused = true;
            Debug.Log("Jeu en pause.");
        }
    }

    private void ResumeGame()
    {
        if (isGamePaused)
        {
            Time.timeScale = 1f; // Reprend le temps
            isGamePaused = false;
            Debug.Log("Jeu repris.");
        }
    }
}
