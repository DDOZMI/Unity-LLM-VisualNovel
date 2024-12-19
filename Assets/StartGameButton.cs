using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Linq;
using SFB; // StandaloneFileBrowser namespace

public class MenuSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName;

    [Header("Button References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;

    [Header("UI Components")]
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        InitializeButtons();
        InitializeChatManager();
    }

    private void InitializeButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClick);

        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClick);
    }

    private void InitializeChatManager()
    {
        if (ChatDataManager.Instance == null)
        {
            Debug.LogWarning("ChatDataManager was not found, creating a new instance.");
        }
    }

    private void OnStartButtonClick()
    {
        ChatDataManager.Instance.ClearSelectedChat();
        LoadTargetScene();
    }

    private void OnLoadButtonClick()
    {
        // 파일 브라우저 필터 설정
        var filters = new[]
        {
            new ExtensionFilter("Chat Files", "json"),
            new ExtensionFilter("All Files", "*"),
        };

        // 파일 브라우저 열기
        string[] paths = StandaloneFileBrowser.OpenFilePanel("채팅 기록 불러오기", Application.persistentDataPath, filters, false);

        if (paths == null || paths.Length == 0)
        {
            if (statusText != null)
            {
                statusText.text = "파일을 선택하지 않았습니다.";
            }
            return;
        }

        string filePath = paths[0];

        if (!File.Exists(filePath))
        {
            if (statusText != null)
            {
                statusText.text = "선택한 파일이 존재하지 않습니다.";
            }
            return;
        }

        if (!Path.GetFileName(filePath).StartsWith("ChatHistory_"))
        {
            if (statusText != null)
            {
                statusText.text = "올바른 채팅 기록 파일이 아닙니다.";
            }
            return;
        }

        ChatDataManager.Instance.SetSelectedChatFile(filePath);
        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set!");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{targetSceneName}' is not included in build settings!");
        }
    }
}

// ChatDataManager는 이전과 동일하게 유지
public class ChatDataManager : MonoBehaviour
{
    private static ChatDataManager instance;
    public static ChatDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ChatDataManager");
                instance = go.AddComponent<ChatDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private string selectedChatFilePath;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedChatFile(string filePath)
    {
        selectedChatFilePath = filePath;
    }

    public string GetSelectedChatFile()
    {
        return selectedChatFilePath;
    }

    public void ClearSelectedChat()
    {
        selectedChatFilePath = null;
    }
}