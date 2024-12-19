using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class chatManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public ScrollRect scrollRect;
    public GameObject userMessagePrefab;
    public GameObject botMessagePrefab;
    public Transform contentTransform;

    private const string backendUrl = "http://localhost:5000/chat";

    private bool sendMessageCounter = true; // �Է� ���� ���θ� �����ϴ� ����

    private void Start()
    {
        // ����Ű�� �޽����� ������ �� GetResponse �޼��� ȣ��
        inputField.onSubmit.AddListener(delegate { GetResponse(); });
    }

    // �޽��� ������ ȣ�� -> ��ư ���� �� �Ǵ� ���Ͱ� ������ ��
    public void GetResponse()
    {
        if (!sendMessageCounter) return; // ������ �ޱ� �������� �޼��� ����X

        string inputText = inputField.text; // �Է��� �ؽ�Ʈ ��������
        if (string.IsNullOrEmpty(inputText)) return; // ���� �Է��̸� �ȵ�

        DisplayMessage(userMessagePrefab, inputText); // �Է� �޽��� ǥ��

        inputField.text = ""; // ��ǲ�ʵ� �ʱ�ȭ
        inputField.ActivateInputField(); // �ٷ� �Է� �����ϰ� ��ǲ�ʵ� Ȱ��ȭ

        sendMessageCounter = false;

        StartCoroutine(SendRequest(inputText)); // ������ ��û ����
    }

    // prefab�� �޼��� ��Ƽ� �߰��ϱ�
    void DisplayMessage(GameObject prefab, string messageText)
    {
        GameObject messageInstance = Instantiate(prefab, contentTransform); // Prefab �ν��Ͻ�ȭ
        TMP_Text messageTextComponent = messageInstance.GetComponentInChildren<TMP_Text>();
        messageTextComponent.text = messageText;
       
        // ���ο� �ؽ�Ʈ�� �߰��� ������ ��ũ���� �� �Ʒ��� �̵�
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    IEnumerator SendRequest(string inputText)
    {
        WWWForm form = new WWWForm();
        form.AddField("message", inputText);

        using (UnityWebRequest www = UnityWebRequest.Post(backendUrl, form))
        {
            yield return www.SendWebRequest(); // ��Ʈ��ũ ��û ����

            // ���� ó��
            if (www.result != UnityWebRequest.Result.Success)
            {
                DisplayMessage(botMessagePrefab, "Error: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text; // ������� json �ؽ�Ʈ
                Response response = JsonUtility.FromJson<Response>(jsonResponse); // json���� response ��ü�� ��ȯ

                // ���� ������ ȭ�鿡 ǥ��
                DisplayMessage(botMessagePrefab, response.response.Trim());
            }
        }

        sendMessageCounter = true;
        inputField.ActivateInputField();
    }

    [System.Serializable]
    private class Response
    {
        public string response;
    }
}
