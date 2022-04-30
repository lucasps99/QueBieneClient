using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    enum State
    {
        Logging,
        LookingForOpponent,
        GameplayPhase,
        End
    }

    ServerFacade m_serverFacade;
    State m_state;
    [SerializeField] MashMoleDelegate m_mashMoleDelegate;
    bool m_retryCanStartGame = false;
    float m_retryTimeLeft;

    void Start()
    {
        m_serverFacade = new ServerFacade();
        StartCoroutine(m_serverFacade.JoinRoom(OnRoomJoined));
        m_state = State.Logging;
        m_mashMoleDelegate.Initialize(m_serverFacade);
    }

    void Update()
    {
        if (m_retryCanStartGame)
        {
            m_retryTimeLeft -= Time.deltaTime;
            if (m_retryTimeLeft <= 0)
            {
                m_retryCanStartGame = false;
                StartCoroutine(m_serverFacade.CanStartGame(OnCanStartGame));
            }
        }
    }

    void OnRoomJoined(ServerFacade.ActionResult i_result)
    {
        if (i_result == ServerFacade.ActionResult.Success)
        {
            m_state = State.LookingForOpponent;
            StartCoroutine(m_serverFacade.CanStartGame(OnCanStartGame));
        }
    }

    void OnCanStartGame(ServerFacade.ActionResult i_result, ServerFacade.StartGameInfo i_startGameInfo)
    {
        if (i_result == ServerFacade.ActionResult.Success)
        {
            m_state = State.GameplayPhase;
            StartCoroutine(m_serverFacade.GetBienes((i_result, i_bienes) => { m_mashMoleDelegate.SetGameInfo(i_startGameInfo, i_bienes); }));
        }
        else
        {
            m_retryCanStartGame = true;
            m_retryTimeLeft = 0.5f;
        }
    }

}
