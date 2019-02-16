using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class SyncPlayerInput
    {
        public SyncPlayerInput(int vehicleID, float xinput, float yinput, ProtobufVector3 lookTargetPos)
        {
            VehicleID = vehicleID;
            Xinput = xinput;
            Yinput = yinput;
            LookTargetPos = lookTargetPos;
        }

        private SyncPlayerInput()
        {
        }

  
        [ProtoMember(1)]
        public int VehicleID { get; private set; }

        [ProtoMember(2)]
        public float Xinput { get; private set; }

        [ProtoMember(3)]
        public float Yinput { get; private set; }

        [ProtoMember(4)]
        public ProtobufVector3 LookTargetPos { get; private set; }
    }
}
