using UnityEngine;

/// <summary>
/// heightMap�� �������� Vertex�� ����
/// Vertex�� Triangle�� �����ؼ� Mesh�� �����
///     Unity���� �޽ø� ���� �����Ϸ��� -> Vertex�� ��ġ�ϰ� Triangle�� �����ؾ��Ѵ�
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// heightMap�� �������� 3D ������ ����
    /// </summary>
    /// <param name="heightMap">�ش� ��ġ�� ����(Terrain�� ������ ������)</param>
    /// <returns>Mesh�� ��, �ﰢ��, UV�����͸� ��� Class</returns>
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var topLeftX = (width - 1) / -2f;
        var topLeftZ = (height - 1) / 2f;


        var meshData = new MeshData(width, height);
        var vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.m_vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.m_uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
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

    //MeshData�� �ﰢ�� �߰�
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