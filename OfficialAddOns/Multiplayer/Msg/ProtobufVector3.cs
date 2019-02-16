using ProtoBuf;
using UnityEngine;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class ProtobufVector3
    {
        private ProtobufVector3()
        {
        }

        public ProtobufVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public ProtobufVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        [ProtoMember(1)]
        public float x { get; private set; }

        [ProtoMember(2)]
        public float y { get; private set; }

        [ProtoMember(3)]
        public float z { get; private set; }

        public Vector3 CovertToUnityV3()
        {
            return new Vector3(x, y, z);
        }

    }
}
