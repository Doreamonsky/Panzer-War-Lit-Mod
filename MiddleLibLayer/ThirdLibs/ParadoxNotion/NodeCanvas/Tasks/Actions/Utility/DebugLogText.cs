using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Debug Log")]
    [Category("✫ Utility")]
    [Description("Display a UI label on the agent's position if seconds to run is not 0 and also logs the message, which can also be mapped to any variable.")]
    public class DebugLogText : ActionTask<Transform>
    {

        public enum LogMode
        {
            Log,
            Warning,
            Error
        }

        public enum VerboseMode
        {
            LogAndDisplayLabel,
            LogOnly,
            DisplayLabelOnly,
        }

        [RequiredField]
        public BBParameter<string> log = "Hello World";
        public float labelYOffset = 0;
        public float secondsToRun = 1f;
        public VerboseMode verboseMode;
        public LogMode logMode;
        public CompactStatus finishStatus = CompactStatus.Success;

        protected override string info {
            get { return "Log " + log.ToString() + ( secondsToRun > 0 ? " for " + secondsToRun + " sec." : "" ); }
        }

        protected override void OnExecute() {
            if ( verboseMode == VerboseMode.LogAndDisplayLabel || verboseMode == VerboseMode.LogOnly ) {
                var label = string.Format("(<b>{0}</b>) {1}", agent.gameObject.name, log.value);
                if ( logMode == LogMode.Log ) {
                    ParadoxNotion.Services.Logger.Log(label, LogTag.EXECUTION, this);
                }
                if ( logMode == LogMode.Warning ) {
                    ParadoxNotion.Services.Logger.LogWarning(label, LogTag.EXECUTION, this);
                }
                if ( logMode == LogMode.Error ) {
                    ParadoxNotion.Services.Logger.LogError(label, LogTag.EXECUTION, this);
                }
            }
            if ( verboseMode == VerboseMode.LogAndDisplayLabel || verboseMode == VerboseMode.DisplayLabelOnly ) {
                if ( secondsToRun > 0 ) {
                    MonoManager.current.onGUI += OnGUI;
                }
            }
        }

        protected override void OnStop() {
            if ( verboseMode == VerboseMode.LogAndDisplayLabel || verboseMode == VerboseMode.DisplayLabelOnly ) {
                if ( secondsToRun > 0 ) {
                    MonoManager.current.onGUI -= OnGUI;
                }
            }
        }

        protected override void OnUpdate() {
            if ( elapsedTime >= secondsToRun ) {
                EndAction(finishStatus == CompactStatus.Success ? true : false);
            }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------

        void OnGUI() {
            if ( Camera.main == null ) { return; }
            var point = Camera.main.WorldToScreenPoint(agent.position + new Vector3(0, labelYOffset, 0));
            var size = GUI.skin.label.CalcSize(new GUIContent(log.value));
            var r = new Rect(point.x - size.x / 2, Screen.height - point.y, size.x + 10, size.y);
            GUI.color = Color.white.WithAlpha(0.5f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.2f, 0.2f);
            r.x += 4;
            GUI.Label(r, log.value);
            GUI.color = Color.white;
        }
    }
}