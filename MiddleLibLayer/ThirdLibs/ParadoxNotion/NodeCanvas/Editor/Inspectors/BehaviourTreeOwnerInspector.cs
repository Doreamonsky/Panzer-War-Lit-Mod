#if UNITY_EDITOR

using NodeCanvas.BehaviourTrees;
using ParadoxNotion;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(BehaviourTreeOwner))]
    public class BehaviourTreeOwnerInspector : GraphOwnerInspector
    {

        private BehaviourTreeOwner owner {
            get { return target as BehaviourTreeOwner; }
        }

        protected override void OnPreExtraGraphOptions() {
            ParadoxNotion.Design.EditorUtils.Separator();
            owner.repeat = EditorGUILayout.Toggle("Repeat", owner.repeat);
            if ( owner.repeat ) {
                GUI.color = Color.white.WithAlpha(owner.updateInterval > 0 ? 1 : 0.5f);
                owner.updateInterval = EditorGUILayout.FloatField("Update Interval", owner.updateInterval);
                GUI.color = Color.white;
            }
        }
    }
}

#endif