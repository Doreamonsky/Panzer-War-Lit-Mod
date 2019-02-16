
using Multiplayer.Msg;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.UDP;
using NetworkCommsDotNet.Tools;
using System.Net;
using UnityEngine;

namespace Multiplayer
{
    public class ServerListenEvents
    {
        public System.Action<PacketHeader, Connection, LoginInfo> onRecLoginInfo;
    }
    public class ClientListenEvents
    {
        public System.Action<PacketHeader, Connection, PlayerInfo> onRecPlayerInfo;

        public System.Action<PacketHeader, Connection, SyncVehicle> onRecSyncVehicle;

        public System.Action<PacketHeader, Connection, GeneratePlayerVehicle> onRecGeneratePlayerVehicle;
    }

    public class NetManager
    {
        public static ConnectionType connectionType = ConnectionType.UDP;

        private static UDPConnection clientConnection;

        public static void ConnectToMaster(string ipAdress, int ipPort, ClientListenEvents clientListenEvents)
        {
            string logFileName = Application.dataPath + "/../Client_Log_" + NetworkComms.NetworkIdentifier + ".txt";
            var logger = new LiteLogger(LiteLogger.LogMode.LogFileOnly, logFileName);
            NetworkComms.EnableLogging(logger);

            var serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAdress), ipPort);

            clientConnection = UDPConnection.GetConnection(new ConnectionInfo(serverEndPoint), UDPOptions.Handshake);

            Debug.LogError($"Established Connection: \n {clientConnection}");

            AppendClientListener(clientListenEvents);

            //Auth
            clientConnection.SendObject("LoginInfo", new LoginInfo("BetaPlayer_Null_UID"));


        }
        public static void StartAsMaster(int ipPort, ServerListenEvents serverListenEvents)
        {
            string logFileName = Application.dataPath + "/../Server_Log_" + NetworkComms.NetworkIdentifier + ".txt";
            var logger = new LiteLogger(LiteLogger.LogMode.LogFileOnly, logFileName);
            NetworkComms.EnableLogging(logger);

            var ipEndPoint = new IPEndPoint(IPAddress.Any, ipPort);

            AppendServerListener(serverListenEvents);

            //To Receive Client Msg
            Connection.StartListening(connectionType, ipEndPoint);

            foreach (IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.UDP))
            {
                Debug.LogErrorFormat("Listen At {0}:{1}", localEndPoint.Address, localEndPoint.Port);
            }
        }



        //Listen
        private static void AppendServerListener(ServerListenEvents listenerEvent)
        {
            NetworkComms.AppendGlobalIncomingPacketHandler<LoginInfo>("LoginInfo",
              (packetHeader, connection, incomingString) =>
                  {
                      listenerEvent.onRecLoginInfo?.Invoke(packetHeader, connection, incomingString);
                  }
              );
        }
        private static void AppendClientListener(ClientListenEvents listenerEvent)
        {
            NetworkComms.AppendGlobalIncomingPacketHandler<PlayerInfo>("PlayerInfo",
              (packetHeader, connection, incomingString) =>
                  {
                      listenerEvent.onRecPlayerInfo?.Invoke(packetHeader, connection, incomingString);
                  }
              );

            NetworkComms.AppendGlobalIncomingPacketHandler<GeneratePlayerVehicle>("GeneratePlayerVehicle",
                  (packetHeader, connection, incomingString) =>
                  {
                      listenerEvent.onRecGeneratePlayerVehicle?.Invoke(packetHeader, connection, incomingString);
                  }
              );

            NetworkComms.AppendGlobalIncomingPacketHandler<SyncVehicle>("SyncVehicle",
              (packetHeader, connection, incomingString) =>
              {
                  listenerEvent.onRecSyncVehicle?.Invoke(packetHeader, connection, incomingString);
              }
          );
        }
    }
}
