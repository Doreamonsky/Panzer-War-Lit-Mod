namespace ParadoxNotion.Serialization.FullSerializer
{

    ///Will receive callbacks on serialization/deserialization
    ///Multiple collectors are possible and are stacked
    public interface ISerializationCollector : ISerializationCollectable
    {
        ///Called when the collector pushed on stack with parent the previous collector
        void OnPush(ISerializationCollector parent);
        ///Called when a collectable is to be collected. The depth is local to this collector only starting from 0
        void OnCollect(ISerializationCollectable child, int depth);
        ///Called when the collector pops from stack with parent the previous collector
        void OnPop(ISerializationCollector parent);
    }

    ///Will be possible to be collected by a collector
    public interface ISerializationCollectable { }
}