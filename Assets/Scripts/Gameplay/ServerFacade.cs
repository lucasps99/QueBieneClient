using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;  

public class ServerFacade : MonoBehaviour
{
    public enum ActionResult
    {
        Success,
        Error
    }

    private const string m_baseURL = "https://boiling-crag-17641.herokuapp.com/";
    private const string m_game = "game";
    private const string m_biene = "biene";
    private const string m_result = "result";

    private string m_clientId;
    public ServerFacade()
    {
        long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        string machineName = Environment.MachineName;
        m_clientId = String.Concat(machineName, timestamp);
    }

    public UnityWebRequest SetupHeaders(UnityWebRequest i_request)
    {
        i_request.SetRequestHeader("userId", m_clientId);
        return i_request;
    }

    public IEnumerator JoinRoom(Action<ActionResult> i_callback)
    {
        UnityWebRequest request = UnityWebRequest.Post(m_baseURL + m_game, new WWWForm());
        request = SetupHeaders(request);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Connection Error");
            i_callback(ActionResult.Error);
        }
        else
        {
            // Show results as text
            Debug.Log("Logged in");
            i_callback(ActionResult.Success);
        }
    }
    [Serializable]
    public class StartGameInfo
    {
        public bool isgameready { get; set; }
        public long timestamp = 30;
        //public BieneInfo[] bienes { get; set; }
        public BieneInfo[] bienes { get; set; }
    }

    public IEnumerator CanStartGame(Action<ActionResult, StartGameInfo> i_callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(m_baseURL + m_game);
        request = SetupHeaders(request);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
            i_callback(ActionResult.Error, null);
        }
        else
        {
            string text = request.downloadHandler.text;
            Debug.Log(text);
            StartGameInfo result = JsonUtility.FromJson<StartGameInfo>(text);

            i_callback(ActionResult.Success, new StartGameInfo());
        }
    }

    [Serializable]
    public class OnBienePressedResponse
    {
        public bool win;
    }

    public IEnumerator OnBienePressed(uint i_bieneId, Action<ActionResult, OnBienePressedResponse> i_callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("bieneId", i_bieneId.ToString());
        UnityWebRequest request = UnityWebRequest.Post(m_baseURL + m_biene, form);
        request = SetupHeaders(request);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
            i_callback(ActionResult.Error, null);
        }
        else
        {

            string text = request.downloadHandler.text;
            Debug.Log(text);
            OnBienePressedResponse result = JsonUtility.FromJson<OnBienePressedResponse>(text);

            i_callback(ActionResult.Success, result);
        }
    }

    [Serializable]
    public class GetResultResponse
    {
        public uint userPoints;
        public uint rivalPoints;
    }
    public IEnumerator GetResult(Action<ActionResult, GetResultResponse> i_callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(m_baseURL + m_result);
        request = SetupHeaders(request);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
            i_callback(ActionResult.Error, null);
        }
        else
        {

            string text = request.downloadHandler.text;
            Debug.Log(text);
            GetResultResponse result = JsonUtility.FromJson<GetResultResponse>(text);

            i_callback(ActionResult.Success, result);
        }
    }


}

