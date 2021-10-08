#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEditor;

namespace NodeCanvas.Editor
{
    [CustomEditor(typeof(AssetBlackboard))]
    public class AssetBlackboardInspector : UnityEditor.Editor
    {

        private AssetBlackboard bb { get { return (AssetBlackboard)target; } }

        public override void OnInspectorGUI() {
            BlackboardEditor.ShowVariables(bb);
            EditorUtils.EndOfInspector();
            Repaint();
        }
    }
}

#endif