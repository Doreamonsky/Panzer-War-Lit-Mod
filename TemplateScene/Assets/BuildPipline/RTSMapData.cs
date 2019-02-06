using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RTSMapData", menuName = "ShanghaiWindy/RTS/RTSMapData")]
public class RTSMapData : ScriptableObject
{
    public string mapName;

    public Sprite mapImg;

    public Vector3 mapPivot;

    public Vector3 cameraPos, cameraEularAngle;

    public FogOfWarEffect.FogMaskType fogMaskType = FogOfWarEffect.FogMaskType.BasicFOV;

    public Color fogColor = Color.black;

    public Vector3 centerPos;

    public float xSize = 256, zSize = 256;

    public int texWidth = 60, texHeight = 60;

    public float heightRange = 15;

    public float blurOffset = 0.01f;

    public int blurInteration = 2;
}
