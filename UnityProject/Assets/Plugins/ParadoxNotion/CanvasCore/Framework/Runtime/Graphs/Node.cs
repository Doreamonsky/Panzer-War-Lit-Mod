using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Services;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Framework
{

    ///<summary>The base class for all nodes that can live in a NodeCanvas Graph</summary>

#if UNITY_EDITOR //handles missing Nodes
    [fsObject(Processor = typeof(fsRecoveryProcessor<Node, MissingNode>))]
#endif

    [ParadoxNotion.Design.SpoofAOT]
    [System.Serializable, fsSerializeAsReference, fsDeserializeOverwrite]
    abstract public partial class Node : IGraphElement, ISerializationCollectable
    {

        //----------------------------------------------------------------------------------------------
        ///<summary>Add on an IList (list/array) field to autosort it automatically when the children nodes are autosorted. Thus keeping the collection the same in respects to the children. Related only to tree graphs.</summary>
        [System.AttributeUsage(System.AttributeTargets.Field)]
        protected class AutoSortWithChildrenConnections : System.Attribute { }
        ///----------------------------------------------------------------------------------------------

        [SerializeField] private string _UID;
        [SerializeField] private string _name;
        [SerializeField] private string _tag;
        [SerializeField, fsIgnoreInBuild] private Vector2 _position;
        [SerializeField, fsIgnoreInBuild] private string _comment;
        [SerializeField, fsIgnoreInBuild] private bool _isBreakpoint;

        //reconstructed OnDeserialization
        private Graph _graph;
        //reconstructed OnDeserialization
        private int _ID;
        //reconstructed OnDeserialization
        private List<Connection> _inConnections = new List<Connection>();
        //reconstructed OnDeserialization
        private List<Connection> _outConnections = new List<Connection>();

        [System.NonSerialized] private Status _status = Status.Resting;
        [System.NonSerialized] private string _nameCache;
        [System.NonSerialized] private string _descriptionCache;
        [System.NonSerialized] private int _priorityCache = int.MinValue;
        //

        ///<summary>The graph this node belongs to.</summary>
        public Graph graph {
            get { return _graph; }
            internal set { _graph = value; }
        }

        ///<summary>The node's int index ID in the graph. This is not persistant in any way. Use UID for that.</summary>
        public int ID {
            get { return _ID; }
            internal set { _ID = value; }
        }

        ///<summary>The Unique ID of the node. One is created only if requested.</summary>
        public string UID => string.IsNullOrEmpty(_UID) ? _UID = System.Guid.NewGuid().ToString() : _UID;

        ///<summary>All incomming connections to this node.</summary>
        public List<Connection> inConnections {
            get { return _inConnections; }
            protected set { _inConnections = value; }
        }

        ///<summary>All outgoing connections from this node.</summary>
        public List<Connection> outConnections {
            get { return _outConnections; }
            protected set { _outConnections = value; }
        }

        ///<summary>The position of the node in the graph.</summary>
        public Vector2 position {
            get { return _position; }
            set { _position = value; }
        }

        ///<summary>The custom title name of the node if any.</summary>
        private string customName {
            get { return _name; }
            set { _name = value; }
        }

        ///<summary>The node tag. Useful for finding nodes through code.</summary>
        public string tag {
            get { return _tag; }
            set { _tag = value; }
        }

        ///<summary>The comments of the node if any.</summary>
        public string comments {
            get { return _comment; }
            set { _comment = value; }
        }

        ///<summary>Is the node set as a breakpoint?</summary>
        public bool isBreakpoint {
            get { return _isBreakpoint; }
            set { _isBreakpoint = value; }
        }

        ///<summary>The title name of the node shown in the window if editor is not in Icon Mode. This is a property so title name may change instance wise</summary>
        virtual public string name {
            get
            {
                if ( !string.IsNullOrEmpty(customName) ) {
                    return customName;
                }

                if ( string.IsNullOrEmpty(_nameCache) ) {
                    var nameAtt = this.GetType().RTGetAttribute<NameAttribute>(true);
                    _nameCache = nameAtt != null ? nameAtt.name : GetType().FriendlyName().SplitCamelCase();
                }
                return _nameCache;
            }
            set { customName = value; }
        }

        ///<summary>The description info of the node</summary>
        virtual public string description {
            get
            {
                if ( string.IsNullOrEmpty(_descriptionCache) ) {
                    var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(true);
                    _descriptionCache = descAtt != null ? descAtt.description : "No Description";
                }
                return _descriptionCache;
            }
        }

        ///<summary>The execution priority order of the node when it matters to the graph system</summary>
        virtual public int priority {
            get
            {
                if ( _priorityCache == int.MinValue ) {
                    var prioAtt = this.GetType().RTGetAttribute<ExecutionPriorityAttribute>(true);
                    _priorityCache = prioAtt != null ? prioAtt.priority : 0;
                }
                return _priorityCache;
            }
        }

        ///<summary>The numer of possible inputs. -1 for infinite.</summary>
        abstract public int maxInConnections { get; }
        ///<summary>The numer of possible outputs. -1 for infinite.</summary>
        abstract public int maxOutConnections { get; }
        ///<summary>The output connection Type this node has.</summary>
        abstract public System.Type outConnectionType { get; }
        ///<summary>Can this node be set as prime (Start)?</summary>
        abstract public bool allowAsPrime { get; }
        // /Can this node connect to itself?
        abstract public bool canSelfConnect { get; }
        ///<summary>Alignment of the comments when shown.</summary>
        abstract public Alignment2x2 commentsAlignment { get; }
        ///<summary>Alignment of the icons.</summary>
        abstract public Alignment2x2 iconAlignment { get; }

        ///<summary>The current status of the node</summary>
        public Status status {
            get { return _status; }
            protected set
            {
                if ( _status == Status.Resting && value == Status.Running ) {
                    timeStarted = graph.elapsedTime;
                }
                _status = value;
            }
        }

        ///<summary>The current agent.</summary>
        public Component graphAgent => ( graph != null ? graph.agent : null );

        ///<summary>The current blackboard.</summary>
        public IBlackboard graphBlackboard => ( graph != null ? graph.blackboard : null );

        ///<summary>The time in seconds the node has been Status.Running after a reset (Status.Resting)</summary>
        public float elapsedTime => ( status == Status.Running ? graph.elapsedTime - timeStarted : 0 );

        ///<summary>mark when status running change</summary>
        private float timeStarted { get; set; }
        //Used to check recursion
        private bool isChecked { get; set; }
        //used to flag breakpoint reached
        private bool breakPointReached { get; set; }


        ///----------------------------------------------------------------------------------------------
        ///----------------------------------------------------------------------------------------------

        //required
        public Node() { /*_UID = System.Guid.NewGuid().ToString();*/ }

        ///<summary>Create a new Node of type and assigned to the provided graph. Use this for constructor</summary>
        public static Node Create(Graph targetGraph, System.Type nodeType, Vector2 pos) {

            if ( targetGraph == null ) {
                Logger.LogError("Can't Create a Node without providing a Target Graph", LogTag.GRAPH);
                return null;
            }

            if ( nodeType.IsGenericTypeDefinition ) {
                nodeType = nodeType.RTMakeGenericType(nodeType.GetFirstGenericParameterConstraintType());
            }

            var newNode = (Node)System.Activator.CreateInstance(nodeType);

            UndoUtility.RecordObject(targetGraph, "Create Node");

            newNode.graph = targetGraph;
            newNode.position = pos;
            BBParameter.SetBBFields(newNode, targetGraph.blackboard);
            newNode.Validate(targetGraph);
            newNode.OnCreate(targetGraph);
            UndoUtility.SetDirty(targetGraph);
            return newNode;
        }

        ///<summary>Duplicate node alone assigned to the provided graph</summary>
        public Node Duplicate(Graph targetGraph) {

            if ( targetGraph == null ) {
                Logger.LogError("Can't duplicate a Node without providing a Target Graph", LogTag.GRAPH);
                return null;
            }

            //deep clone
            var newNode = JSONSerializer.Clone<Node>(this);

            UndoUtility.RecordObject(targetGraph, "Duplicate Node");

            targetGraph.allNodes.Add(newNode);
            newNode.inConnections.Clear();
            newNode.outConnections.Clear();

            if ( targetGraph == this.graph ) {
                newNode.position += new Vector2(50, 50);
            }

            newNode._UID = null;
            newNode.graph = targetGraph;
            BBParameter.SetBBFields(newNode, targetGraph.blackboard);

            foreach ( var task in Graph.GetTasksInElement(newNode) ) {
                task.Validate(targetGraph);
            }
            //--

            newNode.Validate(targetGraph);
            UndoUtility.SetDirty(targetGraph);
            return newNode;
        }

        ///<summary>Validate the node in it's graph</summary>
        public void Validate(Graph assignedGraph) {
            OnValidate(assignedGraph);
            var hardError = GetHardError();
            if ( hardError != null ) { Logger.LogError(hardError, LogTag.VALIDATION, this); }
            if ( this is IGraphAssignable ) { ( this as IGraphAssignable ).ValidateSubGraphAndParameters(); }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>The main execution function of the node. Execute the node for the agent and blackboard provided.</summary>
        public Status Execute(Component agent, IBlackboard blackboard) {

            if ( !graph.isRunning ) { return status; }

#if UNITY_EDITOR
            if ( isBreakpoint ) {
                if ( status == Status.Resting ) {
                    var breakEditor = NodeCanvas.Editor.Prefs.breakpointPauseEditor;
                    var owner = agent as GraphOwner;
                    var contextName = owner != null ? owner.gameObject.name : graph.name;
                    Logger.LogWarning(string.Format("Node: '{0}' | ID: '{1}' | Graph Type: '{2}' | Context Object: '{3}'", name, ID, graph.GetType().Name, contextName), "Breakpoint", this);
                    if ( owner != null ) { owner.PauseBehaviour(); }
                    if ( breakEditor ) { StartCoroutine(YieldBreak(() => { if ( owner != null ) { owner.StartBehaviour(); } })); }
                    breakPointReached = true;
                    status = Status.Running;
                    return Status.Running;
                }
                if ( breakPointReached ) {
                    breakPointReached = false;
                    status = Status.Resting;
                }
            }
#endif

            status = OnExecute(agent, blackboard);
            return status;
        }

        ///<summary>Recursively reset the node and child nodes if it's not Resting already</summary>
        public void Reset(bool recursively = true) {

            if ( status == Status.Resting || isChecked ) {
                return;
            }

            OnReset();
            status = Status.Resting;

            isChecked = true;
            for ( var i = 0; i < outConnections.Count; i++ ) {
                outConnections[i].Reset(recursively);
            }
            isChecked = false;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Helper for breakpoints</summary>
        IEnumerator YieldBreak(System.Action resume) {
            Debug.Break();
            yield return null;
            resume();
        }

        ///<summary>Helper for easier logging</summary>
        public Status Error(object msg) {
            if ( msg is System.Exception ) {
                Logger.LogException((System.Exception)msg, LogTag.EXECUTION, this);
            } else {
                Logger.LogError(msg, LogTag.EXECUTION, this);
            }
            status = Status.Error;
            return Status.Error;
        }

        ///<summary>Helper for easier logging</summary>
        public Status Fail(string msg) {
            Logger.LogError(msg, LogTag.EXECUTION, this);
            status = Status.Failure;
            return Status.Failure;
        }

        ///<summary>Helper for easier logging</summary>
        public void Warn(string msg) {
            Logger.LogWarning(msg, LogTag.EXECUTION, this);
        }

        ///<summary>Set the Status of the node directly. Not recomended if you don't know why!</summary>
        public void SetStatus(Status status) {
            this.status = status;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Sends an event to the graph (same as calling graph.SendEvent)</summary>
        protected void SendEvent(string eventName) {
            graph.SendEvent(eventName, null, this);
        }

        ///<summary>Sends an event to the graph (same as calling graph.SendEvent)</summary>
        protected void SendEvent<T>(string eventName, T value) {
            graph.SendEvent(eventName, value, this);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns whether source and target nodes can generaly be connected together. This only validates max in/out connections that source and target nodes has, along with other validations. Providing an existing refConnection, will bypass source/target validation respectively if that connection is already connected to that source/target node.</summary>
        public static bool IsNewConnectionAllowed(Node sourceNode, Node targetNode, Connection refConnection = null) {

            if ( sourceNode == null || targetNode == null ) {
                Logger.LogWarning("A Node Provided is null.", LogTag.EDITOR, targetNode);
                return false;
            }

            if ( sourceNode == targetNode && !sourceNode.canSelfConnect ) {
                Logger.LogWarning("Node can't connect to itself.", LogTag.EDITOR, targetNode);
                return false;
            }

            if ( refConnection == null || refConnection.sourceNode != sourceNode ) {
                if ( sourceNode.outConnections.Count >= sourceNode.maxOutConnections && sourceNode.maxOutConnections != -1 ) {
                    Logger.LogWarning("Source node can have no more out connections.", LogTag.EDITOR, sourceNode);
                    return false;
                }
            }

            if ( refConnection == null || refConnection.targetNode != targetNode ) {
                if ( targetNode.maxInConnections <= targetNode.inConnections.Count && targetNode.maxInConnections != -1 ) {
                    Logger.LogWarning("Target node can have no more in connections.", LogTag.EDITOR, targetNode);
                    return false;
                }
            }

            var final = true;
            final &= sourceNode.CanConnectToTarget(targetNode);
            final &= targetNode.CanConnectFromSource(sourceNode);
            return final;
        }

        ///<summary>Override for explicit handling</summary>
        virtual protected bool CanConnectToTarget(Node targetNode) { return true; }
        ///<summary>Override for explicit handling</summary>
        virtual protected bool CanConnectFromSource(Node sourceNode) { return true; }

        ///<summary>Are provided nodes connected at all regardless of parent/child relation?</summary>
        public static bool AreNodesConnected(Node a, Node b) {
            Debug.Assert(a != null && b != null, "Null nodes");
            var conditionA = a.outConnections.FirstOrDefault(c => c.targetNode == b) != null;
            var conditionB = b.outConnections.FirstOrDefault(c => c.targetNode == a) != null;
            return conditionA || conditionB;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Nodes can use coroutine as normal through MonoManager.</summary>
        public Coroutine StartCoroutine(IEnumerator routine) {
            return MonoManager.current != null ? MonoManager.current.StartCoroutine(routine) : null;
        }

        ///<summary>Nodes can use coroutine as normal through MonoManager.</summary>
        public void StopCoroutine(Coroutine routine) {
            if ( MonoManager.current != null ) { MonoManager.current.StopCoroutine(routine); }
        }


        ///<summary>Returns all *direct* parent nodes (first depth level)</summary>
        public IEnumerable<Node> GetParentNodes() {
            if ( inConnections.Count != 0 ) {
                return inConnections.Select(c => c.sourceNode);
            }
            return new Node[0];
        }

        ///<summary>Returns all *direct* children nodes (first depth level)</summary>
        public IEnumerable<Node> GetChildNodes() {
            if ( outConnections.Count != 0 ) {
                return outConnections.Select(c => c.targetNode);
            }
            return new Node[0];
        }

        ///<summary>Is node child of parent node?</summary>
        public bool IsChildOf(Node parentNode) {
            return inConnections.Any(c => c.sourceNode == parentNode);
        }

        ///<summary>Is node parent of child node?</summary>
        public bool IsParentOf(Node childNode) {
            return outConnections.Any(c => c.targetNode == childNode);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns a warning string or null if none</summary>
        virtual internal string GetWarningOrError() {
            var hardError = GetHardError();
            if ( hardError != null ) { return "* " + hardError; }

            string result = null;
            var assignable = this as ITaskAssignable;
            if ( assignable != null && assignable.task != null ) {
                result = assignable.task.GetWarningOrError();
            }

            return result;
        }

        ///<summary>A hard error, missing things</summary>
        string GetHardError() {
            if ( this is IMissingRecoverable ) {
                return string.Format("Missing Node '{0}'", ( this as IMissingRecoverable ).missingType);
            }

            if ( this is IReflectedWrapper ) {
                var info = ( this as IReflectedWrapper ).GetSerializedInfo();
                if ( info != null && info.AsMemberInfo() == null ) { return string.Format("Missing Reflected Info '{0}'", info.AsString()); }
            }
            return null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Override to define node functionality. The Agent and Blackboard used to start the Graph are propagated</summary>
        virtual protected Status OnExecute(Component agent, IBlackboard blackboard) { return status; }
        ///<summary>Called when the node gets reseted. e.g. OnGraphStart, after a tree traversal, when interrupted, OnGraphEnd etc...</summary>
        virtual protected void OnReset() { }

        ///<summary>Called once the first time node is created.</summary>
        virtual public void OnCreate(Graph assignedGraph) { }
        ///<summary>Called when the Node is created, duplicated or otherwise needs validation.</summary>
        virtual public void OnValidate(Graph assignedGraph) { }
        ///<summary>Called when the Node is removed from the graph (always through graph.RemoveNode)</summary>
        virtual public void OnDestroy() { }

        ///<summary>Called when an input connection is connected</summary>
        virtual public void OnParentConnected(int connectionIndex) { }
        ///<summary>Called when an input connection is disconnected but before it actually does</summary>
        virtual public void OnParentDisconnected(int connectionIndex) { }
        ///<summary>Called when an output connection is connected</summary>
        virtual public void OnChildConnected(int connectionIndex) { }
        ///<summary>Called when an output connection is disconnected but before it actually does</summary>
        virtual public void OnChildDisconnected(int connectionIndex) { }
        ///<summary>Called when child connection are sorted</summary>
        virtual public void OnChildrenConnectionsSorted(int[] oldIndeces) { }
        ///<summary>Called when the parent graph is started. Use to init values or otherwise.</summary>
        virtual public void OnGraphStarted() { }
        ///<summary>Called when the parent graph is started, but after all OnGraphStarted calls on all nodes.</summary>
        virtual public void OnPostGraphStarted() { }
        ///<summary>Called when the parent graph is stopped.</summary>
        virtual public void OnGraphStoped() { }
        ///<summary>Called when the parent graph is stopped, but after all OnGraphStoped calls on all nodes.</summary>
        virtual public void OnPostGraphStoped() { }
        ///<summary>Called when the parent graph is paused.</summary>
        virtual public void OnGraphPaused() { }
        ///<summary>Called when the parent graph is unpaused.</summary>
        virtual public void OnGraphUnpaused() { }

        ///----------------------------------------------------------------------------------------------

        //...
        public override string ToString() {
            var result = name;
            if ( this is IReflectedWrapper ) {
                var info = ( this as IReflectedWrapper ).GetSerializedInfo()?.AsMemberInfo();
                if ( info != null ) { result = info.FriendlyName(); }
            }
            if ( this is IGraphAssignable ) {
                var subGraph = ( this as IGraphAssignable ).subGraph;
                if ( subGraph != null ) { result = subGraph.name; }
            }
            return string.Format("{0}{1}", result, ( !string.IsNullOrEmpty(tag) ? " (" + tag + ")" : "" ));
        }

    }
}