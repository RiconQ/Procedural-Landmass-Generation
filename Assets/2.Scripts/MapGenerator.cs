using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int m_mapWidth;
    [SerializeField] private int m_mapHeight;
    [SerializeField] private float m_noiseScale;

    [SerializeField] private int m_octaves;
    [Range(0, 1)]
    [SerializeField] private float m_persistance;
    [SerializeField] private float m_lacunarity;

    [SerializeField] private int m_seed;
    [SerializeField] private Vector2 m_offset;

    [Space, Header("Editor")]
    [SerializeField] private bool m_autoUpdate = true;
    public bool AutoUpdate => m_autoUpdate;

    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(m_mapWidth, m_mapHeight, m_seed, m_noiseScale, m_octaves, m_persistance, m_lacunarity, m_offset);

        var display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    private void OnValidate()
    {
        if (m_mapWidth < 1)
        {
            m_mapWidth = 1;
        }
        if (m_mapHeight < 1)
        {
            m_mapHeight = 1;
        }
        if(m_lacunarity < 1)
        {
            m_lacunarity = 1;
        }
        if(m_octaves < 0)
        {
            m_octaves = 0;
        }
    }
}
