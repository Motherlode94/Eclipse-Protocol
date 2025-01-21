using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel; // Panneau de dialogue principal
    public TextMeshProUGUI dialogueText; // Texte principal du dialogue
    public Button continueButton; // Bouton pour passer à la ligne suivante
    public GameObject choicePanel; // Panneau pour afficher les choix
    public Button[] choiceButtons; // Boutons pour les options de choix

    private string[] dialogueLines; // Lignes du dialogue actuel
    private int currentLineIndex = 0; // Index de la ligne de dialogue actuelle
    private bool isChoiceActive = false; // Indique si un choix est actif

    private System.Action<int> onChoiceSelected; // Action déclenchée lors de la sélection d'un choix

    private void Start()
    {
        // Initialisation
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        continueButton.onClick.AddListener(NextLine);
    }

    /// <summary>
    /// Démarre un dialogue linéaire (sans choix).
    /// </summary>
    /// <param name="lines">Les lignes du dialogue à afficher.</param>
    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("Dialogue vide ou non défini !");
            return;
        }

        dialogueLines = lines;
        currentLineIndex = 0;
        isChoiceActive = false;

        dialoguePanel.SetActive(true);
        continueButton.gameObject.SetActive(true); // Affiche le bouton "Continuer"
        choicePanel.SetActive(false); // Cache le panneau des choix
        ShowLine();
    }

    /// <summary>
    /// Démarre un dialogue avec des choix.
    /// </summary>
    /// <param name="question">Question à afficher.</param>
    /// <param name="choices">Liste des choix possibles.</param>
    /// <param name="onChoice">Callback déclenché lors de la sélection d'un choix.</param>
    public void StartChoiceDialogue(string question, string[] choices, System.Action<int> onChoice)
    {
        isChoiceActive = true;

        dialogueText.text = question; // Affiche la question
        choicePanel.SetActive(true); // Affiche le panneau des choix
        continueButton.gameObject.SetActive(false); // Cache le bouton "Continuer"
        onChoiceSelected = onChoice;

        // Configure les boutons pour les choix
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[i];
                int choiceIndex = i; // Capture locale pour éviter les problèmes de closure
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectChoice(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false); // Désactive les boutons non nécessaires
            }
        }
    }

    /// <summary>
    /// Affiche la ligne actuelle du dialogue.
    /// </summary>
    private void ShowLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Passe à la ligne suivante dans le dialogue linéaire.
    /// </summary>
    private void NextLine()
    {
        if (!isChoiceActive)
        {
            currentLineIndex++;
            ShowLine();
        }
    }

    /// <summary>
    /// Termine le dialogue linéaire.
    /// </summary>
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        continueButton.gameObject.SetActive(false);
        Debug.Log("Dialogue terminé !");
    }

    /// <summary>
    /// Gère la sélection d'un choix.
    /// </summary>
    /// <param name="choiceIndex">Index du choix sélectionné.</param>
    private void SelectChoice(int choiceIndex)
    {
        onChoiceSelected?.Invoke(choiceIndex); // Exécute le callback avec l'index du choix
        EndChoice();
    }

    /// <summary>
    /// Termine le mode de choix et cache les UI associées.
    /// </summary>
    private void EndChoice()
    {
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        isChoiceActive = false;
    }
}
