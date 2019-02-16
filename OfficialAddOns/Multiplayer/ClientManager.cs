using UnityEngine;

namespace Multiplayer
{
    public class ClientManager : MonoBehaviour
    {
        private void Start()
        {
            NetManager.ConnectToMaster("127.0.0.1", 6576, new ClientListenEvents()
            {
                onRecPlayerInfo = (header, connection, loginInfo) =>
                {
                    Debug.LogError(loginInfo.PlayerID);
                },
                onRecSyncVehicle = (header, connection, syncVehicle) =>
                {
                    Debug.LogError(syncVehicle.VehicleVelocity.x);
                }
            });
        }
    }
}
