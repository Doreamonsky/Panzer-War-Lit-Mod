#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParadoxNotion.Serialization.FullSerializer.Internal.DirectConverters
{
    public class Vector3Int_DirectConverter : fsDirectConverter<Vector3Int>
    {
        protected override fsResult DoSerialize(Vector3Int model, Dictionary<string, fsData> serialized) {
            SerializeMember(serialized, null, "x", model.x);
            SerializeMember(serialized, null, "y", model.y);
            SerializeMember(serialized, null, "z", model.z);
            return fsResult.Success;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Vector3Int model) {
            var t0 = model.x;
            DeserializeMember(data, null, "x", out t0);
            model.x = t0;

            var t1 = model.y;
            DeserializeMember(data, null, "y", out t1);
            model.y = t1;

            var t2 = model.z;
            DeserializeMember(data, null, "z", out t2);
            model.z = t2;

            return fsResult.Success;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new Vector3Int();
        }
    }
}
#endif