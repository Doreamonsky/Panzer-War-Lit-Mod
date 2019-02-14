using ShanghaiWindy.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ShanghaiWindy.Editor
{

    [CustomEditor(typeof(VehicleHitBox))]
    public class VehicleHitBoxEditor : EditorWindowBase
    {
        public VehicleHitBox vehicleHitBox;

        private GameObject ReferModelInstance;

        private GameObject HitBoxInstance;

        private HitBox externalArmorSelected;

        private HitBox.HitBoxType armorType;

        private int ArmorThickness;





        public override void Awake()
        {
            base.Awake();
            EditorHeadline = "ShanghaiWindy HitBox Editor";
            AssetDatabase.SetLabels(target, new string[] { "VehicleHitBox" });
        }

        public override void OnSelectionChanged()
        {
            if (Selection.activeGameObject != null)
            {
                externalArmorSelected = Selection.activeGameObject.GetComponent<HitBox>();
                if (externalArmorSelected != null)
                {
                    armorType = externalArmorSelected.hitBoxType;
                    ArmorThickness = externalArmorSelected.Armor;
                }


            }
        }

        public override void OnInspectorGUI()
        {
            vehicleHitBox = (VehicleHitBox)target;

            base.OnInspectorGUI();

            if (vehicleHitBox.HitBoxPrefab == null)
            {
                if (GUILayout.Button("Generate Prefab"))
                {
                    GenerateHitBoxPrefab();
                }
                return;
            }

            if (GUILayout.Button("Open Edit Mode"))
            {
                LockEditor();

                OpenEditorScene();

                HitBoxInstance = (GameObject)PrefabUtility.InstantiatePrefab(vehicleHitBox.HitBoxPrefab);

                Selection.activeGameObject = HitBoxInstance;
            }

            if (InEditingSceneObject)
            {

                if (GUILayout.Button("Save"))
                {
                    vehicleHitBox.HitBoxPrefab = PrefabUtility.ReplacePrefab(HitBoxInstance, vehicleHitBox.HitBoxPrefab, ReplacePrefabOptions.ConnectToPrefab);
                }

                if (ReferModelInstance != null)
                {
                    if (GUILayout.Button("Delete Refer Model"))
                    {
                        DestroyImmediate(ReferModelInstance);
                    }
                }

                if (GUILayout.Button("Generate Refer Model"))
                {
                    GenerateReferModel();
                }

                if (GUILayout.Button("Generate HitBox Model"))
                {
                    GenerateExternalArmor();
                }




                if (externalArmorSelected != null)
                {
                    armorType = (HitBox.HitBoxType)EditorGUILayout.EnumPopup("Armor Type", armorType);
                    ArmorThickness = EditorGUILayout.IntField("ThickNess", ArmorThickness);
                    //      private bool isCommander, isDriver, isGunner, isRadioer, isReloader;

                    switch (armorType)
                    {
                        case HitBox.HitBoxType.Simple:
                            {
                                EditorGUILayout.LabelField("Const : 1");
                                break;
                            }
                    }

                }



                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);

                    if (externalArmorSelected != null)
                    {
                        externalArmorSelected.hitBoxType = armorType;
                        externalArmorSelected.Armor = ArmorThickness;
                    }


                }
            }
        }

        private void GenerateHitBoxPrefab()
        {
            Transform HitBoxTransform = new GameObject(vehicleHitBox.name).transform;



            new GameObject("Main").transform.SetParent(HitBoxTransform);
            new GameObject("Turret").transform.SetParent(HitBoxTransform);
            new GameObject("Gun").transform.SetParent(HitBoxTransform);
            new GameObject("Dym").transform.SetParent(HitBoxTransform);



            vehicleHitBox.HitBoxPrefab = PrefabUtility.CreatePrefab(string.Format("Assets/Res/Vehicles/Ground/Data/HitBox/{0}.prefab", vehicleHitBox.name), HitBoxTransform.gameObject);
        }

        private void GenerateExternalArmor()
        {
            GameObject ExternalArmorModel = Instantiate<GameObject>(vehicleHitBox.ExternalArmorModel);
            foreach (Transform externalArmorModel in ExternalArmorModel.GetComponentsInChildren<Transform>())
            {
                if (externalArmorModel.GetComponent<MeshFilter>() != null)
                {
                    MeshCollider collider = externalArmorModel.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = externalArmorModel.GetComponent<MeshFilter>().sharedMesh;

                    Rigidbody physics = externalArmorModel.gameObject.AddComponent<Rigidbody>();
                    physics.isKinematic = true;
                    physics.useGravity = false;

                    externalArmorModel.gameObject.layer = LayerMask.NameToLayer("HitBox");
                    externalArmorModel.gameObject.AddComponent<HitBox>();

                }
            }
        }



        private void GenerateReferModel()
        {
            ReferModelInstance = Instantiate<GameObject>(vehicleHitBox.ReferModel);
            foreach (MeshRenderer meshRender in ReferModelInstance.GetComponentsInChildren<MeshRenderer>())
            {
                meshRender.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                meshRender.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                meshRender.material.SetInt("_ZWrite", 0);
                meshRender.material.DisableKeyword("_ALPHATEST_ON");
                meshRender.material.DisableKeyword("_ALPHABLEND_ON");
                meshRender.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                meshRender.material.renderQueue = 3000;
            }
        }

        void OpenEditorScene()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        }


    }
}