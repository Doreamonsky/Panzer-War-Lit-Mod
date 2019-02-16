using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class SyncVehicle
    {
        public SyncVehicle(int vehicleID, ProtobufVector3 vehiclePosition, ProtobufVector3 vehicleVelocity, ProtobufQuaternion vehicleRotation, ProtobufVector3 lookTargetPos)
        {
            VehicleID = vehicleID;
            VehiclePosition = vehiclePosition;
            VehicleVelocity = vehicleVelocity;
            VehicleRotation = vehicleRotation;
            LookTargetPos = lookTargetPos;
        }

        private SyncVehicle()
        {
        }

 
        [ProtoMember(1)]
        public int VehicleID { get; private set; }

        [ProtoMember(2)]
        public ProtobufVector3 VehiclePosition { get; private set; }

        [ProtoMember(3)]
        public ProtobufVector3 VehicleVelocity { get; private set; }

        [ProtoMember(4)]
        public ProtobufQuaternion VehicleRotation { get; private set; }

        [ProtoMember(5)]
        public ProtobufVector3 LookTargetPos { get; private set; }
    }
}
