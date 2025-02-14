using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int m_mapWidth;
    [SerializeField] private int m_mapHeight;
    [SerializeField] private float m_noiseScale;

    [Space, Header("Editor")]
    [SerializeField] private bool m_autoUpdate = true;
    public bool AutoUpdate => m_autoUpdate;

    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(m_mapWidth, m_mapHeight, m_noiseScale);

        var display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}
