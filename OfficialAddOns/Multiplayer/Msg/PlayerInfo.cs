using NetworkCommsDotNet.Connections;
using ProtoBuf;
using ShanghaiWindy.Core;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class PlayerInfo
    {
        private PlayerInfo()
        {

        }

        public PlayerInfo(int playerID, TeamManager.Team playerTeam, Connection clientConnection)
        {
            PlayerID = playerID;
            PlayerTeam = playerTeam;
            this.clientConnection = clientConnection;
        }

        [ProtoMember(1)]
        public int PlayerID { get; private set; }
        [ProtoMember(2)]
        public TeamManager.Team PlayerTeam { get; private set; }

        public Connection clientConnection { get; private set; }
    }
}
