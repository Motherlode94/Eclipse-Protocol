using System.IO;
using UnityEditor;
using UnityEngine;

public class AIScriptIntegrator : MonoBehaviour
{
    // Chemin du dossier des scripts dans le projet Unity
    private static string scriptsFolderPath = "Assets/Scripts/";

    // Fonction pour écrire ou remplacer un script
    public static void WriteScript(string scriptName, string scriptContent)
    {
        string filePath = Path.Combine(scriptsFolderPath, scriptName + ".cs");

        // Vérifier si le fichier existe déjà
        if (File.Exists(filePath))
        {
            Debug.Log($"Le script {scriptName} existe déjà. Remplacement...");
        }

        // Écrire le contenu dans le fichier
        File.WriteAllText(filePath, scriptContent);
        Debug.Log($"Script {scriptName} enregistré à {filePath}");

        // Rafraîchir Unity pour charger le nouveau script
        AssetDatabase.Refresh();
    }

    // Fonction pour charger un script (pour lecture)
    public static string ReadScript(string scriptName)
    {
        string filePath = Path.Combine(scriptsFolderPath, scriptName + ".cs");

        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }

        Debug.LogError($"Le script {scriptName} n'existe pas.");
        return null;
    }
}
