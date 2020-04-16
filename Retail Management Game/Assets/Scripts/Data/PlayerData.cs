using System;
using UnityEngine;

public static class PlayerData
{
    private const string SAVE_FILE_EXTENSION = ".sls";
    private const string MONEY_KEY = "MONEY";

    public enum Slots
    {
        None,
        Slot1,
        Slot2,
        Slot3,
    };

    private static Slots _currentSlot;

    public class DataInfo
    {
        // Save data
        public int money;

        // Current game data
        public int profit;

        public DataInfo(int m)
        {
            money = m;
        }

        public DataInfo() 
        {
        }
    }

    public static DataInfo currentInfo;

    /************************************************************************/
    /* Slot Data Methods                                                    */
    /************************************************************************/

    public static bool SaveSlotData()
    {
        if (_currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return false;
        }

        string slotKey = _currentSlot.ToString();
        string saveFileName = slotKey + SAVE_FILE_EXTENSION;

        // Unpack data
        ES3.Save<int>(slotKey + MONEY_KEY, currentInfo.money, saveFileName);

        return true;
    }

    public static bool SaveSlotSegmentData(string key, bool value)
    {
        if (_currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return false;
        }

        string slotKey = _currentSlot.ToString();
        string saveFileName = slotKey + SAVE_FILE_EXTENSION;

        ES3.Save<bool>(slotKey + key, value, saveFileName);

        return true;
    }

    public static void LoadSlotData(Slots slot)
    {
        // Set the current slot
        switch (slot)
        {
            case Slots.Slot1:
                _currentSlot = Slots.Slot1;
                break;
            case Slots.Slot2:
                _currentSlot = Slots.Slot2;
                break;
            case Slots.Slot3:
                _currentSlot = Slots.Slot3;
                break;
            case Slots.None:
                _currentSlot = Slots.None;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }

        if (_currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return;
        }

        string slotKey = _currentSlot.ToString();
        string saveFileName = slotKey + SAVE_FILE_EXTENSION;

        DataInfo newData = new DataInfo();

        if (!ES3.FileExists(saveFileName))
        {
            // New slot!
            Debug.Log("Slot save not found! - Building new save...");
            newData.money = 0;
        }
        else
        {
            // Unpack data
            Debug.Log("Slot save found! - Loading data...");
            newData.money = ES3.Load<int>(slotKey + MONEY_KEY, saveFileName);
        }

        currentInfo = null;
        currentInfo = newData;

        SaveSlotData();

        if (currentInfo != null)
        {
            Debug.Log("Slot was successfully loaded!");
        }
    }

    public static bool LoadSlotSegmentData(string key)
    {
        if (_currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return false;
        }

        string slotKey = _currentSlot.ToString();
        string saveFileName = slotKey + SAVE_FILE_EXTENSION;

        return ES3.KeyExists(slotKey + key) && ES3.Load<bool>(slotKey + key, saveFileName);
    }

    public static DataInfo GetSlotInfo(Slots slot)
    {
        string slotKey = slot.ToString();
        string saveFileName = slotKey + SAVE_FILE_EXTENSION;

        if (!ES3.FileExists(saveFileName))
        {
            return null;
        }

        // Build data set
        DataInfo newData = new DataInfo
        {
            money = ES3.Load<int>(slotKey + MONEY_KEY, saveFileName),
        };

        return newData;
    }

    public static void DeleteSlotData(Slots slot)
    {
        string saveFileName;

        // Set the current slot
        switch (slot)
        {
            case Slots.Slot1:
                saveFileName = Slots.Slot1 + SAVE_FILE_EXTENSION;
                break;
            case Slots.Slot2:
                saveFileName = Slots.Slot2 + SAVE_FILE_EXTENSION;
                break;
            case Slots.Slot3:
                saveFileName = Slots.Slot3 + SAVE_FILE_EXTENSION;
                break;
            case Slots.None:
                saveFileName = Slots.None.ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }

        // Check slot if it exists and delete it
        if (ES3.FileExists(saveFileName))
        {
            ES3.DeleteFile(saveFileName);
        }
    }

    public static void UnloadData()
    {
        currentInfo = null;
    }

    public static bool IsASlotLoaded()
    {
        return currentInfo != null;
    }

    public static DataInfo GetCurrentDataInfo()
    {
        return currentInfo;
    }
}
