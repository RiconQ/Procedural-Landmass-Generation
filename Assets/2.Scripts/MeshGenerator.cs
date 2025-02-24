using UnityEngine;

/// <summary>
/// heightMap을 바탕으로 Vertex를 생성
/// Vertex를 Triangle로 연결해서 Mesh를 만든다
///     Unity에서 메시를 직접 생성하려면 -> Vertex를 배치하고 Triangle로 연결해야한다
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// heightMap을 바탕으로 3D 지형을 생성
    /// </summary>
    /// <param name="heightMap">해당 위치의 높이(Terrain의 높낮이 데이터)</param>
    /// <returns>Mesh의 점, 삼각형, UV데이터를 담는 Class</returns>
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        var tmpHeightCurve = new AnimationCurve(heightCurve.keys);
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var topLeftX = (width - 1) / -2f;
        var topLeftZ = (height - 1) / 2f;

        var meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        var verticePerLine = ((width - 1) / meshSimplificationIncrement) + 1;


        var meshData = new MeshData(verticePerLine, verticePerLine);
        var vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {

                meshData.m_vertices[vertexIndex] =
                    new Vector3(topLeftX + x, tmpHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);

                meshData.m_uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticePerLine + 1, vertexIndex + verticePerLine);
                    meshData.AddTriangle(vertexIndex + verticePerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return meshData;
    }
}


public class MeshData
{
    public Vector3[] m_vertices;
    public int[] m_triangles;
    public Vector2[] m_uvs;

    private int m_triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        m_vertices = new Vector3[meshWidth * meshHeight];
        m_uvs = new Vector2[meshWidth * meshHeight];
        m_triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    //MeshData에 삼각형 추가
    public void AddTriangle(int a, int b, int c)
    {
        m_triangles[m_triangleIndex] = a;
        m_triangles[m_triangleIndex + 1] = b;
        m_triangles[m_triangleIndex + 2] = c;

        m_triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = m_vertices;
        mesh.triangles = m_triangles;
        mesh.uv = m_uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}