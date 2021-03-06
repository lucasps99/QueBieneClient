using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MashMoleDelegate : MonoBehaviour
{
    ServerFacade m_serverFacade;
    ServerFacade.StartGameInfo m_bieneInfo = null;
    ServerFacade.Bienes m_bienes = null;
    bool m_initialized = false;
    int m_currentBiene = 0;
    float m_moleTimeLeft = 0;
    bool m_isMoleActive = false;
    long m_startTime;
    long m_serverTime;
    float m_serverDelta;

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

    float m_requestStateTimeLeft = 0.3f;

    public void Start()
    {
        foreach (ButtonImage button in m_buttons)
        {
            button.moleImage.enabled = false;
        }
        m_scoreText = m_score.GetComponent<TextMeshProUGUI>();
        //Debug.Assert(m_scoreText != null, "Score text is null");
    }

    public void Initialize(ServerFacade i_serverFacade)
    {
        m_serverFacade = i_serverFacade;
        StartCoroutine(m_serverFacade.GetServerTimestamp(SetTimestamp));
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
        m_bienes = null;
        m_initialized = false;
    }

    public void SetGameInfo(ServerFacade.StartGameInfo i_bieneInfo, ServerFacade.Bienes i_bienes)
    {
        m_greyScreen.SetActive(false);
        m_bieneInfo = i_bieneInfo;
        m_bienes = i_bienes;
        m_startTime = m_bieneInfo.timestamp;
        //Debug.Log($"---------------------------");
        //Debug.Log($"Start time is {m_startTime}");
        //Debug.Log($"---------------------------");
        m_initialized = true;
    }

    public void Update()
    {
        m_serverDelta += Time.deltaTime;
        long currentServerTime = m_serverTime + (long)(m_serverDelta*1000);
        if (!m_initialized)
        {
            return;
        }
        else if (currentServerTime > m_startTime && m_bieneInfo.isgameready && !m_isMoleActive && m_currentBiene < m_bienes.bienes.Length)
        {
            long timeElapsed = currentServerTime - m_startTime;
            if(timeElapsed > (m_bienes.bienes[m_currentBiene].delta * 1000))
            {
                //Debug.Log($"NEW biene with id {m_bienes.bienes[m_currentBiene].bieneId}, at position {m_bienes.bienes[m_currentBiene].position}");
                m_moleTimeLeft = m_moleTimeInSeconds;
                ButtonImage currentButton = m_buttons[m_bienes.bienes[m_currentBiene].position];
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
                m_requestStateTimeLeft = 0.3f;
                ButtonImage currentButton = m_buttons[m_bienes.bienes[m_currentBiene].position];
                currentButton.moleImage.enabled = false;
                currentButton.button.onClick.RemoveListener(OnCurrentBienePressed);
                //Debug.Log($"EXPIRED biene with id {m_bienes.bienes[m_currentBiene].bieneId}");
                m_currentBiene += 1;
                if (m_currentBiene >= m_bienes.bienes.Length)
                {
                    OnGameEnded();
                }
            }
        }
        if (m_requestStateTimeLeft >= 0)
        {
            m_requestStateTimeLeft -= Time.deltaTime;
            if (m_requestStateTimeLeft <= 0)
            {
                StartCoroutine(m_serverFacade.GetCurrentState(ResetUserData));
            }
        }
    }

    public void OnCurrentBienePressed()
    {
        //Debug.Log($"CURRENT BIENE PRESSED biene with id {m_bienes.bienes[m_currentBiene].bieneId}");
        ButtonImage currentButton = m_buttons[m_bienes.bienes[m_currentBiene].position];
        currentButton.moleImage.enabled = false;
        m_isMoleActive = false;
        StartCoroutine(m_serverFacade.OnBienePressed(m_bienes.bienes[m_currentBiene].bieneId, OnBienePressedCallback));
        m_currentBiene += 1;
        currentButton.button.onClick.RemoveListener(OnCurrentBienePressed);
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
        if (m_currentBiene >= m_bienes.bienes.Length)
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

    void ResetUserData(ServerFacade.ActionResult i_actionResult, ServerFacade.GetState i_response)
    {
        m_playerScore = i_response.userPoints;
        m_rivalScore = i_response.rivalPoints;
        m_scoreText.SetText($"{m_playerScore} - {m_rivalScore}");
    }

    void SetTimestamp(ServerFacade.ActionResult i_actionResult, long i_timestamp)
    {
        m_serverTime = i_timestamp;
        m_serverDelta = 0;
    }
}