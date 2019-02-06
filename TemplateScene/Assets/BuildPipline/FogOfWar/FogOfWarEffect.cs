using ASL.FogOfWar;
using System.Collections.Generic;
using UnityEngine;

public struct FOWMapPos
{
    public int x;
    public int y;

    public FOWMapPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

/// <summary>
/// 屏幕空间战争迷雾
/// </summary>
public class FogOfWarEffect : MonoBehaviour
{

    public enum FogMaskType
    {
        /// <summary>
        /// 精确计算的FOV
        /// </summary>
        AccurateFOV,
        /// <summary>
        /// 基础FOV
        /// </summary>
        BasicFOV,
        /// <summary>
        /// 简单圆形
        /// </summary>
        Circular,
    }

    public static FogOfWarEffect Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<FogOfWarEffect>();
            }

            return instance;
        }
    }

    private static FogOfWarEffect instance;



    public Texture2D fowMaskTexture {
        get {
            if (m_Map != null)
            {
                return m_Map.GetFOWTexture();
            }

            return null;
        }
    }

    public RenderTexture minimapMask {
        get {
            if (!generateMinimapMask)
            {
                return null;
            }

            return m_Renderer.GetMimiMapMask();
        }
    }

    public FogMaskType fogMaskType;
    public Color fogColor = Color.black;
    public float xSize;
    public float zSize;
    public int texWidth;
    public int texHeight;
    public Vector3 centerPosition;
    public float heightRange;
    public float blurOffset;
    public int blurInteration;
    public bool generateMinimapMask;
    public bool isMaskingArea;

    /// <summary>
    /// 迷雾特效shader
    /// </summary>
    public Shader effectShader;
    /// <summary>
    /// 模糊shader
    /// </summary>
    public Shader blurShader;
    /// <summary>
    /// 小地图蒙版渲染shader
    /// </summary>
    public Shader minimapRenderShader;



    /// <summary>
    /// 预生成的地图FOV数据（如果为空则使用实时计算FOV）
    /// </summary>
    //public FOWPregenerationFOVMapData pregenerationFOVMapData;

    /// <summary>
    /// 战争迷雾地图对象
    /// </summary>
    private FOWMap m_Map;
    /// <summary>
    /// 战争迷雾渲染器
    /// </summary>
    private FOWRenderer m_Renderer;

    private bool m_IsInitialized;

    private float m_MixTime = 0.0f;
    private float m_RefreshTime = 0.0f;

    private float m_DeltaX;
    private float m_DeltaZ;
    private float m_InvDeltaX;
    private float m_InvDeltaZ;

    private Camera m_Camera;

    private const float kDispearSpeed = 3f;
    private const float kRefreshTextureSpeed = 4.0f;

    private Vector3 m_BeginPos;

    private List<FOWFieldData> m_FieldDatas;

    private bool m_IsFieldDatasUpdated;


    void OnDestroy()
    {
        if (m_Renderer != null)
        {
            m_Renderer.Release();
        }

        if (m_Map != null)
        {
            m_Map.Release();
        }

        if (m_FieldDatas != null)
        {
            m_FieldDatas.Clear();
        }

        m_FieldDatas = null;
        m_Renderer = null;
        m_Map = null;
        instance = null;
    }

    void FixedUpdate()
    {
        /*
        更新迷雾纹理
        */
        if (m_MixTime >= 1.0f)
        {
            if (m_RefreshTime >= 1.0f)
            {
                m_RefreshTime = 0.0f;
                if (m_Map.RefreshFOWTexture())
                {

                    m_Renderer.SetFogFade(0);
                    m_MixTime = 0;
                    m_IsFieldDatasUpdated = false;
                    //m_Renderer.SetFogTexture(m_Map.GetFOWTexture());
                }
            }
            else
            {
                m_RefreshTime += Time.deltaTime * kRefreshTextureSpeed;
            }
        }
        else
        {
            m_MixTime += Time.deltaTime * kDispearSpeed;
            m_Renderer.SetFogFade(m_MixTime);
        }
    }

    public void Initialize()
    {
        if (xSize <= 0 || zSize <= 0 || texWidth <= 0 || texHeight <= 0)
        {
            m_IsInitialized = false;
        }

        if (effectShader == null || !effectShader.isSupported)
        {
            m_IsInitialized = false;
        }

        m_Camera = gameObject.GetComponent<Camera>();

        if (m_Camera == null)
        {
            m_IsInitialized = false;
        }

        m_Camera.depthTextureMode |= DepthTextureMode.Depth;
        m_DeltaX = xSize / texWidth;
        m_DeltaZ = zSize / texHeight;
        m_InvDeltaX = 1.0f / m_DeltaX;
        m_InvDeltaZ = 1.0f / m_DeltaZ;
        m_BeginPos = centerPosition - new Vector3(xSize * 0.5f, 0, zSize * 0.5f);
        m_Renderer = new FOWRenderer(effectShader, blurShader, minimapRenderShader, centerPosition, xSize, zSize, fogColor, blurOffset, blurInteration);
        m_Map = new FOWMap(fogMaskType, m_BeginPos, xSize, zSize, texWidth, texHeight, heightRange);
        IFOWMapData md = gameObject.GetComponent<IFOWMapData>();
        if (md != null)
        {
            m_Map.SetMapData(md);
        }
        else
        {
            m_Map.SetMapData(new FOWMapData(texHeight, texHeight));
            m_Map.GenerateMapData(heightRange);
        }
        if (minimapRenderShader != null)
        {
            generateMinimapMask = true;
        }
        m_IsInitialized = true;
    }

    /// <summary>
    /// 世界坐标转战争迷雾坐标
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static FOWMapPos WorldPositionToFOW(Vector3 position)
    {
        if (!Instance)
        {
            return default(FOWMapPos);
        }

        if (!Instance.m_IsInitialized)
        {
            return default(FOWMapPos);
        }

        int x = Mathf.FloorToInt((position.x - Instance.m_BeginPos.x) * Instance.m_InvDeltaX);
        int z = Mathf.FloorToInt((position.z - Instance.m_BeginPos.z) * Instance.m_InvDeltaZ);

        return new FOWMapPos(x, z);
    }

    public static Vector2 WorldPositionTo2DLocal(Vector3 position)
    {
        if (!Instance)
        {
            return default(Vector2);
        }

        if (!Instance.m_IsInitialized)
        {
            return default(Vector2);
        }

        Vector2 pos = default(Vector2);
        pos.x = (position.x - Instance.m_BeginPos.x) / Instance.xSize;
        pos.y = (position.z - Instance.m_BeginPos.z) / Instance.zSize;

        return pos;
    }

    ///// <summary>
    ///// 将指定位置设置为可见
    ///// </summary>
    ///// <param name="fieldData">视野</param>
    //public static void SetVisibleAtPosition(FOWFieldData fieldData)
    //{
    //    if (!Instance)
    //        return;
    //    if (!Instance.m_IsInitialized)
    //        return;
    //    if (fieldData == null)
    //        return;

    //    Instance.m_Map.SetVisible(fieldData);

    //}

    public static void UpdateFOWFieldData(FOWFieldData data)
    {
        if (!Instance)
        {
            return;
        }

        if (!Instance.m_IsInitialized)
        {
            return;
        }

        if (Instance.m_FieldDatas == null)
        {
            Instance.m_FieldDatas = new List<FOWFieldData>();
        }

        if (!Instance.m_FieldDatas.Contains(data))
        {
            Instance.m_FieldDatas.Add(data);
        }
        if (!Instance.m_IsFieldDatasUpdated)
        {
            //lock (Instance.m_FieldDatas)
            {
                Instance.m_Map.SetVisible(Instance.m_FieldDatas);
                Instance.m_IsFieldDatasUpdated = true;
            }
        }
    }

    public static void ReleaseFOWFieldData(FOWFieldData data)
    {
        if (!instance)
        {
            return;
        }

        if (!instance.m_IsInitialized)
        {
            return;
        }
        //lock (instance.m_FieldDatas)
        {
            if (instance.m_FieldDatas != null && instance.m_FieldDatas.Contains(data))
            {
                instance.m_FieldDatas.Remove(data);
            }
        }
    }

    /// <summary>
    /// 是否在地图中可见
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static bool IsVisibleInMap(Vector3 position)
    {
        if (!Instance)
        {
            return true;
        }

        if (!Instance.m_IsInitialized)
        {
            return true;
        }

        int x = Mathf.FloorToInt((position.x - Instance.m_BeginPos.x) * Instance.m_InvDeltaX);
        int z = Mathf.FloorToInt((position.z - Instance.m_BeginPos.z) * Instance.m_InvDeltaZ);

        return Instance.m_Map.IsVisibleInMap(x, z);

    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!m_IsInitialized || !isMaskingArea)
        {
            Graphics.Blit(src, dst);
        }
        else
        {
            m_Renderer.RenderFogOfWar(m_Camera, m_Map.GetFOWTexture(), src, dst);
        }
    }

    void OnDrawGizmosSelected()
    {
        FOWUtils.DrawFogOfWarGizmos(centerPosition, xSize, zSize, texWidth, texHeight, heightRange);
    }
}
