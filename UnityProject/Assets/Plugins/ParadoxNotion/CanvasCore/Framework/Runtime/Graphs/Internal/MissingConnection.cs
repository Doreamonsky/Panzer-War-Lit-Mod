using UnityEngine;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion;

namespace NodeCanvas.Framework.Internal
{

    ///<summary>Missing node types are deserialized into this on deserialization and can load back if type is found</summary>
    [DoNotList]
    [Description("Please resolve the MissingConnection issue by either replacing the connection, importing the missing connection type, or refactoring the type in GraphRefactor.")]
    sealed public class MissingConnection : Connection, IMissingRecoverable
    {

        [SerializeField]
        private string _missingType;
        [SerializeField]
        private string _recoveryState;

        string IMissingRecoverable.missingType {
            get { return _missingType; }
            set { _missingType = value; }
        }

        string IMissingRecoverable.recoveryState {
            get { return _recoveryState; }
            set { _recoveryState = value; }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override Color defaultColor => Color.red;
        public override TipConnectionStyle tipConnectionStyle => TipConnectionStyle.None;
        public override ParadoxNotion.PlanarDirection direction => ParadoxNotion.PlanarDirection.Auto;

        protected override void OnConnectionInspectorGUI() {
            GUILayout.Label(_missingType.FormatError());
            GUILayout.Label(_recoveryState);
        }
#endif

    }
}