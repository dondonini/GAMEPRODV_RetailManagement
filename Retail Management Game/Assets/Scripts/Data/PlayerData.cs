using UnityEngine;

public static class PlayerData
{
    const string saveFileExtension = ".sls";
    const string moneyKey = "MONEY";

    public enum Slots
    {
        None,
        Slot1,
        Slot2,
        Slot3,
    };

    public static Slots currentSlot;

    public class DataInfo
    {
        // Save data
        public int money;

        // Current game data
        public int profit;

        public DataInfo(int _m)
        {
            money = _m;
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
        if (currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return false;
        }

        string slotKey = currentSlot.ToString();
        string saveFileName = slotKey + saveFileExtension;

        // Unpack data
        ES3.Save<int>(slotKey + moneyKey, currentInfo.money, saveFileName);

        return true;
    }

    public static bool SaveSlotSegmentData(string key, bool value)
    {
        if (currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return false;
        }

        string slotKey = currentSlot.ToString();
        string saveFileName = slotKey + saveFileExtension;

        ES3.Save<bool>(slotKey + key, value, saveFileName);

        return true;
    }

    public static void LoadSlotData(Slots _slot)
    {
        // Set the current slot
        switch (_slot)
        {
            case Slots.Slot1:
                currentSlot = Slots.Slot1;
                break;
            case Slots.Slot2:
                currentSlot = Slots.Slot2;
                break;
            case Slots.Slot3:
                currentSlot = Slots.Slot3;
                break;
            case Slots.None:
                currentSlot = Slots.None;
                break;
        }

        if (currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return;
        }

        string key = currentSlot.ToString();
        string saveFileName = key + saveFileExtension;

        DataInfo newData = new DataInfo();

        if (!ES3.KeyExists(key))
        {
            // New slot!

            newData.money = 0;
        }
        else
        {
            // Unpack data

            newData.money = ES3.Load<int>(key + moneyKey, saveFileName);
        }

        currentInfo = null;
        currentInfo = newData;
    }

    public static bool LoadSlotSegmentData(string key)
    {
        if (currentSlot == Slots.None)
        {
            Debug.LogError("No slot has been selected!");
            return false;
        }

        string slotKey = currentSlot.ToString();
        string saveFileName = slotKey + saveFileExtension;

        return ES3.Load<bool>(slotKey + key, saveFileName);
    }

    public static DataInfo GetSlotInfo(Slots _slot)
    {
        string slotKey = _slot.ToString();
        string saveFileName = slotKey + saveFileExtension;

        if (ES3.KeyExists(slotKey))
        {
            return null;
        }

        // Build data set
        DataInfo newData = new DataInfo
        {
            money = ES3.Load<int>(slotKey + moneyKey, saveFileName),
        };

        return newData;
    }

    public static void DeleteSlotData(Slots _slot)
    {
        string saveFileName = Slots.None.ToString();

        // Set the current slot
        switch (_slot)
        {
            case Slots.Slot1:
                saveFileName = Slots.Slot1.ToString() + saveFileExtension;
                break;
            case Slots.Slot2:
                saveFileName = Slots.Slot2.ToString() + saveFileExtension;
                break;
            case Slots.Slot3:
                saveFileName = Slots.Slot3.ToString() + saveFileExtension;
                break;
            case Slots.None:
                saveFileName = Slots.None.ToString();
                break;
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
}
