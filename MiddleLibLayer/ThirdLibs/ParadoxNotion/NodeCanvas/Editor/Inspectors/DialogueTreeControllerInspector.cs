#if UNITY_EDITOR 

using NodeCanvas.DialogueTrees;
using UnityEditor;

namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(DialogueTreeController))]
    public class DialogueTreeControllerInspector : GraphOwnerInspector
    {

        private DialogueTreeController controller {
            get { return target as DialogueTreeController; }
        }

        protected override void OnPostExtraGraphOptions() {
            if ( controller.graph != null ) { DialogueTreeInspector.ShowActorParameters((DialogueTree)controller.graph); }
        }
    }
}

#endif