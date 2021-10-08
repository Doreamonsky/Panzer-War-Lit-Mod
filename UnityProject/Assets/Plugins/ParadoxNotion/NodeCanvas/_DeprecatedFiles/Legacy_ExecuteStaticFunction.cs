using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [System.Obsolete("Execute Function now supports static functions as well")]
    public class ExecuteStaticFunction : ActionTask, ISubParametersContainer
    {

        [SerializeField]
        protected ReflectedWrapper functionWrapper;

        BBParameter[] ISubParametersContainer.GetSubParameters() {
            return functionWrapper != null ? functionWrapper.GetVariables() : null;
        }

        private MethodInfo targetMethod {
            get { return functionWrapper != null ? functionWrapper.GetMethod() : null; }
        }

        protected override string info {
            get
            {
                if ( functionWrapper == null ) { return "No Method Selected"; }
                if ( targetMethod == null ) { return functionWrapper.AsString().FormatError(); }

                var variables = functionWrapper.GetVariables();
                var returnInfo = "";
                var paramInfo = "";
                if ( targetMethod.ReturnType == typeof(void) ) {
                    for ( var i = 0; i < variables.Length; i++ )
                        paramInfo += ( i != 0 ? ", " : "" ) + variables[i].ToString();
                } else {
                    returnInfo = variables[0].isNone ? "" : variables[0] + " = ";
                    for ( var i = 1; i < variables.Length; i++ )
                        paramInfo += ( i != 1 ? ", " : "" ) + variables[i].ToString();
                }

                return string.Format("{0}{1}.{2} ({3})", returnInfo, targetMethod.DeclaringType.FriendlyName(), targetMethod.Name, paramInfo);
            }
        }

        public override void OnValidate(ITaskSystem ownerSystem) {
            if ( functionWrapper != null && functionWrapper.HasChanged() ) {
                SetMethod(functionWrapper.GetMethod());
            }
        }

        //store the method info on init
        protected override string OnInit() {
            if ( targetMethod == null ) { return "Missing Method"; }

            try {
                functionWrapper.Init(null);
                return null;
            }
            catch { return "ExecuteFunction Error"; }
        }

        //do it by calling delegate or invoking method
        protected override void OnExecute() {

            if ( targetMethod == null ) {
                EndAction(false);
                return;
            }

            if ( functionWrapper is ReflectedActionWrapper ) {
                ( functionWrapper as ReflectedActionWrapper ).Call();
            } else {
                ( functionWrapper as ReflectedFunctionWrapper ).Call();
            }

            EndAction();
        }

        void SetMethod(MethodInfo method) {
            if ( method != null ) {
                functionWrapper = ReflectedWrapper.Create(method, blackboard);
            }
        }
    }
}