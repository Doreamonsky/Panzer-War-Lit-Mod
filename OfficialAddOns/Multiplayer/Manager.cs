
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.UDP;
using NetworkCommsDotNet.DPSBase;
using ShanghaiWindy.Core;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityMod;

namespace Multiplayer
{
    /// <summary>
    /// Currently,I am using the a simple socket contributed by https://github.com/PlaneZhong/PESocket.
    /// Because the new network system is very raw. 
    /// 2018/02/15
    /// </summary>
    public class Manager : IGeneralAddOn, IGameModule
    {
        public static GameObject PlayerPrefab;
        //private readonly ConnectionType connectionTypeToUse;
        //private readonly DataSerializer dataSerializer;
        //private readonly List<DataProcessor> dataProcessors;
        //private readonly Dictionary<string, string> dataProcessorOptions;
        public IPEndPoint IPEndPoint;

        //public static bool isServer = false;
        //public static bool isClient = false;


        public void OnExitBattle()
        {

        }

        public void OnFixedUpdate()
        {
        }

        public void OnInitialized()
        {
            //connectionTypeToUse = ConnectionType.UDP;
            //dataSerializer = DPSManager.GetDataSerializer<BinaryFormaterSerializer>();
            //dataProcessors = new List<DataProcessor>();
            //dataProcessorOptions = new Dictionary<string, string>();
            IPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2677);

            //NetworkComms.DefaultSendReceiveOptions = new SendReceiveOptions(dataSerializer, dataProcessors, dataProcessorOptions);
        }

        public void OnNewSceneLoaded(string name)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateGUI()
        {
            //if (GUILayout.Button("Start Networking"))
            //{
            //   GameObject.FindObjectOfType<AssetLoader>().StartCoroutine(AssetBundleManager.RequestScene(MapDataManager.Instance.GetMapData("Desert"), null, () =>
            //    {
            //        PoolManager.Initialize();

            //        BattleMainUIModule.Init(this);

            //        BattleMainUIModule.instance.onToggleSelectVehicleUIObject(false);
            //    }));
            //}



            if (GUILayout.Button("Start As Server"))
            {

                //NetworkComms.AppendGlobalIncomingPacketHandler<BinaryFormatterCustomObject>("CustomObject",
                //                    (header, connection, customObject) =>
                //                    {
                //                        Debug.LogError(connection);
                //                        Debug.LogError(customObject.IntValue);
                //                    });


                //List<EndPoint> localListeningEndPoints = Connection.ExistingLocalListenEndPoints(connectionTypeToUse);

                //foreach (IPEndPoint localEndPoint in localListeningEndPoints)
                //{
                //    Debug.LogError(("{0}:{1}", localEndPoint.Address, localEndPoint.Port));
                //}

                NetworkComms.AppendGlobalIncomingPacketHandler<BinaryFormatterCustomObject>("Message",
                    (packetHeader, connection, incomingString) =>
                    {
                        Debug.LogError(incomingString);
                    }
                    );

                Connection.StartListening(ConnectionType.UDP, IPEndPoint);

                //NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", (packetHeader, connection, incomingString) => { Debug.LogError(incomingString); });
                //Connection.StartListening(ConnectionType.TCP, IPEndPoint);
            }


            if (GUILayout.Button("Send To Client"))
            {
                //{
                //    NetworkComms.SendObject("Message", "127.0.0.1", 3231, "Hello");
                //Connection connectionToUse = UDPConnection.GetConnection(new ConnectionInfo(IPEndPoint, ApplicationLayerProtocolStatus.Disabled), UDPOptions.None);
                var connection = UDPConnection.GetConnection(new ConnectionInfo(IPEndPoint), UDPOptions.Handshake);
                connection.SendObject("Message", new BinaryFormatterCustomObject(123,"Hi"));
                //NetworkComms.SendObject("CustomObject", "127.0.0.1", 3333, new BinaryFormatterCustomObject(123, "Hello World Protobuf"));
                Debug.LogError("Send Msg");
            }
        }
    }
}
