using ShanghaiWindy.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(PathData))]
    public class PathDataEditor : EditorWindowBase
    {
        private PathData m_PathData;

        private List<GameObject> m_DumpObjectsList = new List<GameObject>();

        private System.Action<RaycastHit> onClickTerrain;

        private readonly string[] nodeUnitStateStr = new string[] { "Attack", "Defence" };

        private Rect winRect = new Rect(25, 25, 250, 200);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("Press Ctrl + Space to Insert the PointNode", MessageType.Info);

            if (GUI.changed)
            {
                UpdatePathNodeData();

                EditorUtility.SetDirty(target);
            }
        }

        private void OnEnable()
        {
            m_PathData = target as PathData;

            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;

            onClickTerrain += (RaycastHit rayHit) =>
            {
                var pos = rayHit.point;

                var lastNodePos = m_DumpObjectsList.Count == 0 ? Vector3.zero : m_PathData.pathList[m_PathData.pathList.Count - 1].pos;
                var rot = Quaternion.LookRotation(pos - lastNodePos);

                var pathNode = new PathNode()
                {
                    pos = pos,
                    rot = rot.eulerAngles,
                    unitState = PathNodeUnitState.Attack
                };

                m_PathData.pathList.Add(pathNode);

                AddDumpObject(pathNode);
                UpdateDumpObject();
            };

            for (int i = 0; i < m_PathData.pathList.Count; i++)
            {
                var pathNode = m_PathData.pathList[i];
                AddDumpObject(pathNode);
            }

            UpdateDumpObject();
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnScene;

            while (m_DumpObjectsList.Count != 0)
            {
                var index = m_DumpObjectsList.Count - 1;

                DestroyImmediate(m_DumpObjectsList[index]);
                m_DumpObjectsList.RemoveAt(index);
            }
        }
        //Just Add Cube
        private void AddDumpObject(PathNode pathNode)
        {
            var dump = GameObject.CreatePrimitive(PrimitiveType.Cube);

            dump.transform.localScale = new Vector3(2, 2, 2);

            m_DumpObjectsList.Add(dump);
        }

        //Remove Cube and Remove it and PathNode
        private void RemoveDumpObjectAndLink(PathNode pathNode)
        {
            var index = m_PathData.pathList.IndexOf(pathNode);

            DestroyImmediate(m_DumpObjectsList[index]);

            m_PathData.pathList.RemoveAt(index);
            m_DumpObjectsList.RemoveAt(index);
        }
        //Update Dump from PathNode
        private void UpdateDumpObject()
        {
            for (var i = 0; i < m_PathData.pathList.Count; i++)
            {
                var dumpObject = m_DumpObjectsList[i];
                var pathData = m_PathData.pathList[i];

                dumpObject.transform.position = pathData.pos;
                dumpObject.transform.eulerAngles = pathData.rot;
            }
        }
        //Update PathNode from Dump
        private void UpdatePathNodeData()
        {
            for (var i = 0; i < m_PathData.pathList.Count; i++)
            {
                var dumpObject = m_DumpObjectsList[i];
                var pathData = m_PathData.pathList[i];

                pathData.pos = dumpObject.transform.position;
                pathData.rot = dumpObject.transform.eulerAngles;
            }
        }

        private void OnScene(SceneView sceneView)
        {
            EditorShortCut();

            Handles.BeginGUI();

            winRect = GUILayout.Window(1, winRect, winID =>
            {
                if (InEditingSceneObject)
                {
                    if (GUILayout.Button("Exit Edit Mode"))
                    {
                        UnlockEditor();
                    }
                    if (GUILayout.Button("Save Node Information"))
                    {
                        UpdatePathNodeData();
                    }

                    var selectedNodeIndex = m_DumpObjectsList.IndexOf(Selection.activeGameObject);

                    if (selectedNodeIndex != -1)
                    {
                        GUILayout.Label($"Node:{selectedNodeIndex}");

                        if (GUILayout.Button("Delete"))
                        {
                            RemoveDumpObjectAndLink(m_PathData.pathList[selectedNodeIndex]);
                        }

                        m_PathData.pathList[selectedNodeIndex].unitState = (PathNodeUnitState)EditorGUILayout.EnumPopup(m_PathData.pathList[selectedNodeIndex].unitState);
                    }
                }
                else
                {
                    if (GUILayout.Button("Open Edit Mode"))
                    {
                        LockEditor();
                    }
                }

                GUI.DragWindow(winRect);
            }, "Path Editor");


            Handles.EndGUI();

            //Draw Lines
            for (var i = 0; i < m_DumpObjectsList.Count; i++)
            {
                if (i + 1 < m_DumpObjectsList.Count)
                {
                    Handles.DrawLine(m_DumpObjectsList[i].transform.position, m_DumpObjectsList[i + 1].transform.position);
                }

                Handles.color = Color.green;
                Handles.ArrowCap(i, m_DumpObjectsList[i].transform.position + Vector3.up * 2, m_DumpObjectsList[i].transform.rotation, 1);
                Handles.color = Color.white;
            }

        }

        public void EditorShortCut()
        {
            var e = Event.current;

            if (e.keyCode == KeyCode.Space && e.control && e.rawType == EventType.KeyDown)
            {
                var ray = HandleUtility.GUIPointToWorldRay(new Vector2(Mathf.Round(e.mousePosition.x), Mathf.Round(e.mousePosition.y)));

                var isHit = Physics.Raycast(ray, out var rayHit, 10000);

                if (isHit)
                {
                    onClickTerrain?.Invoke(rayHit);

                    e.Use();
                }
            }
        }
    }

}
