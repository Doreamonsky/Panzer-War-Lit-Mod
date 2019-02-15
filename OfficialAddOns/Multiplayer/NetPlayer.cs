
using UnityEngine;
using ShanghaiWindy.Core;

namespace Multiplayer
{
    public class NetPlayer : MonoBehaviour
    {

        public void Initialize()
        {
            //Debug.Log($"ID:{netId} Authority: { localPlayerAuthority} Is Server:{isServer} isLocalPlayer{isLocalPlayer}");

            //if (isPrefab || isInitialized)
            //{
            //    return;
            //}

            //isInitialized = true;

            //vehicle = GetComponent<TankInitSystem>();

            //vehicle.VehicleName = "T-34";
            //vehicle.BulletCountList = new int[] { 99, 99, 99 };

            //if (isLocalPlayer)
            //{
            //    vehicle._InstanceNetType = InstanceNetType.GameNetWorkOffline;
            //}
            //else
            //{
            //    vehicle._InstanceNetType = InstanceNetType.GameNetworkBotOffline;
            //    vehicle.thinkLogic = ScriptableObject.CreateInstance<RemotePlayerLoic>();
            //}

            //vehicle.InitTankInitSystem();

            //vehicle.onVehicleLoaded += () =>
            //{
            //    GetComponent<NetworkTransformChild>().target = transform.GetChild(0);
            //};
        }

        private void Update()
        {
            //if (isPrefab)
            //{
            //    return;
            //}

            //if (!isInitialized)
            //{
            //    Initialize();
            //    return;
            //}

            //if (isServer)
            //{
            //    vehiclePostion = vehicle.vehicleComponents.playerTracksController.transform.position;
            //}

            //if (isLocalPlayer)
            //{
            //    vehicle.vehicleComponents.playerTracksController.transform.position = vehiclePostion;
            //    Debug.Log("isLocalPlayer");
            //}

        }
    }

}
