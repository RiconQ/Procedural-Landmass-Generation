using UnityEditor;
using UnityEngine;

/// <summary>
/// MapGenerator ��ũ��Ʈ�� Inspector�� ǥ�õǴ� Ŀ���� ������
/// </summary>

// Attribute�� �߰��Ͽ� MapGenerator��ũ��Ʈ Ŀ���� ������ �߰�
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    /// <summary>
    /// Unity�� Inspector �׸��� ȣ��Ǵ� �޼���
    /// </summary>
    public override void OnInspectorGUI()
    {
        // target�� ���� ���õ� ������Ʈ�� ������Ʈ (MapGenerator)
        MapGenerator mapGen = (MapGenerator)target;
    
        //Unity �⺻ �ν����͸� �׷���(��ũ��Ʈ �⺻ �ʵ�)
        //Inspector�� ���� ����ɶ����� ȣ��
        if(DrawDefaultInspector())
        {
            if(mapGen.AutoUpdate)
            {
                mapGen.GenerateMap();
            }
        }

        // Generate ��ư�߰�, Ŭ���� mapGen�� GenerateMap() ����
        if(GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}
