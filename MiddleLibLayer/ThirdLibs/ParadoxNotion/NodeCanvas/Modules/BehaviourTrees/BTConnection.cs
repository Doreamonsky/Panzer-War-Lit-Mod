using NodeCanvas.Framework;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    ///<summary>The connection object for BehaviourTree nodes</summary>
    public class BTConnection : Connection
    {


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override ParadoxNotion.PlanarDirection direction {
            get { return ParadoxNotion.PlanarDirection.Vertical; }
        }

#endif
        ///----------------------------------------------------------------------------------------------


    }
}