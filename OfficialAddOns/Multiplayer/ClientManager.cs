using Multiplayer.Msg;
using NetworkCommsDotNet;
using ShanghaiWindy.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class ClientManager : MonoBehaviour
    {
        private class VehicleStatus
        {
            public bool isLocalPlayer;

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

        private VehicleStatus currentPlayerVehicleStatus;

        private List<VehicleStatus> vehicleStatusList = new List<VehicleStatus>();


        private void Start()
        {
            NetManager.ConnectToMaster("127.0.0.1", 6576, new ClientListenEvents()
            {
                onRecPlayerInfo = (header, connection, playerInfo) =>
                {
                    currentPlayerInfo = playerInfo;

                    Debug.LogError($"Sync Player Info  ID: {currentPlayerInfo.PlayerID}");
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

                        var vehicleStatus = new VehicleStatus()
                        {
                            isLocalPlayer= isLocalPlayer,
                            OwnerPlayerID = generatePlayerVehicle.OwnerPlayerID,
                            VehicleID = generatePlayerVehicle.VehicleID,
                            tankInitSystem = vehicle,
                            syncVehicle = new SyncVehicle(generatePlayerVehicle.VehicleID, new ProtobufVector3(0, 0, 0), new ProtobufVector3(0, 0, 0), new ProtobufQuaternion(0, 0, 0, 1), new ProtobufVector3(0, 0, 0))
                        };

                        if (isLocalPlayer)
                        {
                            currentPlayerVehicleStatus = vehicleStatus;
                        }

                        vehicleStatusList.Add(vehicleStatus);

                        Debug.LogError($"isLocalPlayer : {isLocalPlayer} VehicleID {generatePlayerVehicle.VehicleID}");
                    });
                }
                ,
                onRecSyncVehicle = (header, connection, syncVehicle) =>
                {
                    var toSyncVehicleStatus = vehicleStatusList.Find(val => val.VehicleID == syncVehicle.VehicleID);

                    if (toSyncVehicleStatus != null)
                    {
                        toSyncVehicleStatus.syncVehicle = syncVehicle;
                    }

                    //Debug.LogError($"{syncVehicle.VehicleID} {syncVehicle.VehiclePosition.CovertToUnityV3()}");
                }
            });

            StartCoroutine(SyncInputToMaster());
        }

        private void OnDestroy()
        {
            NetworkComms.Shutdown();
        }
        private IEnumerator SyncInputToMaster()
        {
            while (true)
            {
                var ptc = currentPlayerVehicleStatus?.tankInitSystem?.vehicleComponents?.playerTracksController;

                if (ptc != null)
                {
                    var lookAtPos = currentPlayerVehicleStatus.tankInitSystem.vehicleComponents.mainTurretController.target.position;

                    var playerInput = new SyncPlayerInput(currentPlayerVehicleStatus.VehicleID, ptc.accelG, ptc.steerG, new ProtobufVector3(lookAtPos.x, lookAtPos.y, lookAtPos.z));

                    NetManager.clientConnection.SendObject("SyncPlayerInput", playerInput);
                }

                yield return new WaitForSeconds(NetManager.UpdateInterval);
            }
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

                if (!vehicleStatus.isLocalPlayer)
                {
                    var lookTarget = vehicleStatus.tankInitSystem.vehicleComponents.mainTurretController.target;

                    lookTarget.position = vehicleStatus.syncVehicle.LookTargetPos.CovertToUnityV3();
                }
             
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
