using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityMod;

namespace UnityModExample
{
    public class PlaygroundCheaterPlayerData : IVehicleAddOn
    {
        public static TankInitSystem playerVehicle;

        public void OnVehicleLoaded(int instanceID)
        {
            foreach (var vehicle in GameObject.FindObjectsOfType<TankInitSystem>())
            {
                if (vehicle.gameObject.GetInstanceID() == instanceID)
                {
                    if (vehicle._InstanceNetType == InstanceNetType.GameNetWorkOffline)
                    {
                        playerVehicle = vehicle;

                        var defaultReloadTime = playerVehicle.vehicleComponents.tankFire.tankFireParams.ReloadTime;
                        var defaultAdvanceReloadTime = playerVehicle.vehicleComponents.tankFire.tankFireParams.advanceFireClass.LargeReloadTime;

                        //Fast Reload Binding
                        PlaygroundCheater.OnToggleFastReload = (state) =>
                        {
                            if (state)
                            {
                                playerVehicle.vehicleComponents.tankFire.tankFireParams.ReloadTime = 0.5f;
                                playerVehicle.vehicleComponents.tankFire.tankFireParams.advanceFireClass.LargeReloadTime = 0.5f;
                            }
                            else
                            {
                                playerVehicle.vehicleComponents.tankFire.tankFireParams.ReloadTime = defaultReloadTime;
                                playerVehicle.vehicleComponents.tankFire.tankFireParams.advanceFireClass.LargeReloadTime = defaultAdvanceReloadTime;
                            }
                        };
                    }
                }
            }
        }
    }

    public class PlaygroundCheater : IGeneralAddOn
    {
        public static string currentScene;

        public static System.Action<bool> OnToggleFastReload;

        private bool isFastReload = false;

        private Rect winRect = new Rect(45, 45, 400, 600);

        private bool isAttackable = true;

        private readonly List<string> vehicleSpawn = new List<string>()
        {
            "T-34","KV2","T-44","M4_Sherman","T54E1","M36","IS","ISU-152","Leopard1A5","Bat_Chatillon25t"
        };

        public void OnFixedUpdate()
        {

        }

        public void OnInitialized()
        {

        }

        public void OnNewSceneLoaded(string name)
        {
            currentScene = name;
        }

        public void OnUpdate()
        {

        }

        public void OnUpdateGUI()
        {
            GUILayout.Label("Playground Cheater Mod Active. Author:Doreamonsky");

            if (!currentScene.Contains("Training"))
            {
                return;
            }

            if (PlaygroundCheaterPlayerData.playerVehicle == null)
            {
                return;
            }

            winRect = GUI.Window(1, winRect, (winID) =>
             {
                 GUILayout.Label("Author: Doreamonsky 超级哆啦酱.");

                 GUILayout.Space(25);

                 GUILayout.Label("Property Modfication");

                 GUILayout.Label($"Fast Reload State:{isFastReload.ToString()}");

                 if (GUILayout.Button("Fast Reload Toggle"))
                 {
                     isFastReload = !isFastReload;

                     OnToggleFastReload?.Invoke(isFastReload);
                 }

                 GUILayout.Space(25);

                 GUILayout.Label("Spawn Manager");

                 isAttackable = GUILayout.Toggle(isAttackable, "is Attackable");


                 foreach (var vehicle in vehicleSpawn)
                 {
                     if (GUILayout.Button($"Spawn: {vehicle}"))
                     {
                         var rv = Random.insideUnitCircle;

                         var expectPos = new Vector3(rv.x, 0, rv.y) * Random.Range(10, 150);

                         var navHit = new NavMeshHit();

                         var isHit = NavMesh.SamplePosition(expectPos, out navHit, 500, 1 << 0);

                         if (isHit)
                         {
                             CreateBot(vehicle, PunTeams.Team.blue, isAttackable ? ScriptableObject.CreateInstance<SimpleBotLogic>() as BotLogic : ScriptableObject.CreateInstance<TrainBotLogic>() as BotLogic, navHit.position, Vector3.zero);
                         }
                     }
                 }

     


                 GUI.DragWindow();
             }, "Cheater");
        }

        private void CreateBot(string _vehicle, PunTeams.Team _botTeam, BotLogic botLogic, Vector3 pos, Vector3 euler)
        {
            TankInitSystem vehicle = new GameObject("Vehicle", typeof(TankInitSystem)).GetComponent<TankInitSystem>();
            vehicle.VehicleName = _vehicle;
            vehicle._InstanceNetType = InstanceNetType.GameNetworkBotOffline;
            vehicle.ownerTeam = _botTeam;

            vehicle.BulletCountList = new int[] { 1000, 1000, 1000 };
            vehicle.thinkLogic = botLogic;

            vehicle.InitTankInitSystem();
            vehicle.transform.position = pos;
            vehicle.transform.eulerAngles = euler;
        }
    }
}
