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
    public Button[] choiceButtons; // Les boutons Oui et Non directement dans le Dialogue Panel

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isChoiceActive = false;
    private System.Action<int> onChoiceSelected;

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f; // Vitesse d'apparition des lettres
    private Coroutine typingCoroutine;

    private void Start()
    {
    dialoguePanel.SetActive(false); 
    continueButton.onClick.AddListener(NextLine);

    // Test pour fermer immédiatement le panneau
    Invoke("CloseDialogue", 2f); 
    }

    /// <summary>
    /// Démarre un dialogue linéaire
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

    // Activation de l'interface
    dialoguePanel.SetActive(true);
    continueButton.gameObject.SetActive(true);

    foreach (Button btn in choiceButtons)
    {
        btn.gameObject.SetActive(false);
    }

    ShowLine();
    }

    /// <summary>
    /// Affiche la ligne actuelle avec un effet de "machine à écrire".
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
        Debug.Log("Toutes les lignes ont été affichées, fin du dialogue.");
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

    /// <summary>
    /// Passe à la ligne suivante du dialogue.
    /// </summary>
    public void NextLine()
    {
    if (currentLineIndex < dialogueLines.Length - 1)
    {
        currentLineIndex++;
        ShowLine();
    }
    else
    {
        CloseDialogue(); // Ferme le dialogue si toutes les lignes ont été lues
    }
    }

    public void CloseDialogue()
{
    dialoguePanel.SetActive(false); // Désactive le panneau de dialogue
    Debug.Log("Dialogue fermé !");
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
    continueButton.gameObject.SetActive(false); // Cache le bouton Continue
    onChoiceSelected = onChoice;

    for (int i = 0; i < choiceButtons.Length; i++)
    {
        if (i < choices.Length)
        {
            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].interactable = true;
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


    /// <summary>
    /// Gère la sélection d'un choix.
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        Debug.Log($"Choix sélectionné : {choiceIndex}");
        onChoiceSelected?.Invoke(choiceIndex);
        EndChoice();
    }

    /// <summary>
    /// Termine un dialogue avec choix.
    /// </summary>
    private void EndChoice()
    {
        foreach (Button btn in choiceButtons)
        {
            btn.gameObject.SetActive(false); // Cache les boutons Oui et Non
        }

        continueButton.gameObject.SetActive(true); // Rétablit le bouton Continue
        isChoiceActive = false;

        EndDialogue(); // Termine le dialogue
    }

    /// <summary>
    /// Termine un dialogue linéaire ou avec choix.
    /// </summary>
    private void EndDialogue()
    {
    Debug.Log("Fin du dialogue - désactivation de l'interface.");
    dialoguePanel.SetActive(false);
    continueButton.gameObject.SetActive(false);
    }

    public void CheckProximityAndCloseDialogue(Transform player, Transform target, float interactionRadius)
{
    float distance = Vector3.Distance(player.position, target.position);
    if (distance > interactionRadius)
    {
        Debug.Log("Le joueur est hors de portée. Fermeture du dialogue.");
        CloseDialogue(); // Assurez-vous que cette méthode désactive le dialogue correctement
    }
}

}
