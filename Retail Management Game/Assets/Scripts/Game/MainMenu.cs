using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Settings")]
    [SerializeField] GameObject m_Main = null;

    [Header("Confirmation Menu Settings")]
    [SerializeField] GameObject m_Confirm = null;

    [Header("Load Menu Settings")]
    [SerializeField] GameObject m_Load = null;
    [SerializeField] string loadSlotName = "Load Slot ";
    [SerializeField] TextMeshProUGUI m_Slot1Text = null;
    [SerializeField] TextMeshProUGUI m_Slot2Text = null;
    [SerializeField] TextMeshProUGUI m_Slot3Text = null;

    [Header("Level Menu Settings")]
    [SerializeField] GameObject m_LevelMenu = null;
    [SerializeField] TextMeshProUGUI m_slotMoneyText = null;

    private void Start()
    {
        if (PlayerData.IsASlotLoaded())
        {
            ShowLevelMenu();
        }
    }

    private void Update()
    {
        if (PlayerData.IsASlotLoaded())
        {
            m_slotMoneyText.text = "Money: $" + PlayerData.GetCurrentDataInfo().money.ToString();
        }
        else
        {
            m_slotMoneyText.text = "";
        }
    }

    // UI Button Presses

    public void ShowMainMenu()
    {
        m_Main.SetActive(true);
        m_Confirm.SetActive(false);
        m_Load.SetActive(false);
        m_LevelMenu.SetActive(false);
    }

    public void ShowConfirmMenu()
    {
        m_Main.SetActive(false);
        m_Confirm.SetActive(true);
        m_Load.SetActive(false);
        m_LevelMenu.SetActive(false);
    }

    public void ShowLoadMenu()
    {
        // Update slot info
        UpdateLoadSlotInfo();

        m_Main.SetActive(false);
        m_Confirm.SetActive(false);
        m_Load.SetActive(true);
        m_LevelMenu.SetActive(false);
    }

    public void ShowLevelMenu()
    {
        m_Main.SetActive(false);
        m_Confirm.SetActive(false);
        m_Load.SetActive(false);
        m_LevelMenu.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);

#else
        Application.Quit();

#endif
    }

    // Load Menu Info

    void UpdateLoadSlotInfo()
    {
        PlayerData.DataInfo slot1 = PlayerData.GetSlotInfo(PlayerData.Slots.Slot1);
        PlayerData.DataInfo slot2 = PlayerData.GetSlotInfo(PlayerData.Slots.Slot2);
        PlayerData.DataInfo slot3 = PlayerData.GetSlotInfo(PlayerData.Slots.Slot3);

        int slot1Money = 0;
        int slot2Money = 0;
        int slot3Money = 0;

        if (slot1 != null)
        {
            slot1Money = slot1.money;
        }

        if (slot2 != null)
        {
            slot2Money = slot2.money;
        }

        if (slot3 != null)
        {
            slot3Money = slot3.money;
        }

        m_Slot1Text.text = loadSlotName + " 1 \t$" + slot1Money.ToString();
        m_Slot2Text.text = loadSlotName + " 2 \t$" + slot2Money.ToString();
        m_Slot3Text.text = loadSlotName + " 3 \t$" + slot3Money.ToString();
    }

    public void LoadSlot1()
    {
        LoadSlot(PlayerData.Slots.Slot1);
    }

    public void LoadSlot2()
    {
        LoadSlot(PlayerData.Slots.Slot2);
    }

    public void LoadSlot3()
    {
        LoadSlot(PlayerData.Slots.Slot3);
    }

    void LoadSlot(PlayerData.Slots slot)
    {
        PlayerData.LoadSlotData(slot);
    }

    public void LoadLevel1()
    {
        SceneManager.LoadSceneAsync("Store1");
    }
}
