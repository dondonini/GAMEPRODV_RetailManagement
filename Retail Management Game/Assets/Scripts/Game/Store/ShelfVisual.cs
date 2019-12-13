using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfVisual : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [SerializeField] float stockAmount = 1.0f;

    [SerializeField] List<GameObject> stockVisual = new List<GameObject>();

    private void OnValidate()
    {
        UpdateVisuals();
    }

    public float StockAmountPercentage
    {
        set
        {
            stockAmount = Mathf.Clamp01(value);
            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < stockVisual.Count; i++)
        {
            if (i < Mathf.FloorToInt(stockAmount * stockVisual.Count))
            {
                stockVisual[i].SetActive(true);
            }
            else
            {
                stockVisual[i].SetActive(false);
            }
        }
    }

    public int AddVisuals(Transform visual)
    {
        List<GameObject> foundGameObjects = new List<GameObject>();

        for (int i = 0; i < visual.childCount; i++)
        {
            foundGameObjects.Add(visual.GetChild(i).gameObject);
        }

        AddVisuals(foundGameObjects.ToArray());

        return visual.childCount;
    }

    public void AddVisuals(GameObject visual)
    {
        AddVisuals(new GameObject[] { visual });
    }

    public void AddVisuals(GameObject[] visuals)
    {
        for (int i = 0; i < visuals.Length; i++)
        {
            if (stockVisual.Contains(visuals[i]))
            {
                Debug.Log(visuals[i] + " already is a visual on this shelf!", this);
                continue;
            }

            stockVisual.Add(visuals[i]);

            // Disable all 
            visuals[i].GetComponent<StockItem>().enabled = false;
            Destroy(visuals[i].GetComponent<Rigidbody>());
            visuals[i].tag = "Untagged";
        }
    }
}
