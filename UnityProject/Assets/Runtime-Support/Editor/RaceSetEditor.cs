using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ShanghaiWindy.Core;
namespace ShanghaiWindy.Editor
{

    [CustomEditor(typeof(RaceSet))]
    public class RaceSetEditor : EditorWindowBase
    {
        private GameObject AidNode;

        RaceSet raceSet;

        private Vector3 previousVector3;

        public override void Awake()
        {
            base.Awake();
            EditorHeadline = "ShanghaiWindy 赛道";

        }

        public override void OnSelectionChanged()
        {
            base.OnSelectionChanged();
        }

        public override void ShortCut()
        {
            base.ShortCut();
            GameObject active = Selection.activeGameObject;
            var e = Event.current;


            if (e.keyCode == KeyCode.C)
            {
                Event.current.Use();

                if (ArrayUtility.Contains(raceSet.raceNodes, active.transform.position))
                    return;

                previousVector3 = active.transform.position;

                ArrayUtility.Add<Vector3>(ref raceSet.raceNodes, active.transform.position);//raceSet.raceNodes

                AidNode.GetComponent<LineRenderer>().positionCount = raceSet.raceNodes.Length;

                AidNode.GetComponent<LineRenderer>().SetPositions(raceSet.raceNodes);
            }
        }



        public override void OnInspectorGUI()
        {
            raceSet = (RaceSet)target;


            if (GUILayout.Button("进入编辑模式") && AidNode == null)
            {
                AidNode = new GameObject("Aid Node", typeof(LineRenderer));
                AidNode.GetComponent<LineRenderer>().positionCount = raceSet.raceNodes.Length;

                AidNode.GetComponent<LineRenderer>().SetPositions(raceSet.raceNodes);
                IconManager.SetIcon(AidNode, IconManager.LabelIcon.Orange);
                LockEditor();
            }

            if (AidNode)
            {
                if (GUILayout.Button("关闭编辑模式"))
                {
                    DestroyImmediate(AidNode);
                    UnlockEditor();
                }
            }


            base.OnInspectorGUI();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}
