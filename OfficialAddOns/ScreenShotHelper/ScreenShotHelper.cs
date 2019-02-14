using ShanghaiWindy.Core;
using System.Collections;
using UnityEngine;
using UnityMod;

namespace ScreenShotHelper
{
    public class VehicleTurretHelper : IVehicleAddOn
    {
        private static TankInitSystem playerVehicle;

        public void OnVehicleLoaded(int instanceID)
        {
            foreach (var vehicle in GameObject.FindObjectsOfType<TankInitSystem>())
            {
                if (vehicle.gameObject.GetInstanceID() == instanceID)
                {
                    if (vehicle._InstanceNetType == InstanceNetType.GameNetWorkOffline)
                    {
                        playerVehicle = vehicle;
                    }
                }
            }
        }

        public static void LockTurret()
        {
            if (playerVehicle != null)
            {
                playerVehicle.vehicleComponents.mainTurretController.isLocked = true;
            }
        }

        public static void UnLockTurret()
        {
            if (playerVehicle != null)
            {
                playerVehicle.vehicleComponents.mainTurretController.isLocked = false;
            }
        }
    }
    public class ScreenShotHelper : IGeneralAddOn
    {
        private Rect winRect = new Rect(45, 45, 400, 300);

        private bool isClose = false;

        private bool takeScreenShot = false;

        private Canvas[] canvasList;

        public void OnFixedUpdate()
        {
        }

        public void OnInitialized()
        {
        }

        public void OnNewSceneLoaded(string name)
        {
            isClose = false;
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ToTakeScreenShot();
            }
        }

        public void OnUpdateGUI()
        {
            if (isClose || takeScreenShot)
            {
                return;
            }

            winRect = GUI.Window(2, winRect, (id) =>
            {
                if (GUILayout.Button("Take a ScreenShot", GUILayout.Height(50)))
                {
                    ToTakeScreenShot();
                }
                GUILayout.Space(15);

                if (GUILayout.Button("Lock Player Turret", GUILayout.Height(50)))
                {
                    VehicleTurretHelper.LockTurret();
                }
                GUILayout.Space(15);

                if (GUILayout.Button("UnLock Player Turret", GUILayout.Height(50)))
                {
                    VehicleTurretHelper.UnLockTurret();
                }
                GUILayout.Space(15);

                if (GUILayout.Button("Close", GUILayout.Height(50)))
                {
                    isClose = true;
                }

                GUI.DragWindow();

            }, "Game Screen Shot(PC:Press F12 to take screenshot)");
        }

        private void ToTakeScreenShot()
        {
            canvasList = Object.FindObjectsOfType<Canvas>();

            foreach (var canvas in canvasList)
            {
                canvas.gameObject.SetActive(false);
            }

            Object.FindObjectOfType<AssetLoader>().StartCoroutine(TakeScreenShot());

            takeScreenShot = true;
        }

        private IEnumerator TakeScreenShot()
        {
            yield return new WaitForSeconds(0.5f);

            var screenShotId = PlayerPrefs.GetInt("screenShotId", 0) + 1;

            ScreenCapture.CaptureScreenshot($"{screenShotId}.png");

            PlayerPrefs.SetInt("screenShotId", screenShotId);

            yield return new WaitForSeconds(0.5f);

            foreach (var canvas in canvasList)
            {
                canvas.gameObject.SetActive(true);
            }

            takeScreenShot = false;

            yield break;
        }
    }
}
