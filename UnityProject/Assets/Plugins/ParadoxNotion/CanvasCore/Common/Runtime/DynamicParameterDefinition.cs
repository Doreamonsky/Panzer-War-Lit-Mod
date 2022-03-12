using System;
using UnityEngine;

namespace ParadoxNotion
{

    ///<summary>Defines a dynamic (type-wise) parameter.</summary>
    [Serializable]
    sealed public class DynamicParameterDefinition : ISerializationCallbackReceiver
    {

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if ( type != null ) { _type = type.FullName; }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            type = ReflectionTools.GetType(_type, /*fallback?*/ true);
        }

        [SerializeField] private string _ID;
        [SerializeField] private string _name;
        [SerializeField] private string _type;

        //The ID of the definition
        public string ID {
            get
            {
                //for correct update prior versions
                if ( string.IsNullOrEmpty(_ID) ) { _ID = name; }
                return _ID;
            }
            private set { _ID = value; }
        }

        //The name of the definition
        public string name {
            get { return _name; }
            set { _name = value; }
        }

        ///<summary>The Type of the definition</summary>
        public Type type { get; set; }

        public DynamicParameterDefinition() { }
        public DynamicParameterDefinition(string name, Type type) {
            this.ID = Guid.NewGuid().ToString();
            this.name = name;
            this.type = type;
        }
    }
}