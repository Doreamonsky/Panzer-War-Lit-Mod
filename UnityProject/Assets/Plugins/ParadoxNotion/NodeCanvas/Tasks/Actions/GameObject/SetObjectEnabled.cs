using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Set Enabled")]
    [Category("GameObject")]
    [Description("Set the monobehaviour's enabled state.")]
    public class SetObjectEnabled : ActionTask<MonoBehaviour>
    {
        public enum SetEnableMode
        {
            Disable = 0,
            Enable = 1,
            Toggle = 2
        }

        public SetEnableMode setTo = SetEnableMode.Toggle;

        protected override string info {
            get { return string.Format("{0} {1}", setTo, agentInfo); }
        }

        protected override void OnExecute() {

            bool value;

            if ( setTo == SetEnableMode.Toggle ) {

                value = !agent.enabled;

            } else {

                value = (int)setTo == 1;
            }

            agent.enabled = value;
            EndAction();
        }
    }
}