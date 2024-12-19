using System.Collections.Generic;
using UnityEngine;

public class ClearChat : MonoBehaviour
{
    public GameObject chatPanel; // ä���� ������� �г�
    private List<GameObject> chatMessages = new List<GameObject>(); // ä�� �޽��� ����Ʈ

    // ä�� �ʱ�ȭ �Լ�
    public void ResetChat()
    {
        // ä�� �޽��� ����Ʈ �ʱ�ȭ
        chatMessages.Clear();

        // ��� �ڽ� ������Ʈ ����
        foreach (Transform child in chatPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
