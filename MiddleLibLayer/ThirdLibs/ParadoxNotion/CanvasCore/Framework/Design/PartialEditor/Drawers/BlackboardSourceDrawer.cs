#if UNITY_EDITOR

using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Editor
{

    ///<summary>A drawer for BlackboardSource</summary>
    public class BlackboardSourceDrawer : ObjectDrawer<BlackboardSource>
    {
        public override BlackboardSource OnGUI(GUIContent content, BlackboardSource instance) {
            if ( instance != null ) {
                BlackboardEditor.ShowVariables(instance, contextUnityObject);
                return instance;
            }

            if ( GUILayout.Button("Create Blackboard") ) {
                instance = new BlackboardSource();
            }

            return instance;
        }
    }
}

#endif