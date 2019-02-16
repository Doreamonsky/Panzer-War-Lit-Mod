using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class ProtobufVector3
    {
        private ProtobufVector3()
        {
        }

        public ProtobufVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [ProtoMember(1)]
        public double x { get; private set; }

        [ProtoMember(2)]
        public double y { get; private set; }

        [ProtoMember(3)]
        public double z { get; private set; }
    }
}
