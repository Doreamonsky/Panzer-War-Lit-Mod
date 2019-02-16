using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class SyncVehicle
    {
        private SyncVehicle()
        {
        }

        public SyncVehicle(int vehicleID, ProtobufVector3 vehiclePosition, ProtobufVector3 vehicleVelocity)
        {
            VehicleID = vehicleID;
            VehiclePosition = vehiclePosition;
            VehicleVelocity = vehicleVelocity;
        }

        [ProtoMember(1)]
        public int VehicleID { get; private set; }

        [ProtoMember(2)]
        public ProtobufVector3 VehiclePosition { get; private set; }

        [ProtoMember(3)]
        public ProtobufVector3 VehicleVelocity { get; private set; }
    }
}
