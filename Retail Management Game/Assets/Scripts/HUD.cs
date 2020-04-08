using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    [Header("Countdown")]
    [Rename("Countdown UI")]
    [SerializeField] TextMeshProUGUI u_CountDown;
    [Rename("Countdown Animator")]
    [SerializeField] Animator a_CountDown;

    [Header("Timer")]
    [Rename("Timer UI")]
    [SerializeField] TextMeshProUGUI u_Timer;
    [Rename("Timer Animator")]
    [SerializeField] Animator a_Timer;

    [Header("Cash")]
    [Rename("Cash Text UI")]
    [SerializeField] TextMeshProUGUI u_Cash;
    [Rename("Cash Animator")]
    [SerializeField] Animator a_Cash;

    [Header("Upset Customers")]
    [Rename("Upset Cus. Text UI")]
    [SerializeField] TextMeshProUGUI u_UpsetC;
    [Rename("Upset Cus. Animator")]
    [SerializeField] Animator a_UpsetC;

    [Header("Game Over Screen")]
    [Rename("Game Over Panel")]
    [SerializeField] GameObject c_GameOver;
    [Rename("Game Over Text UI")]
    [SerializeField] TextMeshProUGUI u_GameOver;
    [Rename("Game Over Animator")]
    [SerializeField] Animator a_GameOver; 

    [Header("References")]
    [Rename("Pause Menu")]
    [SerializeField] GameObject u_PauseMenu;

    string previousTime = "";
    int previousUpsetCount = 0;

    public void UpdateTimer(float newTime)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(newTime);
        u_Timer.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);

        if (newTime < 10.0f)
        {
            if (u_Timer.text != previousTime)
            {
                a_Timer.SetTrigger("Bounce");
            }

            previousTime = u_Timer.text;
        }
    }

    public void UpdateCash(int newCash)
    {
        u_Cash.text = newCash.ToString("C0", CultureInfo.CurrentCulture);
        a_Cash.SetTrigger("Bounce");
    }

    public void UpdateUpsetCustomers(int newAmount)
    {
        u_UpsetC.text = newAmount.ToString();

        if (a_UpsetC)
            if (newAmount != previousUpsetCount && previousUpsetCount == 0)
            {
                a_UpsetC.SetBool("IsIn", true);
            }
            else
            {
                a_UpsetC.SetTrigger("Bounce");
            }

        previousUpsetCount = newAmount;
    }

    public void UpdateCountDown(string newText, bool toShow = true)
    {
        if (newText == u_CountDown.text) return;

        u_CountDown.text = newText;

        if (newText.Length == 0)
        {
            u_CountDown.enabled = false;
            return;
        }
        
        if (toShow)
        {
            u_CountDown.enabled = true;
            a_CountDown.SetTrigger("Show");
        }
    }

    public GameObject GetPauseMenu()
    {
        return u_PauseMenu;
    }

    public void SetGameOver(string message)
    {
        c_GameOver.SetActive(true);

        // Change game over message colour depending on win or lose state
        switch (GameManager.GetInstance().GetGameState())
        {
            case GameState.Won:
                {
                    u_GameOver.color = Color.white;
                    break;
                }

            case GameState.Lost:
                {
                    u_GameOver.color = Color.red;
                    break;
                }
        }

        u_GameOver.text = message;
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
