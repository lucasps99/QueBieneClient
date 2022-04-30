using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class ClientFacade : MonoBehaviour
{
    private const string m_joinRoomURL = "http://www.my-server.com";
    private const string m_canStartGameURL = "http://www.my-server.com";

    private string m_clientId;
    ClientFacade()
    {
        long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        string machineName = Environment.MachineName;
        m_clientId = String.Concat(machineName, timestamp);
        StartCoroutine(JoinRoom());
    }

    UnityWebRequest SetupHeaders(UnityWebRequest i_request)
    {
        i_request.SetRequestHeader("userId", m_clientId);
        return i_request;
    }

    IEnumerator JoinRoom()
    {
        UnityWebRequest www = UnityWebRequest.Post(m_joinRoomURL, new WWWForm());
        www = SetupHeaders(www);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Connection Error");
        }
        else
        {
            // Show results as text
            Debug.Log("Logged in");
        }
    }

    IEnumerator CanStartGame()
    {
        UnityWebRequest www = UnityWebRequest.Get(m_canStartGameURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }


}

