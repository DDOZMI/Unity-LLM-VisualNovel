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

    private bool sendMessageCounter = true; // 입력 가능 여부를 저장하는 변수

    private void Start()
    {
        // 엔터키로 메시지를 전송할 때 GetResponse 메서드 호출
        inputField.onSubmit.AddListener(delegate { GetResponse(); });
    }

    // 메시지 보낼때 호출 -> 버튼 누를 시 또는 엔터가 눌렸을 때
    public void GetResponse()
    {
        if (!sendMessageCounter) return; // 응답을 받기 전까지는 메세지 전송X

        string inputText = inputField.text; // 입력한 텍스트 가져오기
        if (string.IsNullOrEmpty(inputText)) return; // 공백 입력이면 안됨

        DisplayMessage(userMessagePrefab, inputText); // 입력 메시지 표시

        inputField.text = ""; // 인풋필드 초기화
        inputField.ActivateInputField(); // 바로 입력 가능하게 인풋필드 활성화

        sendMessageCounter = false;

        StartCoroutine(SendRequest(inputText)); // 서버에 요청 전송
    }

    // prefab에 메세지 담아서 추가하기
    void DisplayMessage(GameObject prefab, string messageText)
    {
        GameObject messageInstance = Instantiate(prefab, contentTransform); // Prefab 인스턴스화
        TMP_Text messageTextComponent = messageInstance.GetComponentInChildren<TMP_Text>();
        messageTextComponent.text = messageText;
       
        // 새로운 텍스트가 추가될 때마다 스크롤을 맨 아래로 이동
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    IEnumerator SendRequest(string inputText)
    {
        WWWForm form = new WWWForm();
        form.AddField("message", inputText);

        using (UnityWebRequest www = UnityWebRequest.Post(backendUrl, form))
        {
            yield return www.SendWebRequest(); // 네트워크 요청 전송

            // 에러 처리
            if (www.result != UnityWebRequest.Result.Success)
            {
                DisplayMessage(botMessagePrefab, "Error: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text; // 응답받은 json 텍스트
                Response response = JsonUtility.FromJson<Response>(jsonResponse); // json에서 response 객체로 변환

                // 봇의 응답을 화면에 표시
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
