/*\
非专业人用请勿修改此脚本 
 */
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace WaroftanksSDK.Editor.Pack
{

    [CustomEditor(typeof(PackGame))]

    public class PackGame : EditorWindow
    {
        [MenuItem("Tools/Load Language From csv")]
        static void Lang()
        {
            StreamReader streamReader = new StreamReader("Others/Lang.csv", System.Text.Encoding.UTF8);

            string line = null;
            Dictionary<string, Dictionary<string, string>> LangDictionary = new Dictionary<string, Dictionary<string, string>>();
            List<string> LangOrder = new List<string>();

            while ((line = streamReader.ReadLine()) != null)
            {
                string[] LineElements = line.Split(',');
                if (line.Contains("Key/Language"))
                {
                    for (int i = 1; i < LineElements.Length; i++)
                    {
                        try
                        {
                            LangDictionary.Add(LineElements[i], new Dictionary<string, string>());
                            LangOrder.Add(LineElements[i]);
                        }
                        catch (System.Exception)
                        {
                            Debug.Log(i);
                        }

                    }

                    continue;
                }
                if (line.Contains("//"))
                {
                    continue;
                }

                for (int i = 1; i < LineElements.Length; i++)
                {
                    try
                    {
                        LangDictionary[LangOrder[i - 1]].Add(LineElements[0], LineElements[i]);
                    }
                    catch (System.Exception)
                    {
                        Debug.Log(i);
                    }
                }
            }


            for (int i = 0; i < LangOrder.Count; i++)
            {
                Dictionary<string, string> CurrentLangDictionary = LangDictionary[LangOrder[i]];
                bool Append = false;
                foreach (string Key in CurrentLangDictionary.Keys)
                {
                    WritePrivateProfileString(Key, CurrentLangDictionary[Key], string.Format("Assets/Lang/Lang_{0}.txt", LangOrder[i]), Append);
                    Append = true;
                }
            }
        }

        public static void WritePrivateProfileString(string Key, string Value, string File, bool Append = true)
        {

            StreamWriter sw = new StreamWriter(File, Append, System.Text.Encoding.UTF8);
            if (Append)
            {
                sw.Write("\n");
            }
            sw.Write(string.Format("{0}={1}", Key, Value));
            sw.Flush();
            sw.Close();
        }
        [MenuItem("Tools/ReplaceFont")]
        static void ReplaceFont()
        {
            Font myFont = Resources.Load<Font>("MainFont");//Resources.GetBuiltinResource<Font>("Arial.ttf");
            foreach (UnityEngine.UI.Text textComponent in GameObject.FindObjectsOfType<UnityEngine.UI.Text>())
            {
                textComponent.font = myFont;
            }
        }
        [MenuItem("Tools/Build System(Legacy)")]
        static void InitPackAsset()
        {
            Rect wr = new Rect(0, 0, 500, 500);
            PackGame window = (PackGame)EditorWindow.GetWindowWithRect(typeof(PackGame), wr, true, "Pack Assets-----TankWar");
            window.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Build Path"))
            {
                List<Vector3> V3_OriginPoints = new List<Vector3>();
                List<Transform> T_OriginPoints = new List<Transform>();
                foreach (GameObject _PatrolPoint in GameObject.FindGameObjectsWithTag("PatorlPoint"))
                {
                    V3_OriginPoints.Add(_PatrolPoint.transform.position);
                    T_OriginPoints.Add(_PatrolPoint.transform);
                }
                Vector3 StartPointOffSet = new Vector3(0, 2f, 0);
                foreach (GameObject _PatrolPoint in GameObject.FindGameObjectsWithTag("TeamAStartPoint"))
                {
                    V3_OriginPoints.Add(_PatrolPoint.transform.position);
                    T_OriginPoints.Add(_PatrolPoint.transform);
                }

                foreach (GameObject _PatrolPoint in GameObject.FindGameObjectsWithTag("TeamBStartPoint"))
                {
                    V3_OriginPoints.Add(_PatrolPoint.transform.position);
                    T_OriginPoints.Add(_PatrolPoint.transform);
                }

                for (int i = 0; i < T_OriginPoints.Count; i++)
                {
                    NavMeshHit navHit = new NavMeshHit();

                    NavMesh.SamplePosition(V3_OriginPoints[i], out navHit, float.MaxValue, NavMesh.AllAreas);

                    T_OriginPoints[i].position = navHit.position;
                    if (T_OriginPoints[i].name.Contains("StartPoint"))
                    {
                        T_OriginPoints[i].position += StartPointOffSet;
                    }
                    else
                    {
                        T_OriginPoints[i].position += new Vector3(0, 1.5f, 0);
                    }
                }
            }

            return;

        }
        public static string AssetNameCorretor(string str)
        {
            return str.Replace(" ", "").ToLower();

        }

    }
}