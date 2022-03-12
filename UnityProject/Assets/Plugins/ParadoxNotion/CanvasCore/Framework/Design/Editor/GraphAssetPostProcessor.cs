#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.IO;

namespace NodeCanvas.Editor
{
    ///<summary>Handles post processing of graph assets</summary>
    public class GraphAssetPostProcessor
    {

        [InitializeOnLoadMethod]
        static void PreInit() {
            EditorApplication.delayCall -= Init;
            EditorApplication.delayCall += Init;
        }

        static void Init() {

            // TODO: HOTFIX COMMENT OUT UNTIL FURTHER DEVELOPEMENT.
            // THIS WAS CREATING ISSUES, THUS JSON APPEND TO ASSET FEATURE REMOVED FOR NOW!
            // #if UNITY_2019_3_OR_NEWER
            //             ParadoxNotion.Design.AssetTracker.onAssetsImported -= OnAssetsImported;
            //             ParadoxNotion.Design.AssetTracker.onAssetsImported += OnAssetsImported;
            // #endif


            //we track graph assets so that we can access them on a diff thread
            AssetTracker.BeginTrackingAssetsOfType(typeof(Graph));
        }

        /*
                private const string SERIALIZATION_START = "#---GRAPH_START---";
                private const string SERIALIZATION_END = "#---GRAPH_END---";
                private const string IS_YAML = "%YAML";

                //Asset Tracker callback
                static void OnAssetsImported(string[] paths) {
                    var willRefresh = false;
                    foreach ( var path in paths ) {
                        var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Graph));
                        if ( asset is Graph ) {
                            willRefresh = true;
                            AppendPrettyJSONComments((Graph)asset, path);
                        }
                    }
                    if ( willRefresh ) { EditorApplication.delayCall += () => AssetDatabase.Refresh(); }
                }

                ///<summary>Append prety json to yaml file as comments for version control diff purposes</summary>
                static void AppendPrettyJSONComments(Graph graph, string path) {
                    var systemPath = EditorUtils.AssetToSystemPath(path);
                    var lines = File.ReadAllLines(systemPath);

                    //not a yaml? bail out.
                    if ( lines.Length == 0 || !lines[0].StartsWith(IS_YAML) ) { return; }

                    var result = new List<string>(lines.Length);

                    //clear previous. Unity actually does not keep any changes made to the file, but I don't trust this will always be the case.
                    var skip = false;
                    for ( var i = 0; i < lines.Length; i++ ) {
                        var line = lines[i];
                        if ( line.StartsWith(SERIALIZATION_START) ) { skip = true; }
                        if ( skip ) { continue; }
                        if ( line.StartsWith(SERIALIZATION_END) ) {
                            skip = false;
                            continue;
                        }
                        result.Add(line);
                    }

                    //add new
                    result.Add(SERIALIZATION_START);
                    result.Add("#The pretty formatted json serialization bellow is only a reference to help in version control diff. Other than that it is not used at all.");
                    var pretyJson = ParadoxNotion.Serialization.JSONSerializer.PrettifyJson(graph.GetSerializedJsonData());
                    var split = pretyJson.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);
                    for ( var i = 0; i < split.Length; i++ ) {
                        var newLine = '#' + split[i];
                        result.Add(newLine);
                    }
                    result.Add(SERIALIZATION_END);

                    File.WriteAllLines(systemPath, result);
                }
        */

    }
}

#endif