#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParadoxNotion.Serialization.FullSerializer.Internal.DirectConverters
{
    public class Vector2_DirectConverter : fsDirectConverter<Vector2>
    {
        protected override fsResult DoSerialize(Vector2 model, Dictionary<string, fsData> serialized) {
            SerializeMember(serialized, null, "x", model.x);
            SerializeMember(serialized, null, "y", model.y);
            return fsResult.Success;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Vector2 model) {
            var t0 = model.x;
            DeserializeMember(data, null, "x", out t0);
            model.x = t0;

            var t1 = model.y;
            DeserializeMember(data, null, "y", out t1);
            model.y = t1;

            return fsResult.Success;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new Vector2();
        }
    }
}
#endif