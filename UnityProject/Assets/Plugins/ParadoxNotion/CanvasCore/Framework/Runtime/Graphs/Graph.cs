using System.Linq;
using System.Collections.Generic;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Services;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;
using UndoUtility = ParadoxNotion.Design.UndoUtility;

namespace NodeCanvas.Framework
{

    ///<summary>This is the base and main class of NodeCanvas and graphs. All graph System are deriving from this.</summary>
    [System.Serializable]
    abstract public partial class Graph : ScriptableObject, ITaskSystem, ISerializationCallbackReceiver
    {
        ///<summary>Update mode of the graph (see 'StartGraph')</summary>
        public enum UpdateMode
        {
            NormalUpdate = 0,
            LateUpdate = 1,
            FixedUpdate = 2,
            Manual = 3,
        }

        ///----------------------------------------------------------------------------------------------

        //the json graph
        [SerializeField] private string _serializedGraph;
        //the unity references used for json graph
        [SerializeField] private List<UnityEngine.Object> _objectReferences;
        //the actual graph data. Mixed serialized by Unity/Json
        [SerializeField] private GraphSource _graphSource = new GraphSource();
        //used to halt self-serialization when something went wrong in deserialization
        [SerializeField] private bool _haltSerialization;

        [System.NonSerialized] private bool haltForUndo;

        ///<summary>Invoked after graph serialization.</summary>
        public static event System.Action<Graph> onGraphSerialized;
        ///<summary>Invoked after graph deserialization.</summary>
        public static event System.Action<Graph> onGraphDeserialized;

        ///----------------------------------------------------------------------------------------------
        void ISerializationCallbackReceiver.OnBeforeSerialize() { SelfSerialize(); }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { SelfDeserialize(); }
        ///----------------------------------------------------------------------------------------------

        ///----------------------------------------------------------------------------------------------
        protected void OnEnable() { Validate(); OnGraphObjectEnable(); }
        protected void OnDisable() { OnGraphObjectDisable(); }
        protected void OnDestroy() { if ( Threader.applicationIsPlaying ) { Stop(); } OnGraphObjectDestroy(); }
        protected void OnValidate() { /*we dont need this now*/  }
        protected void Reset() { OnGraphValidate(); }
        ///----------------------------------------------------------------------------------------------

        ///<summary>Serialize the Graph. Return if serialization changed</summary>
        public bool SelfSerialize() {

            //if something went wrong on deserialization, dont serialize back, but rather keep what we had until a deserialization attempt is successful.
            if ( _haltSerialization ) {
                return false;
            }

            if ( haltForUndo /*|| Threader.applicationIsPlaying*/ ) {
                return false;
            }

            var newReferences = new List<UnityEngine.Object>();
            var newSerialization = this.Serialize(newReferences);
            if ( newSerialization != _serializedGraph || !newReferences.SequenceEqual(_objectReferences) ) {

                haltForUndo = true;
                UndoUtility.RecordObjectComplete(this, UndoUtility.GetLastOperationNameOr("Graph Change"));
                haltForUndo = false;

                //store
                _serializedGraph = newSerialization;
                _objectReferences = newReferences;

#if UNITY_EDITOR
                //notify owner (this is basically used for bound graphs)
                var owner = agent as GraphOwner;
                if ( owner != null ) {
                    owner.OnAfterGraphSerialized(this);
                }
#endif

                //raise event
                if ( onGraphSerialized != null ) {
                    onGraphSerialized(this);
                }

                //purge cache and refs
                graphSource.PurgeRedundantReferences();
                flatMetaGraph = null;
                fullMetaGraph = null;
                nestedMetaGraph = null;

                return true;
            }

            return false;
        }

        ///<summary>Deserialize the Graph. Return if that succeed</summary>
        public bool SelfDeserialize() {

            if ( Deserialize(_serializedGraph, _objectReferences, false) ) {

                //raise event
                if ( onGraphDeserialized != null ) {
                    onGraphDeserialized(this);
                }

                return true;
            }

            return false;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Serialize the graph and returns the serialized json string. The provided objectReferences list will be cleared and populated with the found unity object references.</summary>
        public string Serialize(List<UnityEngine.Object> references) {
            if ( references == null ) { references = new List<Object>(); }
            UpdateNodeIDs(true);
            var result = JSONSerializer.Serialize(typeof(GraphSource), graphSource.Pack(this), references);
            return result;
        }

        ///<summary>Deserialize the json serialized graph provided. Returns the data or null if failed.</summary>
        //The provided references list will be used to read serialized unity object references.
        //IMPORTANT: Validate should be called true in all deserialize cases outside of Unity's 'OnAfterDeserialize',
        //like for example when loading from json, or manualy calling this outside of OnAfterDeserialize.
        //Otherwise, Validate can also be called separately.
        public bool Deserialize(string serializedGraph, List<UnityEngine.Object> references, bool validate) {
            if ( string.IsNullOrEmpty(serializedGraph) ) {
                Logger.LogWarning("JSON is null or empty on graph when deserializing.", LogTag.SERIALIZATION, this);
                return false;
            }

            //the list to load the references from. If not provided explicitely we load from the local list
            if ( references == null ) { references = this._objectReferences; }

            try {
                //deserialize provided serialized graph into a new GraphSerializationData object and load it
                JSONSerializer.TryDeserializeOverwrite<GraphSource>(graphSource, serializedGraph, references);
                if ( graphSource.type != this.GetType().FullName ) {
                    Logger.LogError("Can't Load graph because of different Graph type serialized and required.", LogTag.SERIALIZATION, this);
                    _haltSerialization = true;
                    return false;
                }

                this._graphSource = graphSource.Unpack(this);
                this._serializedGraph = serializedGraph;
                this._objectReferences = references;
                this._haltSerialization = false;
                if ( validate ) { Validate(); }
                return true;
            }

            catch ( System.Exception e ) {
                Logger.LogException(e, LogTag.SERIALIZATION, this);
                this._haltSerialization = true;
                return false;
            }
        }

        ///----------------------------------------------------------------------------------------------
        internal GraphSource GetGraphSource() { return _graphSource; }
        internal string GetSerializedJsonData() { return _serializedGraph; }
        internal List<UnityEngine.Object> GetSerializedReferencesData() { return _objectReferences?.ToList(); }
        internal GraphSource GetGraphSourceMetaDataCopy() { return new GraphSource().SetMetaData(graphSource); }
        internal void SetGraphSourceMetaData(GraphSource source) { graphSource.SetMetaData(source); }
        ///----------------------------------------------------------------------------------------------

        ///<summary>Serialize the local blackboard of the graph alone. The provided references list will be cleared and populated anew.</summary>
        public string SerializeLocalBlackboard(ref List<UnityEngine.Object> references) {
            if ( references != null ) { references.Clear(); }
            return JSONSerializer.Serialize(typeof(BlackboardSource), localBlackboard, references);
        }

        ///<summary>Deserialize the local blackboard of the graph alone.</summary>
        public bool DeserializeLocalBlackboard(string json, List<UnityEngine.Object> references) {
            localBlackboard = JSONSerializer.TryDeserializeOverwrite<BlackboardSource>(localBlackboard, json, references);
            return true;
        }

        ///<summary>Clones the graph as child of parentGraph and returns the new one.</summary>
        public static T Clone<T>(T graph, Graph parentGraph) where T : Graph {
            var newGraph = Instantiate<T>(graph);
            newGraph.name = newGraph.name.Replace("(Clone)", string.Empty);
            newGraph.parentGraph = parentGraph;
            return (T)newGraph;
        }

        ///<summary>Validate the graph, it's nodes and tasks. Also called from OnEnable callback.</summary>
        public void Validate() {

            if ( string.IsNullOrEmpty(_serializedGraph) ) {
                //we dont really have anything to validate in this case
                return;
            }

#if UNITY_EDITOR
            if ( !Threader.applicationIsPlaying ) {
                UpdateReferences(this.agent, this.parentBlackboard, true);
            }
#endif

            for ( var i = 0; i < allNodes.Count; i++ ) {
                try { allNodes[i].Validate(this); } //validation could be critical. we always continue
                catch ( System.Exception e ) { Logger.LogException(e, LogTag.VALIDATION, allNodes[i]); continue; }
            }

            for ( var i = 0; i < allTasks.Count; i++ ) {
                try { allTasks[i].Validate(this); } //validation could be critical. we always continue
                catch ( System.Exception e ) { Logger.LogException(e, LogTag.VALIDATION, allTasks[i]); continue; }
            }

            OnGraphValidate();
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Raised when the graph is Stoped/Finished if it was Started at all. Important: After the event raised, it is also cleared from all subscribers!</summary>
        public event System.Action<bool> onFinish;

        private static List<Graph> _runningGraphs;

        private bool hasInitialized { get; set; }
        private HierarchyTree.Element flatMetaGraph { get; set; }
        private HierarchyTree.Element fullMetaGraph { get; set; }
        private HierarchyTree.Element nestedMetaGraph { get; set; }

        ///----------------------------------------------------------------------------------------------

        ///<summary>The base type of all nodes that can live in this system</summary>
        abstract public System.Type baseNodeType { get; }
        ///<summary>Is this system allowed to start with a null agent?</summary>
        abstract public bool requiresAgent { get; }
        ///<summary>Does the system needs a prime Node to be set for it to start?</summary>
        abstract public bool requiresPrimeNode { get; }
        ///<summary>Is the graph considered to be a tree? (and thus nodes auto sorted on position x)</summary>
        abstract public bool isTree { get; }
        ///<summary>Is overriding local blackboard and parametrizing local blackboard variables allowed?</summary>
        abstract public bool allowBlackboardOverrides { get; }
        ///<summary>Whether the graph can accept variables Drag&Drop</summary>
        abstract public bool canAcceptVariableDrops { get; }

        ///----------------------------------------------------------------------------------------------

        ///<summary>The graph data container</summary>
        private GraphSource graphSource {
            get { return _graphSource; }
            set { _graphSource = value; }
        }

        ///<summary>Graph category</summary>
        public string category {
            get { return graphSource.category; }
            set { graphSource.category = value; }
        }

        ///<summary>Graph Comments</summary>
        public string comments {
            get { return graphSource.comments; }
            set { graphSource.comments = value; }
        }

        ///<summary>The translation of the graph in the total canvas</summary>
        public Vector2 translation {
            get { return graphSource.translation; }
            set { graphSource.translation = value; }
        }

        ///<summary>The zoom of the graph</summary>
        public float zoomFactor {
            get { return graphSource.zoomFactor; }
            set { graphSource.zoomFactor = value; }
        }

        ///<summary>All nodes assigned to this graph</summary>
        public List<Node> allNodes {
            get { return graphSource.nodes; }
            set { graphSource.nodes = value; }
        }

        ///<summary>The canvas groups of the graph</summary>
        public List<CanvasGroup> canvasGroups {
            get { return graphSource.canvasGroups; }
            set { graphSource.canvasGroups = value; }
        }

        ///<summary>The local blackboard of the graph</summary>
        private BlackboardSource localBlackboard {
            get { return graphSource.localBlackboard; }
            set { graphSource.localBlackboard = value; }
        }

        private List<Task> allTasks => graphSource.allTasks;
        private List<BBParameter> allParameters => graphSource.allParameters;
        private List<Connection> allConnections => graphSource.connections;

        ///----------------------------------------------------------------------------------------------

        ///<summary>In runtime only, returns the root graph in case this is a nested graph. Returns itself if not.</summary>
        public Graph rootGraph {
            get
            {
                var current = this;
                while ( current.parentGraph != null ) {
                    current = current.parentGraph;
                }
                return current;
            }
        }

        ///<summary>Is serialization halted? (could be in case of deserialization error)</summary>
        public bool serializationHalted => _haltSerialization;

        ///<summary>All currently running graphs</summary>
        public static IEnumerable<Graph> runningGraphs => _runningGraphs;

        ///<summary>The parent Graph if any when this graph is a nested one. Set in runtime only after the nested graph (this) is instantiated via 'Clone' method.</summary>
        public Graph parentGraph { get; private set; }

        ///<summary>The time in seconds this graph is running</summary>
        public float elapsedTime { get; private set; }

        ///<summary>The delta time used to update the graph</summary>
        public float deltaTime { get; private set; }

        ///<summary>The last frame (Time.frameCount) the graph was updated</summary>
        public int lastUpdateFrame { get; private set; }

        ///<summary>Did the graph update this or the previous frame?</summary>
        public bool didUpdateLastFrame => ( lastUpdateFrame >= Time.frameCount - 1 );

        ///<summary>Is the graph running?</summary>
        public bool isRunning { get; private set; }

        ///<summary>Is the graph paused?</summary>
        public bool isPaused { get; private set; }

        ///<summary>The current update mode used for the graph</summary>
        public UpdateMode updateMode { get; private set; }

        ///<summary>The 'Start' node. It should always be the first node in the nodes collection</summary>
        public Node primeNode {
            get { return allNodes.Count > 0 && allNodes[0].allowAsPrime ? allNodes[0] : null; }
            set
            {
                if ( primeNode != value && allNodes.Contains(value) ) {
                    if ( value != null && value.allowAsPrime ) {
                        if ( isRunning ) {
                            if ( primeNode != null ) { primeNode.Reset(); }
                            value.Reset();
                        }
                        UndoUtility.RecordObjectComplete(this, "Set Start");
                        allNodes.Remove(value);
                        allNodes.Insert(0, value);
                        UpdateNodeIDs(true);
                        UndoUtility.SetDirty(this);
                    }
                }
            }
        }

        ///<summary>The agent currently used by the graph</summary>
        public Component agent { get; private set; }

        ///<summary>The local blackboard of the graph where parentBlackboard if any is parented to</summary>
        public IBlackboard blackboard => localBlackboard;

        ///<summary>The blackboard which is parented to the graph's local blackboard Should be the same as '.blackboard.parent' and usually refers to the GraphOwner (agent) .blackboard</summary>
        public IBlackboard parentBlackboard { get; private set; }

        ///<summary>The UnityObject of the ITaskSystem. In this case the graph itself</summary>
        UnityEngine.Object ITaskSystem.contextObject => this;

        ///----------------------------------------------------------------------------------------------

        ///<summary>See UpdateReferences</summary>
        public void UpdateReferencesFromOwner(GraphOwner owner, bool force = false) {
            UpdateReferences(owner, owner != null ? owner.blackboard : null, force);
        }

        ///<summary>Update the Agent/Component and Blackboard references. This is done when the graph initialize or start, and in the editor for convenience.</summary>
        public void UpdateReferences(Component newAgent, IBlackboard newParentBlackboard, bool force = false) {
            if ( !ReferenceEquals(this.agent, newAgent) || !ReferenceEquals(this.parentBlackboard, newParentBlackboard) || force ) {
                this.agent = newAgent;
                this.parentBlackboard = newParentBlackboard;
                if ( !ReferenceEquals(newParentBlackboard, this.localBlackboard) && allowBlackboardOverrides ) {
                    this.localBlackboard.parent = newParentBlackboard;
                } else {
                    this.localBlackboard.parent = null;
                }

                this.localBlackboard.propertiesBindTarget = newAgent;
                this.localBlackboard.unityContextObject = this;

                UpdateNodeBBFields();
                ( (ITaskSystem)this ).UpdateTasksOwner();
            }
        }

        ///<summary>Update all graph node's BBFields for current Blackboard.</summary>
        void UpdateNodeBBFields() {
            for ( var i = 0; i < allParameters.Count; i++ ) {
                allParameters[i].bb = blackboard;
            }
        }

        ///<summary>Sets all graph Tasks' owner system (which is this graph).</summary>
        void ITaskSystem.UpdateTasksOwner() {
            for ( var i = 0; i < allTasks.Count; i++ ) {
                allTasks[i].SetOwnerSystem(this);
            }
        }

        ///<summary>Update the IDs of the nodes in the graph. Is automatically called whenever a change happens in the graph by the adding, removing, connecting etc.</summary>
        public void UpdateNodeIDs(bool alsoReorderList) {

            if ( allNodes.Count == 0 ) {
                return;
            }

            var lastID = -1;
            var parsed = new Node[allNodes.Count];

            if ( primeNode != null ) {
                lastID = AssignNodeID(primeNode, lastID, ref parsed);
            }

            foreach ( var node in allNodes.OrderBy(n => ( n.inConnections.Count == 0 ? 0 : 1 ) + n.priority * -1) ) {
                lastID = AssignNodeID(node, lastID, ref parsed);
            }

            if ( alsoReorderList ) {
                allNodes = parsed.ToList();
            }
        }

        //Used above to assign a node's ID and list order
        int AssignNodeID(Node node, int lastID, ref Node[] parsed) {
            if ( !parsed.Contains(node) ) {
                lastID++;
                node.ID = lastID;
                parsed[lastID] = node;
                for ( var i = 0; i < node.outConnections.Count; i++ ) {
                    var targetNode = node.outConnections[i].targetNode;
                    lastID = AssignNodeID(targetNode, lastID, ref parsed);
                }
            }
            return lastID;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>used for thread safe init calls</summary>
        private event System.Action delayedInitCalls;
        ///<summary>used for thread safe init calls</summary>
        protected void ThreadSafeInitCall(System.Action call) {
            if ( Threader.isMainThread ) { call(); } else { delayedInitCalls += call; }
        }

        ///<summary>Load overwrite the graph async. Used by GraphOwner.</summary>
        public async void LoadOverwriteAsync(GraphLoadData data, System.Action callback) {
            await System.Threading.Tasks.Task.Run(() => LoadOverwrite(data));
            if ( delayedInitCalls != null ) {
                delayedInitCalls();
                delayedInitCalls = null;
            }
            callback();
        }

        ///<summary>Load overwrite the graph. Used by GraphOwner.</summary>
        public void LoadOverwrite(GraphLoadData data) {
            SetGraphSourceMetaData(data.source);
            Deserialize(data.json, data.references, false);
            UpdateReferences(data.agent, data.parentBlackboard);
            Validate();
            OnGraphInitialize();
            // TODO: Make subgraphs instance in main thread and init them as parallel tasks
            if ( data.preInitializeSubGraphs ) { ThreadSafeInitCall(PreInitializeSubGraphs); }
            ThreadSafeInitCall(() => localBlackboard.InitializePropertiesBinding(data.agent, false));
            hasInitialized = true;
        }

        ///<summary>Initialize the graph for target agent/blackboard with option to preload subgraphs. This is called from StartGraph as well if Initialize has not been called before.</summary>
        public void Initialize(Component newAgent, IBlackboard newParentBlackboard, bool preInitializeSubGraphs) {
            Debug.Assert(Threader.applicationIsPlaying, "Initialize should have been called in play mode only.");
            Debug.Assert(!hasInitialized, "Graph is already initialized.");
            UpdateReferences(newAgent, newParentBlackboard);
            OnGraphInitialize();
            if ( preInitializeSubGraphs ) { PreInitializeSubGraphs(); }
            localBlackboard.InitializePropertiesBinding(newAgent, false);
            hasInitialized = true;
        }

        ///<summary>Preloads and initialize all subgraphs of this graph recursively</summary>
        void PreInitializeSubGraphs() {
            foreach ( var assignable in allNodes.OfType<IGraphAssignable>() ) {
                var instance = assignable.CheckInstance();
                if ( instance != null ) {
                    instance.Initialize(this.agent, this.blackboard.parent, /*Preinit Subs:*/ true);
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Start the graph for the agent and blackboard provided with specified update mode. Optionally provide a callback for when the graph stops/ends</summary>
        public void StartGraph(Component newAgent, IBlackboard newParentBlackboard, UpdateMode newUpdateMode, System.Action<bool> callback = null) {

#if UNITY_EDITOR
            Debug.Assert(Application.isPlaying, "StartGraph should have been called in play mode only.");
            Debug.Assert(!UnityEditor.EditorUtility.IsPersistent(this), "You have tried to start a graph which is an asset, not an instance! You should Instantiate the graph first.");
#endif

            Debug.Assert(newParentBlackboard != this.blackboard, "StartGraph called with blackboard parameter being the same as the graph blackboard");

            if ( newAgent == null && requiresAgent ) {
                Logger.LogError("You've tried to start a graph with null Agent.", LogTag.GRAPH, this);
                return;
            }

            if ( primeNode == null && requiresPrimeNode ) {
                Logger.LogError("You've tried to start graph without a 'Start' node.", LogTag.GRAPH, this);
                return;
            }

            if ( isRunning && !isPaused ) {
                Logger.LogWarning("Graph is already Active and not Paused.", LogTag.GRAPH, this);
                return;
            }


            if ( !hasInitialized ) {
                Initialize(newAgent, newParentBlackboard, false);
            } else {
                //if the graph has pre-initialized with same targets, this call does basically nothing,
                //but we still need to call it in case the graph is started with different targets.
                UpdateReferences(newAgent, newParentBlackboard);
            }

            if ( callback != null ) { onFinish = callback; }

            if ( isRunning && isPaused ) {
                Resume();
                return;
            }

            if ( _runningGraphs == null ) { _runningGraphs = new List<Graph>(); }
            _runningGraphs.Add(this);
            elapsedTime = 0;

            isRunning = true;
            isPaused = false;

            OnGraphStarted();

            for ( var i = 0; i < allNodes.Count; i++ ) {
                allNodes[i].OnGraphStarted();
            }

            for ( var i = 0; i < allNodes.Count; i++ ) {
                allNodes[i].OnPostGraphStarted();
            }

            updateMode = newUpdateMode;
            if ( updateMode != UpdateMode.Manual ) {
                MonoManager.current.AddUpdateCall((MonoManager.UpdateMode)updateMode, UpdateGraph);
            }
        }

        ///<summary>Stops the graph completely and resets all nodes.</summary>
        public void Stop(bool success = true) {

            if ( !isRunning ) {
                return;
            }

            _runningGraphs.Remove(this);
            if ( updateMode != UpdateMode.Manual ) {
                MonoManager.current.RemoveUpdateCall((MonoManager.UpdateMode)updateMode, UpdateGraph);
            }

            for ( var i = 0; i < allNodes.Count; i++ ) {
                var node = allNodes[i];
                //try stop subgraphs first
                if ( node is IGraphAssignable ) { ( node as IGraphAssignable ).TryStopSubGraph(); }
                node.Reset(false);
                node.OnGraphStoped();
            }

            for ( var i = 0; i < allNodes.Count; i++ ) {
                allNodes[i].OnPostGraphStoped();
            }

            OnGraphStoped();

            isRunning = false;
            isPaused = false;

            if ( onFinish != null ) {
                onFinish(success);
                onFinish = null;
            }

            //reset elapsed time after onFinish callback in case it is needed info
            elapsedTime = 0;
        }

        ///<summary>Pauses the graph from updating as well as notifying all nodes.</summary>
        public void Pause() {

            if ( !isRunning || isPaused ) {
                return;
            }

            if ( updateMode != UpdateMode.Manual ) {
                MonoManager.current.RemoveUpdateCall((MonoManager.UpdateMode)updateMode, UpdateGraph);
            }

            isRunning = true;
            isPaused = true;

            for ( var i = 0; i < allNodes.Count; i++ ) {
                var node = allNodes[i];
                if ( node is IGraphAssignable ) { ( node as IGraphAssignable ).TryPauseSubGraph(); }
                node.OnGraphPaused();
            }

            OnGraphPaused();
        }

        ///<summary>Resumes a paused graph</summary>
        public void Resume() {

            if ( !isRunning || !isPaused ) {
                return;
            }

            isRunning = true;
            isPaused = false;

            OnGraphUnpaused();

            for ( var i = 0; i < allNodes.Count; i++ ) {
                var node = allNodes[i];
                if ( node is IGraphAssignable ) { ( node as IGraphAssignable ).TryResumeSubGraph(); }
                node.OnGraphUnpaused();
            }

            if ( updateMode != UpdateMode.Manual ) {
                MonoManager.current.AddUpdateCall((MonoManager.UpdateMode)updateMode, UpdateGraph);
            }
        }

        ///<summary>Same as Stop - Start</summary>
        public void Restart() {
            Stop();
            StartGraph(agent, blackboard, updateMode, onFinish);
        }

        ///<summary>Updates the graph. Normaly this is updated by MonoManager since at StartGraph, this method is registered for updating by GraphOwner.</summary>
        public void UpdateGraph() { UpdateGraph(Time.deltaTime); }
        public void UpdateGraph(float deltaTime) {
            // UnityEngine.Profiling.Profiler.BeginSample(string.Format("Graph Update ({0})", agent != null? agent.name : this.name) );
            if ( isRunning ) {
                this.deltaTime = deltaTime;
                elapsedTime += deltaTime;
                lastUpdateFrame = Time.frameCount;
                OnGraphUpdate();
            } else {
                Logger.LogWarning("UpdateGraph called in a non-running, non-paused graph. StartGraph() or StartBehaviour() should be called first.", LogTag.EXECUTION, this);
            }
            // UnityEngine.Profiling.Profiler.EndSample();
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Graph can override this for derived data serialization if needed</summary>
        virtual public object OnDerivedDataSerialization() { return null; }
        ///<summary>Graph can override this for derived data deserialization if needed</summary>
        virtual public void OnDerivedDataDeserialization(object data) { }

        ///<summary>Override for graph initialization</summary>
        virtual protected void OnGraphInitialize() { }
        ///<summary>Override for graph specific stuff to run when the graph is started</summary>
        virtual protected void OnGraphStarted() { }
        ///<summary>Override for graph specific per frame logic. Called every frame if the graph is running</summary>
        virtual protected void OnGraphUpdate() { }
        ///<summary>Override for graph specific stuff to run when the graph is stoped</summary>
        virtual protected void OnGraphStoped() { }
        ///<summary>Override for graph stuff to run when the graph is paused</summary>
        virtual protected void OnGraphPaused() { }
        ///<summary>Override for graph stuff to run when the graph is resumed</summary>
        virtual protected void OnGraphUnpaused() { }

        ///<summary>Called when the unity object graph is enabled</summary>
        virtual protected void OnGraphObjectEnable() { }
        ///<summary>Called when the unity object graph is disabled</summary>
        virtual protected void OnGraphObjectDisable() { }
        ///<summary>Called when the unity object graph is destroyed</summary>
        virtual protected void OnGraphObjectDestroy() { }
        ///<summary>Use this for derived graph Validation</summary>
        virtual protected void OnGraphValidate() { }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Invokes named onCustomEvent on EventRouter</summary>
        public void SendEvent(string name, object value, object sender) {
            if ( agent == null || !isRunning ) { return; }
            Logger.Log(string.Format("Event '{0}' Send to '{1}'", name, agent.gameObject.name), LogTag.EVENT, agent);
            var router = agent.GetComponent<EventRouter>();
            if ( router != null ) { router.InvokeCustomEvent(name, value, sender); }
        }

        ///<summary>Invokes named onCustomEvent on EventRouter</summary>
        public void SendEvent<T>(string name, T value, object sender) {
            if ( agent == null || !isRunning ) { return; }
            Logger.Log(string.Format("Event '{0}' Send to '{1}'", name, agent.gameObject.name), LogTag.EVENT, agent);
            var router = agent.GetComponent<EventRouter>();
            if ( router != null ) { router.InvokeCustomEvent(name, value, sender); }
        }

        ///<summary>Invokes named onCustomEvent on EventRouter globaly for all running graphs</summary>
        public static void SendGlobalEvent(string name, object value, object sender) {
            if ( _runningGraphs == null ) { return; }
            var sent = new List<GameObject>();
            foreach ( var graph in _runningGraphs.ToArray() ) {
                if ( graph.agent != null && !sent.Contains(graph.agent.gameObject) ) {
                    sent.Add(graph.agent.gameObject);
                    graph.SendEvent(name, value, sender);
                }
            }
        }

        ///<summary>Invokes named onCustomEvent on EventRouter globaly for all running graphs</summary>
        public static void SendGlobalEvent<T>(string name, T value, object sender) {
            if ( _runningGraphs == null ) { return; }
            var sent = new List<GameObject>();
            foreach ( var graph in _runningGraphs.ToArray() ) {
                if ( graph.agent != null && !sent.Contains(graph.agent.gameObject) ) {
                    sent.Add(graph.agent.gameObject);
                    graph.SendEvent(name, value, sender);
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns all BBParameters serialized in the graph</summary>
        public IEnumerable<BBParameter> GetAllParameters() { return allParameters; }

        ///<summary>Returns all connections</summary>
        public IEnumerable<Connection> GetAllConnections() { return allConnections; }

        ///<summary>Returns all tasks of type T</summary>
        public IEnumerable<T> GetAllTasksOfType<T>() where T : Task { return allTasks.OfType<T>(); }

        ///<summary>Get a node by it's ID, null if not found. The ID should always be the same as the node's index in allNodes list.</summary>
        public Node GetNodeWithID(int searchID) {
            if ( searchID < allNodes.Count && searchID >= 0 ) {
                return allNodes.Find(n => n.ID == searchID);
            }
            return null;
        }

        ///<summary>Get all nodes of a specific type</summary>
        public IEnumerable<T> GetAllNodesOfType<T>() where T : Node {
            return allNodes.OfType<T>();
        }

        ///<summary>Get a node by it's tag name</summary>
        public T GetNodeWithTag<T>(string tagName) where T : Node {
            return allNodes.OfType<T>().FirstOrDefault(n => n.tag == tagName);
        }

        ///<summary>Get all nodes taged with such tag name</summary>
        public IEnumerable<T> GetNodesWithTag<T>(string tagName) where T : Node {
            return allNodes.OfType<T>().Where(n => n.tag == tagName);
        }

        ///<summary>Get all taged nodes regardless tag name</summary>
        public IEnumerable<T> GetAllTagedNodes<T>() where T : Node {
            return allNodes.OfType<T>().Where(n => !string.IsNullOrEmpty(n.tag));
        }

        ///<summary>Get all nodes of the graph that have no incomming connections</summary>
        public IEnumerable<Node> GetRootNodes() {
            return allNodes.Where(n => n.inConnections.Count == 0);
        }

        ///<summary>Get all nodes of the graph that have no outgoing connections</summary>
        public IEnumerable<Node> GetLeafNodes() {
            return allNodes.Where(n => n.outConnections.Count == 0);
        }

        ///<summary>Get all Nested graphs of this graph</summary>
        public IEnumerable<T> GetAllNestedGraphs<T>(bool recursive) where T : Graph {
            var graphs = new List<T>();
            foreach ( var node in allNodes.OfType<IGraphAssignable>() ) {
                if ( node.subGraph is T ) {
                    if ( !graphs.Contains((T)node.subGraph) ) {
                        graphs.Add((T)node.subGraph);
                    }
                    if ( recursive ) {
                        graphs.AddRange(node.subGraph.GetAllNestedGraphs<T>(recursive));
                    }
                }
            }
            return graphs;
        }

        ///<summary>Get all runtime instanced Nested graphs of this graph and it's sub-graphs</summary>
        public IEnumerable<Graph> GetAllInstancedNestedGraphs() {
            var instances = new List<Graph>();
            foreach ( var node in allNodes.OfType<IGraphAssignable>() ) {
                if ( node.instances != null ) {
                    var subInstances = node.instances.Values;
                    instances.AddRange(subInstances);
                    foreach ( var subInstance in subInstances ) {
                        instances.AddRange(subInstance.GetAllInstancedNestedGraphs());
                    }
                }
            }
            return instances;
        }

        ///<summary>Returns all defined BBParameter names found in graph</summary>
        public IEnumerable<BBParameter> GetDefinedParameters() {
            return allParameters.Where(p => p != null && p.isDefined);
        }

        ///<summary>Utility function to create all defined parameters of this graph as variables into the provided blackboard.</summary>
        public void PromoteMissingParametersToVariables(IBlackboard bb) {
            foreach ( var bbParam in GetDefinedParameters() ) {
                if ( bbParam.varRef == null && !bbParam.isPresumedDynamic ) {
                    bbParam.PromoteToVariable(bb);
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Given an object returns the relevant graph if any can be resolved</summary>
        public static Graph GetElementGraph(object obj) {
            if ( obj is GraphOwner ) { return ( obj as GraphOwner ).graph; }
            if ( obj is Graph ) { return (Graph)obj; }
            if ( obj is Node ) { return ( obj as Node ).graph; }
            if ( obj is Connection ) { return ( obj as Connection ).graph; }
            if ( obj is Task ) { return ( obj as Task ).ownerSystem as Graph; }
            if ( obj is BlackboardSource ) { return ( obj as BlackboardSource ).unityContextObject as Graph; }
            return null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns a structure of the graph that includes Nodes, Connections, Tasks and BBParameters, but with nodes elements all being root to the graph (instead of respective parent connections).</summary>
        public HierarchyTree.Element GetFlatMetaGraph() {

            if ( flatMetaGraph != null ) {
                return flatMetaGraph;
            }

            var root = new HierarchyTree.Element(this);
            int lastID = 0;
            for ( var i = 0; i < allNodes.Count; i++ ) {
                root.AddChild(GetTreeNodeElement(allNodes[i], false, ref lastID));
            }
            return flatMetaGraph = root;
        }

        ///<summary>Returns a structure of the graph that includes Nodes, Connections, Tasks and BBParameters, but where node elements are parent to their respetive connections. Only possible for tree-like graphs.</summary>
        public HierarchyTree.Element GetFullMetaGraph() {

            if ( fullMetaGraph != null ) {
                return fullMetaGraph;
            }

            var root = new HierarchyTree.Element(this);
            int lastID = 0;
            if ( primeNode != null ) {
                root.AddChild(GetTreeNodeElement(primeNode, true, ref lastID));
            }
            for ( var i = 0; i < allNodes.Count; i++ ) {
                var node = allNodes[i];
                if ( node.ID > lastID && node.inConnections.Count == 0 ) {
                    root.AddChild(GetTreeNodeElement(node, true, ref lastID));
                }
            }
            return fullMetaGraph = root;
        }

        ///<summary>Returns a structure of all nested graphs recursively, contained within this graph.</summary>
        public HierarchyTree.Element GetNestedMetaGraph() {

            if ( nestedMetaGraph != null ) {
                return nestedMetaGraph;
            }

            var root = new HierarchyTree.Element(this);
            DigNestedGraphs(this, root);
            return nestedMetaGraph = root;
        }

        //Used above
        static void DigNestedGraphs(Graph currentGraph, HierarchyTree.Element currentElement) {
            for ( var i = 0; i < currentGraph.allNodes.Count; i++ ) {
                var assignable = currentGraph.allNodes[i] as IGraphAssignable;
                if ( assignable != null && assignable.subGraph != null ) {
                    DigNestedGraphs(assignable.subGraph, currentElement.AddChild(new HierarchyTree.Element(assignable)));
                }
            }
        }

        ///<summary>Used above. Returns a node hierarchy element optionaly along with all it's children recursively</summary>
        static HierarchyTree.Element GetTreeNodeElement(Node node, bool recurse, ref int lastID) {
            var nodeElement = CollectSubElements(node);
            for ( var i = 0; i < node.outConnections.Count; i++ ) {
                var connectionElement = CollectSubElements(node.outConnections[i]);
                nodeElement.AddChild(connectionElement);
                if ( recurse ) {
                    var targetNode = node.outConnections[i].targetNode;
                    if ( targetNode.ID > node.ID ) { //ensure no recursion loop
                        connectionElement.AddChild(GetTreeNodeElement(targetNode, recurse, ref lastID));
                    }
                }
            }
            lastID = node.ID;
            return nodeElement;
        }

        ///<summary>Returns an element that includes tasks and parameters for target object recursively</summary>
        static HierarchyTree.Element CollectSubElements(IGraphElement obj) {
            HierarchyTree.Element parentElement = null;
            var stack = new Stack<HierarchyTree.Element>();

            JSONSerializer.SerializeAndExecuteNoCycles(obj.GetType(), obj, (o) =>
            {
                if ( o is ISerializationCollectable ) {
                    var e = new HierarchyTree.Element(o);
                    if ( stack.Count > 0 ) { stack.Peek().AddChild(e); }
                    stack.Push(e);
                }
            }, (o, d) =>
            {
                if ( o is ISerializationCollectable ) {
                    parentElement = stack.Pop();
                }
            });

            return parentElement;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Get the parent graph element (node/connection) from target Task.</summary>
        public IGraphElement GetTaskParentElement(Task targetTask) {
            var targetElement = GetFlatMetaGraph().FindReferenceElement(targetTask);
            return targetElement != null ? targetElement.GetFirstParentReferenceOfType<IGraphElement>() : null;
        }

        ///<summary>Get the parent graph element (node/connection) from target BBParameter</summary>
        public IGraphElement GetParameterParentElement(BBParameter targetParameter) {
            var targetElement = GetFlatMetaGraph().FindReferenceElement(targetParameter);
            return targetElement != null ? targetElement.GetFirstParentReferenceOfType<IGraphElement>() : null;
        }

        ///<summary>Get all Tasks found in target</summary>
        public static IEnumerable<Task> GetTasksInElement(IGraphElement target) {
            var result = new List<Task>();
            JSONSerializer.SerializeAndExecuteNoCycles(target.GetType(), target, (o, d) =>
            {
                if ( o is Task ) { result.Add((Task)o); }
            });
            return result;
        }

        ///<summary>Get all BBParameters found in target</summary>
        public static IEnumerable<BBParameter> GetParametersInElement(IGraphElement target) {
            var result = new List<BBParameter>();
            JSONSerializer.SerializeAndExecuteNoCycles(target.GetType(), target, (o, d) =>
            {
                if ( o is BBParameter ) { result.Add((BBParameter)o); }
            });
            return result;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Add a new node to this graph</summary>
        public T AddNode<T>() where T : Node { return (T)AddNode(typeof(T)); }
        public T AddNode<T>(Vector2 pos) where T : Node { return (T)AddNode(typeof(T), pos); }
        public Node AddNode(System.Type nodeType) { return AddNode(nodeType, new Vector2(-translation.x + 100, -translation.y + 100)); }
        public Node AddNode(System.Type nodeType, Vector2 pos) {

            if ( !nodeType.RTIsSubclassOf(baseNodeType) ) {
                Logger.LogWarning(nodeType + " can't be added to " + this.GetType().FriendlyName() + " graph.", LogTag.GRAPH, this);
                return null;
            }

            var newNode = Node.Create(this, nodeType, pos);

            UndoUtility.RecordObject(this, "New Node");

            allNodes.Add(newNode);

            if ( primeNode == null ) {
                primeNode = newNode;
            }

            UpdateNodeIDs(false);
            UndoUtility.SetDirty(this);

            return newNode;
        }

        ///<summary>Disconnects and then removes a node from this graph</summary>
        public void RemoveNode(Node node, bool recordUndo = true, bool force = false) {

            if ( !force && node.GetType().RTIsDefined<ParadoxNotion.Design.ProtectedSingletonAttribute>(true) ) {
                if ( allNodes.Where(n => n.GetType() == node.GetType()).Count() == 1 ) {
                    return;
                }
            }

            if ( !allNodes.Contains(node) ) {
                Logger.LogWarning("Node is not part of this graph.", "NodeCanvas", this);
                return;
            }

            if ( !Application.isPlaying ) {
                //auto reconnect parent & child of deleted node. Just a workflow convenience
                if ( isTree && node.inConnections.Count == 1 && node.outConnections.Count == 1 ) {
                    var relinkNode = node.outConnections[0].targetNode;
                    if ( relinkNode != node.inConnections[0].sourceNode ) {
                        RemoveConnection(node.outConnections[0]);
                        node.inConnections[0].SetTargetNode(relinkNode);
                    }
                }
            }

#if UNITY_EDITOR
            if ( NodeCanvas.Editor.GraphEditorUtility.activeElement == node ) {
                NodeCanvas.Editor.GraphEditorUtility.activeElement = null;
            }
#endif

            //callback
            node.OnDestroy();

            //disconnect parents
            for ( var i = node.inConnections.Count; i-- > 0; ) {
                RemoveConnection(node.inConnections[i]);
            }

            //disconnect children
            for ( var i = node.outConnections.Count; i-- > 0; ) {
                RemoveConnection(node.outConnections[i]);
            }

            if ( recordUndo ) { UndoUtility.RecordObject(this, "Delete Node"); }

            allNodes.Remove(node);

            if ( node == primeNode ) {
                primeNode = GetNodeWithID(primeNode.ID + 1);
            }

            UpdateNodeIDs(false);
            UndoUtility.SetDirty(this);
        }

        ///<summary>Connect two nodes together to a specific port index of the source and target node. Leave index at -1 to add at the end of the list.</summary>
        public Connection ConnectNodes(Node sourceNode, Node targetNode, int sourceIndex = -1, int targetIndex = -1) {

            if ( Node.IsNewConnectionAllowed(sourceNode, targetNode) == false ) {
                return null;
            }

            UndoUtility.RecordObject(this, "Add Connection");

            var newConnection = Connection.Create(sourceNode, targetNode, sourceIndex, targetIndex);

            UpdateNodeIDs(false);
            UndoUtility.SetDirty(this);

            return newConnection;
        }

        ///<summary>Removes a connection</summary>
        public void RemoveConnection(Connection connection, bool recordUndo = true) {

            //for live editing
            if ( Application.isPlaying ) {
                connection.Reset();
            }

            if ( recordUndo ) { UndoUtility.RecordObject(this, "Remove Connection"); }

            //callbacks
            connection.OnDestroy();
            connection.sourceNode.OnChildDisconnected(connection.sourceNode.outConnections.IndexOf(connection));
            connection.targetNode.OnParentDisconnected(connection.targetNode.inConnections.IndexOf(connection));

            connection.sourceNode.outConnections.Remove(connection);
            connection.targetNode.inConnections.Remove(connection);

#if UNITY_EDITOR
            if ( NodeCanvas.Editor.GraphEditorUtility.activeElement == connection ) {
                NodeCanvas.Editor.GraphEditorUtility.activeElement = null;
            }
#endif

            UpdateNodeIDs(false);
            UndoUtility.SetDirty(this);
        }

        ///<summary>Makes a copy of provided nodes and if targetGraph is provided, puts those new nodes in that graph.</summary>
        public static List<Node> CloneNodes(List<Node> originalNodes, Graph targetGraph = null, Vector2 originPosition = default(Vector2)) {

            if ( targetGraph != null ) {
                if ( originalNodes.Any(n => n.GetType().IsSubclassOf(targetGraph.baseNodeType) == false) ) {
                    return null;
                }
            }

            var newNodes = new List<Node>();
            var linkInfo = new Dictionary<Connection, KeyValuePair<int, int>>();

            //duplicate all nodes first
            foreach ( var original in originalNodes ) {
                var newNode = targetGraph != null ? original.Duplicate(targetGraph) : JSONSerializer.Clone<Node>(original);
                newNodes.Add(newNode);
                //store the out connections that need dulpicate along with the indeces of source and target
                foreach ( var c in original.outConnections ) {
                    var sourceIndex = originalNodes.IndexOf(c.sourceNode);
                    var targetIndex = originalNodes.IndexOf(c.targetNode);
                    linkInfo[c] = new KeyValuePair<int, int>(sourceIndex, targetIndex);
                }
            }

            //duplicate all connections that we stored as 'need duplicating' providing new source and target
            foreach ( var linkPair in linkInfo ) {
                if ( linkPair.Value.Value != -1 ) { //we check this to see if the target node is part of the duplicated nodes since IndexOf returns -1 if element is not part of the list
                    var newSource = newNodes[linkPair.Value.Key];
                    var newTarget = newNodes[linkPair.Value.Value];
                    linkPair.Key.Duplicate(newSource, newTarget);
                }
            }

            //position nodes nicely
            if ( originPosition != default(Vector2) && newNodes.Count > 0 ) {
                if ( newNodes.Count == 1 ) {
                    newNodes[0].position = originPosition;
                } else {
                    var diff = newNodes[0].position - originPosition;
                    newNodes[0].position = originPosition;
                    for ( var i = 1; i < newNodes.Count; i++ ) {
                        newNodes[i].position -= diff;
                    }
                }
            }

            //revalidate all new nodes in their new graph
            if ( targetGraph != null ) {
                for ( var i = 0; i < newNodes.Count; i++ ) {
                    newNodes[i].Validate(targetGraph);
                }
            }

            return newNodes;
        }

        ///<summary>Clears the whole graph</summary>
        public void ClearGraph() {
            UndoUtility.RecordObject(this, "Clear");
            canvasGroups = null;
            foreach ( var node in allNodes.ToArray() ) {
                RemoveNode(node);
            }
            UndoUtility.SetDirty(this);
        }

        [System.Obsolete("Use 'Graph.StartGraph' with the 'Graph.UpdateMode' parameter.")]
        public void StartGraph(Component newAgent, IBlackboard newBlackboard, bool autoUpdate, System.Action<bool> callback = null) {
            StartGraph(newAgent, newBlackboard, autoUpdate ? UpdateMode.NormalUpdate : UpdateMode.Manual, callback);
        }

    }
}