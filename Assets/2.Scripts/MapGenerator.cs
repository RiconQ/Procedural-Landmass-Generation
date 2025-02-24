using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    const int MAP_CHUNK_SIZE = 241;

    private enum EDrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

    [Header("Noise")]
    [SerializeField] private float m_noiseScale;

    [SerializeField] private int m_octaves;
    [Range(0, 1)]
    [SerializeField] private float m_persistance;
    [SerializeField] private float m_lacunarity;

    [SerializeField] private int m_seed;
    [SerializeField] private Vector2 m_offset;

    [Header("LOD")]
    [Tooltip("숫자가 작아질수록 디테일업")]
    [Range(0, 6)]
    [SerializeField] private int m_levelOfDetail;

    [Space, Header("Terrain")]
    [SerializeField] private TerrainType[] m_regions;

    [Space, Header("Mesh")]
    [SerializeField] private float m_meshHeightMultiplier;
    [SerializeField] private AnimationCurve m_meshHeightCurve;

    [Space, Header("Editor")]
    [SerializeField] private bool m_autoUpdate = true;
    [SerializeField] private EDrawMode m_drawMode;
    public bool AutoUpdate => m_autoUpdate;

    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(MAP_CHUNK_SIZE, MAP_CHUNK_SIZE, m_seed, m_noiseScale, m_octaves, m_persistance, m_lacunarity, m_offset);

        //Color Map 설정
        var colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
        for (int y = 0; y < MAP_CHUNK_SIZE; y++)
        {
            for (int x = 0; x < MAP_CHUNK_SIZE; x++)
            {
                var currentHeight = noiseMap[x, y];
                for (int i = 0; i < m_regions.Length; i++)
                {
                    if (currentHeight <= m_regions[i].height)
                    {
                        colorMap[y * MAP_CHUNK_SIZE + x] = m_regions[i].color;

                        break;
                    }
                }
            }
        }

        // Mode별 Draw
        var display = FindObjectOfType<MapDisplay>();
        if (m_drawMode == EDrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(m_drawMode == EDrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
        }
        else if(m_drawMode == EDrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, m_meshHeightMultiplier, m_meshHeightCurve, m_levelOfDetail), 
                TextureGenerator.TextureFromColorMap(colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
        }
    }

    private void OnValidate()
    {
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
