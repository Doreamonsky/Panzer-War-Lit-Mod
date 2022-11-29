using System.Collections.Generic;

namespace NodeCanvas.Framework.Internal
{
    ///<summary>Contains data that a graph can load/deserialize from AND initialize. Can be passed to Graph.LoadOverwrite or Graph.LoadOverwriteAsync</summary>
    public struct GraphLoadData
    {
        public GraphSource source;
        public string json;
        public List<UnityEngine.Object> references;
        public UnityEngine.Component agent;
        public IBlackboard parentBlackboard;
        public bool preInitializeSubGraphs;
    }
}