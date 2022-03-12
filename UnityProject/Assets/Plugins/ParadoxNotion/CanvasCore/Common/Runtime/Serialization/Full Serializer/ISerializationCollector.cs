namespace ParadoxNotion.Serialization.FullSerializer
{

    ///<summary>Will receive callbacks on serialization/deserialization. Multiple collectors are possible and are stacked.</summary>
    public interface ISerializationCollector : ISerializationCollectable
    {
        ///<summary>Called when the collector pushed on stack with parent the previous collector</summary>
        void OnPush(ISerializationCollector parent);
        ///<summary>Called when a collectable is to be collected. The depth is local to this collector only starting from 0</summary>
        void OnCollect(ISerializationCollectable child, int depth);
        ///<summary>Called when the collector pops from stack with parent the previous collector</summary>
        void OnPop(ISerializationCollector parent);
    }

    ///<summary>Will be possible to be collected by a collector</summary>
    public interface ISerializationCollectable { }
}