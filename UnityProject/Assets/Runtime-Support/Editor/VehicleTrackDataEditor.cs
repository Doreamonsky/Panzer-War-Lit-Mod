using ShanghaiWindy.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(VehicleTrackData))]
    public class VehicleTrackDataEditor : EditorWindowBase
    {
        //List<Transform>
        public GameObject EditorTarget;
        public GameObject TrackTransform;
        List<GameObject> Points = new List<GameObject>();

        VehicleTrackData vehicleTrackData;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("ShanghaiWindy 地面载具履带管理&制作系统(预览显示左侧履带)", MessageType.Info);

            vehicleTrackData = (VehicleTrackData)target;

            if (EditorTarget == null)
            {
                EditorTarget = EditorGUILayout.ObjectField("Target Edit Object/目标编辑对象", EditorTarget, typeof(GameObject), true) as GameObject;
            }
            if (TrackTransform == null)
            {
                if (GUILayout.Button("Load Suspension Data/读取悬挂信息"))
                {
                    TrackTransform = EditorTarget.transform.GetChild(0).Find("VehicleDynamics/RightWheel").gameObject;
                    ActiveEditorTracker.sharedTracker.isLocked = true;
                    for (int i = 0; i < vehicleTrackData.AdvanceLeftBones.Length; i++)
                    {
                        GameObject NodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        LastInstancePoint = NodeObject;
                        NodeObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        NodeObject.name = "Node" + (Points.Count + 1).ToString();
                        NodeObject.transform.position = vehicleTrackData.AdvanceLeftBones[i];
                        Points.Add(NodeObject);
                    }
                }
            }
            if (TrackTransform != null)
            {
                EditorGUILayout.HelpBox("Lock Inspector/视窗锁定", MessageType.Error);
                if (GUILayout.Button("Unlock Inspector/取消锁定"))
                {
                    ActiveEditorTracker.sharedTracker.isLocked = false;
                    if (tcPreview != null)
                    {
                        DestroyImmediate(tcPreview.gameObject);
                    }
                }
                if (Selection.activeGameObject != null)
                {
                    if (Selection.activeGameObject.name.Contains("Node"))
                    {
                        if (GUILayout.Button("Gravity Affected Node/节点重力"))
                        {
                            vehicleTrackData.SpringNode = ArrayUtil<int>.Add(ref vehicleTrackData.SpringNode, int.Parse(Selection.activeGameObject.name.Replace("Node", null)));
                        }
                    }
                }

                EditorGUILayout.HelpBox(string.Format("Suspension Count/负重轮数量:{0}", TrackTransform.transform.childCount.ToString()), MessageType.None);
                if (GUILayout.Button("Create Node/创建节点"))
                {
                    Points.Add(CreatePoint());
                }
                if (GUILayout.Button("Delete Node/删除节点"))
                {
                    DeletePoint();
                }
                if (GUILayout.Button("Save/保存设置"))
                {
                    Save();
                }
                if (GUILayout.Button("Turn on Material Instancing/实例材质"))
                {
                    Material NewInstance = Instantiate(vehicleTrackData.TrackMesh.GetComponentInChildren<MeshRenderer>().sharedMaterial);
                    if (!NewInstance.enableInstancing)
                    {
                        NewInstance.enableInstancing = true;
                    }

                    string VehicleName = EditorTarget.transform.GetChild(0).name;

                    Directory.CreateDirectory(string.Format("Assets/Res/Vehicles/Ground/res/Tracks/{0}/", VehicleName));

                    string MaterialPath = string.Format("Assets/Res/Vehicles/Ground/res/Tracks/{0}/{0}_Track.mat", VehicleName);
                    string ModelPath = string.Format("Assets/Res/Vehicles/Ground/res/Tracks/{0}/{0}_Track.prefab", VehicleName);

                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(vehicleTrackData.TrackMesh.GetInstanceID()), ModelPath);
                    GameObject NewInstanceTrackModel = (GameObject)AssetDatabase.LoadAssetAtPath(ModelPath, typeof(GameObject));
                    NewInstanceTrackModel.GetComponentInChildren<MeshRenderer>().sharedMaterial = NewInstance;
                    vehicleTrackData.TrackMesh = NewInstanceTrackModel;
                    AssetDatabase.CreateAsset(NewInstance, MaterialPath);
                }

                if (tcPreview == null)
                {
                    if (GUILayout.Button("Preview Track/预览悬挂"))
                    {
                        Preview();
                        Save();
                    }
                }
                else
                {
                    if (GUILayout.Button("Close Preview/取消预览"))
                    {
                        RemovePreview();
                    }
                    if (GUILayout.Button("Update Preview/更新预览"))
                    {
                        RemovePreview();
                        Preview();
                        Save();
                    }
                }

            }
            base.OnInspectorGUI();

            foreach (GameObject Point in Points)
            {
                EditorGUILayout.HelpBox(string.Format("ID/序号: {0}  Position/坐标: {1}", Point.name, Point.transform.position.ToString()), MessageType.None);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        GameObject LastInstancePoint = null;

        void DeletePoint()
        {
            DestroyImmediate(Points[Points.Count - 1]);
            Points.RemoveAt(Points.Count - 1);

            if (Points.Count > 0)
            {
                LastInstancePoint = Points[Points.Count - 1];
            }

            ReSelect();
        }

        void ReSelect()
        {
            Selection.activeGameObject = LastInstancePoint;
            Save();
        }

        void Save()
        {
            vehicleTrackData.AdvanceLeftBones = new Vector3[Points.Count];
            vehicleTrackData.AdvanceRightBones = new Vector3[Points.Count];

            for (int i = 0; i < Points.Count; i++)
            {
                vehicleTrackData.AdvanceLeftBones[i] = new Vector3(Points[i].transform.position.x, Points[i].transform.position.y, Points[i].transform.position.z);
                vehicleTrackData.AdvanceRightBones[i] = new Vector3(-Points[i].transform.position.x, Points[i].transform.position.y, Points[i].transform.position.z);
            }
        }

        GameObject CreatePoint()
        {
            GameObject NodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            LastInstancePoint = NodeObject;
            NodeObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            NodeObject.name = "Node" + (Points.Count + 1).ToString();
            if (Points.Count < TrackTransform.transform.childCount)
            {
                NodeObject.transform.position = TrackTransform.transform.GetChild(Points.Count).position + new Vector3(0, -Getradius(), 0);
            }
            else
            {
                NodeObject.transform.position = Points[Points.Count - 1].transform.position + new Vector3(0, 0, 0.5f);
            }
            ReSelect();
            return NodeObject;
        }

        float Getradius()
        {
            return EditorTarget.GetComponentInChildren<TankInitSystem>().PTCParameter.TankWheelCollider.GetComponent<WheelCollider>().radius;
        }

        TracksController tcPreview;

        void Preview()
        {
            tcPreview = new GameObject("PreView Track").AddComponent<TracksController>();


            #region 获取编辑顶点坐标
            tcPreview.transform.position += new Vector3(Points[0].transform.position.x, 0, 0) + vehicleTrackData.TrackCenterOffSet;//到履带位置
            List<Transform> ConvertG2T = new List<Transform>();
            for (int i = 0; i < Points.Count; i++)
            {
                ConvertG2T.Add(Points[i].transform);
                Points[i].transform.SetParent(tcPreview.transform); //修正尝试-01
            }
            Transform[] TrackPoints = ConvertG2T.ToArray();
            #endregion

            #region 设置曲线
            tcPreview.trackSpline = new AnimationSpline(WrapMode.Loop);
            tcPreview.trackSpline.InitSpline(TrackPoints);
            float length = tcPreview.trackSpline.CreateSpline(TrackPoints, false, vehicleTrackData.TrackDistance);
            #endregion

            #region 生成履带
            int count = Mathf.RoundToInt(length / vehicleTrackData.TrackDistance) + 2;
            Transform[] tracks = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                tracks[i] = Instantiate(vehicleTrackData.TrackMesh).transform;
                tracks[i].parent = tcPreview.transform;
            }
            tcPreview.trackSpline.AnimateTracks(TrackPoints, tracks, vehicleTrackData.TrackDistance, false, 1, 1 * Random.Range(1, 10), length);
            #endregion

            #region 生成坐标数据
            Transform TankTransform = EditorTarget.transform.GetChild(0).Find("VehicleDynamics");
            tcPreview.transform.SetParent(TankTransform);
            vehicleTrackData.RightTrackCenter = tcPreview.transform.localPosition;
            vehicleTrackData.LeftTrackCenter = new Vector3(-vehicleTrackData.RightTrackCenter.x, vehicleTrackData.RightTrackCenter.y, vehicleTrackData.RightTrackCenter.z);
            vehicleTrackData.RelativeBones = new Vector3[vehicleTrackData.AdvanceLeftBones.Length];

            for (int i = 0; i < vehicleTrackData.AdvanceLeftBones.Length; i++)
            {
                vehicleTrackData.RelativeBones[i] = Points[i].transform.localPosition;
            }
            #endregion
        }
        void RemovePreview()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].transform.SetParent(null);
            }
            DestroyImmediate(tcPreview.gameObject);
        }
    }
}