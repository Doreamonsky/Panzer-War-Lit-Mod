#if UNITY_EDITOR

using System.Collections.Generic;
using ParadoxNotion.Serialization;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace ParadoxNotion.Design
{

    ///<summary>An editor for preferred types</summary>
    public class TypePrefsEditorWindow : EditorWindow
    {

        private List<System.Type> typeList;
        private List<System.Type> alltypes;
        private Vector2 scrollPos;

        ///<summary>Open window</summary>
        public static void ShowWindow() {
            var window = GetWindow<TypePrefsEditorWindow>();
            window.Show();
        }

        //...
        void OnEnable() {
            titleContent = new GUIContent("Preferred Types");
            typeList = TypePrefs.GetPreferedTypesList();
            alltypes = ReflectionTools.GetAllTypes(true).Where(t => !t.IsGenericType && !t.IsGenericTypeDefinition).ToList();
        }

        //...
        void OnGUI() {

            GUI.skin.label.richText = true;
            EditorGUILayout.HelpBox("Here you can specify frequently used types for your project and for easier access wherever you need to select a type, like for example when you create a new blackboard variable or using any refelection based actions. Furthermore, it is essential when working with AOT platforms like iOS or WebGL, that you generate an AOT Classes and link.xml files with the relevant button bellow. To add types in the list quicker, you can also Drag&Drop an object, or a Script file in this editor window.\n\nIf you save a preset in your 'Editor Default Resources/" + TypePrefs.SYNC_FILE_NAME + "' it will automatically sync with the list. Useful when working with others on source control.", MessageType.Info);

            if ( GUILayout.Button("Add New Type", EditorStyles.miniButton) ) {
                GenericMenu.MenuFunction2 Selected = delegate (object o)
                {
                    if ( o is System.Type ) {
                        AddType((System.Type)o);
                    }
                    if ( o is string ) { //namespace
                        foreach ( var type in alltypes ) {
                            if ( type.Namespace == (string)o ) {
                                AddType(type);
                            }
                        }
                    }
                };

                var menu = new GenericMenu();
                var namespaces = new List<string>();
                menu.AddItem(new GUIContent("Classes/System/Object"), false, Selected, typeof(object));
                foreach ( var t in alltypes ) {
                    var a = ( string.IsNullOrEmpty(t.Namespace) ? "No Namespace/" : t.Namespace.Replace(".", "/") + "/" ) + t.FriendlyName();
                    var b = string.IsNullOrEmpty(t.Namespace) ? string.Empty : " (" + t.Namespace + ")";
                    var friendlyName = a + b;
                    var category = "Classes/";
                    if ( t.IsValueType ) category = "Structs/";
                    if ( t.IsInterface ) category = "Interfaces/";
                    if ( t.IsEnum ) category = "Enumerations/";
                    menu.AddItem(new GUIContent(category + friendlyName), typeList.Contains(t), Selected, t);
                    if ( t.Namespace != null && !namespaces.Contains(t.Namespace) ) {
                        namespaces.Add(t.Namespace);
                    }
                }

                menu.AddSeparator("/");
                foreach ( var ns in namespaces ) {
                    var path = "Whole Namespaces/" + ns.Replace(".", "/") + "/Add " + ns;
                    menu.AddItem(new GUIContent(path), false, Selected, ns);
                }

                menu.ShowAsBrowser("Add Preferred Type");
            }


            if ( GUILayout.Button("Generate AOTClasses.cs and link.xml Files", EditorStyles.miniButton) ) {
                if ( EditorUtility.DisplayDialog("Generate AOT Classes", "A script relevant to AOT compatibility for certain platforms will now be generated.", "OK") ) {
                    var path = EditorUtility.SaveFilePanelInProject("AOT Classes File", "AOTClasses", "cs", "");
                    if ( !string.IsNullOrEmpty(path) ) {
                        AOTClassesGenerator.GenerateAOTClasses(path, TypePrefs.GetPreferedTypesList(true).ToArray());
                    }
                }

                if ( EditorUtility.DisplayDialog("Generate link.xml File", "A file relevant to 'code stripping' for platforms that have code stripping enabled will now be generated.", "OK") ) {
                    var path = EditorUtility.SaveFilePanelInProject("AOT link.xml", "link", "xml", "");
                    if ( !string.IsNullOrEmpty(path) ) {
                        AOTClassesGenerator.GenerateLinkXML(path, TypePrefs.GetPreferedTypesList().ToArray());
                    }
                }

                AssetDatabase.Refresh();
            }

            GUILayout.BeginHorizontal();

            if ( GUILayout.Button("Reset Defaults", EditorStyles.miniButtonLeft) ) {
                if ( EditorUtility.DisplayDialog("Reset Preferred Types", "Are you sure?", "Yes", "NO!") ) {
                    TypePrefs.ResetTypeConfiguration();
                    typeList = TypePrefs.GetPreferedTypesList();
                    Save();
                }
            }

            if ( GUILayout.Button("Save Preset", EditorStyles.miniButtonMid) ) {
                var path = EditorUtility.SaveFilePanelInProject("Save Types Preset", "PreferredTypes", "typePrefs", "");
                if ( !string.IsNullOrEmpty(path) ) {
                    System.IO.File.WriteAllText(path, JSONSerializer.Serialize(typeof(List<System.Type>), typeList, null, true));
                    AssetDatabase.Refresh();
                }
            }

            if ( GUILayout.Button("Load Preset", EditorStyles.miniButtonRight) ) {
                var path = EditorUtility.OpenFilePanel("Load Types Preset", "Assets", "typePrefs");
                if ( !string.IsNullOrEmpty(path) ) {
                    var json = System.IO.File.ReadAllText(path);
                    typeList = JSONSerializer.Deserialize<List<System.Type>>(json);
                    Save();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            var syncPath = TypePrefs.SyncFilePath();
            EditorGUILayout.HelpBox(syncPath != null ? "List synced with file: " + syncPath.Replace(Application.dataPath, ".../Assets") : "No sync file found in '.../Assets/Editor Default Resources'. Types are currently saved in Unity EditorPrefs only.", MessageType.None);
            GUILayout.Space(5);

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            for ( int i = 0; i < typeList.Count; i++ ) {
                if ( EditorGUIUtility.isProSkin ) { GUI.color = Color.black.WithAlpha(i % 2 == 0 ? 0.3f : 0); }
                if ( !EditorGUIUtility.isProSkin ) { GUI.color = Color.white.WithAlpha(i % 2 == 0 ? 0.3f : 0); }
                GUILayout.BeginHorizontal("box");
                GUI.color = Color.white;
                var type = typeList[i];
                if ( type == null ) {
                    GUILayout.Label("MISSING TYPE", GUILayout.Width(300));
                    GUILayout.Label("---");
                } else {
                    var name = type.FriendlyName();
                    var icon = TypePrefs.GetTypeIcon(type);
                    GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                    GUILayout.Label(name, GUILayout.Width(300));
                    GUILayout.Label(type.Namespace);
                }
                if ( GUILayout.Button("X", GUILayout.Width(18)) ) {
                    RemoveType(type);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            AcceptDrops();
            Repaint();
        }


        //Handles Drag&Drop operations
        void AcceptDrops() {
            var e = Event.current;
            if ( e.type == EventType.DragUpdated ) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }

            if ( e.type == EventType.DragPerform ) {
                DragAndDrop.AcceptDrag();

                foreach ( var o in DragAndDrop.objectReferences ) {

                    if ( o == null ) {
                        continue;
                    }

                    if ( o is MonoScript ) {
                        var type = ( o as MonoScript ).GetClass();
                        if ( type != null ) {
                            AddType(type);
                        }
                        continue;
                    }

                    AddType(o.GetType());
                }
            }
        }

        ///<summary>Add a type</summary>
        void AddType(System.Type t) {
            if ( !typeList.Contains(t) ) {
                typeList.Add(t);
                Save();
                ShowNotification(new GUIContent(string.Format("Type '{0}' Added!", t.FriendlyName())));
                return;
            }

            ShowNotification(new GUIContent(string.Format("Type '{0}' is already in the list.", t.FriendlyName())));
        }

        ///<summary>Remove a type</summary>
        void RemoveType(System.Type t) {
            typeList.Remove(t);
            Save();
            ShowNotification(new GUIContent(string.Format("Type '{0}' Removed.", t.FriendlyName())));
        }

        ///<summary>Save changes</summary>
        void Save() {
            TypePrefs.SetPreferedTypesList(typeList);
        }
    }
}

#endif