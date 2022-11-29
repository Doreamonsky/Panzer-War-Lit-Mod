using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    ///----------------------------------------------------------------------------------------------
    class PrioritySelector_0 : BTComposite
    {
        [SerializeField] public List<BBParameter<float>> priorities = null;
    }
    ///----------------------------------------------------------------------------------------------

    [Category("Composites")]
    [Description("Used for Utility AI, the Priority Selector executes the child with the highest utility weight. If it fails, the Priority Selector will continue with the next highest utility weight child until one Succeeds, or until all Fail (similar to how a normal Selector does).\n\nEach child path represents a desire, where each desire has one or more consideration which are all averaged.\nConsiderations are a pair of input evaluator value and curve, which together produce the consideration utility weight.")]
    [ParadoxNotion.Design.Icon("Priority")]
    [Color("b3ff7f")]
    [fsMigrateVersions(typeof(PrioritySelector_0))]
    public class PrioritySelector : BTComposite, IMigratable<PrioritySelector_0>
    {
        ///----------------------------------------------------------------------------------------------
        void IMigratable<PrioritySelector_0>.Migrate(PrioritySelector_0 model) {
            this.desires = new List<Desire>();
            foreach ( var priority in model.priorities ) {
                var desire = new Desire();
                this.desires.Add(desire);
                var consideration = desire.AddConsideration(graphBlackboard);
                consideration.input = priority;
            }
        }
        ///----------------------------------------------------------------------------------------------

        [System.Serializable]
        public class Desire
        {
            [ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
            public string name;
            [ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
            public bool foldout;
            public List<Consideration> considerations = new List<Consideration>();

            public Consideration AddConsideration(IBlackboard bb) {
                var result = new Consideration(bb);
                considerations.Add(result);
                return result;
            }

            public void RemoveConsideration(Consideration consideration) { considerations.Remove(consideration); }

            public float GetCompoundUtility() {
                float total = 0;
                for ( var i = 0; i < considerations.Count; i++ ) {
                    total += considerations[i].utility;
                }
                return total / considerations.Count;
            }
        }

        [System.Serializable]
        public class Consideration
        {
            public BBParameter<float> input;
            public BBParameter<AnimationCurve> function;
            public float utility => function.value != null ? function.value.Evaluate(input.value) : input.value;
            public Consideration(IBlackboard blackboard) {
                input = new BBParameter<float> { value = 1f, bb = blackboard };
                function = new BBParameter<AnimationCurve> { bb = blackboard };
            }
        }

        ///----------------------------------------------------------------------------------------------

        [AutoSortWithChildrenConnections]
        public List<Desire> desires;

        private Connection[] orderedConnections;
        private int current = 0;

        public override void OnChildConnected(int index) {
            if ( desires == null ) { desires = new List<Desire>(); }
            if ( desires.Count < outConnections.Count ) { desires.Insert(index, new Desire()); }
        }

        public override void OnChildDisconnected(int index) { desires.RemoveAt(index); }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( status == Status.Resting ) {
                orderedConnections = outConnections.OrderBy(c => desires[outConnections.IndexOf(c)].GetCompoundUtility()).ToArray();
            }

            for ( var i = orderedConnections.Length; i-- > 0; ) {
                status = orderedConnections[i].Execute(agent, blackboard);
                if ( status == Status.Success ) {
                    return Status.Success;
                }

                if ( status == Status.Running ) {
                    current = i;
                    return Status.Running;
                }
            }

            return Status.Failure;
        }

        protected override void OnReset() { current = 0; }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------

#if UNITY_EDITOR

        //..
        public override string GetConnectionInfo(int i) {
            var desire = desires[i];
            var desireName = string.IsNullOrEmpty(desire.name) ? "DESIRE " + i.ToString() : desire.name;
            var result = desireName.ToUpper() + "\n";
            for ( var j = 0; j < desire.considerations.Count; j++ ) {
                result += desire.considerations[j].input.ToString() + " (" + desire.considerations[j].utility.ToString("0.00") + ")" + "\n";
            }
            return result += string.Format("<b>Avg.</b> ({0})", desire.GetCompoundUtility().ToString("0.00"));
        }

        //..
        public override void OnConnectionInspectorGUI(int i) {
            var desire = desires[i];
            var optionsB = new EditorUtils.ReorderableListOptions();
            optionsB.allowAdd = false;
            optionsB.allowRemove = true;
            EditorUtils.ReorderableList(desire.considerations, optionsB, (j, pickedB) =>
            {
                var consideration = desire.considerations[j];
                GUILayout.BeginVertical("box");
                consideration.input = (BBParameter<float>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Input", consideration.input, true);
                consideration.function = (BBParameter<AnimationCurve>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Curve", consideration.function);
                GUILayout.EndVertical();
            });

            if ( GUILayout.Button("Add Consideration") ) { desire.AddConsideration(graphBlackboard); }
            EditorUtils.Separator();
        }

        //..
        protected override void OnNodeInspectorGUI() {

            if ( outConnections.Count == 0 ) {
                GUILayout.Label("Make some connections first");
                return;
            }

            var optionsA = new EditorUtils.ReorderableListOptions();
            optionsA.allowAdd = false;
            optionsA.allowRemove = false;
            EditorUtils.ReorderableList(desires, optionsA, (i, pickedA) =>
            {
                var desire = desires[i];
                var desireName = string.IsNullOrEmpty(desire.name) ? "DESIRE " + i.ToString() : desire.name;
                desire.foldout = UnityEditor.EditorGUILayout.Foldout(desire.foldout, new GUIContent(desireName));
                if ( desire.foldout ) {
                    desire.name = UnityEditor.EditorGUILayout.TextField("   Friendly Name", desire.name);
                    OnConnectionInspectorGUI(i);
                }
            });
        }
#endif

    }
}