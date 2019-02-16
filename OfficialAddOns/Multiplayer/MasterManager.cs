using Multiplayer.Msg;
using NetworkCommsDotNet;
using ShanghaiWindy.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MasterManager : MonoBehaviour
    {
        public System.Action<PlayerInfo> OnPlayerDisconnected;

        public System.Action<PlayerInfo> OnNewPlayerConnected;

        public List<PlayerInfo> PlayerList = new List<PlayerInfo>();

        public List<GeneratePlayerVehicle> VehicleList = new List<GeneratePlayerVehicle>();

        private int allocatedVehicleID = 0;

        private int allocatedPlayerID = 0;

        /// <summary>
        /// All tasks related to UnityEngine should be done on the Main Thread.  qwq.
        /// </summary>
        private Queue<System.Action> mainThreadTasks = new Queue<System.Action>();

        private void Start()
        {
            var freeCamera = new GameObject("Free Camera",typeof(FreeCamera),typeof(Camera));

            //Notify New Player
            OnNewPlayerConnected += (playerInfo) =>
            {
                Debug.LogError($"New Player With UID:{playerInfo.Uid} Joined the room. Assigned ID : {playerInfo.PlayerID}");
            };

            //Spawn Player Vehicle once They joined. You can change the spawn policy  to your perference
            OnNewPlayerConnected += (playerInfo) =>
            {
                mainThreadTasks.Enqueue(() =>
                {
                    StartCoroutine(SpawnVehicle(playerInfo));
                });
            };

            //Notfiy New Disconnected
            OnPlayerDisconnected += (playerInfo) =>
            {
                Debug.LogError($"Player: {playerInfo.PlayerID} has left the room!");
            };

            OnPlayerDisconnected += (playerInfo) =>
            {
                PlayerList.Remove(playerInfo);

                var playerVehicle = VehicleList.Find(val => val.OwnerPlayerID == playerInfo.PlayerID);

                //Player Vehicle May Destroyed
                if (playerVehicle != null)
                {
                    Debug.LogError($"Remove Player Vehicle!{playerVehicle.VehicleID}");

                    VehicleList.Remove(playerVehicle);

                    mainThreadTasks.Enqueue(() =>
                    {
                        VehicleUtility.RemoveVehicle(playerVehicle.tankInitSystem);
                    });
                }
            };

            NetManager.StartAsMaster(6576, new ServerListenEvents()
            {
                onRecLoginInfo = (header, connection, loginInfo) =>
                {
                    //Assign PlayerInfo
                    var playerID = allocatedPlayerID;

                    var playerInfo = new PlayerInfo(loginInfo.Uid, playerID, TeamManager.Team.red, connection);

                    allocatedPlayerID += 1;

                    PlayerList.Add(playerInfo);

                    OnNewPlayerConnected?.Invoke(playerInfo);

                    connection.SendObject("PlayerInfo", playerInfo);

                    connection.AppendShutdownHandler(onShutDown =>
                    {
                        OnPlayerDisconnected?.Invoke(playerInfo);
                    });
                },
                onRecSyncPlayerInput = (header, connection, syncPlayerInput) =>
               {
                   mainThreadTasks.Enqueue(() =>
                   {
                       var vehicle = VehicleList.Find(v => v.VehicleID == syncPlayerInput.VehicleID);

                       var ptc = vehicle?.tankInitSystem?.vehicleComponents?.playerTracksController;

                       if (ptc != null)
                       {
                           ptc.accelG = syncPlayerInput.Xinput;
                           ptc.steerG = syncPlayerInput.Yinput;

                           var turretTarget = vehicle.tankInitSystem.vehicleComponents.mainTurretController?.target;

                           if (turretTarget != null)
                           {
                               turretTarget.position = syncPlayerInput.LookTargetPos.CovertToUnityV3();
                           }
                       }

                   });
               }
            });

            StartCoroutine(SyncVehicleLoop());
        }

        private IEnumerator SpawnVehicle(PlayerInfo playerInfo)
        {
            var vehicleLogic = ScriptableObject.CreateInstance<MasterPlayerVehicleLogic>();

            var startPoint = GameObject.FindGameObjectWithTag("TeamAStartPoint").transform.position;

            var connection = playerInfo.clientConnection;

            if (!connection.ConnectionAlive())
            {
                connection.EstablishConnection();
                Debug.LogError($"Reestablish Connection");
            }

            //Notify New Player to Spawn Other Player Vehicle
            for (int i = 0; i < VehicleList.Count; i++)
            {
                yield return new WaitForSeconds(0.25f);

                connection.SendObject("GeneratePlayerVehicle", VehicleList[i]);

                Debug.LogError($"{playerInfo.clientConnection}  Vehicle ID {VehicleList[i].VehicleID} PlayerID {VehicleList[i].OwnerPlayerID} {connection.ConnectionAlive()}");
            }

            //Generate Player Vehicle
            var vehicle = VehicleUtility.CreateVehicle("T-34", InstanceNetType.GameNetworkBotOffline, vehicleLogic);

            vehicle.transform.position = startPoint;

            var generateData = new GeneratePlayerVehicle(allocatedVehicleID, "T-34", playerInfo.PlayerID, vehicle);

            VehicleList.Add(generateData);

            allocatedVehicleID += 1;

            //Notify All Players To Spawn New Player Vehicle
            foreach (var player in PlayerList)
            {
                player.clientConnection.SendObject("GeneratePlayerVehicle", generateData);
            }
        }

        private void Update()
        {
            while (mainThreadTasks.Count != 0)
            {
                var task = mainThreadTasks.Dequeue();
                task?.Invoke();
            }

        }

        private void OnDestroy()
        {
            NetworkComms.Shutdown();
        }


        /// <summary>
        /// Sync Vehicle Position Rotation on server ... to the clients
        /// </summary>
        /// <returns></returns>
        private IEnumerator SyncVehicleLoop()
        {
            while (true)
            {
                foreach (var vehicle in VehicleList)
                {
                    if (vehicle?.tankInitSystem?.vehicleComponents?.playerTracksController == null)
                    {
                        continue;
                    }

                    var internalPos = vehicle.tankInitSystem.vehicleComponents.playerTracksController.transform.position;

                    var pos = new ProtobufVector3(internalPos);

                    var internalRot = vehicle.tankInitSystem.vehicleComponents.playerTracksController.transform.rotation;

                    var rot = new ProtobufQuaternion(internalRot.x, internalRot.y, internalRot.z, internalRot.w);


                    var internalVelocity = vehicle.tankInitSystem.vehicleComponents.playerTracksController.GetComponent<Rigidbody>().velocity;

                    var velocity = new ProtobufVector3(internalVelocity);

                    var internalLookTargetPos = vehicle.tankInitSystem.vehicleComponents.mainTurretController.target.position;

                    var targetPos = new ProtobufVector3(internalLookTargetPos);

                    var syncVehicle = new SyncVehicle(vehicle.VehicleID, pos, velocity, rot, targetPos);


                    //Sync Position To All Players
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].clientConnection.SendObject("SyncVehicle", syncVehicle);
                    }
                }
                yield return new WaitForSeconds(NetManager.UpdateInterval);
            }
        }

    }
}
