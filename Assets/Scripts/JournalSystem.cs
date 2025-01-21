using UnityEngine;
using System.Collections.Generic;
public class JournalSystem : MonoBehaviour
{
    public enum EntryCategory { Quête, Événement, Découverte, Général }
    public List<JournalEntry> journalEntries = new List<JournalEntry>();
    public int maxEntries = 100; // Limitation des entrées

    [System.Serializable]
    public class JournalEntry
    {
        public string entryText;
        public EntryCategory category;
        public string timestamp;

        public JournalEntry(string text, EntryCategory cat)
        {
            entryText = text;
            category = cat;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public override string ToString()
        {
            return $"[{timestamp}] [{category}] {entryText}";
        }
    }

    public void AddEntry(string entry, EntryCategory category = EntryCategory.Général)
    {
        if (journalEntries.Count >= maxEntries)
        {
            journalEntries.RemoveAt(0); // Supprime l'entrée la plus ancienne
        }

        var newEntry = new JournalEntry(entry, category);
        journalEntries.Add(newEntry);
        Debug.Log($"Nouvelle entrée ajoutée : {newEntry}");
    }

    public void ShowJournal(EntryCategory? filterCategory = null)
    {
        Debug.Log("=== Journal ===");
        foreach (var entry in journalEntries)
        {
            if (filterCategory == null || entry.category == filterCategory)
            {
                Debug.Log(entry.ToString());
            }
        }
    }

    public void ClearJournal()
    {
        journalEntries.Clear();
        Debug.Log("Journal effacé.");
    }
}
