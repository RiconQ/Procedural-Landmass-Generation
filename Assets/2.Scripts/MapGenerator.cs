using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

public class MapGenerator : MonoBehaviour
{
    public const int MAP_CHUNK_SIZE = 241;

    private enum EDrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

    #region Inspector Variables
    [Header("Noise")]
    [SerializeField] private float m_noiseScale;

    [SerializeField] private int m_octaves;
    [Range(0, 1)]
    [SerializeField] private float m_persistance;
    [SerializeField] private float m_lacunarity;

    [SerializeField] private int m_seed;
    [SerializeField] private Vector2 m_offset;
    [SerializeField] private Noise.ENormalizeMode m_normalizeMode;

    [Header("LOD")]
    [Tooltip("숫자가 작아질수록 디테일업")]
    [Range(0, 6)]
    [SerializeField] private int m_editorPreviewLOD;

    [Space, Header("Terrain")]
    [SerializeField] private TerrainType[] m_regions;

    [Space, Header("Mesh")]
    [SerializeField] private float m_meshHeightMultiplier;
    [SerializeField] private AnimationCurve m_meshHeightCurve;

    [Space, Header("Editor")]
    [SerializeField] private bool m_autoUpdate = true;
    [SerializeField] private EDrawMode m_drawMode;
    public bool AutoUpdate => m_autoUpdate;
    #endregion


    private Queue<MapThreadInfo<MapData>> m_mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> m_meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


    public void DrawMapInEditor()
    {
        var mapData = GenerateMapData(Vector2.zero);

        var display = FindObjectOfType<MapDisplay>();
        if (m_drawMode == EDrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.m_heightMap));
        }
        else if (m_drawMode == EDrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.m_colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
        }
        else if (m_drawMode == EDrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.m_heightMap, m_meshHeightMultiplier, m_meshHeightCurve, m_editorPreviewLOD),
                TextureGenerator.TextureFromColorMap(mapData.m_colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
        }
    }


    #region Thread 사용

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = () => MapDataThread(center, callback);

        new Thread(threadStart).Start();
    }
    private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        var mapData = GenerateMapData(center);
        lock (m_mapDataThreadInfoQueue)
        {
            m_mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = () => MeshDataThread(mapData, lod, callback);

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        var meshData = MeshGenerator.GenerateTerrainMesh(mapData.m_heightMap, m_meshHeightMultiplier, m_meshHeightCurve, lod);
        lock (m_meshDataThreadInfoQueue)
        {
            m_meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (m_mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < m_mapDataThreadInfoQueue.Count; i++)
            {
                var threadInfo = m_mapDataThreadInfoQueue.Dequeue();
                threadInfo.m_callback(threadInfo.m_parameter);
            }
        }

        if (m_meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < m_meshDataThreadInfoQueue.Count; i++)
            {
                var threadInfo = m_meshDataThreadInfoQueue.Dequeue();
                threadInfo.m_callback(threadInfo.m_parameter);
            }
        }
    }
    #endregion

    #region Unitask 사용
    /*
    public async UniTask<MapData> RequestMapData()
    {
        return await UniTask.RunOnThreadPool(() => GenerateMapData());
    }

    public async UniTask<MeshData> RequestMeshData(MapData mapData)
    {
        return await UniTask.RunOnThreadPool(() =>
        MeshGenerator.GenerateTerrainMesh(mapData.m_heightMap, m_meshHeightMultiplier, m_meshHeightCurve, m_levelOfDetail);
        );
    }*/

    #endregion

    private MapData GenerateMapData(Vector2 center)
    {
        var noiseMap = Noise.GenerateNoiseMap(
            MAP_CHUNK_SIZE, MAP_CHUNK_SIZE, m_seed, m_noiseScale, m_octaves, m_persistance, m_lacunarity, center + m_offset, m_normalizeMode);

        //Color Map 설정
        var colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
        for (int y = 0; y < MAP_CHUNK_SIZE; y++)
        {
            for (int x = 0; x < MAP_CHUNK_SIZE; x++)
            {
                var currentHeight = noiseMap[x, y];
                for (int i = 0; i < m_regions.Length; i++)
                {
                    if (currentHeight >= m_regions[i].height)
                    {
                        colorMap[y * MAP_CHUNK_SIZE + x] = m_regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
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

    private struct MapThreadInfo<T>
    {
        public readonly Action<T> m_callback;
        public readonly T m_parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            m_callback = callback;
            m_parameter = parameter;
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

public struct MapData
{
    public readonly float[,] m_heightMap;
    public readonly Color[] m_colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.m_heightMap = heightMap;
        this.m_colorMap = colorMap;
    }
}