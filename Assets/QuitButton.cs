using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위한 네임스페이스
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button quitButton; // 종료 버튼
    [SerializeField] private TextMeshProUGUI buttonText; // 버튼의 텍스트

    private void Awake()
    {
        // 버튼이 할당되지 않았다면 현재 게임오브젝트에서 찾기
        if (quitButton == null)
            quitButton = GetComponent<Button>();

        // 버튼 텍스트가 할당되지 않았다면 버튼의 자식 오브젝트에서 찾기
        if (buttonText == null)
            buttonText = quitButton.GetComponentInChildren<TextMeshProUGUI>();

        // 버튼에 종료 함수 연결
        quitButton.onClick.AddListener(ExitGame);
    }

    // 버튼 텍스트 변경 메서드
    public void SetButtonText(string text)
    {
        if (buttonText != null)
            buttonText.text = text;
    }

    // 게임 종료 메서드
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}