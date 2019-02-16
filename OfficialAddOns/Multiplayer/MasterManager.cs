using UnityEngine;

namespace Multiplayer
{
    public class MasterManager : MonoBehaviour
    {
        private void Start()
        {
            NetManager.StartAsMaster(6576, new ServerListenEvents()
            {
                onRecLoginInfo = (header, connection, loginInfo) =>
                {
                    Debug.LogError(loginInfo.Uid);

                    var playerID = PlayerList.Count; //TODO: Not Consider Removing the player.

                    var playerInfo = new PlayerInfo(playerID, TeamManager.Team.red, connection);

                    PlayerList.Add(playerInfo);

                    connection.SendObject("PlayerInfo", playerInfo);

                    var testPos = new ProtobufVector3(1, 2, 3);

                    connection.SendObject("SyncVehicle", new SyncVehicle(1, testPos, testPos));
                }
            });
        }
    }
}
