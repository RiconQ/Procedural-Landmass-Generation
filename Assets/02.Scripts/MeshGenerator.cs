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
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        var heightCurve = new AnimationCurve(_heightCurve.keys);
        var meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        var borderedSize = heightMap.GetLength(0);
        var meshSize = borderedSize - 2 * meshSimplificationIncrement;
        var meshSizeUnsimplified = borderedSize - 2;

        var topLeftX = (meshSizeUnsimplified - 1) / -2f;
        var topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        var verticePerLine = ((meshSize - 1) / meshSimplificationIncrement) + 1;

        var meshData = new MeshData(verticePerLine);

        var vertexIndicesMap = new int[borderedSize, borderedSize];
        var meshVertexIndex = 0;
        var borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                var isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }


        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                var vertexIndex = vertexIndicesMap[x, y];
                var percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                var height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                var vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    var a = vertexIndicesMap[x, y];
                    var b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    var c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    var d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }
        meshData.BakedNormals();
        return meshData;
    }
}


public class MeshData
{
    private Vector3[] m_vertices;
    private int[] m_triangles;
    private Vector2[] m_uvs;
    private Vector3[] m_bakedNormals;

    private Vector3[] m_borderVertices;
    private int[] m_borderTriangles;

    private int m_triangleIndex;
    private int m_borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        m_vertices = new Vector3[verticesPerLine * verticesPerLine];
        m_uvs = new Vector2[verticesPerLine * verticesPerLine];
        m_triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        m_borderVertices = new Vector3[verticesPerLine * 4 + 4];
        m_borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            m_borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            m_vertices[vertexIndex] = vertexPosition;
            m_uvs[vertexIndex] = uv;
        }
    }

    //MeshData에 삼각형 추가
    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            m_borderTriangles[m_borderTriangleIndex] = a;
            m_borderTriangles[m_borderTriangleIndex + 1] = b;
            m_borderTriangles[m_borderTriangleIndex + 2] = c;

            m_borderTriangleIndex += 3;
        }
        else
        {
            m_triangles[m_triangleIndex] = a;
            m_triangles[m_triangleIndex + 1] = b;
            m_triangles[m_triangleIndex + 2] = c;

            m_triangleIndex += 3;
        }
    }

    private Vector3[] CalculateNormals()
    {
        var vertexNormals = new Vector3[m_vertices.Length];
        var triangleCount = m_triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            var normalTriangleIndex = i * 3;
            var vertexIndexA = m_triangles[normalTriangleIndex];
            var vertexIndexB = m_triangles[normalTriangleIndex + 1];
            var vertexIndexC = m_triangles[normalTriangleIndex + 2];

            var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        var borderTriangleCount = m_borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            var normalTriangleIndex = i * 3;
            var vertexIndexA = m_borderTriangles[normalTriangleIndex];
            var vertexIndexB = m_borderTriangles[normalTriangleIndex + 1];
            var vertexIndexC = m_borderTriangles[normalTriangleIndex + 2];

            var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }
        return vertexNormals;
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        var pointA = (indexA < 0) ? m_borderVertices[-indexA - 1] : m_vertices[indexA];
        var pointB = (indexB < 0) ? m_borderVertices[-indexB - 1] : m_vertices[indexB];
        var pointC = (indexC < 0) ? m_borderVertices[-indexC - 1] : m_vertices[indexC];

        var sideAB = pointB - pointA;
        var sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC);
    }
    
    public void BakedNormals()
    {
        m_bakedNormals = CalculateNormals();
    }

    public Mesh CreateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = m_vertices;
        mesh.triangles = m_triangles;
        mesh.uv = m_uvs;
        mesh.normals = m_bakedNormals;
        return mesh;
    }
}