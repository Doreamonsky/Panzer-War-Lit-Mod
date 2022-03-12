#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using ParadoxNotion;
using System.Collections.Generic;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using System.Linq;

namespace NodeCanvas.Editor
{

    [InitializeOnLoad]
    public class ActiveOwnersOverview : EditorWindow
    {

        private List<GraphOwner> activeOwners;
        private bool willRepaint;

        private string search;
        private Vector2 scrollPos;

        ///----------------------------------------------------------------------------------------------

        ///<summary>Show the finder window</summary>
        public static void ShowWindow() {
            GetWindow<ActiveOwnersOverview>().Show();
        }

        //...
        void OnEnable() {
            titleContent = new GUIContent("Overview", StyleSheet.canvasIcon);
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            search = null;
            scrollPos = default(Vector2);

            activeOwners = new List<GraphOwner>();
            if ( Application.isPlaying ) {
                foreach ( var owner in FindObjectsOfType<GraphOwner>().Where(o => o.isRunning) ) {
                    if ( !activeOwners.Contains(owner) ) { activeOwners.Add(owner); }
                }
            }

            GraphOwner.onOwnerBehaviourStateChange -= OwnerBehaviourChange;
            GraphOwner.onOwnerBehaviourStateChange += OwnerBehaviourChange;

            willRepaint = true;
            RemoveNotification();
        }

        void OnDisable() {
            GraphOwner.onOwnerBehaviourStateChange -= OwnerBehaviourChange;
            activeOwners = null;
        }

        //...
        void OwnerBehaviourChange(GraphOwner owner) {
            if ( !Application.isPlaying ) { return; }
            if ( owner.isRunning ) {
                //we check contains for case of paused
                if ( !activeOwners.Contains(owner) ) {
                    activeOwners.Add(owner);
                }
            } else {
                activeOwners.Remove(owner);
            }

            willRepaint = true;
        }

        ///----------------------------------------------------------------------------------------------

        //...
        void Update() {
            if ( willRepaint ) {
                willRepaint = false;
                Repaint();
            }
        }

        //...
        void OnGUI() {


            EditorGUILayout.HelpBox("In PlayMode only, you can use this Utility, to search and find GraphOwners in the scene which are actively running.", MessageType.Info);

            search = EditorUtils.SearchField(search);
            EditorUtils.BoldSeparator();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            var hasResult = false;
            foreach ( var owner in activeOwners ) {
                if ( owner == null ) { continue; }
                hasResult = true;

                var displayName = string.Format("<size=9><b>{0}</b> ({1})</size>", owner.name, owner.graphType.FriendlyName());

                if ( !string.IsNullOrEmpty(search) ) {
                    if ( !StringUtils.SearchMatch(search, displayName) ) {
                        continue;
                    }
                }

                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(displayName);
                GUILayout.EndHorizontal();

                var elementRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(elementRect, MouseCursor.Link);
                if ( elementRect.Contains(Event.current.mousePosition) ) {
                    if ( Event.current.type == EventType.MouseMove ) {
                        willRepaint = true;
                    }
                    GUI.color = new Color(0.5f, 0.5f, 1, 0.3f);
                    GUI.DrawTexture(elementRect, EditorGUIUtility.whiteTexture);
                    GUI.color = Color.white;
                    if ( Event.current.type == EventType.MouseDown ) {
                        Selection.activeObject = owner;
                        EditorGUIUtility.PingObject(owner);
                        if ( Event.current.clickCount >= 2 ) {
                            GraphEditor.OpenWindow(owner);
                        }
                        Event.current.Use();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            if ( !hasResult ) {
                ShowNotification(new GUIContent(Application.isPlaying ? "No GraphOwner is actively running." : "Application is not playing."));
            }

            if ( Event.current.type == EventType.MouseLeaveWindow ) {
                willRepaint = true;
            }
        }
    }
}

#endif