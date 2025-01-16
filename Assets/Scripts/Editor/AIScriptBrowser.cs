using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

public class AIScriptBrowser : EditorWindow
{
    private string userPrompt = ""; // Contient le texte saisi par l'utilisateur
    private string aiResponse = ""; // Contient la réponse de l'IA
    private Vector2 scrollPosResponse; // Pour le défilement de la réponse

    // URL et clé API pour l'IA
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private const string apiKey = "YOUR_API_KEY"; // Remplace par ta clé d'API OpenAI

    [MenuItem("Tools/IA Script Browser")]
    public static void ShowWindow()
    {
        GetWindow<AIScriptBrowser>("IA Script Browser");
    }

    private void OnGUI()
    {
        GUILayout.Label("Communication avec l'IA", EditorStyles.boldLabel);

        GUILayout.Space(10);

        // Zone de texte pour saisir une requête
        GUILayout.Label("Entrez votre demande :", EditorStyles.label);
        userPrompt = EditorGUILayout.TextArea(userPrompt, GUILayout.Height(75));

        // Bouton pour envoyer la requête
        if (GUILayout.Button("Envoyer la demande"))
        {
            SendRequestToAI();
        }

        GUILayout.Space(10);

        // Affichage de la réponse de l'IA avec défilement
        GUILayout.Label("Réponse de l'IA :", EditorStyles.label);
        scrollPosResponse = GUILayout.BeginScrollView(scrollPosResponse, GUILayout.Height(200));
        GUILayout.Label(aiResponse, EditorStyles.wordWrappedLabel);
        GUILayout.EndScrollView();
    }

    private void SendRequestToAI()
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            Debug.LogError("La demande est vide. Veuillez saisir du texte.");
            aiResponse = "Erreur : La demande est vide. Veuillez saisir du texte.";
            return;
        }

        // Appelle la coroutine pour envoyer la requête
        EditorCoroutineUtility.StartCoroutineOwnerless(SendAIRequestCoroutine());
    }

    private IEnumerator SendAIRequestCoroutine()
    {
        // Corps de la requête JSON
        string jsonData = JsonUtility.ToJson(new
        {
            model = "gpt-3.5-turbo", // Modèle utilisé
            messages = new[] { new { role = "user", content = userPrompt } },
            max_tokens = 150
        });

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Réponse reçue : " + request.downloadHandler.text);
                aiResponse = ParseResponse(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Erreur lors de la requête : " + request.error);
                aiResponse = "Erreur : Impossible de communiquer avec l'API.";
            }
        }
    }

    private string ParseResponse(string jsonResponse)
    {
        // Analyse simplifiée pour récupérer le contenu du message
        var response = JsonUtility.FromJson<AIResponse>(jsonResponse);
        return response.choices[0].message.content;
    }

    // Classes pour le parsing JSON
    [System.Serializable]
    private class AIResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [System.Serializable]
    private class Message
    {
        public string content;
    }
}
