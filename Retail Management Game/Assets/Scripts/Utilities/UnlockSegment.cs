using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnlockSegment
{
    public bool m_enableSegment = false;
    public string m_segmentKey = "Segment";
    public string[] m_incompatibleKeys = System.Array.Empty<string>();
    public GameObject[] m_segments;

    public bool CompareKey(string newKey)
    {
        return newKey == m_segmentKey;
    }

    public void SetKey(string newKey)
    {
        m_segmentKey = newKey;
    }

    public bool IsEnabled()
    {
        return m_enableSegment;
    }

    public bool enableSegment
    {
        get
        {
            return m_enableSegment;
        }
        set
        {
            m_enableSegment = value;
            SetupSegments(m_enableSegment);
        }
    }

    void SetupSegments(bool isEnabled)
    {
        for (int i = 0; i < m_segments.Length; i++)
        {
            if (isEnabled)
                m_segments[i].SetActive(true);
            else
                m_segments[i].SetActive(false);
        }
    }

    public void UpdateSegment()
    {
        for (int i = 0; i < m_segments.Length; i++)
        {
            if (m_enableSegment)
                m_segments[i].SetActive(true);
            else
                m_segments[i].SetActive(false);
        }
    }
}
