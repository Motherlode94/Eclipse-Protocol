using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public GameObject choicePanel;
    public Button[] choiceButtons;

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isChoiceActive = false;
    private System.Action<int> onChoiceSelected;

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f; // Vitesse d'apparition du texte
    private Coroutine typingCoroutine;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        continueButton.onClick.AddListener(NextLine);
    }

    /// <summary>
    /// Démarre un dialogue linéaire.
    /// </summary>
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
        continueButton.gameObject.SetActive(true);
        choicePanel.SetActive(false);
        ShowLine();
    }

    /// <summary>
    /// Affiche une ligne avec un effet de "taper à la machine".
    /// </summary>
    private void ShowLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
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

    /// <summary>
    /// Démarre un dialogue avec des choix.
    /// </summary>
    public void StartChoiceDialogue(string question, string[] choices, System.Action<int> onChoice)
    {
        isChoiceActive = true;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        dialogueText.text = question;
        choicePanel.SetActive(true);
        continueButton.gameObject.SetActive(false);
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
        dialoguePanel.SetActive(false);
        isChoiceActive = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        continueButton.gameObject.SetActive(false);
        Debug.Log("Dialogue terminé !");
    }

    /// <summary>
    /// Sauvegarde l'état actuel du dialogue.
    /// </summary>
    public void SaveDialogueState()
    {
        PlayerPrefs.SetInt("DialogueLineIndex", currentLineIndex);
        Debug.Log("État du dialogue sauvegardé !");
    }

    /// <summary>
    /// Charge l'état du dialogue depuis une sauvegarde.
    /// </summary>
    public void LoadDialogueState(string[] savedLines)
    {
        dialogueLines = savedLines;
        currentLineIndex = PlayerPrefs.GetInt("DialogueLineIndex", 0);
        StartDialogue(dialogueLines);
    }
}
