using System;

namespace ParadoxNotion.Serialization.FullSerializer
{

    ///<summary> The serialization converter allows for customization of the serialization process.</summary>
    public abstract class fsConverter : fsBaseConverter
    {

        ///<summary> Can this converter serialize and deserialize the given object type?</summary>
        public abstract bool CanProcess(Type type);
    }
}