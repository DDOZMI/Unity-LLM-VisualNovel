using System.Collections.Generic;
using UnityEngine;

public class ClearChat : MonoBehaviour
{
    public GameObject chatPanel; // 채팅이 띄워지는 패널
    private List<GameObject> chatMessages = new List<GameObject>(); // 채팅 메시지 리스트

    // 채팅 초기화 함수
    public void ResetChat()
    {
        // 채팅 메시지 리스트 초기화
        chatMessages.Clear();

        // 모든 자식 오브젝트 삭제
        foreach (Transform child in chatPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
