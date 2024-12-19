using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� ���ӽ����̽�
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button quitButton; // ���� ��ư
    [SerializeField] private TextMeshProUGUI buttonText; // ��ư�� �ؽ�Ʈ

    private void Awake()
    {
        // ��ư�� �Ҵ���� �ʾҴٸ� ���� ���ӿ�����Ʈ���� ã��
        if (quitButton == null)
            quitButton = GetComponent<Button>();

        // ��ư �ؽ�Ʈ�� �Ҵ���� �ʾҴٸ� ��ư�� �ڽ� ������Ʈ���� ã��
        if (buttonText == null)
            buttonText = quitButton.GetComponentInChildren<TextMeshProUGUI>();

        // ��ư�� ���� �Լ� ����
        quitButton.onClick.AddListener(ExitGame);
    }

    // ��ư �ؽ�Ʈ ���� �޼���
    public void SetButtonText(string text)
    {
        if (buttonText != null)
            buttonText.text = text;
    }

    // ���� ���� �޼���
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}