using System.IO;
using UnityEditor;
using UnityEngine;

public class AIScriptTest : EditorWindow
{
    // Ajout du menu personnalisé dans l'éditeur Unity
    [MenuItem("Tools/Tester l'IA")]
    public static void ShowWindow()
    {
        // Nom et contenu du script à créer ou modifier
        string scriptName = "PlayerMovement";

        // Utilisation de chaînes concaténées pour éviter les problèmes de syntaxe
        string scriptContent =
@"using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    void Update()
    {
        Debug.Log(""Player is moving..."");
    }
}";

        // Appeler la méthode WriteScript depuis AIScriptManager
        AIScriptManager.WriteScript(scriptName, scriptContent);
    }
}

public class AIScriptManager
{
    // Chemin par défaut pour sauvegarder les scripts dans Unity
    private static string scriptsFolderPath = "Assets/Scripts/";

    public static void WriteScript(string scriptName, string scriptContent)
    {
        try
        {
            // Valider les entrées
            if (string.IsNullOrWhiteSpace(scriptName) || string.IsNullOrWhiteSpace(scriptContent))
            {
                Debug.LogError("Nom ou contenu du script invalide.");
                return;
            }

            // S'assurer que le dossier existe
            if (!Directory.Exists(scriptsFolderPath))
            {
                Directory.CreateDirectory(scriptsFolderPath);
                Debug.Log($"Dossier créé : {scriptsFolderPath}");
            }

            string filePath = Path.Combine(scriptsFolderPath, scriptName + ".cs");

            // Si le fichier existe, demander confirmation
            if (File.Exists(filePath))
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Remplacement du script",
                    $"Le script '{scriptName}' existe déjà. Voulez-vous le remplacer ?",
                    "Oui",
                    "Non");

                if (!replace)
                {
                    Debug.Log("Remplacement annulé.");
                    return;
                }
            }

            // Écriture ou remplacement du fichier
            File.WriteAllText(filePath, scriptContent);
            Debug.Log($"Script '{scriptName}' enregistré avec succès à : {filePath}");

            // Rafraîchir Unity pour charger le nouveau script
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur lors de la création du script : {ex.Message}");
        }
    }
}
