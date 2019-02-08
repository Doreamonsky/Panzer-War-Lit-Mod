using UnityEditor;
using UnityEngine;
using ShanghaiWindy.Core;

namespace ShanghaiWindy.Editor
{

    [CustomEditor(typeof(WaveDefenceMission))]
    public class MissionEditor : EditorWindowBase
    {
        //public static WaveDefenceMission CurrentMission;
        private int currentEditingWave = 0;

        public override void OnInspectorGUI()
        {
            var mission = target as WaveDefenceMission;

            //CurrentMission = mission;

            base.BaseGUI();

            if (GUILayout.Button("Lock"))
            {
                LockEditor();
            }

            if (Selection.activeGameObject != null)
            {
                EditorGUILayout.HelpBox(string.Format("Selction:{0} {1}", Selection.activeGameObject.name, Selection.activeGameObject.transform.position), MessageType.Info);
            }

            base.OnInspectorGUI();
        }
        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate += OnScene;
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnScene;
        }

        private void OnScene(SceneView sceneview)
        {
            var mission = target as WaveDefenceMission;


            if (InEditingSceneObject)
            {
                //Wave 设置
                for (int i = 0; i < mission.waves.Length; i++)
                {
                    WaveAttack wave = mission.waves[i];

                    //选择当前编辑的Wave
                    Handles.BeginGUI();

                    if (GUILayout.Button(string.Format("Wave:{0}", i)))
                    {
                        currentEditingWave = i;
                    }

                    Handles.EndGUI();

                    //不是当前选择的波数
                    if (currentEditingWave != i)
                    {
                        continue;
                    }

                    //显示波数的启动位置与编辑箭头
                    for (int j = 0; j < wave.waveList.Length; j++)
                    {
                        WaveAttackInfo t = wave.waveList[j];

                        for (int k = 0; k < t.posList.Count; k++)
                        {
                            Vector3 pos = t.posList[k];
                            Quaternion rot = t.rotList[k];

                            pos = Handles.PositionHandle(pos, rot);
                            rot = Handles.RotationHandle(rot, pos);

                            mission.waves[i].waveList[j].posList[k] = pos;
                            mission.waves[i].waveList[j].rotList[k] = rot;

                            Handles.Label(pos, string.Format("Vehicle :{0} Pos:{1}", t.vehicleName, pos));
                            Handles.DrawWireCube(pos, new Vector3(4, 2, 4));
                        }
                    }
                }

                //玩家设置
                mission.startPoint = Handles.PositionHandle(mission.startPoint, mission.startRotation);
                mission.startRotation = Handles.RotationHandle(mission.startRotation, mission.startPoint);
                Handles.DrawWireCube(mission.startPoint, new Vector3(4, 2, 4));

            }
        }
    }
}
