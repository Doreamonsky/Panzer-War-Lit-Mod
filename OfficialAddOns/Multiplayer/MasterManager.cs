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

        private readonly float UpdateInterval = 0.05f;

        private int GenertatedVehicleIndex = 0;

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
                        //Notify New Player to Spawn Other Player Vehicle
                        foreach (var otherPlayerVehicle in PlayerVehicles)
                        {
                            connection.SendObject("GeneratePlayerVehicle", otherPlayerVehicle);
                        }

                        //Generate Player Vehicle
                        var vehicle = VehicleUtility.CreateVehicle("T-34", InstanceNetType.GameNetworkBotOffline, vehicleLogic);

                        vehicle.transform.position = startPoint;

                        var generateData = new GeneratePlayerVehicle(GenertatedVehicleIndex, "T-34", playerInfo.PlayerID, vehicle);

                        PlayerVehicles.Add(generateData);

                        GenertatedVehicleIndex += 1;

                        //connection.SendObject("GeneratePlayerVehicle", generateData);

                        //Notify All Players To Spawn New Player Vehicle
                        foreach (var player in PlayerList)
                        {
                            player.clientConnection.SendObject("GeneratePlayerVehicle", generateData);
                        }
                    });
                }
            });

            StartCoroutine(SyncVehicleLoop());
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

                    var pos = new ProtobufVector3(internalPos.x, internalPos.y, internalPos.z);

                    var internalRot = vehicle.tankInitSystem.vehicleComponents.playerTracksController.transform.rotation;

                    var rot = new ProtobufQuaternion(internalRot.x, internalRot.y, internalRot.z, internalRot.w);


                    var internalVelocity = vehicle.tankInitSystem.vehicleComponents.playerTracksController.GetComponent<Rigidbody>().velocity;

                    var velocity = new ProtobufVector3(internalVelocity.x, internalVelocity.y, internalVelocity.z);

                    var syncVehicle = new SyncVehicle(vehicle.VehicleID, pos, velocity, rot);

                    //Sync Postion To All Players
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].clientConnection.SendObject("SyncVehicle", syncVehicle);
                       
                    }
                    //TODO: player.clientConnection.ConnectionAlive()
                }
                yield return new WaitForSeconds(UpdateInterval);
            }
        }

    }
}
