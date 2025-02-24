using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float MAX_VIEW_DISTANCE = 450;

    [SerializeField] private Transform m_viewer;
    public static Vector2 viewrPosition;
    private static MapGenerator m_mapGenerator;
    [SerializeField] private Material m_mapMaterial;

    private int m_chunkSize;
    private int m_chunkVisibleInViewDistance;

    private Dictionary<Vector2, TerrainChunk> m_terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> m_terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        m_mapGenerator = FindObjectOfType<MapGenerator>();

        m_chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        m_chunkVisibleInViewDistance = Mathf.RoundToInt(MAX_VIEW_DISTANCE / m_chunkSize);
    }

    private void Update()
    {
        viewrPosition = new Vector2(m_viewer.position.x, m_viewer.position.z);

        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        for(int i = 0; i < m_terrainChunkVisibleLastUpdate.Count;i++ )
        {
            m_terrainChunkVisibleLastUpdate[i].SetVisible(false);
        }
        m_terrainChunkVisibleLastUpdate.Clear();

        var currentChunkCoordX = Mathf.RoundToInt(viewrPosition.x / m_chunkSize);
        var currentChunkCoordY = Mathf.RoundToInt(viewrPosition.y / m_chunkSize);

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
                    if (m_terrainChunkDictionary[viewedChunkCoord].IsVisible)
                    {
                        m_terrainChunkVisibleLastUpdate.Add(m_terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    //저장된적 없는 Terrain이라면 (처음보는 Terrain이라면_
                    //Dictionary에 추가하기
                    m_terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, m_chunkSize, transform, m_mapMaterial));
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

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            m_position = coord * size;
            m_bounds = new Bounds(m_position, Vector2.one * size);
            var positionVec3 = new Vector3(m_position.x, 0, m_position.y);

            //m_meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_meshObject = new GameObject("Terrain Chunk");
            m_meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            m_meshRenderer.material = material;

            m_meshObject.transform.position = positionVec3;
            m_meshObject.transform.parent = parent;

            SetVisible(false);
            m_mapGenerator.RequestMapData(OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            m_mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            m_meshFilter.mesh = meshData.CreateMesh();
        }

        /// <summary>
        /// Terrain의 상태를 Update하기 위한 메서드
        /// </summary>
        public void UpdateTerrainChunk()
        {
            var viewerDistFromNearestEdge = Mathf.Sqrt(m_bounds.SqrDistance(viewrPosition));
            var isVisible = viewerDistFromNearestEdge <= MAX_VIEW_DISTANCE;
            SetVisible(isVisible);
        }

        public void SetVisible(bool visible)
        {
            m_meshObject.SetActive(visible);
        }
    }
}
