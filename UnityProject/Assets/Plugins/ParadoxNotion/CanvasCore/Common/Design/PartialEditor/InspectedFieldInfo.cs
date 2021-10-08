#if UNITY_EDITOR

using System.Reflection;

namespace ParadoxNotion.Design
{

    ///Contains info about inspected field rin regards to reflected inspector and object/attribute drawers
    public struct InspectedFieldInfo
    {
        ///The field inspected
        public FieldInfo field;
        ///the unityengine object serialization context
        public UnityEngine.Object unityObjectContext;
        ///the parent instance the field lives within
        public object parentInstanceContext;
        ///In case instance lives in wrapped context (eg lists) otherwise the same as parentInstanceContext
        public object wrapperInstanceContext;
        ///attributes found on field
        public object[] attributes;

        //...
        public InspectedFieldInfo(UnityEngine.Object unityObjectContext, FieldInfo field, object parentInstanceContext, object[] attributes) {
            this.unityObjectContext = unityObjectContext;
            this.field = field;
            this.parentInstanceContext = parentInstanceContext;
            this.wrapperInstanceContext = parentInstanceContext;
            this.attributes = attributes;
        }
    }
}

#endif