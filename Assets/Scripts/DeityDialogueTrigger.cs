using UnityEngine;

public class DeityDialogueTrigger : MonoBehaviour
{
    public DialogueSystem dialogueSystem; // Référence au système de dialogue
    public string[] deityDialogueLines; // Lignes de dialogue de la divinité

    private bool isPlayerInRange = false;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) // Touche "E" pour interagir
        {
            dialogueSystem.StartDialogue(deityDialogueLines);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
