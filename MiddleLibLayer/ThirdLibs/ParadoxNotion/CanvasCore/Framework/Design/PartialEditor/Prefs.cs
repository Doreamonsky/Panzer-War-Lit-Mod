#if UNITY_EDITOR

using UnityEditor;
using ParadoxNotion.Serialization;

namespace NodeCanvas.Editor
{

    ///<summary>NC framework preferences</summary>
    public static partial class Prefs
    {

        private const string PREFS_KEY_NAME = "NodeCanvas.EditorPreferences";

        [System.Serializable]
        partial class SerializedData
        {
            public bool isEditorLocked = false;

            public bool useExternalInspector = false;
            public bool showBlackboard = true;
            public bool showNodePanel = true;
            public float inspectorPanelWidth = 330;
            public float blackboardPanelWidth = 350;
            public bool showWelcomeWindow2 = true;

            public bool showNodeInfo = true;
            public bool showIcons = true;
            public bool showTaskSummary = true;
            public bool showComments = true;
            public bool showNodeIDs = false;
            public bool showNodeElapsedTimes = false;
            public bool showHierarchyIcons = true;
            public bool showGrid = true;
            public bool snapToGrid = false;
            public bool logEventsInfo = true;
            public bool logVariablesInfo = true;
            public bool animatePanels = true;
            public float connectionsMLT = 0.8f;

            public bool hierarchicalMove = false;
            public bool breakpointPauseEditor = true;
            public bool collapseGenericTypes = false;

            public bool consoleLogInfo = true;
            public bool consoleLogWarning = true;
            public bool consoleLogError = true;
            public bool consoleClearOnPlay = true;
            public ConsoleLogOrder consoleLogOrder = ConsoleLogOrder.Ascending;
            public bool explorerShowTypeNames = true;
            public UnityEngine.Vector2 minimapSize = new UnityEngine.Vector2(170, 100);
        }

        private static SerializedData _data;
        private static SerializedData data {
            get
            {
                if ( _data == null ) { _data = new SerializedData(); }
                return _data;
            }
        }

        [InitializeOnLoadMethod]
        static void LoadData() {
            var pref = EditorPrefs.GetString(PREFS_KEY_NAME);
            if ( !string.IsNullOrEmpty(pref) ) { _data = JSONSerializer.Deserialize<SerializedData>(pref); }
            if ( _data == null ) { _data = new SerializedData(); }
        }

        ///----------------------------------------------------------------------------------------------

        public readonly static UnityEngine.Vector2 MINIMAP_MIN_SIZE = new UnityEngine.Vector2(50, 30);
        public readonly static UnityEngine.Vector2 MINIMAP_MAX_SIZE = new UnityEngine.Vector2(500, 300);

        ///----------------------------------------------------------------------------------------------

        public enum ConsoleLogOrder
        {
            Ascending,
            Descending
        }

        public static bool isEditorLocked {
            get { return data.isEditorLocked; }
            set { if ( data.isEditorLocked != value ) { data.isEditorLocked = value; Save(); } }
        }

        public static bool showBlackboard {
            get { return data.showBlackboard; }
            set { if ( data.showBlackboard != value ) { data.showBlackboard = value; Save(); } }
        }

        public static bool showNodePanel {
            get { return data.showNodePanel; }
            set { if ( data.showNodePanel != value ) { data.showNodePanel = value; Save(); } }
        }

        public static bool showNodeInfo {
            get { return data.showNodeInfo; }
            set { if ( data.showNodeInfo != value ) { data.showNodeInfo = value; Save(); } }
        }

        public static bool showIcons {
            get { return data.showIcons; }
            set { if ( data.showIcons != value ) { data.showIcons = value; Save(); } }
        }

        public static bool showTaskSummary {
            get { return data.showTaskSummary; }
            set { if ( data.showTaskSummary != value ) { data.showTaskSummary = value; Save(); } }
        }

        public static bool showComments {
            get { return data.showComments; }
            set { if ( data.showComments != value ) { data.showComments = value; Save(); } }
        }

        public static bool showNodeIDs {
            get { return data.showNodeIDs; }
            set { if ( data.showNodeIDs != value ) { data.showNodeIDs = value; Save(); } }
        }

        public static bool showNodeElapsedTimes {
            get { return data.showNodeElapsedTimes; }
            set { if ( data.showNodeElapsedTimes != value ) { data.showNodeElapsedTimes = value; Save(); } }
        }

        public static bool showGrid {
            get { return data.showGrid; }
            set { if ( data.showGrid != value ) { data.showGrid = value; Save(); } }
        }

        public static bool snapToGrid {
            get { return data.snapToGrid; }
            set { if ( data.snapToGrid != value ) { data.snapToGrid = value; Save(); } }
        }

        public static bool hierarchicalMove {
            get { return data.hierarchicalMove; }
            set { if ( data.hierarchicalMove != value ) { data.hierarchicalMove = value; Save(); } }
        }

        public static bool useExternalInspector {
            get { return data.useExternalInspector; }
            set { if ( data.useExternalInspector != value ) { data.useExternalInspector = value; Save(); } }
        }

        public static bool showWelcomeWindow {
            get { return data.showWelcomeWindow2; }
            set { if ( data.showWelcomeWindow2 != value ) { data.showWelcomeWindow2 = value; Save(); } }
        }

        public static bool logEventsInfo {
            get { return data.logEventsInfo; }
            set { if ( data.logEventsInfo != value ) { data.logEventsInfo = value; Save(); } }
        }

        public static bool logVariablesInfo {
            get { return data.logVariablesInfo; }
            set { if ( data.logVariablesInfo != value ) { data.logVariablesInfo = value; Save(); } }
        }

        public static bool showHierarchyIcons {
            get { return data.showHierarchyIcons; }
            set { if ( data.showHierarchyIcons != value ) { data.showHierarchyIcons = value; Save(); } }
        }

        public static bool breakpointPauseEditor {
            get { return data.breakpointPauseEditor; }
            set { if ( data.breakpointPauseEditor != value ) { data.breakpointPauseEditor = value; Save(); } }
        }

        public static bool collapseGenericTypes {
            get { return data.collapseGenericTypes; }
            set { if ( data.collapseGenericTypes != value ) { data.collapseGenericTypes = value; Save(); } }
        }

        ///----------------------------------------------------------------------------------------------

        public static float inspectorPanelWidth {
            get { return data.inspectorPanelWidth; }
            set { if ( data.inspectorPanelWidth != value ) { data.inspectorPanelWidth = UnityEngine.Mathf.Clamp(value, 300, 600); Save(); } }
        }

        public static float blackboardPanelWidth {
            get { return data.blackboardPanelWidth; }
            set { if ( data.blackboardPanelWidth != value ) { data.blackboardPanelWidth = UnityEngine.Mathf.Clamp(value, 300, 600); Save(); } }
        }

        public static bool animatePanels {
            get { return data.animatePanels; }
            set { if ( data.animatePanels != value ) { data.animatePanels = value; Save(); } }
        }

        public static float connectionsMLT {
            get { return data.connectionsMLT; }
            set { if ( data.connectionsMLT != value ) { data.connectionsMLT = value; Save(); } }
        }

        ///----------------------------------------------------------------------------------------------

        public static bool consoleLogInfo {
            get { return data.consoleLogInfo; }
            set { if ( data.consoleLogInfo != value ) { data.consoleLogInfo = value; Save(); } }
        }

        public static bool consoleLogWarning {
            get { return data.consoleLogWarning; }
            set { if ( data.consoleLogWarning != value ) { data.consoleLogWarning = value; Save(); } }
        }

        public static bool consoleLogError {
            get { return data.consoleLogError; }
            set { if ( data.consoleLogError != value ) { data.consoleLogError = value; Save(); } }
        }

        public static bool consoleClearOnPlay {
            get { return data.consoleClearOnPlay; }
            set { if ( data.consoleClearOnPlay != value ) { data.consoleClearOnPlay = value; Save(); } }
        }

        public static ConsoleLogOrder consoleLogOrder {
            get { return data.consoleLogOrder; }
            set { if ( data.consoleLogOrder != value ) { data.consoleLogOrder = value; Save(); } }
        }

        ///----------------------------------------------------------------------------------------------

        public static bool explorerShowTypeNames {
            get { return data.explorerShowTypeNames; }
            set { if ( data.explorerShowTypeNames != value ) { data.explorerShowTypeNames = value; Save(); } }
        }

        ///----------------------------------------------------------------------------------------------

        public static UnityEngine.Vector2 minimapSize {
            get
            {
                var result = data.minimapSize;
                result = UnityEngine.Vector2.Max(result, MINIMAP_MIN_SIZE);
                result = UnityEngine.Vector2.Min(result, MINIMAP_MAX_SIZE);
                return result;
            }
            set
            {
                if ( data.minimapSize != value ) {
                    data.minimapSize = UnityEngine.Vector2.Max(value, MINIMAP_MIN_SIZE);
                    data.minimapSize = UnityEngine.Vector2.Min(value, MINIMAP_MAX_SIZE);
                    Save();
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        //Save the prefs
        static void Save() {
            EditorPrefs.SetString(PREFS_KEY_NAME, JSONSerializer.Serialize(typeof(SerializedData), data));
        }
    }
}

#endif