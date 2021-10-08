using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using NodeCanvas.Framework.Internal;

namespace NodeCanvas.DialogueTrees
{
    [Category("SubGraphs")]
    [Color("ffe4e1")]
    abstract public class DTNodeNested<T> : DTNode, IGraphAssignable<T> where T : Graph
    {
        [SerializeField] private List<BBMappingParameter> _variablesMap;

        abstract public T subGraph { get; set; }
        abstract public BBParameter subGraphParameter { get; }

        public T currentInstance { get; set; }
        public Dictionary<Graph, Graph> instances { get; set; }
        public List<BBMappingParameter> variablesMap { get { return _variablesMap; } set { _variablesMap = value; } }

        Graph IGraphAssignable.subGraph { get { return subGraph; } set { subGraph = (T)value; } }
        Graph IGraphAssignable.currentInstance { get { return currentInstance; } set { currentInstance = (T)value; } }
    }
}