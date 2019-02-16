using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class GeneratePlayerVehicle
    {
        private GeneratePlayerVehicle()
        {
        }

        public GeneratePlayerVehicle(int vehicleID, string vehicleName)
        {
            VehicleID = vehicleID;
            VehicleName = vehicleName;
        }

        [ProtoMember(1)]
        public int VehicleID { get; private set; }

        [ProtoMember(2)]
        public string VehicleName { get; private set; }
    }
}
