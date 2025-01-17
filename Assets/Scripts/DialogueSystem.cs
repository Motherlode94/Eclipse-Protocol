using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public GameObject dialoguePanel; // Le panneau de dialogue
    public Text dialogueText; // Texte pour afficher le dialogue
    public Button continueButton; // Bouton pour continuer le dialogue

    private string[] dialogueLines; // Les lignes de dialogue
    private int currentLineIndex = 0; // Index de la ligne actuelle

    private void Start()
    {
        dialoguePanel.SetActive(false); // Cacher le panneau au départ
        continueButton.onClick.AddListener(NextLine); // Lier le bouton à la fonction NextLine
    }

    public void StartDialogue(string[] lines)
    {
        dialogueLines = lines;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        ShowLine();
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
        currentLineIndex++;
        ShowLine();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
