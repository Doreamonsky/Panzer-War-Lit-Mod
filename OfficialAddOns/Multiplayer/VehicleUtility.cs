
using ShanghaiWindy.Core;
using UnityEngine;

namespace Multiplayer
{
    public class VehicleUtility
    {
        /// <summary>
        /// Currently. I am using BotLogic for remote players.
        /// </summary>
        /// <param name="vehicleName"></param>
        /// <param name="instanceNetType"></param>
        /// <returns></returns>
        public static TankInitSystem CreateVehicle(string vehicleName, InstanceNetType instanceNetType, BotLogic thinkLogic)
        {
            var vehicle = new GameObject("Vehicle", typeof(TankInitSystem)).GetComponent<TankInitSystem>();

            vehicle.VehicleName = vehicleName;

            vehicle._InstanceNetType = instanceNetType;

            vehicle.thinkLogic = thinkLogic;

            vehicle.BulletCountList = new int[] { 999, 999, 999 };

            vehicle.InitTankInitSystem();

            return vehicle;
        }

        public static void RemoveVehicle(TankInitSystem vehicle)
        {
            var isVehicleLoaded = vehicle.vehicleComponents?.playerTracksController != null;

            //Be sure to  Avoid AssetBundle Loading Error!
            if (isVehicleLoaded)
            {
                GameObject.Destroy(vehicle.gameObject);
            }
            else
            {
                vehicle.onVehicleLoaded += () =>
                {
                    GameObject.Destroy(vehicle.gameObject);
                };
            }
        }
    }
}
