using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float SCALE = 5f;
    const float VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = 25f;
    const float SQR_VIEW_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE;

    [SerializeField] private LODInfo[] m_detailLevel;
    public static float m_maxViewDistance;

    [SerializeField] private Transform m_viewer;
    public static Vector2 m_viewerPosition;
    private Vector2 m_viewerPositionOld;

    private static MapGenerator m_mapGenerator;
    [SerializeField] private Material m_mapMaterial;

    private int m_chunkSize;
    private int m_chunkVisibleInViewDistance;

    private Dictionary<Vector2, TerrainChunk> m_terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> m_terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        m_mapGenerator = FindObjectOfType<MapGenerator>();

        m_maxViewDistance = m_detailLevel[m_detailLevel.Length - 1].m_visibleDistanceThreshold;
        m_chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        m_chunkVisibleInViewDistance = Mathf.RoundToInt(m_maxViewDistance / m_chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        m_viewerPosition = new Vector2(m_viewer.position.x, m_viewer.position.z) / SCALE;

        if ((m_viewerPositionOld - m_viewerPosition).sqrMagnitude > SQR_VIEW_MOVE_THRESHOLD_FOR_CHUNK_UPDATE)
        {
            m_viewerPositionOld = m_viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < m_terrainChunkVisibleLastUpdate.Count; i++)
        {
            m_terrainChunkVisibleLastUpdate[i].SetVisible(false);
        }
        m_terrainChunkVisibleLastUpdate.Clear();

        var currentChunkCoordX = Mathf.RoundToInt(m_viewerPosition.x / m_chunkSize);
        var currentChunkCoordY = Mathf.RoundToInt(m_viewerPosition.y / m_chunkSize);

        for (int yOffset = -m_chunkVisibleInViewDistance; yOffset <= m_chunkVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -m_chunkVisibleInViewDistance; xOffset <= m_chunkVisibleInViewDistance; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (m_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    //해당 Terrain이 이미 Dictionary에 있다면(이미 스폰된적 있는 Terrain이라면
                    //저장되어 있는 Terrain 불러오기
                    m_terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    //저장된적 없는 Terrain이라면 (처음보는 Terrain이라면_
                    //Dictionary에 추가하기
                    m_terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, m_chunkSize, m_detailLevel, transform, m_mapMaterial));
                }
            }
        }
    }
    public class TerrainChunk
    {
        private GameObject m_meshObject;
        private Vector2 m_position;
        private Bounds m_bounds;

        private MeshRenderer m_meshRenderer;
        private MeshFilter m_meshFilter;

        public bool IsVisible => m_meshObject.activeSelf;

        private LODInfo[] m_detailLevels;
        private LODMesh[] m_lodMeshes;

        private MapData m_mapData;
        private bool m_mapDataReceived;
        private int m_previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            m_detailLevels = detailLevels;

            m_position = coord * size;
            m_bounds = new Bounds(m_position, Vector2.one * size);
            var positionVec3 = new Vector3(m_position.x, 0, m_position.y);

            //m_meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_meshObject = new GameObject("Terrain Chunk");
            m_meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            m_meshRenderer.material = material;

            m_meshObject.transform.position = positionVec3 * SCALE;
            m_meshObject.transform.parent = parent;
            m_meshObject.transform.localScale = Vector3.one * SCALE;

            SetVisible(false);
            m_lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                m_lodMeshes[i] = new LODMesh(detailLevels[i].m_lod, UpdateTerrainChunk);
            }

            m_mapGenerator.RequestMapData(m_position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            //m_mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            m_mapData = mapData;
            m_mapDataReceived = true;

            var texture = TextureGenerator.TextureFromColorMap(mapData.m_colorMap, MapGenerator.MAP_CHUNK_SIZE, MapGenerator.MAP_CHUNK_SIZE);
            m_meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            //m_meshFilter.mesh = meshData.CreateMesh();
        }

        /// <summary>
        /// Terrain의 상태를 Update하기 위한 메서드
        /// </summary>
        public void UpdateTerrainChunk()
        {
            if (m_mapDataReceived)
            {

                var viewerDistFromNearestEdge = Mathf.Sqrt(m_bounds.SqrDistance(m_viewerPosition));
                var isVisible = viewerDistFromNearestEdge <= m_maxViewDistance;

                if (isVisible)
                {
                    var lodIndex = 0;

                    for (int i = 0; i < m_detailLevels.Length - 1; i++)
                    {
                        if (viewerDistFromNearestEdge > m_detailLevels[i].m_visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != m_previousLODIndex)
                    {
                        var lodMesh = m_lodMeshes[lodIndex];
                        if (lodMesh.m_hasMesh)
                        {
                            m_previousLODIndex = lodIndex;
                            m_meshFilter.mesh = lodMesh.m_mesh;
                        }
                        else if (!lodMesh.m_hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(m_mapData);
                        }
                    }

                    m_terrainChunkVisibleLastUpdate.Add(this);
                }

                SetVisible(isVisible);
            }
        }

        public void SetVisible(bool visible)
        {
            m_meshObject.SetActive(visible);
        }
    }

    private class LODMesh
    {
        public Mesh m_mesh;
        public bool m_hasRequestedMesh;
        public bool m_hasMesh;
        private int m_lod;

        private Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            m_lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            m_mesh = meshData.CreateMesh();
            m_hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            m_hasRequestedMesh = true;
            m_mapGenerator.RequestMeshData(mapData, m_lod, OnMeshDataReceived);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        public int m_lod;
        public float m_visibleDistanceThreshold;
    }
}
