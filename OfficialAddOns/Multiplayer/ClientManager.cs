using Multiplayer.Msg;
using NetworkCommsDotNet;
using ShanghaiWindy.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class ClientManager : MonoBehaviour
    {
        private class VehicleStatus
        {
            public int OwnerPlayerID;

            public int VehicleID;

            public TankInitSystem tankInitSystem;

            public SyncVehicle syncVehicle;
        }

        /// <summary>
        /// All tasks related to UnityEngine should be done on the Main Thread.  qwq.
        /// </summary>
        private Queue<System.Action> mainThreadTasks = new Queue<System.Action>();

        private PlayerInfo currentPlayerInfo;

        private List<VehicleStatus> vehicleStatusList = new List<VehicleStatus>();


        private void Start()
        {
            NetManager.ConnectToMaster("127.0.0.1", 6576, new ClientListenEvents()
            {
                onRecPlayerInfo = (header, connection, playerInfo) =>
                {
                    currentPlayerInfo = playerInfo;
                },
                onRecGeneratePlayerVehicle = (header, connection, generatePlayerVehicle) =>
                {
                    mainThreadTasks.Enqueue(() =>
                    {
                        var isLocalPlayer = currentPlayerInfo.PlayerID == generatePlayerVehicle.OwnerPlayerID;

                        var netType = isLocalPlayer ? InstanceNetType.GameNetWorkOffline : InstanceNetType.GameNetworkBotOffline;
                        var thinkLogic = isLocalPlayer ? null : ScriptableObject.CreateInstance<ClientOtherPlayerVehicleLogic>();

                        var vehicle = VehicleUtility.CreateVehicle(generatePlayerVehicle.VehicleName, netType, thinkLogic);

                        generatePlayerVehicle.tankInitSystem = vehicle;

                        vehicleStatusList.Add(new VehicleStatus()
                        {
                            OwnerPlayerID = generatePlayerVehicle.OwnerPlayerID,
                            VehicleID = generatePlayerVehicle.VehicleID,
                            tankInitSystem = vehicle,
                            syncVehicle = new SyncVehicle(generatePlayerVehicle.VehicleID, new ProtobufVector3(0, 0, 0), new ProtobufVector3(0, 0, 0), new ProtobufQuaternion(0, 0, 0, 1))
                        });
                    });
                }
                ,
                onRecSyncVehicle = (header, connection, syncVehicle) =>
                {
                    var toSyncVehicleStatus = vehicleStatusList.Find(val => val.VehicleID == syncVehicle.VehicleID);

                    toSyncVehicleStatus.syncVehicle = syncVehicle;

                    //Debug.LogError($"{syncVehicle.VehicleID} {syncVehicle.VehiclePosition.CovertToUnityV3()}");
                }
            });
        }

        private void OnDestroy()
        {
            NetworkComms.Shutdown();
        }

        private void Update()
        {
            //Sync Vehicle Status /Postion Rotation Velocity
            foreach (var vehicleStatus in vehicleStatusList)
            {
                var ptcTransform = vehicleStatus.tankInitSystem?.vehicleComponents?.playerTracksController?.transform;

                if (ptcTransform == null)
                {
                    continue;
                }

                var nextPos = vehicleStatus.syncVehicle.VehiclePosition.CovertToUnityV3();

                if (Vector3.Distance(ptcTransform.position, nextPos) > 15)
                {
                    ptcTransform.position = nextPos;
                }
                else
                {
                    ptcTransform.position = Vector3.Lerp(ptcTransform.position, nextPos, Time.deltaTime * 5);
                }

                var nextRot = vehicleStatus.syncVehicle.VehicleRotation.CovertToUnityProtobufQuaternion();

                ptcTransform.rotation = Quaternion.Lerp(ptcTransform.rotation, nextRot, Time.deltaTime * 5);

                //Debug.LogError($"VehicleID: {vehicleStatus.VehicleID} OwnerPlayerID: {vehicleStatus.OwnerPlayerID}Sync: {vehicleStatus.syncVehicle.GetHashCode()}");
            }

            while (mainThreadTasks.Count != 0)
            {
                var task = mainThreadTasks.Dequeue();
                task?.Invoke();
            }
        }
    }
}
