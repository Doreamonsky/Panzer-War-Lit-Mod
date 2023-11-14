using System.Collections.Generic;
using System.IO;
using ShanghaiWindy.Editor;
using Simplygon;
using Simplygon.Unity.EditorPlugin;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Core
{
    public class SimplygonLodGenProvider : ISimplygonLODProvider
    {
        private static string outputBasePath = "Assets/Res/Cooked/SimplygonMesh";

        public Mesh ReduceGameObjectPolygon(GameObject go, string saveName, int index,
            SimplygonParam simplygonParam)
        {
            try
            {
                // Check folders
                var outputBaseFolder = new DirectoryInfo(outputBasePath);
                if (!outputBaseFolder.Exists)
                {
                    outputBaseFolder.Create();
                }

                using var _simplygon = Loader.InitSimplygon
                    (out var simplygonErrorCode, out var simplygonErrorMessage);

                if (simplygonErrorCode != EErrorCodes.NoError)
                {
                    Debug.LogError($"Simplygon Initializing failed! Message: {simplygonErrorMessage}");
                    return null;
                }

                var hash = AssetDatabase.GetAssetDependencyHash(
                    AssetDatabase.GetAssetPath(go.GetComponent<MeshFilter>().sharedMesh)).ToString();

                var assetFolderPath = $"{outputBasePath}/{saveName}-{index}";
                var assetFolder = new DirectoryInfo(assetFolderPath);
                var hashFile = new FileInfo($"{assetFolderPath}/lod.hash");

                if (assetFolder.Exists)
                {
                    if (hashFile.Exists && File.ReadAllText(hashFile.FullName) == hash)
                    {
                        Debug.Log($"Use local lod cache for lod {saveName}");
                        var assetPath = AssetDatabase.FindAssets("t:mesh", new string[] { assetFolderPath })[0];
                        return AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(assetPath));
                    }

                    assetFolder.Delete(true);
                }

                assetFolder.Create();
                AssetDatabase.Refresh();

                var exportTempDirectory = SimplygonUtils.GetNewTempDirectory();

                var instance = Object.Instantiate(go);
                for (var i = instance.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(instance.transform.GetChild(i).gameObject);
                }

                using var sgScene =
                    SimplygonExporter.Export(_simplygon, exportTempDirectory, new List<GameObject>() { instance });
                Object.DestroyImmediate(instance);


                using var reductionPipeline = _simplygon.CreateReductionPipeline();
                using var reductionSettings = reductionPipeline.GetReductionSettings();

                if (simplygonParam.IsUseRatio())
                {
                    reductionSettings.SetReductionTargets(EStopCondition.All, true, false, false, false);
                    reductionSettings.SetReductionTargetTriangleRatio(simplygonParam.polygonRatio);
                }
                else
                {
                    reductionSettings.SetReductionTargets(EStopCondition.All, false, true, false, false);
                    reductionSettings.SetReductionTargetTriangleCount(simplygonParam.polygonMaxPoly);
                }

                reductionPipeline.RunScene(sgScene, EPipelineRunMode.RunInThisProcess);

                var startingLodIndex = 0;
                var importedGameObjects = new List<GameObject>();

                SimplygonImporter.Import(_simplygon, reductionPipeline, ref startingLodIndex,
                    assetFolderPath, saveName, importedGameObjects);

                var mesh = importedGameObjects[0].GetComponent<MeshFilter>().sharedMesh;

                foreach (var importedGameObject in importedGameObjects)
                {
                    Object.DestroyImmediate(importedGameObject);
                }

                var textureGuids = AssetDatabase.FindAssets("t:texture2d", new string[] { assetFolderPath });
                foreach (var textureGuid in textureGuids)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(textureGuid));
                }

                File.WriteAllText(hashFile.FullName, hash);
                Debug.Log(mesh);
                return mesh;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to create lod " + e.Message + e.StackTrace);
                return null;
            }
        }
    }
}