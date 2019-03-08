using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class DestroyPlayerVehicle
    {
        private DestroyPlayerVehicle()
        {
        }

        public DestroyPlayerVehicle(int vehicleID)
        {
            VehicleID = vehicleID;
        }

        public int VehicleID { get; private set; }
    }
}
