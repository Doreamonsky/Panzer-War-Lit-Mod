#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace ParadoxNotion.Design
{

    ///<summary> AssetDatabase related utility</summary>
	partial class EditorUtils
    {

        ///<summary>Create asset of type T with a dialog prompt to chose path</summary>
        public static T CreateAsset<T>() where T : ScriptableObject {
            return (T)CreateAsset(typeof(T));
        }

        ///<summary>Create asset of type T at target path</summary>
        public static T CreateAsset<T>(string path) where T : ScriptableObject {
            return (T)CreateAsset(typeof(T), path);
        }

        ///<summary>Create asset of type and show or not the File Panel</summary>
        public static ScriptableObject CreateAsset(System.Type type) {
            ScriptableObject asset = null;
            var path = EditorUtility.SaveFilePanelInProject(
                        "Create Asset of type " + type.ToString(),
                           type.Name + ".asset",
                        "asset", "");
            asset = CreateAsset(type, path);
            return asset;
        }

        ///<summary>Create asset of type at target path</summary>
        public static ScriptableObject CreateAsset(System.Type type, string path) {
            if ( string.IsNullOrEmpty(path) ) {
                return null;
            }
            ScriptableObject data = null;
            data = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            return data;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Get a unique path at current project selection for creating an asset, providing the "filename.type"</summary>
        public static string GetAssetUniquePath(string fileName) {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if ( path == "" ) {
                path = "Assets";
            }
            if ( System.IO.Path.GetExtension(path) != "" ) {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            return AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName);
        }

        ///<summary>Get MonoScript reference from type if able</summary>
        public static MonoScript MonoScriptFromType(System.Type targetType) {
            if ( targetType == null ) return null;
            var typeName = targetType.Name;
            if ( targetType.IsGenericType ) {
                targetType = targetType.GetGenericTypeDefinition();
                typeName = typeName.Substring(0, typeName.IndexOf('`'));
            }
            return AssetDatabase.FindAssets(string.Format("{0} t:MonoScript", typeName))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .FirstOrDefault(m => m != null && m.GetClass() == targetType);
        }

        ///<summary>Opens the MonoScript of a type if able</summary>
        public static bool OpenScriptOfType(System.Type type) {
            var mono = MonoScriptFromType(type);
            if ( mono != null ) {
                AssetDatabase.OpenAsset(mono);
                return true;
            }
            ParadoxNotion.Services.Logger.Log(string.Format("Can't open script of type '{0}', because a script with the same name does not exist.", type.FriendlyName()));
            return false;
        }

        ///<summary>Asset path to absolute system path</summary>
        public static string AssetToSystemPath(UnityEngine.Object obj) {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            return AssetToSystemPath(assetPath);
        }

        ///<summary>Asset path to absolute system path</summary>
        public static string AssetToSystemPath(string assetPath) {
            if ( !string.IsNullOrEmpty(assetPath) ) {
                return Application.dataPath.Remove(Application.dataPath.LastIndexOf('/')) + '/' + assetPath;
            }
            return null;
        }

        ///<summary>Get all scene names (added in build settings)</summary>
        public static List<string> GetSceneNames() {
            var allSceneNames = new List<string>();
            foreach ( var scene in EditorBuildSettings.scenes ) {
                if ( scene.enabled ) {
                    var name = scene.path.Substring(scene.path.LastIndexOf("/") + 1);
                    name = name.Substring(0, name.Length - 6);
                    allSceneNames.Add(name);
                }
            }

            return allSceneNames;
        }

    }
}


#endif