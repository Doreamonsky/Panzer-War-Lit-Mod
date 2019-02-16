using NetworkCommsDotNet.Connections;
using ProtoBuf;
using ShanghaiWindy.Core;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class PlayerInfo
    {
        public PlayerInfo(string uid, int playerID, TeamManager.Team playerTeam, Connection clientConnection)
        {
            Uid = uid;
            PlayerID = playerID;
            PlayerTeam = playerTeam;
            this.clientConnection = clientConnection;
        }

        private PlayerInfo()
        {

        }

        [ProtoMember(1)]
        public string Uid { get; private set; }

        [ProtoMember(2)]
        public int PlayerID { get; private set; }

        [ProtoMember(3)]
        public TeamManager.Team PlayerTeam { get; private set; }

        public Connection clientConnection { get; private set; }


    }
}
