using System.IO;
using UnityEditor;
using UnityEngine;

public class MergeMaskEditor : EditorWindow
{
    private Texture2D metallic, occlusion, smoothness;
    private Texture2D _metallic, _occlusion, _smoothness;
    private bool roughness, _roughness;

    private Material mat;
    private bool changed;

    [MenuItem("Mod/合并遮罩贴图 - MergeMasks")]
    static void OpenMaskMapEditor()
    {
        MergeMaskEditor mergeMaskEditor = GetWindow<MergeMaskEditor>(false, "MaskMap Editor");
        mergeMaskEditor.titleContent.text = "MaskMap Editor";
        mergeMaskEditor.Init();
    }

    private void Init()
    {
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/MaskMap"));
            mat.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public void OnGUI()
    {
        GetInput();
        Preview();
        CreateMask();
    }

    private void CreateMask()
    {
        if (GUILayout.Button("Create Mask Map"))
        {
            GenPicture();
        }
    }

    private void GenPicture()
    {
        Texture2D output = GetTexture();

        var fullPath = EditorPrefs.GetString("MergeMaskPath", $"{Application.dataPath}/mask.png");

        string savePath =
            EditorUtility.SaveFilePanel("Save", Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath), "png");
        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }

        EditorPrefs.SetString("MergeMaskPath", savePath);
        File.WriteAllBytes(savePath, output.EncodeToPNG());
        AssetDatabase.Refresh();
    }

    private Texture2D GetTexture()
    {
        if (metallic != null)
        {
            mat.SetTexture("_Metallic", metallic); //给Shader的属性赋值
        }

        if (occlusion != null)
        {
            mat.SetTexture("_Occlusion", occlusion);
        }

        if (smoothness != null)
        {
            mat.SetTexture("_Smoothness", smoothness);
        }

        mat.SetFloat("_UseRoughness", _roughness ? 1 : 0);

        RenderTexture
            tempRT = new RenderTexture(metallic.width, metallic.height, 32,
                RenderTextureFormat.ARGB32);
        tempRT.Create();
        Texture2D temp2 = new Texture2D(tempRT.width, tempRT.height, TextureFormat.ARGB32, false);
        Graphics.Blit(temp2, tempRT, mat);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = tempRT;

        Texture2D output = new Texture2D(tempRT.width, tempRT.height, TextureFormat.ARGB32, false);
        output.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        output.Apply();
        RenderTexture.active = prev;

        return output;
    }


    private void GetInput()
    {
        EditorGUILayout.BeginHorizontal("Box");
        GUILayout.Label("Input Textures");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("Box");
        _metallic = TextureField("metallic", metallic);
        if (_metallic != metallic)
        {
            metallic = _metallic;
            changed = true;
        }

        _occlusion = TextureField("occlusion", occlusion);
        if (_occlusion != occlusion)
        {
            occlusion = _occlusion;
            changed = true;
        }

        _smoothness = TextureField(_roughness ? "roughness" : "smoothness", smoothness);
        if (_smoothness != smoothness)
        {
            smoothness = _smoothness;
            changed = true;
        }

        _roughness = EditorGUILayout.Toggle("Is Roughtness", roughness);
        if (_roughness != roughness)
        {
            roughness = _roughness;
            changed = true;
        }

        EditorGUILayout.EndHorizontal();
    }

    private Texture2D preview = null;

    private void Preview()
    {
        EditorGUILayout.BeginHorizontal("Box");
        GUILayout.Label("Preview Output");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("Box");

        Rect previewRect = new Rect(this.position.width / 2 - 75, 250, 150, 150);
        if (preview == null)
        {
            preview = Texture2D.blackTexture;
        }

        if (changed)
        {
            preview = GetTexture();
            changed = false;
        }

        EditorGUI.DrawPreviewTexture(previewRect, preview);
        EditorGUILayout.Space(160);
        EditorGUILayout.EndHorizontal();
    }

    private Texture2D TextureField(string name, Texture2D texture)
    {
        EditorGUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fixedWidth = 150;
        GUILayout.Label(name, style);
        Texture2D result =
            EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(150), GUILayout.Height(150))
                as Texture2D;
        EditorGUILayout.EndHorizontal();
        return result;
    }
}