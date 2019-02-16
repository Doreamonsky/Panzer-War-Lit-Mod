using ProtoBuf;
using ShanghaiWindy.Core;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class GeneratePlayerVehicle
    {
        private GeneratePlayerVehicle()
        {

        }

        public GeneratePlayerVehicle(int vehicleID, string vehicleName, int ownerPlayerID, TankInitSystem tankInitSystem)
        {
            VehicleID = vehicleID;
            VehicleName = vehicleName;
            OwnerPlayerID = ownerPlayerID;
            this.tankInitSystem = tankInitSystem;
        }

        [ProtoMember(1)]
        public int VehicleID { get; private set; }

        [ProtoMember(2)]
        public string VehicleName { get; private set; }

        [ProtoMember(3)]
        public int OwnerPlayerID { get; private set; }

        public TankInitSystem tankInitSystem;
    }
}
