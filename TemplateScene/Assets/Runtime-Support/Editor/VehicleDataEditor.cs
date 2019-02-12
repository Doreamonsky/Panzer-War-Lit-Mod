using ShanghaiWindy.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(VehicleData))]
[CanEditMultipleObjects]
public class VehicleDataEditor : EditorWindowBase
{
    VehicleData vehicleData;

    public override void Awake()
    {
        base.Awake();
        EditorHeadline = "ShanghaiWindy Ground Vehicle Manager";
    }

    public override void OnSelectionChanged()
    {
        base.OnSelectionChanged();
    }

    public override void ShortCut()
    {
        base.ShortCut();
        GameObject active = Selection.activeGameObject;
        var e = Event.current;
        if (e.keyCode == KeyCode.K)
        {
            if (active != null)
            {
                Vector3 P0 = active.transform.localPosition;
                Vector3 E0 = active.transform.localEulerAngles;
                vehicleData = (VehicleData)target;

                foreach (FieldInfo fieldInfo in vehicleData.cacheData.GetType().GetFields())
                {
                    if (active.name == fieldInfo.Name)
                    {
                        fieldInfo.SetValue(vehicleData.cacheData, new VehicleObjectTransformData()
                        {
                            localPosition = P0,
                            localEulerAngle = E0
                        });
                        e.Use();
                        EditorHeadline = string.Format("Transform:{0} is updated to asset at{1}", active.name, System.DateTime.Now);
                        Repaint();
                    }
                }
            }

        }
    }

    public override void OnInspectorGUI()
    {
        vehicleData = (VehicleData)target;


        if (InEditingSceneObject)
        {
            EditorGUILayout.HelpBox("Press Key [K] to save the position and rotation of the selected dump to the cache", MessageType.Error);
        }

        if (GUILayout.Button("Open Edit Mode"))
        {
            LockEditor();
            OpenEditorScene();

            InitTankPrefabs(vehicleData);
        }
        if (GUILayout.Button("Pack Asset"))
        {
            PackAsset(vehicleData);
        }
        if (GUILayout.Button("Try Vehicle"))
        {
            EditorSceneManager.OpenScene("Assets/Res/UnitTest/illustration.unity");
        }
        //EditorGUILayout.TextField ("Layout number", gd.);
        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private static void OpenEditorScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
    }

    public static TankInitSystem InitTankPrefabs(VehicleData vehicleData)
    {
        #region Collision Detect
        if (vehicleData.modelData.MainModel.GetComponentsInChildren<BoxCollider>().Length == 0)
        {
            if (EditorUtility.DisplayDialog("Error", "Collision should be set.We will redirct you to it.", "OK,l will set it now!", "No,l will choose another one"))
            {
                EditorGUIUtility.PingObject(vehicleData.modelData.MainModel.GetInstanceID());
                OpenEditorScene();

                GameObject EditColliderInstance = Instantiate<GameObject>(vehicleData.modelData.MainModel);
                EditColliderInstance.name = vehicleData.modelData.MainModel.name;

                string TempPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(vehicleData.modelData.MainModel.GetInstanceID()));
                string PrefabStoreDir = System.IO.Directory.CreateDirectory(string.Format(TempPath + "/Collisions_{0}", vehicleData.modelData.MainModel.name)).FullName;

                AssetDatabase.Refresh();
                return null;
                //PrefabUtility.CreatePrefab (string.Format (PrefabStoreDir + "/{0}.prefab", vehicleData.modelData.MainModel.name),EditColliderInstance);
            }
            else
            {
                return null;
            }
        }
        #endregion
        GameObject TankPrefabs = new GameObject();
        GameObject InstanceMesh = Instantiate(vehicleData.modelData.MainModel);

        try
        {
            #region Init
            InstanceMesh.name = vehicleData.vehicleTextData.AssetName;
            #endregion

            TankPrefabs.name = vehicleData.vehicleTextData.AssetName + "_Pre";

            InstanceMesh.transform.parent = TankPrefabs.transform;
            InstanceMesh.transform.localPosition = vehicleData.cacheData.StartPoint;

            GameObject VehicleDynamics = new GameObject("VehicleDynamics");
            VehicleDynamics.transform.parent = InstanceMesh.transform;

            Transform RightWheel, LeftWheel, RightUpperWheels, LeftUpperWheels, Turret, Gun, GunDym;
            #region Find Dumps
            RightWheel = InstanceMesh.transform.Find("RightWheel");
            LeftWheel = InstanceMesh.transform.Find("LeftWheel");
            Turret = InstanceMesh.transform.Find("Turret");
            Gun = Turret.transform.Find("Gun");
            GunDym = Gun.GetChild(0);


            RightUpperWheels = InstanceMesh.transform.Find("RightUpperWheel");
            LeftUpperWheels = InstanceMesh.transform.Find("LeftUpperWheel");
            #endregion
            RightWheel.parent = VehicleDynamics.transform;
            LeftWheel.parent = VehicleDynamics.transform;
            LeftUpperWheels.parent = VehicleDynamics.transform;
            RightUpperWheels.parent = VehicleDynamics.transform;

            #region FireSystem
            VehicleComponentsReferenceManager referenceManager = InstanceMesh.AddComponent<VehicleComponentsReferenceManager>();

            var mutiTurretTransList = new List<FireSystemTransform>();

            //多炮塔配置
            foreach (var multiTurret in vehicleData.vehicleTextData.MultiFireSystemData)
            {
                var mTurret = InstanceMesh.transform.Find(multiTurret.turretPath);
                var mGun = mTurret.Find(multiTurret.gunPath);
                var mDym = mGun.Find(multiTurret.dymPath);

                var turretParent = mTurret.parent;

                CreateWrapper(mDym, true);
                CreateWrapper(mGun, true);
                CreateWrapper(mTurret, true);

                mDym.parent.SetParent(mGun.parent);
                mGun.parent.SetParent(mTurret.parent);
                mTurret.parent.SetParent(turretParent);

                var gunFollowerTrans = new GameObject("GunFollowerTrans").transform;
                gunFollowerTrans.SetParent(mGun.parent);
                gunFollowerTrans.localPosition = multiTurret.gunFollowerOffSet;


                var turretFollowerTrans = new GameObject("TurretFollowerTrans").transform;
                turretFollowerTrans.SetParent(mGun.parent);
                turretFollowerTrans.localPosition = multiTurret.turretFollowerOffSet;



                //多炮塔毕竟少数
                var mffpoint = new GameObject("FFPoint").transform;
                mffpoint.SetParent(mDym.parent);
                mffpoint.rotation = mDym.parent.rotation;
                mffpoint.position = mDym.parent.position + mDym.parent.forward * multiTurret.ffPointOffSet;

                var meffectStart = new GameObject("EffectStart").transform;
                meffectStart.SetParent(mDym.parent);
                meffectStart.rotation = mDym.parent.rotation;
                meffectStart.position = mDym.parent.position + mDym.parent.forward * multiTurret.effectStartOffSet;

                //但是 我们依然设置 Icon 方便编辑
                IconManager.SetIcon(mffpoint.gameObject, IconManager.LabelIcon.Red);
                IconManager.SetIcon(meffectStart.gameObject, IconManager.LabelIcon.Red);

                mutiTurretTransList.Add(
                    new FireSystemTransform
                    {
                        turret = mTurret.parent,
                        gun = mGun.parent,
                        dym = mDym.parent,
                        ffPoint = mffpoint,
                        fireEffectPoint = meffectStart,
                        gunFollowerTrans = gunFollowerTrans,
                    });
            }
            referenceManager.mutiTurretTrans = mutiTurretTransList;

            GameObject TurretTransform = new GameObject("TurretTransform");
            GameObject GunTransform = new GameObject("GunTransform");
            GameObject GunDymTransform = new GameObject("GunDym");


            TurretTransform.transform.SetParent(InstanceMesh.transform);
            TurretTransform.transform.position = Turret.transform.position;

            GunTransform.transform.position = Gun.transform.position;
            Turret.parent = TurretTransform.transform;

            Gun.parent = GunTransform.transform;
            GunTransform.transform.SetParent(TurretTransform.transform);

            GunDymTransform.transform.position = GunDym.transform.position;
            GunDymTransform.transform.SetParent(GunTransform.transform);
            GunDym.SetParent(GunDymTransform.transform);
            #endregion


            AddDumpNode("FFPoint", GunTransform.transform, vehicleData, true, referenceManager);
            AddDumpNode("EffectStart", GunTransform.transform, vehicleData, true, referenceManager);
            AddDumpNode("FireForceFeedbackPoint", GunTransform.transform, vehicleData, true, referenceManager);
            AddDumpNode("EngineSmoke", InstanceMesh.transform, vehicleData, true, referenceManager);
            AddDumpNode("EngineSound", InstanceMesh.transform, vehicleData, true, referenceManager);
            AddDumpNode("MainCameraFollowTarget", InstanceMesh.transform, vehicleData, true, referenceManager);
            AddDumpNode("MainCameraGunner", GunTransform.transform, vehicleData, true, referenceManager);
            AddDumpNode("CenterOfGravity", InstanceMesh.transform, vehicleData, true, referenceManager);
            AddDumpNode("BotPoint", InstanceMesh.transform, vehicleData, true, referenceManager);

            referenceManager.LeftTrack = InstanceMesh.transform.Find("LeftTrack").gameObject;
            referenceManager.RightTrack = InstanceMesh.transform.Find("RightTrack").gameObject;

            referenceManager.LeftWheels = ResortWheelTrans(LeftWheel.GetComponentsInChildren<Transform>());
            referenceManager.RightWheels = ResortWheelTrans(RightWheel.GetComponentsInChildren<Transform>());

            referenceManager.LeftUpperWheels = ResortWheelTrans(LeftUpperWheels.GetComponentsInChildren<Transform>());
            referenceManager.RightUpperWheels = ResortWheelTrans(RightUpperWheels.GetComponentsInChildren<Transform>());

            GameObject HitBoxInstance = Instantiate<GameObject>(vehicleData.modelData.HitBox.HitBoxPrefab);
            HitBoxInstance.transform.position += vehicleData.cacheData.StartPoint;

            referenceManager.MainHitBox = HitBoxInstance.transform.Find("Main").gameObject;
            referenceManager.TurretHitBox = HitBoxInstance.transform.Find("Turret").gameObject;
            referenceManager.GunHitBox = HitBoxInstance.transform.Find("Gun").gameObject;
            referenceManager.DymHitBox = HitBoxInstance.transform.Find("Dym").gameObject;

            HitBoxInstance.transform.Find("Main").name = "MainHitBox";
            HitBoxInstance.transform.Find("MainHitBox").SetParent(InstanceMesh.transform);

            HitBoxInstance.transform.Find("Turret").name = "TurretHitBox";
            HitBoxInstance.transform.Find("TurretHitBox").SetParent(TurretTransform.transform);

            HitBoxInstance.transform.Find("Gun").name = "GunHitBox";
            HitBoxInstance.transform.Find("GunHitBox").SetParent(GunTransform.transform);

            HitBoxInstance.transform.Find("Dym").name = "DymHitBox";
            HitBoxInstance.transform.Find("DymHitBox").SetParent(GunDymTransform.transform);

            DestroyImmediate(HitBoxInstance);



            new GameObject("MainCameraTarget").transform.SetParent(TurretTransform.transform);

            TankInitSystem initySystem = TankPrefabs.AddComponent<TankInitSystem>();
            initySystem.vehicleTextData = vehicleData.vehicleTextData;

            #region Vehicle Physic
            var vehiclePhysic = InstanceMesh.AddComponent<Rigidbody>();
            vehiclePhysic.mass = vehicleData.vehicleTextData.PTCParameter.Mass;
            vehiclePhysic.drag = 0.1f;
            vehiclePhysic.angularDrag = 2.5f;
            vehiclePhysic.useGravity = true;
            referenceManager.VehiclePhysic = vehiclePhysic;
            #endregion

            referenceManager.VehicleDynamics = VehicleDynamics;

            referenceManager.TurretGameObject = TurretTransform;
            referenceManager.GunGameObject = GunTransform;
            referenceManager.GunDymGameObject = GunDymTransform;

            return initySystem;
        }
        catch (System.Exception exception)
        {
            EditorUtility.DisplayDialog("Exception", exception.Message + "\n" + exception.StackTrace, "OK");
            DestroyImmediate(TankPrefabs);
            throw exception;
        }

    }


    public static Transform[] ResortWheelTrans(Transform[] t)
    {
        Transform[] ReturnTransform = new Transform[t.Length - 1];
        int i;
        for (i = 0; i < t.Length; i++)
        {
            if (i != 0)
            {
                ReturnTransform[i - 1] = t[i];
            }
        }
        Transform temp = null;
        for (i = 0; i < ReturnTransform.Length - 1; i++)
        {
            for (int j = i + 1; j < ReturnTransform.Length; j++)
            {
                if (ReturnTransform[i].localPosition.y > ReturnTransform[j].localPosition.y)
                {
                    temp = ReturnTransform[i];
                    ReturnTransform[i] = ReturnTransform[j];
                    ReturnTransform[j] = temp;
                }
                ReturnTransform[i].name = "w" + i.ToString();
                ReturnTransform[i].SetAsLastSibling();
            }

        }
        ReturnTransform[ReturnTransform.Length - 1].name = "w" + (ReturnTransform.Length - 1).ToString();
        ReturnTransform[ReturnTransform.Length - 1].SetAsLastSibling();

        return ReturnTransform;
    }

    public static void PackAsset(VehicleData vehicleData)
    {
        string CurrentAssetName = vehicleData.vehicleTextData.AssetName;

        string Path = "Assets/res/Cooked/" + CurrentAssetName.ToLower();

        TankInitSystem tankInitSystem = InitTankPrefabs(vehicleData);

        GameObject Prefab = tankInitSystem.transform.GetChild(0).gameObject;

        #region 游戏内模型 预制体处理
        GameObject Origin = PrefabUtility.CreatePrefab(Path + "_Pre.prefab", Prefab); // 要打包的物体



        #region 客户端打包文件
        AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Origin));
        assetImporter.assetBundleName = CurrentAssetName + "_Pre";
        assetImporter.assetBundleVariant = "clientextramesh";
        #endregion


        ProjectWindowUtil.ShowCreatedAsset(tankInitSystem.gameObject);
        #endregion

        #region 服务器资源处理

        foreach (MeshRenderer meshRenderer in Prefab.GetComponentsInChildren<MeshRenderer>())
        {
            DestroyImmediate(meshRenderer);
        }
        foreach (MeshFilter mesh in Prefab.GetComponentsInChildren<MeshFilter>())
        {
            DestroyImmediate(mesh);
        }


        #region 服务器打包文件
        GameObject DelicateAsset = PrefabUtility.CreatePrefab("Assets/res/Cooked/DelicatedServer/" + CurrentAssetName.ToLower() + "_Pre" + ".prefab", Prefab);
        assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(DelicateAsset));
        assetImporter.SetAssetBundleNameAndVariant(CurrentAssetName + "_Pre", "masterextramesh");
        #endregion

        #endregion

        DestroyImmediate(Prefab);
        DestroyImmediate(tankInitSystem.gameObject);

    }
    private void SetLOD(TankInitSystem root)
    {
        Transform vehicleComponentRoot = root.transform.GetChild(0); //主要组件位置

        MeshFilter[] OriginalMeshes = root.GetComponentsInChildren<MeshFilter>();

        if (vehicleData.modelData.LOD == null)
        {
            return;
        }

        GameObject VehicleLOD = Instantiate(vehicleData.modelData.LOD);
        VehicleLOD.name = vehicleData.modelData.LOD.name;
        VehicleLOD.transform.SetParent(vehicleComponentRoot);


        LODGroup MainLODgroup = vehicleComponentRoot.gameObject.AddComponent<LODGroup>();

        List<Renderer> MainMainMeshes = new List<Renderer>();
        List<Renderer> MainLODMeshes = new List<Renderer>();

        GetRenderByName(vehicleComponentRoot.transform, ref MainMainMeshes, "MainBody", "TankTransform", "TurretTransform");
        GetRenderByName(VehicleLOD.transform, ref MainLODMeshes, "LeftUpperWheel", "LeftWheel", "MainBody", "RightUpperWheel", "RightWheel", "Turret");

        MainLODgroup.SetLODs(new LOD[] {
            new LOD (0.25f, MainMainMeshes.ToArray()),
            new LOD (0, MainLODMeshes.ToArray ())
        });



        Transform TurretTransform = vehicleComponentRoot.transform.Find("TurretTransform");

        Transform Turret = TurretTransform.Find("Turret");
        Transform Gun = TurretTransform.Find("GunTransform/Gun");
        Transform Dym = TurretTransform.Find("GunTransform/GunDym/Dym");

        Transform LODTurret = VehicleLOD.transform.Find("Turret_LOD");
        Transform LODGun = LODTurret.GetChild(0);
        Transform LODDym = LODGun.GetChild(0);

        LODTurret.SetParent(Turret.parent);
        LODGun.SetParent(Gun.parent);
        LODDym.SetParent(Dym.parent);
    }


    private static void AddDumpNode(string dumpName, Transform parent, VehicleData vehicleData, bool isAddedToReference = false, VehicleComponentsReferenceManager referenceManager = null)
    {
        GameObject DumpNode = new GameObject(dumpName);
        DumpNode.transform.SetParent(parent);
        DumpNode.transform.localPosition = ((VehicleObjectTransformData)vehicleData.cacheData.GetType().GetField(dumpName).GetValue(vehicleData.cacheData)).localPosition;
        DumpNode.transform.localEulerAngles = ((VehicleObjectTransformData)vehicleData.cacheData.GetType().GetField(dumpName).GetValue(vehicleData.cacheData)).localEulerAngle;
        IconManager.SetIcon(DumpNode, IconManager.LabelIcon.Orange);
        if (isAddedToReference)
        {
            referenceManager.GetType().GetField(dumpName).SetValue(referenceManager, DumpNode);
        }
    }

    private static void CreateWrapper(Transform trans, bool isIdentityRot = false)
    {
        var wrapper = new GameObject("Wrapper").transform;
        wrapper.SetParent(trans.parent);

        wrapper.localPosition = trans.localPosition;
        wrapper.rotation = isIdentityRot ? Quaternion.identity : trans.rotation;
        wrapper.localScale = trans.localScale;

        trans.SetParent(wrapper);
    }

    private static Transform[] GetChilds(Transform t)
    {
        return t.GetComponentsInChildren<Transform>().Where(child => child.parent == t).ToArray();
    }

    private static void GetRenderByName(Transform Root, ref List<Renderer> RenderList, params string[] RootChildMatchNames)
    {
        foreach (Transform ChildT in GetChilds(Root))
        {
            bool isNameValid = false;

            foreach (string MatchName in RootChildMatchNames)
            {
                if (ChildT.name.Contains(MatchName))
                {
                    isNameValid = true;
                }
            }

            if (!isNameValid)
            {
                continue;
            }

            foreach (Renderer Child in ChildT.GetComponentsInChildren<Renderer>())
            {
                if (Child.GetComponent<HitBox>() != null)
                {
                    continue;
                }

                RenderList.Add(Child);
            }
        }
    }
}
