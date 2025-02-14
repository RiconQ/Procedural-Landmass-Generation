using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private enum EDrawMode
    {
        NoiseMap,
        ColorMap
    }

    [Header("Noise")]
    [SerializeField] private int m_mapWidth;
    [SerializeField] private int m_mapHeight;
    [SerializeField] private float m_noiseScale;

    [SerializeField] private int m_octaves;
    [Range(0, 1)]
    [SerializeField] private float m_persistance;
    [SerializeField] private float m_lacunarity;

    [SerializeField] private int m_seed;
    [SerializeField] private Vector2 m_offset;

    [Space, Header("Terrain")]
    [SerializeField] private TerrainType[] m_regions;

    [Space, Header("Editor")]
    [SerializeField] private bool m_autoUpdate = true;
    [SerializeField] private EDrawMode m_drawMode;
    public bool AutoUpdate => m_autoUpdate;

    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(m_mapWidth, m_mapHeight, m_seed, m_noiseScale, m_octaves, m_persistance, m_lacunarity, m_offset);

        var colorMap = new Color[m_mapWidth * m_mapHeight];
        for (int y = 0; y < m_mapHeight; y++)
        {
            for (int x = 0; x < m_mapWidth; x++)
            {
                var currentHeight = noiseMap[x, y];
                for (int i = 0; i < m_regions.Length; i++)
                {
                    if (currentHeight <= m_regions[i].height)
                    {
                        colorMap[y * m_mapWidth + x] = m_regions[i].color;

                        break;
                    }
                }
            }
        }

        var display = FindObjectOfType<MapDisplay>();
        if (m_drawMode == EDrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(m_drawMode == EDrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, m_mapWidth, m_mapHeight));
        }
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
        if (m_lacunarity < 1)
        {
            m_lacunarity = 1;
        }
        if (m_octaves < 0)
        {
            m_octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
