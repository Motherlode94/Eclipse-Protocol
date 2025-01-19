using UnityEngine;
using System.Collections.Generic;

public class JournalSystem : MonoBehaviour
{
    public List<string> journalEntries = new List<string>();

    public void AddEntry(string entry)
    {
        journalEntries.Add($"[{System.DateTime.Now}] {entry}");
        Debug.Log($"Entrée ajoutée au journal : {entry}");
    }

    public void ShowJournal()
    {
        foreach (var entry in journalEntries)
        {
            Debug.Log(entry);
        }
    }
}
