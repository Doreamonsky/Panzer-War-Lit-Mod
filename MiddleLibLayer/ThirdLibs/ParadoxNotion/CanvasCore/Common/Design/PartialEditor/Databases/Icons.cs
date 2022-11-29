#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace ParadoxNotion.Design
{

    ///<summary>Common Icons Database</summary>
	[InitializeOnLoad]
    public static class Icons
    {

        static Icons() { Load(); }

        [InitializeOnLoadMethod]
        static void Load() {
            playIcon = EditorGUIUtility.FindTexture("d_PlayButton");
            pauseIcon = EditorGUIUtility.FindTexture("d_PauseButton");
            stepIcon = EditorGUIUtility.FindTexture("d_StepButton");
            viewIcon = EditorGUIUtility.FindTexture("d_ViewToolOrbit On");
            csIcon = EditorGUIUtility.FindTexture("cs Script Icon");
            tagIcon = EditorGUIUtility.FindTexture("d_FilterByLabel");
            searchIcon = EditorGUIUtility.FindTexture("Search Icon");
            infoIcon = EditorGUIUtility.FindTexture("d_console.infoIcon.sml");
            warningIcon = EditorGUIUtility.FindTexture("d_console.warnicon.sml");
            warningIconBig = EditorGUIUtility.FindTexture("d_console.warnicon");
            errorIcon = EditorGUIUtility.FindTexture("d_console.erroricon.sml");
            errorIconBig = EditorGUIUtility.FindTexture("d_console.erroricon");
            redCircle = EditorGUIUtility.FindTexture("d_winbtn_mac_close");
            folderIcon = EditorGUIUtility.FindTexture("Folder Icon");
            favoriteIcon = EditorGUIUtility.FindTexture("Favorite Icon");
            gearPopupIcon = EditorGUIUtility.FindTexture("d__Popup");
            gearIcon = EditorGUIUtility.FindTexture("EditorSettings Icon");
            scaleIcon = EditorGUIUtility.FindTexture("d_ScaleTool");
            minMaxIcon = EditorGUIUtility.FindTexture("d_winbtn_win_max");
            plusIcon = EditorGUIUtility.FindTexture("d_CreateAddNew");
        }

        public static Texture2D playIcon { get; private set; }
        public static Texture2D pauseIcon { get; private set; }
        public static Texture2D stepIcon { get; private set; }
        public static Texture2D viewIcon { get; private set; }
        public static Texture2D csIcon { get; private set; }
        public static Texture2D tagIcon { get; private set; }
        public static Texture2D searchIcon { get; private set; }
        public static Texture2D infoIcon { get; private set; }
        public static Texture2D warningIcon { get; private set; }
        public static Texture2D warningIconBig { get; private set; }
        public static Texture2D errorIcon { get; private set; }
        public static Texture2D errorIconBig { get; private set; }
        public static Texture2D redCircle { get; private set; }
        public static Texture2D folderIcon { get; private set; }
        public static Texture2D favoriteIcon { get; private set; }
        public static Texture2D gearPopupIcon { get; private set; }
        public static Texture2D gearIcon { get; private set; }
        public static Texture2D scaleIcon { get; private set; }
        public static Texture2D minMaxIcon { get; private set; }
        public static Texture2D plusIcon { get; private set; }


        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns a type icon</summary>
        public static Texture GetTypeIcon(System.Type type) {
            return TypePrefs.GetTypeIcon(type);
        }
    }
}

#endif