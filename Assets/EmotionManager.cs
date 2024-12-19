using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class EmotionalCharacterSystem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private Button submitButton;
    
    [Header("Character Images")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Sprite neutralExpression;
    [SerializeField] private Sprite positiveExpression;
    [SerializeField] private Sprite negativeExpression;

    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "http://localhost:5001/analyze_sentiment";

    private void Start()
    {
        // �⺻ ǥ���� �߸����� ����
        if (characterImage != null && neutralExpression != null)
        {
            characterImage.sprite = neutralExpression;
        }

        // ��ư �̺�Ʈ ����
        submitButton.onClick.AddListener(OnSubmitChat);

        // Enter Ű �Է� ó��
        chatInput.onSubmit.AddListener((_) => OnSubmitChat());
    }

    private void OnSubmitChat()
    {
        if (string.IsNullOrEmpty(chatInput.text))
            return;

        string userInput = chatInput.text;
        StartCoroutine(AnalyzeEmotion(userInput));
        
        // �Է�â �ʱ�ȭ
        chatInput.text = "";
    }

    private IEnumerator AnalyzeEmotion(string text)
    {
        // ��û ������ �غ�
        var requestData = new EmotionRequest { text = text };
        string jsonData = JsonConvert.SerializeObject(requestData);

        // POST ��û ����
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // ��û ����
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // ���� �Ľ�
                string response = request.downloadHandler.text;
                EmotionResponse emotionResponse = JsonConvert.DeserializeObject<EmotionResponse>(response);

                // ������ ���� ĳ���� ǥ�� ����
                UpdateCharacterExpression(emotionResponse.sentiment);
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
            }
        }
    }

    private void UpdateCharacterExpression(string emotion)
    {
        if (characterImage == null)
            return;

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
}

[Serializable]
public class EmotionRequest
{
    public string text;
}

[Serializable]
public class EmotionResponse
{
    public string text;
    public string sentiment;
    public float confidence;
    public Probabilities probabilities;
}

[Serializable]
public class Probabilities
{
    public float negative;
    public float neutral;
    public float positive;
}