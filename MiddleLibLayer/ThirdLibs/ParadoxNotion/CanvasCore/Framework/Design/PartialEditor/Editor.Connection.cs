#if UNITY_EDITOR

using NodeCanvas.Editor;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEditor;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;


namespace NodeCanvas.Framework
{

    partial class Connection
    {

        public enum RelinkState
        {
            None,
            Source,
            Target
        }

        public enum TipConnectionStyle
        {
            None,
            Circle,
            Arrow
        }

        ///----------------------------------------------------------------------------------------------

        [SerializeField, fsIgnoreInBuild]
        private bool _infoCollapsed;

        const float RELINK_DISTANCE_SNAP = 20f;
        const float STATUS_BLINK_DURATION = 0.25f;
        const float STATUS_BLINK_SIZE_ADD = 2f;
        const float STATUS_BLINK_PACKET_SPEED = 0.8f;
        const float STATUS_BLINK_PACKET_SIZE = 10f;
        const float STATUS_BLINK_PACKET_COUNT = 4f;

        public Rect startRect { get; private set; }
        public Rect endRect { get; private set; }

        private Rect centerRect;
        private Vector2 fromTangent;
        private Vector2 toTangent;

        private Status lastStatus = Status.Resting;
        private Color color = StyleSheet.GetStatusColor(Status.Resting);
        private float size = 3;
        private float statusChangeTime;

        private Vector2? relinkClickPos;
        private bool relinkSnaped;

        ///----------------------------------------------------------------------------------------------

        ///<summary>Editor. Is info expanded?</summary>
        private bool infoExpanded {
            get { return !_infoCollapsed; }
            set { _infoCollapsed = !value; }
        }

        ///<summary>Editor. Is currently actively relinking?</summary>
        private bool isRelinkingActive {
            get { return relinkClickPos != null && relinkSnaped; }
        }

        ///<summary>Editor. Current relinking state</summary>
        public RelinkState relinkState { get; private set; }

        ///<summary>Editor. Default Color of connection</summary>
        virtual public Color defaultColor {
            get { return StyleSheet.GetStatusColor(status); }
        }

        ///<summary>Editor. Will animate connection? By default if status running</summary>
        virtual public bool animate {
            get { return status == Status.Running; }
        }

        ///<summary>Editor. Defacult size of connection</summary>
        virtual public float defaultSize {
            get { return 3f; }
        }

        ///<summary>Editor. End Tip connection style</summary>
        virtual public TipConnectionStyle tipConnectionStyle {
            get { return TipConnectionStyle.Circle; }
        }

        ///<summary>Editor. Optional explicit "flow" direction</summary>
        virtual public ParadoxNotion.PlanarDirection direction {
            get { return ParadoxNotion.PlanarDirection.Auto; }
        }

        ///----------------------------------------------------------------------------------------------

        //Draw connection from-to
        public void DrawConnectionGUI(Vector2 fromPos, Vector2 toPos) {

            var _startRect = new Rect(0, 0, 12, 12);
            _startRect.center = fromPos;
            startRect = _startRect;

            var _endRect = new Rect(0, 0, 16, 16);
            _endRect.center = toPos;
            endRect = _endRect;

            ParadoxNotion.CurveUtils.ResolveTangents(fromPos, toPos, sourceNode.rect, targetNode.rect, Prefs.connectionsMLT, direction, out fromTangent, out toTangent);
            if ( sourceNode == targetNode ) {
                fromTangent = fromTangent.normalized * 120;
                toTangent = toTangent.normalized * 120;
            }

            centerRect.center = ParadoxNotion.CurveUtils.GetPosAlongCurve(fromPos, toPos, fromTangent, toTangent, 0.55f);

            HandleEvents(fromPos, toPos);
            DrawConnection(fromPos, toPos);

            if ( !isRelinkingActive ) {
                if ( Application.isPlaying && isActive ) {
                    UpdateBlinkStatus(fromPos, toPos);
                }
                if ( !DrawPossibleError(fromPos, toPos) ) {
                    DrawInfoRect(fromPos, toPos);
                }
            }
        }

        ///<summary>Handle UI events</summary>
        void HandleEvents(Vector2 fromPos, Vector2 toPos) {

            var e = Event.current;

            //On click select this connection
            if ( GraphEditorUtility.allowClick && e.type == EventType.MouseDown && e.button == 0 ) {
                float norm;
                var onConnection = ParadoxNotion.CurveUtils.IsPosAlongCurve(fromPos, toPos, fromTangent, toTangent, e.mousePosition, out norm);
                var onStart = startRect.Contains(e.mousePosition);
                var onEnd = endRect.Contains(e.mousePosition);
                var onCenter = centerRect.Contains(e.mousePosition);
                if ( onConnection || onStart || onEnd || onCenter ) {
                    GraphEditorUtility.activeElement = this;
                    relinkClickPos = e.mousePosition;
                    relinkSnaped = false;
                    if ( onConnection ) { relinkState = norm <= 0.1f || e.shift ? RelinkState.Source : RelinkState.Target; }
                    if ( onStart ) { relinkState = RelinkState.Source; }
                    if ( onEnd ) { relinkState = RelinkState.Target; }
                    if ( onCenter ) { relinkState = e.shift ? RelinkState.Source : RelinkState.Target; }
                    e.Use();
                }
            }

            if ( relinkClickPos != null ) {

                if ( relinkSnaped == false ) {
                    if ( Vector2.Distance(relinkClickPos.Value, e.mousePosition) > RELINK_DISTANCE_SNAP ) {
                        relinkSnaped = true;
                        sourceNode.OnActiveRelinkStart(this);
                    }
                }

                if ( e.rawType == EventType.MouseUp && e.button == 0 ) {
                    if ( relinkSnaped == true ) {
                        sourceNode.OnActiveRelinkEnd(this);
                    }
                    relinkClickPos = null;
                    relinkSnaped = false;
                    relinkState = RelinkState.None;
                    e.Use();
                }
            }

            if ( GraphEditorUtility.allowClick && e.type == EventType.MouseUp && e.button == 1 && centerRect.Contains(e.mousePosition) ) {
                GraphEditorUtility.PostGUI += () => { GetConnectionMenu().ShowAsContext(); };
                e.Use();
            }
        }

        //The actual connection graphic
        void DrawConnection(Vector2 fromPos, Vector2 toPos) {

            color = isActive ? color : Colors.Grey(0.3f);
            if ( !Application.isPlaying ) {
                color = isActive ? defaultColor : Colors.Grey(0.3f);
                var highlight = GraphEditorUtility.activeElement == this || GraphEditorUtility.activeElement == sourceNode || GraphEditorUtility.activeElement == targetNode;
                if ( startRect.Contains(Event.current.mousePosition) || endRect.Contains(Event.current.mousePosition) ) {
                    highlight = true;
                }
                color.a = highlight ? 1 : color.a;
                size = highlight ? defaultSize + 2 : defaultSize;
            }

            //alter from/to if active relinking
            if ( isRelinkingActive ) {
                if ( relinkState == RelinkState.Source ) {
                    fromPos = Event.current.mousePosition;
                }
                if ( relinkState == RelinkState.Target ) {
                    toPos = Event.current.mousePosition;
                }
                ParadoxNotion.CurveUtils.ResolveTangents(fromPos, toPos, Prefs.connectionsMLT, direction, out fromTangent, out toTangent);
                size = defaultSize;
            }

            var shadow = new Vector2(3.5f, 3.5f);
            Handles.DrawBezier(fromPos, toPos + shadow, fromPos + shadow + fromTangent + shadow, toPos + shadow + toTangent, Color.black.WithAlpha(0.1f), StyleSheet.bezierTexture, size + 10f);
            Handles.DrawBezier(fromPos, toPos, fromPos + fromTangent, toPos + toTangent, color, StyleSheet.bezierTexture, size);

            GUI.color = color.WithAlpha(1);
            if ( tipConnectionStyle == TipConnectionStyle.Arrow ) {
                GUI.DrawTexture(endRect, StyleSheet.GetDirectionArrow(toTangent.normalized));
            }
            if ( tipConnectionStyle == TipConnectionStyle.Circle ) {
                GUI.DrawTexture(endRect, StyleSheet.circle);
            }
            GUI.color = Color.white;
        }

        //Information showing in the middle
        void DrawInfoRect(Vector2 fromPos, Vector2 toPos) {
            var isExpanded = infoExpanded || GraphEditorUtility.activeElement == this || GraphEditorUtility.activeElement == sourceNode;
            var alpha = isExpanded ? 0.8f : 0.25f;
            var info = GetConnectionInfo();
            var extraInfo = sourceNode.GetConnectionInfo(sourceNode.outConnections.IndexOf(this));
            if ( !string.IsNullOrEmpty(info) || !string.IsNullOrEmpty(extraInfo) ) {

                if ( !string.IsNullOrEmpty(extraInfo) && !string.IsNullOrEmpty(info) ) {
                    extraInfo = "\n" + extraInfo;
                }

                var textToShow = isExpanded ? string.Format("<size=9>{0}{1}</size>", info, extraInfo) : "<size=9>...</size>";
                var finalSize = StyleSheet.box.CalcSize(EditorUtils.GetTempContent(textToShow));

                centerRect.width = finalSize.x;
                centerRect.height = finalSize.y;

                EditorGUIUtility.AddCursorRect(centerRect, MouseCursor.Link);

                GUI.color = Colors.Grey(EditorGUIUtility.isProSkin ? 0.17f : 0.5f).WithAlpha(0.95f);
                GUI.DrawTexture(centerRect, Texture2D.whiteTexture);

                GUI.color = Color.white.WithAlpha(alpha);
                GUI.Label(centerRect, textToShow, Styles.centerLabel);
                GUI.color = Color.white;

            } else {

                centerRect.width = 0;
                centerRect.height = 0;
            }
        }

        ///<summary>Draw icon if there are errors</summary>
        bool DrawPossibleError(Vector2 fromPos, Vector2 toPos) {
            var error = GetError();
            if ( error != null ) {
                var r = new Rect(0, 0, 32, 32);
                r.center = this.centerRect.center;
                GUI.DrawTexture(r.ExpandBy(-10, -6), Texture2D.whiteTexture);
                GUI.DrawTexture(r, Icons.errorIconBig);
                return true;
            }
            return false;
        }

        ///<summary>Updates the blink status</summary>
        void UpdateBlinkStatus(Vector2 fromPos, Vector2 toPos) {

            OnBeforeUpdateBlinkStatus();

            if ( !graph.isRunning ) {
                size = defaultSize;
                color = defaultColor;
                return;
            }

            if ( status != lastStatus ) {
                lastStatus = status;
                statusChangeTime = graph.elapsedTime;
            }

            var deltaTimeSinceChange = ( graph.elapsedTime - statusChangeTime );
            if ( status != Status.Resting || size != defaultSize ) {
                size = Mathf.Lerp(defaultSize + STATUS_BLINK_SIZE_ADD, defaultSize, deltaTimeSinceChange / STATUS_BLINK_DURATION);
            }

            if ( status != Status.Resting || size == defaultSize ) {
                color = defaultColor;
            }

            if ( animate ) {
                var packetTraversal = deltaTimeSinceChange * STATUS_BLINK_PACKET_SPEED;
                for ( var i = 0f; i < STATUS_BLINK_PACKET_COUNT; i++ ) {
                    var progression = packetTraversal + ( i / STATUS_BLINK_PACKET_COUNT );
                    var normPos = Mathf.Repeat(progression, 1f);

                    var packetColor = this.color;
                    var pingPong = Mathf.PingPong(normPos, 0.5f);
                    var norm = ( pingPong * 2 ) / 0.5f;
                    var pSize = Mathf.Lerp(0.5f, 1f, norm) * STATUS_BLINK_PACKET_SIZE;
                    packetColor.a = norm * ( deltaTimeSinceChange / ( STATUS_BLINK_DURATION + 0.25f ) );

                    var rect = new Rect(0, 0, pSize, pSize);
                    rect.center = ParadoxNotion.CurveUtils.GetPosAlongCurve(fromPos, toPos, fromTangent, toTangent, normPos); ;
                    GUI.color = packetColor;
                    GUI.DrawTexture(rect, StyleSheet.circle);
                    GUI.color = Color.white;
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        //The connection's inspector
        public static void ShowConnectionInspectorGUI(Connection c) {

            UndoUtility.CheckUndo(c.graph, "Connection Inspector");

            GUILayout.BeginHorizontal();
            GUI.color = new Color(1, 1, 1, 0.5f);

            c.isActive = EditorGUILayout.ToggleLeft("Active", c.isActive, GUILayout.Width(150));

            GUILayout.FlexibleSpace();

            if ( GUILayout.Button("X", GUILayout.Width(20)) ) {
                GraphEditorUtility.PostGUI += () => { c.graph.RemoveConnection(c); };
            }

            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            EditorUtils.BoldSeparator();
            c.OnConnectionInspectorGUI();
            c.sourceNode.OnConnectionInspectorGUI(c.sourceNode.outConnections.IndexOf(c));

            UndoUtility.CheckDirty(c.graph);
        }

        ///<summary>Editor. The information to show in the middle area of the connection</summary>
        virtual protected string GetConnectionInfo() { return null; }
        ///<summary>Editor.Override to show controls in the editor panel when connection is selected</summary>
        virtual protected void OnConnectionInspectorGUI() { }
        ///<summary>Editor. Callback before connections blink status</summary>
        virtual protected void OnBeforeUpdateBlinkStatus() { }
        ///<summary>Editor. Get possible error, null if none</summary>
        virtual protected string GetError() { return null; }

        ///<summary>Returns the mid position rect of the connection</summary>
        public Rect GetMidRect() {
            return centerRect;
        }

        ///<summary>the connection context menu</summary>
        GenericMenu GetConnectionMenu() {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(infoExpanded ? "Collapse Info" : "Expand Info"), false, () => { infoExpanded = !infoExpanded; });
            menu.AddItem(new GUIContent(isActive ? "Disable" : "Enable"), false, () => { isActive = !isActive; });

            var assignable = this as ITaskAssignable;
            if ( assignable != null ) {

                if ( assignable.task != null ) {
                    menu.AddItem(new GUIContent("Copy Assigned Condition"), false, () => { CopyBuffer.Set<Task>(assignable.task); });
                } else { menu.AddDisabledItem(new GUIContent("Copy Assigned Condition")); }

                if ( CopyBuffer.TryGet<Task>(out Task copy) ) {
                    menu.AddItem(new GUIContent(string.Format("Paste Assigned Condition ({0})", copy.name)), false, () =>
                    {
                        if ( assignable.task != null ) {
                            if ( !EditorUtility.DisplayDialog("Paste Condition", string.Format("Connection already has a Condition assigned '{0}'. Replace assigned condition with pasted condition '{1}'?", assignable.task.name, copy.name), "YES", "NO") ) {
                                return;
                            }
                        }

                        try { assignable.task = copy.Duplicate(graph); }
                        catch { Logger.LogWarning("Can't paste Condition here. Incombatible Types.", LogTag.EDITOR, this); }
                    });

                } else { menu.AddDisabledItem(new GUIContent("Paste Assigned Condition")); }

            }

            menu.AddSeparator("/");
            menu.AddItem(new GUIContent("Delete"), false, () => { graph.RemoveConnection(this); });
            return menu;
        }

    }
}

#endif