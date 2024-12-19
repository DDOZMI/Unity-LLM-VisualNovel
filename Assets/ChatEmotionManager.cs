using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using SFB;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnifiedChatEmotionManager : MonoBehaviour
{
    [Header("Chat UI Components")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject userMessagePrefab;
    [SerializeField] private GameObject botMessagePrefab;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    [Header("Chat History Options")]
    [SerializeField] private bool clearChatOnLoad = true;
    [SerializeField] private bool showTimeStamp = true;

    [Header("Character Components")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Sprite neutralExpression;
    [SerializeField] private Sprite positiveExpression;
    [SerializeField] private Sprite negativeExpression;

    [Header("Server Settings")]
    [SerializeField] private string emotionAnalysisUrl = "http://localhost:5001/analyze_sentiment";
    [SerializeField] private string chatUrl = "http://localhost:5000/chat";

    private bool canSendMessage = true;
    private List<ChatMessage> chatHistory = new List<ChatMessage>();

    private void Start()
    {
        if (characterImage != null && neutralExpression != null)
        {
            characterImage.sprite = neutralExpression;
        }

        submitButton.onClick.AddListener(ProcessInput);
        inputField.onSubmit.AddListener(delegate { ProcessInput(); });

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveChatHistory);
        }

        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadChatHistoryFromButton);
        }

        // ChatDataManager에서 선택된 파일이 있는지 확인하고 자동 로드
        string selectedChatFile = ChatDataManager.Instance?.GetSelectedChatFile();
        if (!string.IsNullOrEmpty(selectedChatFile))
        {
            LoadSpecificChatHistory(selectedChatFile);
            ChatDataManager.Instance.ClearSelectedChat(); // 로드 후 클리어
        }
    }

    public void ClearChat()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        chatHistory.Clear();
    }

    private void LoadChatHistoryFromButton()
    {
        // 파일 브라우저 필터 설정
        var filters = new[] {
        new ExtensionFilter("Chat Files", "json"),
        new ExtensionFilter("All Files", "*"),
    };

        // 파일 브라우저 열기
        string[] paths = StandaloneFileBrowser.OpenFilePanel("채팅 기록 불러오기", Application.persistentDataPath, filters, false);

        // 파일 선택 취소 시 리턴
        if (paths == null || paths.Length == 0)
        {
            return;
        }

        string path = paths[0];

        // ChatHistory_ 로 시작하지 않는 파일 필터링
        if (!Path.GetFileName(path).StartsWith("ChatHistory_"))
        {
            DisplayMessage(botMessagePrefab, "올바른 채팅 기록 파일이 아닙니다.", false);
            return;
        }

        LoadSpecificChatHistory(path);
    }

    private void LoadSpecificChatHistory(string path)
    {
        try
        {
            string jsonContent = File.ReadAllText(path);
            ChatHistoryData loadedHistory = JsonConvert.DeserializeObject<ChatHistoryData>(jsonContent);

            if (clearChatOnLoad)
            {
                ClearChat();
            }

            foreach (var message in loadedHistory.messages)
            {
                GameObject prefab = message.isUser ? userMessagePrefab : botMessagePrefab;
                DisplayLoadedMessage(prefab, message);
            }

            DisplayMessage(botMessagePrefab, $"채팅 기록이 복원되었습니다. (저장 시간: {loadedHistory.savedAt:yyyy-MM-dd HH:mm:ss})", false);
        }
        catch (Exception e)
        {
            Debug.LogError($"채팅 기록 로드 실패: {e.Message}");
            DisplayMessage(botMessagePrefab, "채팅 기록을 불러오는데 실패했습니다.", false);
        }
    }

    private void DisplayLoadedMessage(GameObject prefab, ChatMessage message)
    {
        GameObject messageInstance = Instantiate(prefab, contentTransform);
        TMP_Text messageTextComponent = messageInstance.GetComponentInChildren<TMP_Text>();

        string displayText = showTimeStamp
            ? $"[{message.timestamp:HH:mm:ss}] {message.content}"
            : message.content;

        messageTextComponent.text = displayText;

        chatHistory.Add(message);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void DisplayMessage(GameObject prefab, string messageText, bool isUser)
    {
        GameObject messageInstance = Instantiate(prefab, contentTransform);
        TMP_Text messageTextComponent = messageInstance.GetComponentInChildren<TMP_Text>();

        string displayText = showTimeStamp
            ? $"[{DateTime.Now:HH:mm:ss}] {messageText}"
            : messageText;

        messageTextComponent.text = displayText;

        ChatMessage message = new ChatMessage
        {
            content = messageText,
            timestamp = DateTime.Now,
            isUser = isUser
        };
        chatHistory.Add(message);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void SaveChatHistory()
    {
        if (chatHistory.Count == 0)
        {
            DisplayMessage(botMessagePrefab, "저장할 채팅 기록이 없습니다.", false);
            return;
        }

        ChatHistoryData historyData = new ChatHistoryData
        {
            messages = chatHistory,
            savedAt = DateTime.Now
        };

        string json = JsonConvert.SerializeObject(historyData, Formatting.Indented);
        string fileName = $"ChatHistory_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Chat history saved to: {filePath}");
            DisplayMessage(botMessagePrefab, "채팅 기록이 저장되었습니다.", false);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save chat history: {e.Message}");
            DisplayMessage(botMessagePrefab, "채팅 기록 저장에 실패했습니다.", false);
        }
    }

    public void ProcessInput()
    {
        if (!canSendMessage) return;

        string inputText = inputField.text;
        if (string.IsNullOrEmpty(inputText)) return;

        DisplayMessage(userMessagePrefab, inputText, true);

        inputField.text = "";
        inputField.ActivateInputField();

        canSendMessage = false;
        StartCoroutine(ProcessMessageFlow(inputText));
    }

    private IEnumerator ProcessMessageFlow(string inputText)
    {
        yield return StartCoroutine(AnalyzeEmotion(inputText));
        yield return StartCoroutine(GetChatResponse(inputText));

        canSendMessage = true;
        inputField.ActivateInputField();
    }

    private IEnumerator AnalyzeEmotion(string text)
    {
        var requestData = new EmotionRequest { text = text };
        string jsonData = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest(emotionAnalysisUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                EmotionResponse emotionResponse = JsonConvert.DeserializeObject<EmotionResponse>(response);
                UpdateCharacterExpression(emotionResponse.sentiment);
            }
            else
            {
                Debug.LogError($"Emotion Analysis Error: {request.error}");
                UpdateCharacterExpression("neutral");
            }
        }
    }

    private IEnumerator GetChatResponse(string inputText)
    {
        WWWForm form = new WWWForm();
        form.AddField("message", inputText);

        using (UnityWebRequest www = UnityWebRequest.Post(chatUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                DisplayMessage(botMessagePrefab, "Error: " + www.error, false);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                DisplayMessage(botMessagePrefab, response.response.Trim(), false);
            }
        }
    }

    private void UpdateCharacterExpression(string emotion)
    {
        if (characterImage == null) return;

        switch (emotion.ToLower())
        {
            case "positive":
                if (positiveExpression != null)
                    characterImage.sprite = positiveExpression;
                break;

            case "negative":
                if (negativeExpression != null)
                    characterImage.sprite = negativeExpression;
                break;

            case "neutral":
            default:
                if (neutralExpression != null)
                    characterImage.sprite = neutralExpression;
                break;
        }
    }

    [Serializable]
    private class EmotionRequest
    {
        public string text;
    }

    [Serializable]
    private class EmotionResponse
    {
        public string text;
        public string sentiment;
        public float confidence;
        public Probabilities probabilities;
    }

    [Serializable]
    private class Probabilities
    {
        public float negative;
        public float neutral;
        public float positive;
    }

    [Serializable]
    private class ChatResponse
    {
        public string response;
    }

    [Serializable]
    private class ChatMessage
    {
        public string content;
        public DateTime timestamp;
        public bool isUser;
    }

    [Serializable]
    private class ChatHistoryData
    {
        public List<ChatMessage> messages;
        public DateTime savedAt;
    }
}