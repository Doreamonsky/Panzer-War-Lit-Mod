using ShanghaiWindy.Core;
using UnityEngine;
using UnityMod;

namespace Multiplayer
{
    /// <summary>
    /// I changed the net framework to NetworkCommsDotNet. I am currently not very familiar with this framework.
    /// 2018/02/16
    /// </summary>
    public class Manager : IGeneralAddOn, IGameModule
    {
        private bool isMultiplayerMode = false;

        public string serverIP = "127.0.0.1";

        public int serverPort = 6576;

        public void OnExitBattle()
        {

        }

        public void OnFixedUpdate()
        {
        }

        public void OnInitialized()
        {

        }

        public void OnNewSceneLoaded(string name)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateGUI()
        {
            GUILayout.Space(25);

            if (!isMultiplayerMode)
            {
                if (GUILayout.Button("To Multiplayer Mode"))
                {
                    GameObject.FindObjectOfType<AssetLoader>().StartCoroutine(AssetBundleManager.RequestScene(MapDataManager.Instance.GetMapData("Ensk"), null, () =>
                    {
                        PoolManager.Initialize();

                        BattleMainUIModule.Init(this);

                        BattleMainUIModule.instance.onToggleSelectVehicleUIObject(false);

                        isMultiplayerMode = true;

                        new GameObject("Debug", typeof(ServerConsole));
                    }));
                }
            }
            else
            {
                GUILayout.Label("Server IP");
                serverIP = GUILayout.TextField(serverIP);

                GUILayout.Label("Server Port");
                serverPort = int.Parse(GUILayout.TextField(serverPort.ToString()));

                if (GUILayout.Button("Start As Master Server"))
                {
                    var masterServer = new GameObject("Master", typeof(MasterManager));
                    masterServer.GetComponent<MasterManager>().Initialize();

                    Object.DontDestroyOnLoad(masterServer);
                }

                if (GUILayout.Button("Connect To Master Server"))
                {
                    var client = new GameObject("Client", typeof(ClientManager));
                    client.GetComponent<ClientManager>().Initialize();

                    Object.DontDestroyOnLoad(client);
                }
            }





            //if (GUILayout.Button("Start As Server"))
            //{

            //    //NetworkComms.AppendGlobalIncomingPacketHandler<BinaryFormatterCustomObject>("CustomObject",
            //    //                    (header, connection, customObject) =>
            //    //                    {
            //    //                        Debug.LogError(connection);
            //    //                        Debug.LogError(customObject.IntValue);
            //    //                    });


            //    //List<EndPoint> localListeningEndPoints = Connection.ExistingLocalListenEndPoints(connectionTypeToUse);

            //    //foreach (IPEndPoint localEndPoint in localListeningEndPoints)
            //    //{
            //    //    Debug.LogError(("{0}:{1}", localEndPoint.Address, localEndPoint.Port));
            //    //}

            //    NetworkComms.AppendGlobalIncomingPacketHandler<BinaryFormatterCustomObject>("Message",
            //        (packetHeader, connection, incomingString) =>
            //        {
            //            Debug.LogError(incomingString);
            //        }
            //        );

            //    NetworkComms.AppendGlobalIncomingPacketHandler<ProtobufCustomObject>("Protobuf",
            //             (packetHeader, connection, incomingString) =>
            //             {
            //                 Debug.LogError(incomingString);
            //             }
            //             );
            //    Connection.StartListening(ConnectionType.UDP, IPEndPoint);

            //    //NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", (packetHeader, connection, incomingString) => { Debug.LogError(incomingString); });
            //    //Connection.StartListening(ConnectionType.TCP, IPEndPoint);
            //}

            //if(GUILayout.Button("Listen As Client"))
            //{
            //    NetworkComms.AppendGlobalIncomingPacketHandler<ProtobufCustomObject>("Protobuf",
            //   (packetHeader, connection, incomingString) =>
            //   {
            //       Debug.LogError(incomingString);
            //   }
            //   );
            //    Connection.StartListening(ConnectionType.UDP, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            //}
            //if (GUILayout.Button("Send To Client"))
            //{
            //    //{
            //    //    NetworkComms.SendObject("Message", "127.0.0.1", 3231, "Hello");
            //    //Connection connectionToUse = UDPConnection.GetConnection(new ConnectionInfo(IPEndPoint, ApplicationLayerProtocolStatus.Disabled), UDPOptions.None);
            //    var connection = UDPConnection.GetConnection(new ConnectionInfo(IPEndPoint), UDPOptions.Handshake);
            //    connection.SendObject("Message", new BinaryFormatterCustomObject(123, "Hi"));
            //    //NetworkComms.SendObject("CustomObject", "127.0.0.1", 3333, new BinaryFormatterCustomObject(123, "Hello World Protobuf"));
            //    Debug.LogError("Send Msg");
            //}
            //if (GUILayout.Button("Send Protobuf"))
            //{
            //    var connection = UDPConnection.GetConnection(new ConnectionInfo(IPEndPoint), UDPOptions.Handshake);
            //    connection.SendObject("Protobuf", new ProtobufCustomObject(12, "Hello World"));
            //}
            //if(GUILayout.Button("Get Connection"))
            //{
            //    foreach(var connection in NetworkComms.AllConnectionInfo(false))
            //    {
            //        Debug.LogError($"Identifier { connection.NetworkIdentifier} EndPoint { connection.RemoteEndPoint}");
            //    }
            //    foreach(var connection in NetworkComms.GetExistingConnection())
            //    {
            //        connection.SendObject("Protobuf", new ProtobufCustomObject(12, "Hello Client!"));
            //    }
            //}
        }
    }
}
