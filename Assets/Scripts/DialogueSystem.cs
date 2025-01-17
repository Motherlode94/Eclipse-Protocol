using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel; // Panneau de dialogue
    public TextMeshProUGUI dialogueText; // Texte principal pour afficher le dialogue
    public Button continueButton; // Bouton pour continuer le dialogue
    public GameObject choicePanel; // Panneau contenant les boutons de choix
    public Button[] choiceButtons; // Liste de boutons pour les choix

    private string[] dialogueLines; // Les lignes de dialogue
    private int currentLineIndex = 0; // Index de la ligne actuelle
    private bool isChoiceActive = false; // Indique si un choix est actif

    private System.Action<int> onChoiceSelected; // Action pour gérer les conséquences des choix

    private void Start()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        continueButton.onClick.AddListener(NextLine);
    }

    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        dialogueLines = lines;
        currentLineIndex = 0;
        isChoiceActive = false;

        dialoguePanel.SetActive(true);
        continueButton.gameObject.SetActive(true); // Afficher le bouton "Continuer"
        choicePanel.SetActive(false); // Cacher le panneau de choix
        ShowLine();
    }

    public void StartChoiceDialogue(string question, string[] choices, System.Action<int> onChoice)
    {
        isChoiceActive = true;

        dialogueText.text = question; // Afficher la question
        choicePanel.SetActive(true); // Afficher les choix
        continueButton.gameObject.SetActive(false); // Cacher le bouton "Continuer"
        onChoiceSelected = onChoice;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[i];
                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectChoice(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectChoice(int choiceIndex)
    {
        onChoiceSelected?.Invoke(choiceIndex);
        EndChoice();
    }

    private void EndChoice()
    {
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false); // Fermer le panneau de dialogue
        isChoiceActive = false;
    }

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

    private void NextLine()
    {
        if (!isChoiceActive)
        {
            currentLineIndex++;
            ShowLine();
        }
    }

    private void EndDialogue()
    {
        if (!isChoiceActive)
        {
            continueButton.gameObject.SetActive(false); // Cacher le bouton "Continuer"
            choicePanel.SetActive(true); // Afficher le panneau des choix si défini
        }
    }
}
