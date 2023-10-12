#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class makeRoadWindow : EditorWindow
{
    [MenuItem("Window/makeRoad")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<makeRoadWindow>();
    }
    /// <summary>
    /// Sprites懶得再重新拉
    /// </summary>
    GameObject LinkData;
    /// <summary>
    /// 街道生成座標
    /// </summary>
    Vector2 streatPos;
    /// <summary>
    /// 街道資料(總長、總寬、人行道單邊寬)
    /// </summary>
    Vector3Int streatStatus;
    /// <summary>
    /// 圖層層級
    /// </summary>
    int roadLayer;
    /// <summary>
    /// 是否要分雙向道
    /// </summary>
    bool _twoWayTraffic;
    /// <summary>
    /// 道路名稱
    /// </summary>
    string _streatName;
    /// <summary>
    /// 顯示在Windows上面的內容
    /// </summary>
    private void OnGUI()
    {
        //暫時懶得用完整功能，直接先連結到GameObject讀取上面的資料
        GUILayout.Label("道路生成", EditorStyles.boldLabel);
        LinkData = (GameObject)EditorGUILayout.ObjectField("連結資料", LinkData, typeof(GameObject), true);

        streatPos = EditorGUILayout.Vector2Field("生成街道座標", streatPos, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
        GUILayout.Label("", EditorStyles.boldLabel);
        streatStatus = EditorGUILayout.Vector3IntField("街道資料(總長、總寬、人行道單邊寬)", streatStatus, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
        GUILayout.Label("", EditorStyles.boldLabel);
        GUILayout.Label("圖層層級", EditorStyles.boldLabel);
        roadLayer = EditorGUILayout.IntSlider(roadLayer,-5, 0, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        GUILayout.Label("", EditorStyles.boldLabel);
        _twoWayTraffic = EditorGUILayout.Toggle("是否要分雙向道", _twoWayTraffic);
        GUILayout.Label("", EditorStyles.boldLabel);
        _streatName = EditorGUILayout.TextField("道路名稱", _streatName);

        if (GUILayout.Button("makeRoad"))
        {
            LinkData.GetComponent<MakeCivilian>().GUIPushButton(streatPos, streatStatus, roadLayer, _twoWayTraffic, _streatName);
        }
    }
}
#endif