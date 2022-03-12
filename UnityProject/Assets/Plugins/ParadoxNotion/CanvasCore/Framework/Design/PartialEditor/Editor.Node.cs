#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Editor;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Framework
{

    partial class Node
    {

        //Class for the nodeports GUI
        class GUIPort
        {
            public int portIndex { get; private set; }
            public Node parent { get; private set; }
            public Vector2 pos { get; private set; }
            public GUIPort(int index, Node parent, Vector2 pos) {
                this.portIndex = index;
                this.parent = parent;
                this.pos = pos;
            }
        }

        //Verbose level
        public enum VerboseLevel
        {
            Compact = 0,
            Partial = 1,
            Full = 2,
        }

        ///----------------------------------------------------------------------------------------------

        [SerializeField, fsIgnoreInBuild]
        private bool _collapsed;
        [SerializeField, fsIgnoreInBuild]
        private Color _color;
        [SerializeField, fsIgnoreInBuild]
        private VerboseLevel _verboseLevel = VerboseLevel.Full;

        ///----------------------------------------------------------------------------------------------

        readonly private static Vector2 MIN_SIZE = new Vector2(105, 20);
        private const string DEFAULT_HEX_COLOR_LIGHT = "eed9a7";
        private const string DEFAULT_HEX_COLOR_DARK = "333333";
        private static GUIPort clickedPort { get; set; }
        private static int dragDropMisses { get; set; }
        private static List<CanvasGroup> adjustingBoundCanvasGroups { get; set; }

        private Vector2 size = MIN_SIZE;
        private object _icon { get; set; }
        private GUIContent _cachedHeaderContent { get; set; }
        private bool colorLoaded { get; set; }
        private bool hasColorAttribute { get; set; }
        private string hexColor { get; set; }
        private Color colorAttributeColor { get; set; }
        private bool nodeIsPressed { get; set; }
        private bool? _isHidden { get; set; }

        ///<summary>EDITOR! This is to be able to work with rects which is easier in many cases. Size is temporary to the node since it's auto adjusted thus no need to serialize it</summary>
        public Rect rect {
            get { return new Rect(_position.x, _position.y, size.x, size.y); }
            private set
            {
                _position = new Vector2(value.x, value.y);
                size = new Vector2(Mathf.Max(value.width, MIN_SIZE.x), Mathf.Max(value.height, MIN_SIZE.y));
            }
        }

        ///<summary>EDITOR! Active is relevant to the input connections</summary>
        public bool isActive {
            get
            {
                for ( var i = 0; i < inConnections.Count; i++ ) {
                    if ( inConnections[i].isActive ) {
                        return true;
                    }
                }
                return inConnections.Count == 0;
            }
        }

        ///<summary>EDITOR! Are children collapsed?</summary>
        public bool collapsed {
            get { return _collapsed; }
            set
            {
                if ( _collapsed != value ) {
                    _collapsed = value;
                    //just reset all node isHidden cache is easier than other solutions
                    for ( var i = 0; i < graph.allNodes.Count; i++ ) {
                        graph.allNodes[i]._isHidden = null;
                    }
                }
            }
        }

        ///<summary>EDITOR! Is the node hidden due to parent has children collapsed or is hidden?</summary>
        public bool isHidden {
            get
            {
                if ( _isHidden != null ) { return _isHidden.Value; }
                if ( graph.isTree && inConnections.Count > 0 ) {
                    var parent = inConnections[0].sourceNode;
                    if ( parent.ID > this.ID ) { return ( _isHidden = false ).Value; }
                    return ( _isHidden = parent.collapsed || parent.isHidden ).Value;
                }
                return ( _isHidden = false ).Value;
            }
        }

        ///<summary>EDITOR! The custom color set by user.</summary>
        public Color customColor {
            get { return _color; }
            set { _color = value; }
        }

        ///<summary>EDITOR! Verbose level of the node GUI</summary>
        public VerboseLevel verboseLevel {
            get { return _verboseLevel; }
            set { _verboseLevel = value; }
        }

        ///<summary>EDITOR! is the node selected or part of the multi selection?</summary>
        public bool isSelected {
            get { return GraphEditorUtility.activeElement == this || GraphEditorUtility.activeElements.Contains(this); }
        }

        ///<summary>EDITOR! Is NC in icon mode and node has an icon?</summary>
        private bool showIcon {
            get { return Prefs.showIcons && icon != null; }
        }

        ///<summary>EDITOR! cached GUIContent for node header name</summary>
        private GUIContent cachedHeaderContent {
            get
            {
                if ( _cachedHeaderContent == null || _nameCache != name ) {
                    string hex;
                    if ( nodeColor != default(Color) ) {
                        hex = nodeColor.grayscale > 0.6f ? DEFAULT_HEX_COLOR_DARK : DEFAULT_HEX_COLOR_LIGHT;
                    } else {
                        hex = EditorGUIUtility.isProSkin ? hexColor : DEFAULT_HEX_COLOR_DARK;
                    }
                    var finalTitle = this is IGraphAssignable ? string.Format("{{ {0} }}", name) : name;
                    var text = string.Format("<b><color=#{0}>{1}</color></b>", hex, finalTitle);
                    var image = showIcon && iconAlignment == Alignment2x2.Left ? icon : null;
                    _cachedHeaderContent = new GUIContent(text, image);
                }
                return _cachedHeaderContent;
            }
        }

        ///<summary>EDITOR! The icon of the node</summary>
        private Texture2D icon {
            get
            {
                if ( _icon == null ) {
                    if ( this is ITaskAssignable ) {
                        var assignable = this as ITaskAssignable;
                        _icon = assignable.task != null ? assignable.task.icon : null;
                    }
                    if ( _icon == null ) {
                        var iconAtt = this.GetType().RTGetAttribute<ParadoxNotion.Design.IconAttribute>(true);
                        _icon = iconAtt != null ? TypePrefs.GetTypeIcon(iconAtt, this) : null;
                    }
                    if ( _icon == null ) {
                        _icon = new object();
                    }
                }
                return _icon as Texture2D;
            }
        }

        ///<summary>EDITOR! The coloring of the node if any.</summary>
        public Color nodeColor {
            get
            {
                if ( !colorLoaded ) {
                    colorLoaded = true;
                    hasColorAttribute = false;
                    colorAttributeColor = default(Color);
                    hexColor = DEFAULT_HEX_COLOR_LIGHT;
                    var cAtt = this.GetType().RTGetAttribute<ColorAttribute>(true);
                    if ( cAtt != null ) {
                        hasColorAttribute = true;
                        colorAttributeColor = ColorUtils.HexToColor(cAtt.hexColor);
                        hexColor = cAtt.hexColor;
                        _color = default(Color);
                    }
                }

                if ( !hasColorAttribute && customColor != default(Color) ) {
                    return customColor;
                }

                return colorAttributeColor;
            }
            private set
            {
                if ( customColor != value ) {
                    _cachedHeaderContent = null; //flush content
                    if ( value.a <= 0.2f ) {
                        customColor = default(Color);
                        hexColor = DEFAULT_HEX_COLOR_LIGHT;
                        return;
                    }
                    customColor = value;
                    var temp = (Color32)value;
                    hexColor = ( temp.r.ToString("X2") + temp.g.ToString("X2") + temp.b.ToString("X2") ).ToLower();
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>EDITOR! A position relative to the node</summary>
        protected Vector2 GetRelativeNodePosition(Alignment2x2 alignment, float margin = 0) {
            switch ( alignment ) {
                case ( Alignment2x2.Default ):
                    return rect.center;

                case ( Alignment2x2.Left ):
                    return new Vector2(rect.xMin - margin, rect.center.y);

                case ( Alignment2x2.Right ):
                    return new Vector2(rect.xMax + margin, rect.center.y);

                case ( Alignment2x2.Top ):
                    return new Vector2(rect.center.x, rect.yMin - margin);

                case ( Alignment2x2.Bottom ):
                    return new Vector2(rect.center.x, rect.yMax + margin);
            }

            return rect.center;
        }


        ///----------------------------------------------------------------------------------------------

        //The main function for drawing a node's gui.Fires off others.
        public static void ShowNodeGUI(Node node, Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos, float zoomFactor) {

            if ( node.isHidden ) {
                return;
            }

            if ( fullDrawPass || drawCanvas.Overlaps(node.rect) || GraphEditorUtility.activeNode == node ) {
                DrawNodeWindow(node, canvasMousePos, zoomFactor);
                DrawNodeTag(node);
                DrawNodeComments(node);
                DrawNodeElapsedTime(node);
                DrawNodeID(node);
            }

            DrawReferenceLinks(node);
            node.OnNodeExternalGUI();
            node.DrawNodeConnections(drawCanvas, fullDrawPass, canvasMousePos, zoomFactor);
        }

        //Draw the window
        static void DrawNodeWindow(Node node, Vector2 canvasMousePos, float zoomFactor) {

            //un-colapse children ui
            if ( node.collapsed ) {
                var r = new Rect(node.rect.x, ( node.rect.yMax + 10 ), node.rect.width, 20);
                EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
                if ( GUI.Button(r, "COLLAPSED", StyleSheet.box) ) {
                    node.collapsed = false;
                }
            }

            GUI.color = node.isActive ? Color.white : new Color(0.9f, 0.9f, 0.9f, 0.8f);
            GUI.color = GraphEditorUtility.activeElement == node ? new Color(0.9f, 0.9f, 1) : GUI.color;
            //Remark: using MaxWidth and MaxHeight makes GUILayout window contract width and height
            node.rect = GUILayout.Window(node.ID, node.rect, (ID) => { NodeWindowGUI(node, ID); }, string.Empty, StyleSheet.window, GUILayout.MaxHeight(MIN_SIZE.y), GUILayout.MaxWidth(MIN_SIZE.x));

            GUI.color = Color.white;
            Styles.Draw(node.rect, StyleSheet.windowShadow);

            if ( Application.isPlaying && node.status != Status.Resting ) {
                GUI.color = StyleSheet.GetStatusColor(node.status);
                Styles.Draw(node.rect, StyleSheet.windowHighlight);

            } else {

                if ( node.isSelected ) {
                    GUI.color = StyleSheet.GetStatusColor(Status.Resting);
                    Styles.Draw(node.rect, StyleSheet.windowHighlight);
                }
            }

            GUI.color = Color.white;
            if ( GraphEditorUtility.allowClick ) {
                if ( zoomFactor == 1f ) {
                    EditorGUIUtility.AddCursorRect(new Rect(node.rect.x, node.rect.y, node.rect.width, node.rect.height), MouseCursor.Link);
                }
            }
        }


        //This is the callback function of the GUILayout.window. Everything here is called INSIDE the node Window callback.
        //The Window ID is the same as the node.ID.
        static void NodeWindowGUI(Node node, int ID) {

            var e = Event.current;

            ShowHeader(node);
            ShowPossibleWarningError(node);
            HandleEvents(node, e);
            ShowStatusIcons(node);
            ShowBreakpointMark(node);
            ShowNodeContents(node);
            HandleContextMenu(node, e);
            HandleNodePosition(node, e);
        }

        //The title name or icon of the node
        static void ShowHeader(Node node) {

            //text
            if ( !node.showIcon || node.iconAlignment != Alignment2x2.Default ) {
                if ( node.name != null ) {
                    EditorGUIUtility.SetIconSize(new Vector2(16, 16));
                    //Remark: CalcHeight does not take into account SetIconSize. CalcSize does.
                    var headerHeight = StyleSheet.windowTitle.CalcSize(node.cachedHeaderContent).y;
                    if ( node.rect.height <= 35 ) { headerHeight = 35; }
                    if ( node.nodeColor != default(Color) ) {
                        GUI.color = node.nodeColor;
                        Styles.Draw(new Rect(0, 0, node.rect.width, headerHeight), StyleSheet.windowHeader);
                        GUI.color = Color.white;
                    } else {
                        Handles.color = Color.black.WithAlpha(0.5f);
                        Handles.DrawPolyLine(new Vector3(5, headerHeight, 0), new Vector3(node.rect.width - 5, headerHeight, 0));
                        Handles.color = Color.white;
                    }
                    GUILayout.Label(node.cachedHeaderContent, StyleSheet.windowTitle);
                    EditorGUIUtility.SetIconSize(Vector2.zero);
                }
            }

            //icon
            if ( node.showIcon && ( node.iconAlignment == Alignment2x2.Default || node.iconAlignment == Alignment2x2.Bottom ) ) {
                GUI.color = node.nodeColor.a > 0.2f ? node.nodeColor : Color.white;
                //TODO: can be expensive for the light theme -> handle somehow else
                if ( !EditorGUIUtility.isProSkin ) {
                    var assignable = node as ITaskAssignable;
                    ParadoxNotion.Design.IconAttribute att = null;
                    if ( assignable != null && assignable.task != null ) { att = assignable.task.GetType().RTGetAttribute<ParadoxNotion.Design.IconAttribute>(true); }
                    if ( att == null ) { att = node.GetType().RTGetAttribute<ParadoxNotion.Design.IconAttribute>(true); }
                    if ( att != null && att.fixedColor == false ) { GUI.color = Color.black.WithAlpha(0.7f); }
                }

                GUI.backgroundColor = Color.clear;
                GUILayout.Box(node.icon, StyleSheet.box, GUILayout.MaxHeight(32));
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
            }
        }

        ///<summary>Responsible for showing warning/error icons</summary>
        static void ShowPossibleWarningError(Node node) {
            var warning = node.GetWarningOrError();
            if ( warning != null ) {
                var icon = warning.StartsWith("*") ? Icons.errorIcon : Icons.warningIcon;
                var errorRect = new Rect(node.rect.width - 21, 5, 16, 16);
                GUI.Box(errorRect, EditorUtils.GetTempContent(null, icon, warning), GUIStyle.none);
            }
        }

        //Handles events, Mouse downs, ups etc.
        static void HandleEvents(Node node, Event e) {

            //Node click
            if ( e.type == EventType.MouseDown && GraphEditorUtility.allowClick && e.button != 2 ) {

                UndoUtility.RecordObjectComplete(node.graph, "Move Node");

                if ( !e.control ) {
                    GraphEditorUtility.activeElement = node;
                }

                if ( e.control ) {
                    if ( node.isSelected ) { GraphEditorUtility.RemoveActiveElement(node); } else { GraphEditorUtility.AddActiveElement(node); }
                }

                if ( e.button == 0 ) {
                    node.nodeIsPressed = true;
                    adjustingBoundCanvasGroups = new List<CanvasGroup>();
                    if ( node.graph.canvasGroups != null ) {
                        foreach ( var group in node.graph.canvasGroups ) {
                            if ( group.rect.Encapsulates(node.rect) ) {
                                group.GatherContainedNodes(node.graph);
                                adjustingBoundCanvasGroups.Add(group);
                            }
                        }
                    }
                }

                //Double click
                if ( e.button == 0 && e.clickCount == 2 ) {
                    if ( node is IGraphAssignable && ( node as IGraphAssignable ).subGraph != null ) {
                        node.graph.SetCurrentChildGraphAssignable(node as IGraphAssignable);
                        node.nodeIsPressed = false;
                    } else if ( node is ITaskAssignable && ( node as ITaskAssignable ).task != null ) {
                        EditorUtils.OpenScriptOfType(( node as ITaskAssignable ).task.GetType());
                    } else {
                        EditorUtils.OpenScriptOfType(node.GetType());
                    }
                    e.Use();
                }

                node.OnNodePicked();
            }

            //..
            if ( e.type == EventType.MouseDrag ) {
                if ( adjustingBoundCanvasGroups != null && !e.shift ) {
                    foreach ( var group in adjustingBoundCanvasGroups ) { group.AdjustToContainedNodes(); }
                }
            }

            //Mouse up
            if ( e.type == EventType.MouseUp ) {
                if ( node.nodeIsPressed ) {
                    node.TrySortConnectionsByPositionX();
                }
                if ( adjustingBoundCanvasGroups != null ) {
                    foreach ( var group in adjustingBoundCanvasGroups ) { group.FlushContainedNodes(); }
                    adjustingBoundCanvasGroups = null;
                }
                node.nodeIsPressed = false;
                node.OnNodeReleased();
            }
        }

        //Shows the icons relative to the current node status
        static void ShowStatusIcons(Node node) {
            if ( Application.isPlaying && node.status != Status.Resting ) {
                var markRect = new Rect(5, 5, 16, 16);
                if ( node.status == Status.Success ) {
                    GUI.color = EditorGUIUtility.isProSkin ? StyleSheet.GetStatusColor(Status.Success) : Colors.Grey(0.25f);
                    GUI.DrawTexture(markRect, StyleSheet.statusSuccess);

                } else if ( node.status == Status.Running ) {
                    GUI.color = EditorGUIUtility.isProSkin ? StyleSheet.GetStatusColor(Status.Running) : Colors.Grey(0.25f);
                    GUI.DrawTexture(markRect, StyleSheet.statusRunning);

                } else if ( node.status == Status.Failure ) {
                    GUI.color = EditorGUIUtility.isProSkin ? StyleSheet.GetStatusColor(Status.Failure) : Colors.Grey(0.25f);
                    GUI.DrawTexture(markRect, StyleSheet.statusFailure);
                }
            }
        }

        //Shows the breakpoint mark icon if node is set as a breakpoint
        static void ShowBreakpointMark(Node node) {
            if ( node.isBreakpoint ) {
                var rect = new Rect(node.rect.width - 15, 5, 13, 13);
                GUI.DrawTexture(rect, Icons.redCircle);
            }
        }

        //Shows the actual node contents GUI
        static void ShowNodeContents(Node node) {

            GUI.color = Color.white;
            GUI.skin.label.richText = true;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            node.OnNodeGUI();
            TaskAssignableNodeGUI(node);
            GraphAssignableNodeGUI(node);

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }

        //...
        static void TaskAssignableNodeGUI(Node node) {
            if ( node is ITaskAssignable ) {
                GUILayout.BeginVertical(Styles.roundedBox);
                var task = ( node as ITaskAssignable ).task;
                GUILayout.Label(task != null ? task.summaryInfo : "No Task");
                GUILayout.EndVertical();
            }
        }

        //...
        static void GraphAssignableNodeGUI(Node node) {
            if ( node is IGraphAssignable ) {
                var assignable = (IGraphAssignable)node;
                if ( assignable.subGraphParameter != null ) {
                    GUILayout.BeginVertical(Styles.roundedBox);
                    GUILayout.Label(string.Format("Sub{0}\n{1}", assignable.subGraphParameter.varType.Name, assignable.subGraphParameter.ToString()));
                    if ( assignable.subGraph == null ) {
                        if ( !Application.isPlaying && GUILayout.Button("CREATE NEW") ) {
                            var newGraph = (Graph)EditorUtils.CreateAsset(assignable.subGraphParameter.varType);
                            if ( newGraph != null ) {
                                UndoUtility.RecordObjectComplete(node.graph, "New SubGraph");
                                Undo.RegisterCreatedObjectUndo(newGraph, "CreateNested");
                                assignable.subGraph = newGraph;
                                UndoUtility.SetDirty(newGraph);
                                UndoUtility.SetDirty(node.graph);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
        }

        //Handles and shows the right click mouse button for the node context menu
        static void HandleContextMenu(Node node, Event e) {
            var isContextClick = ( e.type == EventType.MouseUp && e.button == 1 ) || ( e.type == EventType.ContextClick );
            if ( GraphEditorUtility.allowClick && isContextClick ) {
                GenericMenu menu;
                if ( GraphEditorUtility.activeElements.Count > 1 ) {
                    menu = GetNodeMenu_Multi(node.graph);
                } else {
                    menu = GetNodeMenu_Single(node);
                }
                if ( menu != null ) {
                    //show in PostGUI due to zoom factor
                    GraphEditorUtility.PostGUI += () => { menu.ShowAsContext(); };
                    e.Use();
                }
            }
        }

        //Returns multi node context menu
        static GenericMenu GetNodeMenu_Multi(Graph graph) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate Selected Nodes"), false, () =>
              {
                  var newNodes = Graph.CloneNodes(GraphEditorUtility.activeElements.OfType<Node>().ToList(), graph);
                  GraphEditorUtility.activeElements = newNodes.Cast<IGraphElement>().ToList();
              });
            menu.AddItem(new GUIContent("Copy Selected Nodes"), false, () => { CopyBuffer.SetCache<Node[]>(Graph.CloneNodes(GraphEditorUtility.activeElements.OfType<Node>().ToList()).ToArray()); });

            //callback graph related extra menu items
            menu = graph.CallbackOnNodesContextMenu(menu, GraphEditorUtility.activeElements.OfType<Node>().ToArray());

            menu.AddSeparator("/");
            menu.AddItem(new GUIContent("Delete Selected Nodes"), false, () => { foreach ( Node n in GraphEditorUtility.activeElements.ToArray() ) graph.RemoveNode(n); });
            return menu;
        }

        //Returns single node context menu
        public static GenericMenu GetNodeMenu_Single(Node node) {
            var menu = new GenericMenu();
            if ( node.graph.primeNode != node && node.allowAsPrime ) {
                menu.AddItem(new GUIContent("Set Start"), false, () => { node.graph.primeNode = node; });
            }

            if ( node is IGraphAssignable ) {
                menu.AddItem(new GUIContent("Edit Nested (Double Click)"), false, () => { node.graph.SetCurrentChildGraphAssignable(node as IGraphAssignable); });
            }

            menu.AddItem(new GUIContent("Duplicate (CTRL+D)"), false, () => { GraphEditorUtility.activeElement = node.Duplicate(node.graph); });
            menu.AddItem(new GUIContent("Copy Node"), false, () => { CopyBuffer.SetCache<Node[]>(new Node[] { node }); });

            if ( node.inConnections.Count > 0 ) {
                menu.AddItem(new GUIContent(node.isActive ? "Disable" : "Enable"), false, () => { node.SetActive(!node.isActive); });
            }

            if ( node.graph.isTree && node.outConnections.Count > 0 ) {
                menu.AddItem(new GUIContent(node.collapsed ? "Expand Children" : "Collapse Children"), false, () => { node.collapsed = !node.collapsed; });
            }

            if ( node is ITaskAssignable ) {
                var assignable = node as ITaskAssignable;
                if ( assignable.task != null ) {
                    menu.AddItem(new GUIContent("Copy Assigned Task"), false, () => { CopyBuffer.SetCache<Task>(assignable.task); });
                } else { menu.AddDisabledItem(new GUIContent("Copy Assigned Task")); }

                if ( CopyBuffer.TryGetCache<Task>(out Task copy) ) {
                    menu.AddItem(new GUIContent("Paste Assigned Task"), false, () =>
                   {
                       if ( assignable.task != null ) {
                           if ( !EditorUtility.DisplayDialog("Paste Task", string.Format("Node already has a Task assigned '{0}'. Replace assigned task with pasted task '{1}'?", assignable.task.name, copy.name), "YES", "NO") ) {
                               return;
                           }
                       }

                       try { assignable.task = copy.Duplicate(node.graph); }
                       catch { ParadoxNotion.Services.Logger.LogWarning("Can't paste Task here. Incombatible Types", LogTag.EDITOR, node); }
                   });

                } else { menu.AddDisabledItem(new GUIContent("Paste Assigned Task")); }
            }

            //extra items with override
            menu = node.OnContextMenu(menu);

            if ( menu != null ) {

                //extra items with attribute
                foreach ( var _m in node.GetType().RTGetMethods() ) {
                    var m = _m;
                    var att = m.RTGetAttribute<ContextMenu>(true);
                    if ( att != null ) {
                        menu.AddItem(new GUIContent(att.menuItem), false, () => { m.Invoke(node, null); });
                    }
                }

                menu.AddSeparator("/");
                menu.AddItem(new GUIContent("Delete (DEL)"), false, () => { node.graph.RemoveNode(node); });
            }
            return menu;
        }

        //basicaly handles the node position and draging etc
        static void HandleNodePosition(Node node, Event e) {

            if ( GraphEditorUtility.allowClick && e.button != 2 ) {
                //drag all selected nodes
                if ( e.type == EventType.MouseDrag && GraphEditorUtility.activeElements.Count > 1 ) {
                    for ( var i = 0; i < GraphEditorUtility.activeElements.Count; i++ ) {
                        ( (Node)GraphEditorUtility.activeElements[i] ).position += e.delta;
                    }
                    return;
                }

                if ( node.nodeIsPressed ) {
                    var hierarchicalMove = Prefs.hierarchicalMove != e.shift;
                    //snap to grid
                    if ( !hierarchicalMove && Prefs.snapToGrid && GraphEditorUtility.activeElements.Count == 0 ) {
                        node.position = new Vector2(Mathf.Round(node.position.x / 15) * 15, Mathf.Round(node.position.y / 15) * 15);
                    }

                    //recursive drag
                    if ( node.graph.isTree && e.type == EventType.MouseDrag ) {
                        if ( hierarchicalMove || node.collapsed ) {
                            RecursivePanNode(node, e.delta);
                        }
                    }
                }

                //this drag
                GUI.DragWindow();
            }
        }

        //The comments of the node sitting next or bottom of it
        static void DrawNodeComments(Node node) {

            if ( !Prefs.showComments || string.IsNullOrEmpty(node.comments) ) {
                return;
            }

            var commentsRect = new Rect();
            var style = StyleSheet.commentsBox;
            var size = style.CalcSize(EditorUtils.GetTempContent(node.comments));

            if ( node.commentsAlignment == Alignment2x2.Top ) {
                size.y = style.CalcHeight(EditorUtils.GetTempContent(node.comments), node.rect.width);
                commentsRect = new Rect(node.rect.x, node.rect.y - size.y, node.rect.width, size.y - 2);
            }
            if ( node.commentsAlignment == Alignment2x2.Bottom ) {
                size.y = style.CalcHeight(EditorUtils.GetTempContent(node.comments), node.rect.width);
                commentsRect = new Rect(node.rect.x, node.rect.yMax + 5, node.rect.width, size.y);
            }
            if ( node.commentsAlignment == Alignment2x2.Left ) {
                var width = Mathf.Min(size.x, node.rect.width * 2);
                commentsRect = new Rect(node.rect.xMin - width, node.rect.yMin, width, node.rect.height);
            }
            if ( node.commentsAlignment == Alignment2x2.Right ) {
                commentsRect = new Rect(node.rect.xMax + 5, node.rect.yMin, Mathf.Min(size.x, node.rect.width * 2), node.rect.height);
            }

            GUI.color = new Color(1, 1, 1, 0.6f);
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.2f);
            GUI.Box(commentsRect, node.comments, StyleSheet.commentsBox);
            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
        }

        //Shows the tag label on the left of the node if it is tagged
        static void DrawNodeTag(Node node) {
            if ( !string.IsNullOrEmpty(node.tag) ) {
                var size = StyleSheet.labelOnCanvas.CalcSize(EditorUtils.GetTempContent(node.tag));
                var tagRect = new Rect(node.rect.x - size.x - 10, node.rect.y, size.x, size.y);
                GUI.Label(tagRect, node.tag, StyleSheet.labelOnCanvas);
                tagRect.width = Icons.tagIcon.width;
                tagRect.height = Icons.tagIcon.height;
                tagRect.y += tagRect.height - 2;
                tagRect.x = node.rect.x - 22;
                GUI.DrawTexture(tagRect, Icons.tagIcon);
            }
        }

        //Show elapsed running time
        static void DrawNodeElapsedTime(Node node) {
            if ( Prefs.showNodeElapsedTimes ) {
                var rect = new Rect(node.rect.x, node.rect.y - 18, node.rect.width, 18);
                if ( node.graph.primeNode == node ) { rect.y -= 20f; }
                GUI.color = Color.grey;
                GUI.Label(rect, string.Format("<size=9>{0}</size>", node.elapsedTime.ToString("0.00")), StyleSheet.labelOnCanvas);
                GUI.color = Color.white;
            }
        }

        //Show the Node ID, mostly for debug purposes
        static void DrawNodeID(Node node) {
            if ( Prefs.showNodeIDs ) {
                var yOffset = Prefs.showNodeElapsedTimes ? 32 : 18;
                if ( node.graph.primeNode == node ) { yOffset += 20; }
                var rect = new Rect(node.rect.x, node.rect.y - yOffset, node.rect.width, 18);
                GUI.color = Color.grey;
                GUI.Label(rect, string.Format("<size=9>#{0}</size>", node.ID.ToString()), StyleSheet.labelOnCanvas);
                GUI.color = Color.white;
            }
        }

        //Function to pan the node with children recursively
        static void RecursivePanNode(Node node, Vector2 delta) {
            node.position += delta;
            for ( var i = 0; i < node.outConnections.Count; i++ ) {
                var child = node.outConnections[i].targetNode;
                if ( child.ID > node.ID ) {
                    RecursivePanNode(child, delta);
                }
            }
        }

        //...
        static void DrawReferenceLinks(Node node) {
            if ( node is IHaveNodeReference && node.isSelected ) {
                var target = ( node as IHaveNodeReference ).targetReference?.Get(node.graph);
                if ( target != null ) {
                    Handles.color = Color.grey;
                    Handles.DrawAAPolyLine(node.rect.center, target.rect.center);
                    Handles.color = Color.white;
                }
            }
        }

        //The inspector of the node shown in the editor panel or else.
        static public void ShowNodeInspectorGUI(Node node) {

            UndoUtility.CheckUndo(node.graph, "Node Inspector");

            if ( Prefs.showNodeInfo ) {
                GUI.backgroundColor = Colors.lightBlue;
                EditorGUILayout.HelpBox(node.description, MessageType.None);
                GUI.backgroundColor = Color.white;
            }

            GUILayout.BeginHorizontal();
            GUI.color = Color.white.WithAlpha(0.5f);
            if ( !node.showIcon && node.allowAsPrime ) {
                node.customName = EditorGUILayout.TextField(node.customName);
                EditorUtils.CommentLastTextField(node.customName, "Name...");
            }

            node.tag = EditorGUILayout.TextField(node.tag);
            EditorUtils.CommentLastTextField(node.tag, "Tag...");

            if ( !node.hasColorAttribute ) {
                node.nodeColor = EditorGUILayout.ColorField(node.nodeColor, GUILayout.Width(30));
            }

            GUILayout.EndHorizontal();

            node.comments = EditorGUILayout.TextArea(node.comments);
            EditorUtils.CommentLastTextField(node.comments, "Comments...");
            GUI.color = Color.white;

            EditorUtils.Separator();
            node.OnNodeInspectorGUI();
            TaskAssignableInspectorGUI(node);
            GraphAssignableInspectorGUI(node);

            UndoUtility.CheckDirty(node.graph);
        }

        //If the node implements ITaskAssignable...
        static void TaskAssignableInspectorGUI(Node node) {

            if ( node is ITaskAssignable ) {

                var assignable = node as ITaskAssignable;
                System.Type taskType = null;
                var interfaces = node.GetType().GetInterfaces();
                for ( var i = 0; i < interfaces.Length; i++ ) {
                    var iType = interfaces[i];
                    if ( iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(ITaskAssignable<>) ) {
                        taskType = iType.RTGetGenericArguments()[0];
                        break;
                    }
                }

                if ( taskType != null ) {
                    TaskEditor.TaskFieldMulti(assignable.task, node.graph, taskType, (t) => { node._icon = null; assignable.task = t; });
                }
            }
        }

        //If the node implements IGraphAssignable...
        static void GraphAssignableInspectorGUI(Node node) {
            if ( node is IGraphAssignable ) {
                var assignable = node as IGraphAssignable;
                assignable.ShowVariablesMappingGUI();
                if ( assignable.subGraphParameter != null ) {
                    if ( ReferenceEquals(assignable.subGraphParameter.value, assignable.graph) ) {
                        ParadoxNotion.Services.Logger.LogWarning("SubGraph can't be itself.", LogTag.EDITOR);
                        assignable.subGraphParameter.value = null;
                    }
                }
            }
        }


        //Activates/Deactivates all inComming connections
        void SetActive(bool active) {

            if ( isChecked ) {
                return;
            }

            isChecked = true;

            //just for visual feedback
            if ( !active ) {
                GraphEditorUtility.activeElement = null;
            }

            UndoUtility.RecordObject(graph, "SetActive");

            //disable all incomming
            foreach ( var cIn in inConnections ) {
                cIn.isActive = active;
            }

            //disable all outgoing
            foreach ( var cOut in outConnections ) {
                cOut.isActive = active;
            }

            //if child is still considered active(= at least 1 incomming is active), continue else SetActive child as well.
            foreach ( var child in outConnections.Select(c => c.targetNode) ) {

                if ( child.isActive == !active ) {
                    continue;
                }

                child.SetActive(active);
            }

            isChecked = false;
        }


        //Editor. Sorts the parent node connections based on all child nodes X position. Possible only when not in play mode.
        public void TrySortConnectionsByPositionX() {
            if ( !Application.isPlaying && graph != null && graph.isTree ) {
                foreach ( var connection in inConnections.ToArray() ) {
                    var node = connection.sourceNode;
                    var original = node.outConnections.ToList();
                    node.outConnections = node.outConnections.OrderBy(c => c.targetNode.rect.center.x).ToList();
                    var oldIndeces = node.outConnections.Select(x => original.IndexOf(x)).ToArray();
                    foreach ( var field in node.GetType().RTGetFields() ) {
                        if ( field.RTIsDefined<AutoSortWithChildrenConnections>(true) ) {
                            var list = field.GetValue(node) as System.Collections.IList;
                            if ( list != null ) {
                                var temp = new object[list.Count];
                                for ( var i = 0; i < list.Count; i++ ) { temp[i] = list[i]; }
                                for ( var i = 0; i < oldIndeces.Length; i++ ) { list[i] = temp[oldIndeces[i]]; }
                            }
                        }
                    }
                    node.OnChildrenConnectionsSorted(oldIndeces);
                }
            }
        }

        ///<summary>Editor. Connection Relink has ended. Handle effect</summary>
        virtual public void OnActiveRelinkEnd(Connection connection) {
            for ( var i = 0; i < graph.allNodes.Count; i++ ) {
                var otherNode = graph.allNodes[i];
                if ( otherNode.rect.Contains(Event.current.mousePosition) ) {
                    if ( connection.relinkState == Connection.RelinkState.Target && otherNode != connection.targetNode ) {
                        if ( Node.IsNewConnectionAllowed(connection.sourceNode, otherNode, connection) ) {
                            connection.SetTargetNode(otherNode);
                        }
                    }
                    if ( connection.relinkState == Connection.RelinkState.Source && otherNode != connection.sourceNode ) {
                        if ( Node.IsNewConnectionAllowed(otherNode, connection.targetNode, connection) ) {
                            connection.SetSourceNode(otherNode);
                        }
                    }
                    return;
                }
            }
        }

        ///<summary>Draw an automatic editor inspector for this node.</summary>
        protected void DrawDefaultInspector() {
            EditorUtils.ReflectedObjectInspector(this, graph);
        }

        ///<summary>Editor. Draw the connections line from this node, to all of its children. This is the default hierarchical tree style. Override in each system's base node class.</summary>
        virtual protected void DrawNodeConnections(Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos, float zoomFactor) {

            var e = Event.current;

            //Receive connections first
            if ( clickedPort != null && e.type == EventType.MouseUp && e.button == 0 ) {

                if ( rect.Contains(e.mousePosition) ) {
                    graph.ConnectNodes(clickedPort.parent, this, clickedPort.portIndex);
                    clickedPort = null;
                    e.Use();

                } else {

                    dragDropMisses++;

                    if ( dragDropMisses == graph.allNodes.Count && clickedPort != null ) {

                        var source = clickedPort.parent;
                        var index = clickedPort.portIndex;
                        var pos = e.mousePosition;
                        clickedPort = null;

                        System.Action<System.Type> Selected = delegate (System.Type type)
                        {
                            var newNode = graph.AddNode(type, pos);
                            graph.ConnectNodes(source, newNode, index);
                            GraphEditorUtility.activeElement = newNode;
                        };

                        var menu = EditorUtils.GetTypeSelectionMenu(graph.baseNodeType, Selected);
                        if ( zoomFactor == 1 ) {
                            menu.ShowAsBrowser(string.Format("Add {0} Node", graph.GetType().Name), graph.baseNodeType);
                        } else {
                            GraphEditorUtility.PostGUI += () => { menu.ShowAsContext(); };
                        }
                        e.Use();
                    }
                }
            }

            if ( maxOutConnections == 0 ) {
                return;
            }

            if ( fullDrawPass || drawCanvas.Overlaps(rect) ) {

                var nodeOutputBox = new Rect(rect.x, rect.yMax - 4, rect.width, 12);
                Styles.Draw(nodeOutputBox, StyleSheet.nodePortContainer);

                //draw the ports
                if ( outConnections.Count < maxOutConnections || maxOutConnections == -1 ) {
                    for ( var i = 0; i < outConnections.Count + 1; i++ ) {
                        var portRect = new Rect(0, 0, 10, 10);
                        portRect.center = new Vector2(( ( rect.width / ( outConnections.Count + 1 ) ) * ( i + 0.5f ) ) + rect.xMin, rect.yMax + 6);
                        GUI.Box(portRect, string.Empty, StyleSheet.nodePortEmpty);

                        if ( collapsed ) {
                            continue;
                        }

                        if ( GraphEditorUtility.allowClick ) {
                            //start a connection by clicking a port
                            EditorGUIUtility.AddCursorRect(portRect, MouseCursor.ArrowPlus);
                            if ( e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition) ) {
                                dragDropMisses = 0;
                                clickedPort = new GUIPort(i, this, portRect.center);
                                e.Use();
                            }
                        }
                    }
                }
            }


            //draw the new drag&drop connection line
            if ( clickedPort != null && clickedPort.parent == this ) {
                var tangA = default(Vector2);
                var tangB = default(Vector2);
                ParadoxNotion.CurveUtils.ResolveTangents(clickedPort.pos, e.mousePosition, Prefs.connectionsMLT, PlanarDirection.Vertical, out tangA, out tangB);
                Handles.DrawBezier(clickedPort.pos, e.mousePosition, clickedPort.pos + tangA, e.mousePosition + tangB, StyleSheet.GetStatusColor(Status.Resting).WithAlpha(0.8f), StyleSheet.bezierTexture, 3);
            }


            //draw all connected lines
            for ( var i = 0; i < outConnections.Count; i++ ) {

                var connection = outConnections[i];
                if ( connection != null ) {

                    if ( connection.targetNode is Internal.MissingNode ) {
                        continue;
                    }

                    var sourcePos = new Vector2(( ( rect.width / ( outConnections.Count + 1 ) ) * ( i + 1 ) ) + rect.xMin, rect.yMax + 6);
                    var targetPos = new Vector2(connection.targetNode.rect.center.x, connection.targetNode.rect.y);

                    var sourcePortRect = new Rect(0, 0, 12, 12);
                    sourcePortRect.center = sourcePos;

                    var targetPortRect = new Rect(0, 0, 15, 15);
                    targetPortRect.center = targetPos;

                    var boundRect = RectUtils.GetBoundRect(sourcePortRect, targetPortRect);
                    if ( fullDrawPass || drawCanvas.Overlaps(boundRect) ) {

                        GUI.Box(sourcePortRect, string.Empty, StyleSheet.nodePortConnected);

                        if ( collapsed || connection.targetNode.isHidden ) {
                            continue;
                        }

                        connection.DrawConnectionGUI(sourcePos, targetPos);

                        if ( GraphEditorUtility.allowClick ) {
                            //On right click disconnect connection from the source.
                            if ( e.type == EventType.ContextClick && sourcePortRect.Contains(e.mousePosition) ) {
                                graph.RemoveConnection(connection);
                                e.Use();
                                return;
                            }

                            //On right click disconnect connection from the target.
                            if ( e.type == EventType.ContextClick && targetPortRect.Contains(e.mousePosition) ) {
                                graph.RemoveConnection(connection);
                                e.Use();
                                return;
                            }
                        }
                    }

                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Editor. When the node is picked</summary>
        virtual protected void OnNodePicked() { }
        ///<summary>Editor. When the node is released (mouse up)</summary>
        virtual protected void OnNodeReleased() { }
        ///<summary>Editor. Override to show controls within the node window</summary>
        virtual protected void OnNodeGUI() { }
        ///<summary>Extra GUI called outside of node window</summary>
        virtual protected void OnNodeExternalGUI() { }
        ///<summary>Editor. Override to show controls within the inline inspector or leave it to show an automatic editor</summary>
        virtual protected void OnNodeInspectorGUI() { DrawDefaultInspector(); }
        ///<summary>Editor. Override to add more entries to the right click context menu of the node</summary>
        virtual protected GenericMenu OnContextMenu(GenericMenu menu) { return menu; }

        ///<summary>Get connection information node wise, to show on top of the connection</summary>
        virtual public string GetConnectionInfo(int index) { return null; }
        ///<summary>Extra inspector controls for the provided OUT connection</summary>
        virtual public void OnConnectionInspectorGUI(int index) { }

        ///<summary>Editor. Connection Relink has started. Handle effect</summary>
        virtual public void OnActiveRelinkStart(Connection connection) { }

        ///----------------------------------------------------------------------------------------------

    }
}

#endif
