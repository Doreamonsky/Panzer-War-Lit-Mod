using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("GameObject")]
    [Description("Action will end in Failure if no objects are found")]
    public class FindAllWithLayer : ActionTask
    {

        [RequiredField]
        public BBParameter<LayerMask> targetLayers;
        [BlackboardOnly]
        public BBParameter<List<GameObject>> saveAs;

        protected override string info {
            get { return "GetObjects in '" + targetLayers + "' as " + saveAs; }
        }

        protected override void OnExecute() {
            saveAs.value = ParadoxNotion.ObjectUtils.FindGameObjectsWithinLayerMask(targetLayers.value).ToList();
            EndAction(saveAs.value.Count != 0);
        }
    }
}