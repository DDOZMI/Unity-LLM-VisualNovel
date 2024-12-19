using System.Diagnostics;
using UnityEngine;
using System.Collections;

using Debug = UnityEngine.Debug;

public class LocalServerLauncher : MonoBehaviour
{
    private Process flaskProcess;

    // 실행 시 서버 실행
    private void Start()
    {
        StartServer();
        StartCoroutine(SendMessage());
    }

    // 종료 시 서버 종료
    private void OnApplicationQuit()
    {
        StopServer();
    }

    void StartServer()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "python";
        startInfo.Arguments = "C://Users//justi//UnityProject//NovelMaker_Ver3//Assets//Server//api.py";
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        flaskProcess = Process.Start(startInfo);

        Debug.Log("서버 실행");
    }

    void StopServer()
    {
        if (flaskProcess != null && !flaskProcess.HasExited)
        {
            flaskProcess.Kill();
            flaskProcess = null;
            Debug.Log("서버 종료");
        }
    }

    IEnumerator SendMessage()
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.PostWwwForm("http://localhost:5000/chat", "message=Hello"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("응답: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("서버 요청 실패: " + request.error);
            }
        }
    }

}