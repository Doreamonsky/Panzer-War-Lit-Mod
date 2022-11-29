using System;
using System.Linq;
using UnityEngine;

namespace NodeCanvas.Framework
{
    ///<summary>Interface for NodeReference</summary>
    public interface INodeReference
    {
        System.Type type { get; }
        Node Get(Graph graph);
        void Set(Node target);
    }

    ///<summary>A utility to have nodes weak reference other nodes</summary>
    [System.Serializable, ParadoxNotion.Serialization.FullSerializer.fsForward(nameof(_targetNodeUID))]
    [ParadoxNotion.Serialization.FullSerializer.fsAutoInstance]
    public class NodeReference<T> : INodeReference where T : Node
    {
        [SerializeField] private string _targetNodeUID;

        [NonSerialized] private WeakReference<T> _targetNodeRef;

        System.Type INodeReference.type => typeof(T);
        Node INodeReference.Get(Graph graph) { return Get(graph); }
        void INodeReference.Set(Node target) { Set(target as T); }

        public NodeReference() { }
        public NodeReference(T target) { Set(target); }

        ///<summary>Get referenced node given the graph it lives within</summary>
        public T Get(Graph graph) {
            T reference;
            if ( _targetNodeRef == null ) {
                var _this = this;
                reference = graph.GetAllNodesOfType<T>().FirstOrDefault(x => x.UID == _this._targetNodeUID);
                _targetNodeRef = new WeakReference<T>(reference);
            }
            _targetNodeRef.TryGetTarget(out reference);
            return reference;
        }

        ///<summary>Set referenced node</summary>
        public void Set(T target) {
            if ( _targetNodeRef == null ) { _targetNodeRef = new WeakReference<T>(target); }
            _targetNodeRef.SetTarget(target);
            _targetNodeUID = target != null ? target.UID : null;
        }
    }
}