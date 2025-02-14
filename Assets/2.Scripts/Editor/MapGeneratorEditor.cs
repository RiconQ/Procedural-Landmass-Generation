using UnityEditor;
using UnityEngine;

/// <summary>
/// MapGenerator 스크립트의 Inspector에 표시되는 커스텀 에디터
/// </summary>

// Attribute를 추가하여 MapGenerator스크립트 커스텀 에디터 추가
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    /// <summary>
    /// Unity가 Inspector 그릴때 호출되는 메서드
    /// </summary>
    public override void OnInspectorGUI()
    {
        // target은 현재 선택된 오브젝트의 컴포넌트 (MapGenerator)
        MapGenerator mapGen = (MapGenerator)target;
    
        //Unity 기본 인스펙터를 그려줌(스크립트 기본 필드)
        //Inspector의 값이 변경될때마다 호출
        if(DrawDefaultInspector())
        {
            if(mapGen.AutoUpdate)
            {
                mapGen.GenerateMap();
            }
        }

        // Generate 버튼추가, 클릭시 mapGen의 GenerateMap() 실행
        if(GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}
