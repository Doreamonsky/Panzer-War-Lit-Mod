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

    ///The base class for all nodes that can live in a NodeCanvas Graph

#if UNITY_EDITOR //handles missing Nodes
    [fsObject(Processor = typeof(fsRecoveryProcessor<Node, MissingNode>))]
#endif

    [ParadoxNotion.Design.SpoofAOT]
    [System.Serializable, fsSerializeAsReference, fsDeserializeOverwrite]
    abstract public partial class Node : IGraphElement, ISerializationCollectable
    {

        //----------------------------------------------------------------------------------------------
        ///Add on an IList (list/array) field to autosort it automatically when the children nodes are autosorted.
        ///Thus keeping the collection the same in respects to the children. Related only to tree graphs.
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
        /////

        ///The graph this node belongs to.
        public Graph graph {
            get { return _graph; }
            internal set { _graph = value; }
        }

        ///The node's int index ID in the graph. This is not persistant in any way. Use UID for that.
        public int ID {
            get { return _ID; }
            internal set { _ID = value; }
        }

        ///The Unique ID of the node. One is created only if requested.
        public string UID => string.IsNullOrEmpty(_UID) ? _UID = System.Guid.NewGuid().ToString() : _UID;

        ///All incomming connections to this node.
        public List<Connection> inConnections {
            get { return _inConnections; }
            protected set { _inConnections = value; }
        }

        ///All outgoing connections from this node.
        public List<Connection> outConnections {
            get { return _outConnections; }
            protected set { _outConnections = value; }
        }

        ///The position of the node in the graph.
        public Vector2 position {
            get { return _position; }
            set { _position = value; }
        }

        ///The custom title name of the node if any.
        private string customName {
            get { return _name; }
            set { _name = value; }
        }

        ///The node tag. Useful for finding nodes through code.
        public string tag {
            get { return _tag; }
            set { _tag = value; }
        }

        ///The comments of the node if any.
        public string comments {
            get { return _comment; }
            set { _comment = value; }
        }

        ///Is the node set as a breakpoint?
        public bool isBreakpoint {
            get { return _isBreakpoint; }
            set { _isBreakpoint = value; }
        }

        ///The title name of the node shown in the window if editor is not in Icon Mode. This is a property so title name may change instance wise
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

        ///The description info of the node
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

        ///The execution priority order of the node when it matters to the graph system
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

        ///The numer of possible inputs. -1 for infinite.
        abstract public int maxInConnections { get; }
        ///The numer of possible outputs. -1 for infinite.
        abstract public int maxOutConnections { get; }
        ///The output connection Type this node has.
        abstract public System.Type outConnectionType { get; }
        ///Can this node be set as prime (Start)?
        abstract public bool allowAsPrime { get; }
        // /Can this node connect to itself?
        abstract public bool canSelfConnect { get; }
        ///Alignment of the comments when shown.
        abstract public Alignment2x2 commentsAlignment { get; }
        ///Alignment of the icons.
        abstract public Alignment2x2 iconAlignment { get; }

        ///The current status of the node
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

        ///The current agent.
        public Component graphAgent => ( graph != null ? graph.agent : null );

        ///The current blackboard.
        public IBlackboard graphBlackboard => ( graph != null ? graph.blackboard : null );

        ///The time in seconds the node has been Status.Running after a reset (Status.Resting)
        public float elapsedTime => ( status == Status.Running ? graph.elapsedTime - timeStarted : 0 );

        ///mark when status running change
        private float timeStarted { get; set; }
        //Used to check recursion
        private bool isChecked { get; set; }
        //used to flag breakpoint reached
        private bool breakPointReached { get; set; }


        ///----------------------------------------------------------------------------------------------
        ///----------------------------------------------------------------------------------------------

        //required
        public Node() { }

        ///Create a new Node of type and assigned to the provided graph. Use this for constructor
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

        ///Duplicate node alone assigned to the provided graph
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

        ///Validate the node in it's graph
        public void Validate(Graph assignedGraph) {
            OnValidate(assignedGraph);
            var hardError = GetHardError();
            if ( hardError != null ) { Logger.LogError(hardError, LogTag.VALIDATION, this); }
            if ( this is IGraphAssignable ) { ( this as IGraphAssignable ).ValidateSubGraphAndParameters(); }
        }

        ///----------------------------------------------------------------------------------------------

        ///The main execution function of the node. Execute the node for the agent and blackboard provided.
        public Status Execute(Component agent, IBlackboard blackboard) {

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

        ///Recursively reset the node and child nodes if it's not Resting already
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

        ///Helper for breakpoints
        IEnumerator YieldBreak(System.Action resume) {
            Debug.Break();
            yield return null;
            resume();
        }

        ///Helper for easier logging
        public Status Error(object msg) {
            if ( msg is System.Exception ) {
                Logger.LogException((System.Exception)msg, LogTag.EXECUTION, this);
            } else {
                Logger.LogError(msg, LogTag.EXECUTION, this);
            }
            status = Status.Error;
            return Status.Error;
        }

        ///Helper for easier logging
        public Status Fail(string msg) {
            Logger.LogError(msg, LogTag.EXECUTION, this);
            status = Status.Failure;
            return Status.Failure;
        }

        ///Helper for easier logging
        public void Warn(string msg) {
            Logger.LogWarning(msg, LogTag.EXECUTION, this);
        }

        ///Set the Status of the node directly. Not recomended if you don't know why!
        public void SetStatus(Status status) {
            this.status = status;
        }

        ///----------------------------------------------------------------------------------------------

        ///Sends an event to the graph (same as calling graph.SendEvent)
        protected void SendEvent(string eventName) {
            graph.SendEvent(eventName, null, this);
        }

        ///Sends an event to the graph (same as calling graph.SendEvent)
        protected void SendEvent<T>(string eventName, T value) {
            graph.SendEvent(eventName, value, this);
        }

        ///----------------------------------------------------------------------------------------------

        ///Returns whether source and target nodes can generaly be connected together.
        ///This only validates max in/out connections that source and target nodes has, along with other validations.
        ///Providing an existing refConnection, will bypass source/target validation respectively if that connection is already connected to that source/target node.
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

        ///Override for explicit handling
        virtual protected bool CanConnectToTarget(Node targetNode) { return true; }
        ///Override for explicit handling
        virtual protected bool CanConnectFromSource(Node sourceNode) { return true; }

        ///Are provided nodes connected at all regardless of parent/child relation?
        public static bool AreNodesConnected(Node a, Node b) {
            Debug.Assert(a != null && b != null, "Null nodes");
            var conditionA = a.outConnections.FirstOrDefault(c => c.targetNode == b) != null;
            var conditionB = b.outConnections.FirstOrDefault(c => c.targetNode == a) != null;
            return conditionA || conditionB;
        }

        ///----------------------------------------------------------------------------------------------

        ///Nodes can use coroutine as normal through MonoManager.
        public Coroutine StartCoroutine(IEnumerator routine) {
            return MonoManager.current != null ? MonoManager.current.StartCoroutine(routine) : null;
        }

        ///Nodes can use coroutine as normal through MonoManager.
        public void StopCoroutine(Coroutine routine) {
            if ( MonoManager.current != null ) { MonoManager.current.StopCoroutine(routine); }
        }


        ///Returns all *direct* parent nodes (first depth level)
        public IEnumerable<Node> GetParentNodes() {
            if ( inConnections.Count != 0 ) {
                return inConnections.Select(c => c.sourceNode);
            }
            return new Node[0];
        }

        ///Returns all *direct* children nodes (first depth level)
        public IEnumerable<Node> GetChildNodes() {
            if ( outConnections.Count != 0 ) {
                return outConnections.Select(c => c.targetNode);
            }
            return new Node[0];
        }

        ///Is node child of parent node?
        public bool IsChildOf(Node parentNode) {
            return inConnections.Any(c => c.sourceNode == parentNode);
        }

        ///Is node parent of child node?
        public bool IsParentOf(Node childNode) {
            return outConnections.Any(c => c.targetNode == childNode);
        }

        ///----------------------------------------------------------------------------------------------

        ///Returns a warning string or null if none
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

        ///A hard error, missing things
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

        ///Override to define node functionality. The Agent and Blackboard used to start the Graph are propagated
        virtual protected Status OnExecute(Component agent, IBlackboard blackboard) { return status; }
        ///Called when the node gets reseted. e.g. OnGraphStart, after a tree traversal, when interrupted, OnGraphEnd etc...
        virtual protected void OnReset() { }

        ///Called once the first time node is created.
        virtual public void OnCreate(Graph assignedGraph) { }
        ///Called when the Node is created, duplicated or otherwise needs validation.
        virtual public void OnValidate(Graph assignedGraph) { }
        ///Called when the Node is removed from the graph (always through graph.RemoveNode)
        virtual public void OnDestroy() { }

        ///Called when an input connection is connected
        virtual public void OnParentConnected(int connectionIndex) { }
        ///Called when an input connection is disconnected but before it actually does
        virtual public void OnParentDisconnected(int connectionIndex) { }
        ///Called when an output connection is connected
        virtual public void OnChildConnected(int connectionIndex) { }
        ///Called when an output connection is disconnected but before it actually does
        virtual public void OnChildDisconnected(int connectionIndex) { }
        ///Called when child connection are sorted
        virtual public void OnChildrenConnectionsSorted(int[] oldIndeces) { }
        ///Called when the parent graph is started. Use to init values or otherwise.
        virtual public void OnGraphStarted() { }
        ///Called when the parent graph is started, but after all OnGraphStarted calls on all nodes.
        virtual public void OnPostGraphStarted() { }
        ///Called when the parent graph is stopped.
        virtual public void OnGraphStoped() { }
        ///Called when the parent graph is stopped, but after all OnGraphStoped calls on all nodes.
        virtual public void OnPostGraphStoped() { }
        ///Called when the parent graph is paused.
        virtual public void OnGraphPaused() { }
        ///Called when the parent graph is unpaused.
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