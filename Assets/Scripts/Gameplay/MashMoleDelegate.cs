﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MashMoleDelegate : MonoBehaviour
{
    ServerFacade m_serverFacade;
    ServerFacade.StartGameInfo m_bieneInfo = null;
    bool m_initialized = false;
    int m_currentBiene = 0;
    float m_moleTimeLeft = 0;
    bool m_isMoleActive = false;
    DateTime m_startTime;

    public UnityEvent onGameEnded;

    uint m_playerScore = 0;
    uint m_rivalScore = 0;

    [Serializable]
    public class ButtonImage
    {
        public Button button;
        public Image moleImage;
    }

    [SerializeField] List<ButtonImage> m_buttons;
    [SerializeField] GameObject m_score;
    [SerializeField] float m_moleTimeInSeconds;
    TextMeshProUGUI m_scoreText;

    [SerializeField] GameObject m_greyScreen;
    [SerializeField] GameObject m_winText;
    [SerializeField] GameObject m_loseText;
    [SerializeField] GameObject m_drawText;

    public void Start()
    {
        foreach (ButtonImage button in m_buttons)
        {
            button.moleImage.enabled = false;
        }
        m_scoreText = m_score.GetComponent<TextMeshProUGUI>();
        Debug.Assert(m_scoreText != null, "Score text is null");
        TextMeshProUGUI winText = m_winText.GetComponent<TextMeshProUGUI>();
        winText.color = new Color(0f, 1f, 0f);
        TextMeshProUGUI loseText = m_loseText.GetComponent<TextMeshProUGUI>();
        winText.color = new Color(1f, 0f, 0f);
        TextMeshProUGUI drawText = m_drawText.GetComponent<TextMeshProUGUI>();
        winText.color = new Color(1f, 1f, 1f);
    }

    public void Initialize(ServerFacade i_serverFacade)
    {
        m_serverFacade = i_serverFacade;
    }

    public void Reset()
    {
        m_currentBiene = 0;
        m_playerScore = 0;
        m_rivalScore = 0;
        m_drawText.SetActive(false);
        m_winText.SetActive(false);
        m_loseText.SetActive(false);
        m_greyScreen.SetActive(false);
        m_bieneInfo = null;
        m_initialized = false;
    }

    public void SetGameInfo(ServerFacade.StartGameInfo i_bieneInfo)
    {
        m_greyScreen.SetActive(false);
        m_bieneInfo = i_bieneInfo;
        m_startTime = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(m_bieneInfo.timestamp);
        m_initialized = true;
    }

    public void Update()
    {
        if (!m_initialized)
        {
            return;
        }
        else if (DateTime.Now > m_startTime && !m_isMoleActive)
        {
            TimeSpan timeElapsed = DateTime.Now - m_startTime;
            if(timeElapsed.Seconds > m_bieneInfo.bienes[m_currentBiene].delta)
            {
                m_moleTimeLeft = m_moleTimeInSeconds;
                ButtonImage currentButton = m_buttons[m_bieneInfo.bienes[m_currentBiene].position];
                currentButton.moleImage.enabled = true;
                currentButton.button.onClick.AddListener(OnCurrentBienePressed);
                m_isMoleActive = true;
            }
        }
        if (m_isMoleActive)
        {
            m_moleTimeLeft -= Time.deltaTime;
            if (m_moleTimeLeft <= 0)
            {
                m_isMoleActive = false;
                //Get state;
                m_currentBiene += 1;
                if (m_currentBiene >= m_bieneInfo.bienes.Length)
                {
                    OnGameEnded();
                }
            }
        }
    }

    public void OnCurrentBienePressed()
    {
        ButtonImage currentButton = m_buttons[m_bieneInfo.bienes[m_currentBiene].position];
        currentButton.button.onClick.RemoveListener(OnCurrentBienePressed);
        currentButton.moleImage.enabled = false;
        StartCoroutine(m_serverFacade.OnBienePressed(m_bieneInfo.bienes[m_currentBiene].bieneId, OnBienePressedCallback));
        m_isMoleActive = false;
    }

    public void OnBienePressedCallback(ServerFacade.ActionResult i_result, ServerFacade.OnBienePressedResponse i_response)
    {
        if(i_response.win)
        {
            m_playerScore += 1;
        }
        else
        {
            m_rivalScore += 1;
        }
        m_scoreText.SetText($"{m_playerScore} - {m_rivalScore}");
        m_currentBiene += 1;
        if (m_currentBiene >= m_bieneInfo.bienes.Length)
        {
            OnGameEnded();
        }
    }

    void OnGameEnded()
    {
        StartCoroutine(m_serverFacade.GetResult(OnGameEndedCallback));
    }

    void OnGameEndedCallback(ServerFacade.ActionResult i_result, ServerFacade.GetResultResponse i_response)
    {
        m_playerScore = i_response.userPoints;
        m_rivalScore = i_response.rivalPoints;
        m_scoreText.SetText($"{m_playerScore} - {m_rivalScore}");
        onGameEnded.Invoke();
        if (m_playerScore > m_rivalScore)
        {
            m_winText.SetActive(true);
        }
        else if (m_playerScore < m_rivalScore)
        {
            m_loseText.SetActive(true);
        }
        else
        {
            m_drawText.SetActive(true);
        }
        m_greyScreen.SetActive(true);
    }


}