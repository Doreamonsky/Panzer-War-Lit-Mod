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
        public List<PlayerInfo> PlayerList = new List<PlayerInfo>();

        public List<GeneratePlayerVehicle> PlayerVehicles = new List<GeneratePlayerVehicle>();

        private int vehicleIndex = 0;

        private int playerIndex = 0;

        private Queue<System.Action> mainThreadTasks = new Queue<System.Action>();

        private void Start()
        {
            var vehicleLogic = ScriptableObject.CreateInstance<MasterPlayerVehicleLogic>();

            var startPoint = GameObject.FindGameObjectWithTag("TeamAStartPoint").transform.position;

            NetManager.StartAsMaster(6576, new ServerListenEvents()
            {
                onRecLoginInfo = (header, connection, loginInfo) =>
                {
                    //New Player Join the game.
                    Debug.LogError(loginInfo.Uid);

                    //Assign PlayerInfo
                    var playerID = PlayerList.Count; //TODO: Not Consider Removing the player.

                    var playerInfo = new PlayerInfo(playerID, TeamManager.Team.red, connection);

                    PlayerList.Add(playerInfo);

                    connection.SendObject("PlayerInfo", playerInfo);

                    mainThreadTasks.Enqueue(() =>
                    {
                        StartCoroutine(SpawnVehicle(connection, vehicleLogic, startPoint, playerInfo));
                    });
                },
                onRecSyncPlayerInput = (header, connection, syncPlayerInput) =>
               {
                   mainThreadTasks.Enqueue(() =>
                   {
                       var vehicle = PlayerVehicles.Find(v => v.VehicleID == syncPlayerInput.VehicleID);

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

        private IEnumerator SpawnVehicle(NetworkCommsDotNet.Connections.Connection connection, MasterPlayerVehicleLogic vehicleLogic, Vector3 startPoint, PlayerInfo playerInfo)
        {
            if (!connection.ConnectionAlive())
            {
                connection.EstablishConnection();
                Debug.LogError($"Establish Connection");
            }

            //Notify New Player to Spawn Other Player Vehicle
            for (int i = 0; i < PlayerVehicles.Count; i++)
            {
                yield return new WaitForSeconds(0.25f);

                connection.SendObject("GeneratePlayerVehicle", PlayerVehicles[i]);

                Debug.LogError($"{playerInfo.clientConnection}  Vehicle ID {PlayerVehicles[i].VehicleID} PlayerID {PlayerVehicles[i].OwnerPlayerID} {connection.ConnectionAlive()}");
            }

            //Generate Player Vehicle
            var vehicle = VehicleUtility.CreateVehicle("T-34", InstanceNetType.GameNetworkBotOffline, vehicleLogic);

            vehicle.transform.position = startPoint;

            var generateData = new GeneratePlayerVehicle(vehicleIndex, "T-34", playerInfo.PlayerID, vehicle);

            PlayerVehicles.Add(generateData);

            vehicleIndex += 1;

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

        private IEnumerator SyncVehicleLoop()
        {
            while (true)
            {
                foreach (var vehicle in PlayerVehicles)
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

                    //Sync Postion To All Players
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].clientConnection.SendObject("SyncVehicle", syncVehicle);

                    }
                    //TODO: player.clientConnection.ConnectionAlive()
                }
                yield return new WaitForSeconds(NetManager.UpdateInterval);
            }
        }

    }
}
