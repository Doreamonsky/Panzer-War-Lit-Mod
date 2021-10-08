using System.Collections.Generic;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;

namespace NodeCanvas.Framework.Internal
{
    ///Graph data and model used for serialization
    [System.Serializable, fsDeserializeOverwrite]
    public class GraphSource : ISerializationCollector
    {

        ///----------------------------------------------------------------------------------------------
        ///We are already parsing everything on serialization/deserialization, so we might just as well collect things at the same time.
        public List<Task> allTasks { get; private set; }
        public List<BBParameter> allParameters { get; private set; }

        void ISerializationCollector.OnPush(ISerializationCollector parent) {
            allTasks = new List<Task>();
            allParameters = new List<BBParameter>();
        }

        void ISerializationCollector.OnCollect(ISerializationCollectable child, int depth) {
            if ( child is Task ) { allTasks.Add((Task)child); }
            if ( child is BBParameter ) { allParameters.Add((BBParameter)child); }
        }

        void ISerializationCollector.OnPop(ISerializationCollector parent) { }
        ///----------------------------------------------------------------------------------------------


        public const float FRAMEWORK_VERSION = 3.20f;

        [SerializeField, fsSerializeAs("version"), fsWriteOnly, fsIgnoreInBuild]
        private float _version;
        [SerializeField, fsSerializeAs("category"), fsWriteOnly, fsIgnoreInBuild]
        private string _category;
        [SerializeField, fsSerializeAs("comments"), fsWriteOnly, fsIgnoreInBuild]
        private string _comments;
        [SerializeField, fsSerializeAs("translation"), fsWriteOnly, fsIgnoreInBuild]
        private Vector2 _translation;
        [SerializeField, fsSerializeAs("zoomFactor"), fsWriteOnly, fsIgnoreInBuild]
        private float _zoomFactor;

        [fsSerializeAs("type")]
        private string _type;
        [fsSerializeAs("nodes")]
        private List<Node> _nodes;
        [fsSerializeAs("connections")]
        private List<Connection> _connections;
        [fsSerializeAs("canvasGroups"), fsIgnoreInBuild]
        private List<CanvasGroup> _canvasGroups;
        [fsSerializeAs("localBlackboard")]
        private BlackboardSource _localBlackboard;
        [fsSerializeAs("derivedData")]
        private object _derivedData;

        ///----------------------------------------------------------------------------------------------

        public float version { get { return _version; } set { _version = value; } }
        public string category { get { return _category; } set { _category = value; } }
        public string comments { get { return _comments; } set { _comments = value; } }
        public Vector2 translation { get { return _translation; } set { _translation = value; } }
        public float zoomFactor { get { return _zoomFactor; } set { _zoomFactor = value; } }

        public string type { get { return _type; } set { _type = value; } }
        public List<Node> nodes { get { return _nodes; } set { _nodes = value; } }
        public List<Connection> connections { get { return _connections; } private set { _connections = value; } }
        public List<CanvasGroup> canvasGroups { get { return _canvasGroups; } set { _canvasGroups = value; } }
        public BlackboardSource localBlackboard { get { return _localBlackboard; } set { _localBlackboard = value; } }
        public object derivedData { get { return _derivedData; } set { _derivedData = value; } }

        ///----------------------------------------------------------------------------------------------

        //...
        public GraphSource() {
            zoomFactor = 1f;
            nodes = new List<Node>();
            canvasGroups = new List<CanvasGroup>();
            localBlackboard = new BlackboardSource();
        }

        ///Must Pack *BEFORE* any serialization
        public GraphSource Pack(Graph graph) {

            this.version = FRAMEWORK_VERSION;
            this.type = graph.GetType().FullName;

            //connections are serialized seperately and not part of their parent node
            var structConnections = new List<Connection>();
            for ( var i = 0; i < nodes.Count; i++ ) {
                for ( var j = 0; j < nodes[i].outConnections.Count; j++ ) {
                    structConnections.Add(nodes[i].outConnections[j]);
                }
            }
            this.connections = structConnections;

            //serialize derived data
            this.derivedData = graph.OnDerivedDataSerialization();
            return this;
        }

        ///Must Unpack *AFTER* any deserialization
        public GraphSource Unpack(Graph graph) {

            //check serialization versions here in the future if needed

            //set local bb context (not really important)
            localBlackboard.unityContextObject = graph;

            //re-set the node's owner and ID
            for ( var i = 0; i < this.nodes.Count; i++ ) {
                nodes[i].outConnections.Clear();
                nodes[i].inConnections.Clear();
                nodes[i].graph = graph;
                nodes[i].ID = i;
            }

            //re-link connections for deserialization
            for ( var i = 0; i < this.connections.Count; i++ ) {
                connections[i].sourceNode.outConnections.Add(connections[i]);
                connections[i].targetNode.inConnections.Add(connections[i]);
            }

            //deserialize derived data
            graph.OnDerivedDataDeserialization(derivedData);
            return this;
        }

        ///Sets only the meta-data from another GraphSource and returns itself
        public GraphSource SetMetaData(GraphSource source) {
            version = source.version;
            category = source.category;
            comments = source.comments;
            translation = source.translation;
            zoomFactor = source.zoomFactor;
            return this;
        }

        //...
        public void PurgeRedundantReferences() {
            connections.Clear();
        }
    }
}