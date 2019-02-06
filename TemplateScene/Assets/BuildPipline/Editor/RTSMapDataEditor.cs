using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RTSMapData))]
public class RTSMapDataEditor : EditorWindowBase
{
    private RTSMapData mapData;

    public override void Awake()
    {
        base.Awake();

        mapData = target as RTSMapData;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Data from Scene"))
        {
            var fogOfWarEffect = FindObjectOfType<FogOfWarEffect>();

            mapData.fogMaskType = fogOfWarEffect.fogMaskType;
            mapData.fogColor = fogOfWarEffect.fogColor;
            mapData.centerPos = fogOfWarEffect.centerPosition;
            mapData.xSize = fogOfWarEffect.xSize;
            mapData.zSize = fogOfWarEffect.zSize;
            mapData.texWidth = fogOfWarEffect.texWidth;
            mapData.texHeight = fogOfWarEffect.texHeight;
            mapData.heightRange = fogOfWarEffect.heightRange;
            mapData.blurOffset = fogOfWarEffect.blurOffset;
            mapData.blurInteration = fogOfWarEffect.blurInteration;

            //var rtsCamera = FindObjectOfType<RTSCamera>();
            //mapData.cameraPos = rtsCamera.transform.position;
            //mapData.cameraEularAngle = rtsCamera.transform.eulerAngles;
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
