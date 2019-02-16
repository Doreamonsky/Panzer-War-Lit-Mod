using ProtoBuf;
using UnityEngine;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class ProtobufQuaternion
    {
        private ProtobufQuaternion()
        {
        }

        public ProtobufQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        [ProtoMember(1)]
        public float x { get; private set; }

        [ProtoMember(2)]
        public float y { get; private set; }

        [ProtoMember(3)]
        public float z { get; private set; }

        [ProtoMember(4)]
        public float w { get; private set; }

        public Quaternion CovertToUnityProtobufQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
}
