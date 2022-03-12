#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion;

namespace NodeCanvas.Editor
{

    public class WelcomeWindow : EditorWindow
    {

        private static Texture2D paradoxHeader;
        private static System.Type assetType;
        private static Texture2D docsIcon;
        private static Texture2D resourcesIcon;
        private static Texture2D supportIcon;
        private static Texture2D communityIcon;

        void OnEnable() {
            titleContent = new GUIContent("Welcome");
            paradoxHeader = Resources.Load("ParadoxNotionHeader") as Texture2D;
            docsIcon = Resources.Load("Manual") as Texture2D;
            resourcesIcon = Resources.Load("Resources") as Texture2D;
            supportIcon = Resources.Load("Support") as Texture2D;
            communityIcon = Resources.Load("Community") as Texture2D;
            var size = new Vector2(paradoxHeader.width, 510);
            minSize = size;
            maxSize = size;
        }

        void OnGUI() {

            var att = assetType != null ? (GraphInfoAttribute)assetType.GetCustomAttributes(typeof(GraphInfoAttribute), true).FirstOrDefault() : null;
            var packageName = att != null ? att.packageName : "NodeCanvas";
            var docsURL = att != null ? att.docsURL : "https://nodecanvas.paradoxnotion.com/documentation/";
            var resourcesURL = att != null ? att.resourcesURL : "https://nodecanvas.paradoxnotion.com/downloads/";
            var forumsURL = att != null ? att.forumsURL : "https://nodecanvas.paradoxnotion.com/forums-page/";
            var discordUrl = "https://discord.gg/97q2Rjh";

            var headerRect = new Rect(0, 0, paradoxHeader.width, paradoxHeader.height);
            EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);
            if ( GUI.Button(headerRect, paradoxHeader, GUIStyle.none) ) {
                UnityEditor.Help.BrowseURL("http://www.paradoxnotion.com");
            }
            GUILayout.Space(paradoxHeader.height);

            GUI.skin.label.richText = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            GUILayout.Label(string.Format("<size=26><b>Welcome to {0}!</b></size>", packageName));
            GUILayout.Label(string.Format("<i>Thanks for using {0}! Following are a few important links to get you started!</i>", packageName));
            GUILayout.Space(10);

            ///----------------------------------------------------------------------------------------------

            ShowEntry(docsIcon, "<size=16><b>Documentation</b></size>\nRead thorough documentation and API reference online.", docsURL);
            ShowEntry(resourcesIcon, "<size=16><b>Resources</b></size>\nDownload samples, extensions and other resources.", resourcesURL);
            ShowEntry(supportIcon, "<size=16><b>Support</b></size>\nJoin the online forums, give feedback and get support.", forumsURL);
            ShowEntry(communityIcon, "<size=16><b>Community</b></size>\nJoin the online Discord community.", discordUrl);


            ///----------------------------------------------------------------------------------------------

            GUILayout.FlexibleSpace();

            GUILayout.Label("Please consider leaving a review to support the product. Thank you!");

            GUILayout.Space(5);

            Prefs.showWelcomeWindow = EditorGUILayout.ToggleLeft("Show On Startup", Prefs.showWelcomeWindow);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
        }

        //...
        void ShowEntry(Texture2D icon, string text, string url) {
            GUILayout.BeginHorizontal(Styles.roundedBox);
            GUI.backgroundColor = Color.clear;
            GUI.contentColor = EditorGUIUtility.isProSkin ? ColorUtils.Grey(0.8f) : Color.black;
            if ( GUILayout.Button(icon, GUILayout.Width(50), GUILayout.Height(50)) ) {
                UnityEditor.Help.BrowseURL(url);
            }
            GUI.contentColor = Color.white;
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.BeginVertical();
            GUILayout.Space(6);
            GUILayout.Label(text);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }

        //...
        public static void ShowWindow(System.Type t) {
            var window = CreateInstance<WelcomeWindow>();
            assetType = t;
            window.ShowUtility();
        }
    }
}

#endif