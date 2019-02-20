using System.IO;
using UnityEditor;
using UnityEngine;
public class EditorWindowBase : Editor
{
    public string EditorHeadline = "ShanghaiWindy...";
    public bool InEditingSceneObject = false;

    private void EditorBaseInspector()
    {
        EditorGUILayout.HelpBox(EditorHeadline, MessageType.Info);


        if (GUILayout.Button("Export Data as Json"))
        {
            string path = EditorUtility.SaveFilePanel("Export As Json", "Others/Data/", target.name, "json");

            FileStream fs = new FileStream(path, FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(JsonUtility.ToJson(target));
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
        if (GUILayout.Button("Override Data By Json"))
        {
            var path = EditorUtility.OpenFilePanel("Export As Json", "Others/Data/", "json");

            var fileSteam = new FileStream(path, FileMode.Open);
            var streamReader = new StreamReader(fileSteam);

            JsonUtility.FromJsonOverwrite(streamReader.ReadToEnd(), target);
        }

        if (GUILayout.Button("Ping Object"))
        {
            EditorGUIUtility.PingObject(target);
        }

        if (InEditingSceneObject)
        {
            EditorGUILayout.HelpBox("In Editor Mode", MessageType.Warning);
            if (GUILayout.Button("Unlock Inspector"))
            {
                ActiveEditorTracker.sharedTracker.isLocked = false;
                InEditingSceneObject = false;
            }
        }

    }

    public override void OnInspectorGUI()
    {
        EditorBaseInspector();

        base.OnInspectorGUI();
    }
    public void LockEditor()
    {
        ActiveEditorTracker.sharedTracker.isLocked = true;
        InEditingSceneObject = true;
    }
    public void UnlockEditor()
    {
        ActiveEditorTracker.sharedTracker.isLocked = false;
        InEditingSceneObject = true;
    }
    public virtual void Awake()
    {
        Selection.selectionChanged += OnSelectionChanged;

        SceneView.onSceneGUIDelegate += view =>
        {
            ShortCut();
        };
    }
    public virtual void OnDestroy()
    {
        ActiveEditorTracker.sharedTracker.isLocked = false;

        Selection.selectionChanged -= OnSelectionChanged;

        SceneView.onSceneGUIDelegate -= view =>
        {
            ShortCut();
        };
    }
    public virtual void OnSelectionChanged()
    {
    }
    public virtual void ShortCut()
    {
    }

}
