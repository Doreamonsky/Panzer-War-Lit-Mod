#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ParadoxNotion.Serialization;
using ParadoxNotion.Services;
using System.Threading.Tasks;

namespace ParadoxNotion.Design
{

    ///<summary>Proviving a UnityEditor.GenericMenu, shows a complete popup browser.</summary>
    public class GenericMenuBrowser : PopupWindowContent
    {

        //class node used in browser
        class Node
        {
            public string name;
            public string fullPath;
            public bool unfolded;
            public EditorUtils.MenuItemInfo item;
            public Node parent;
            public Dictionary<string, Node> children;

            public Node() {
                children = new Dictionary<string, Node>(System.StringComparer.Ordinal);
                unfolded = true;
            }

            //Leafs have menu items
            public bool isLeaf {
                get { return item.isValid; }
            }

            public string category {
                get { return parent != null ? parent.fullPath : string.Empty; }
            }

            //Is node favorite?
            public bool isFavorite {
                get
                {
                    if ( current.favorites.Contains(fullPath) ) {
                        return true;
                    }

                    var p = parent;
                    while ( p != null ) {
                        if ( p.isFavorite ) {
                            return true;
                        }
                        p = p.parent;
                    }
                    return false;
                }
            }

            //Does node has any favorite children?
            public bool HasAnyFavoriteChild() {
                if ( !string.IsNullOrEmpty(fullPath) ) {
                    return current.favorites.Any(p => p.StartsWith(fullPath + "/"));
                }
                return false;
            }

            //Toggle favorite state
            public void ToggleFavorite() {
                SetFavorite(!isFavorite);
            }

            //Set favorite state
            void SetFavorite(bool fav) {
                if ( fav == true && !isFavorite ) {
                    current.AddFavorite(fullPath);
                }

                if ( fav == false && isFavorite ) {
                    current.RemoveFavorite(fullPath);
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Browser preferences and saved favorites per key</summary>
        class SerializationData
        {
            public Dictionary<string, List<string>> allFavorites;
            public bool filterFavorites;
            public SerializationData() {
                allFavorites = new Dictionary<string, List<string>>(System.StringComparer.Ordinal);
                filterFavorites = false;
            }
        }

        ///----------------------------------------------------------------------------------------------

        public static GenericMenuBrowser current { get; private set; }
        private const string PREFERENCES_KEY = "ParadoxNotion.ContextBrowserPreferences_2";
        private const float HELP_RECT_HEIGHT = 58;
        private readonly Color hoverColor = new Color(0.5f, 0.5f, 1, 0.3f);


        private static System.Threading.Thread treeGenerationThread;
        private static System.Threading.Thread searchGenerationThread;

        private SerializationData data;
        private System.Type currentKeyType;
        private List<string> favorites;

        private bool _filterFavorites;
        private bool filterFavorites {
            get { return _filterFavorites && currentKeyType != null; }
            set { _filterFavorites = value; }
        }

        private GenericMenu boundMenu;
        private EditorUtils.MenuItemInfo[] items;
        private Node rootNode;
        private List<Node> leafNodes;
        private Node currentNode;

        private bool willRepaint;
        private float loadProgress;

        private string headerTitle;
        private Vector2 scrollPos;
        private string lastSearch;
        private string search;
        private int lastHoveringIndex;
        private int hoveringIndex;
        private float helpRectRequiredHeight;
        private EditorWindow wasFocusedWindow;


        ///----------------------------------------------------------------------------------------------

        private GUIStyle _helpStyle;
        private GUIStyle helpStyle {
            get
            {
                if ( _helpStyle == null ) {
                    _helpStyle = new GUIStyle(Styles.topLeftLabel);
                    _helpStyle.wordWrap = true;
                }
                return _helpStyle;
            }
        }

        ///----------------------------------------------------------------------------------------------

        //...
        public override Vector2 GetWindowSize() { return new Vector2(480, Mathf.Max(500 + helpRectRequiredHeight, 500)); }

        ///<summary>Shows the popup menu at position and with title immediately</summary>
        public static void ShowAsync(Vector2 pos, string title, System.Type keyType, System.Func<GenericMenu> getMenu) {
            var browser = new GenericMenuBrowser(null, title, keyType);
            var task = Task.Run(() => getMenu()).ContinueWith((m) => browser.SetMenu(m.Result));
            PopupWindow.Show(new Rect(pos.x, pos.y, 0, 0), browser);
        }

        ///<summary>Shows the popup menu at position and with title</summary>
        public static GenericMenuBrowser Show(GenericMenu newMenu, Vector2 pos, string title, System.Type keyType) {
            var browser = new GenericMenuBrowser(newMenu, title, keyType);
            PopupWindow.Show(new Rect(pos.x, pos.y, 0, 0), browser);
            return browser;
        }

        ///<summary>constructor</summary>
        public GenericMenuBrowser(GenericMenu newMenu, string title, System.Type keyType) {
            current = this;
            headerTitle = title;
            currentKeyType = keyType;
            rootNode = new Node();
            currentNode = rootNode;
            lastHoveringIndex = -1;
            hoveringIndex = -1;
            SetMenu(newMenu);
        }

        ///<summary>Set another menu after it's open</summary>
        public void SetMenu(GenericMenu newMenu) {
            if ( newMenu == null ) {
                return;
            }

            willRepaint = true;
            boundMenu = newMenu;
            treeGenerationThread = Threader.StartAction(treeGenerationThread, current.GenerateTree, () =>
           {
               treeGenerationThread = null;
               willRepaint = true;
           });
        }

        //editor opened
        public override void OnOpen() {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            wasFocusedWindow = EditorWindow.focusedWindow;
            LoadPrefs();
        }

        //editor closed
        public override void OnClose() {
            SavePrefs();
            EditorApplication.update -= OnEditorUpdate;
            if ( treeGenerationThread != null && treeGenerationThread.IsAlive ) { treeGenerationThread.Abort(); treeGenerationThread = null; }
            if ( searchGenerationThread != null && searchGenerationThread.IsAlive ) { searchGenerationThread.Abort(); searchGenerationThread = null; }
            current = null;
            EditorWindow.FocusWindowIfItsOpen(wasFocusedWindow != null ? wasFocusedWindow.GetType() : null);
        }

        //check flag and repaint?
        void OnEditorUpdate() {
            if ( willRepaint ) {
                willRepaint = false;
                base.editorWindow.Repaint();
            }
        }

        //...
        private void LoadPrefs() {
            if ( data == null ) {
                var json = EditorPrefs.GetString(PREFERENCES_KEY);
                if ( !string.IsNullOrEmpty(json) ) { data = JSONSerializer.Deserialize<SerializationData>(json); }
                if ( data == null ) { data = new SerializationData(); }

                filterFavorites = data.filterFavorites;
                if ( currentKeyType != null ) {
                    data.allFavorites.TryGetValue(currentKeyType.Name, out favorites);
                }

                if ( favorites == null ) {
                    favorites = new List<string>();
                }
            }
        }

        //...
        private void SavePrefs() {
            data.filterFavorites = filterFavorites;
            if ( currentKeyType != null ) {
                data.allFavorites[currentKeyType.Name] = favorites;
            }
            EditorPrefs.SetString(PREFERENCES_KEY, JSONSerializer.Serialize(typeof(SerializationData), data));
        }

        //...
        public void AddFavorite(string path) {
            if ( !favorites.Contains(path) ) {
                favorites.Add(path);
            }
        }

        //...
        public void RemoveFavorite(string path) {
            if ( favorites.Contains(path) ) {
                favorites.Remove(path);
            }
        }

        //Generate the tree node structure out of the items
        void GenerateTree() {
            loadProgress = 0;
            var tempItems = EditorUtils.GetMenuItems(boundMenu);
            var tempLeafNodes = new List<Node>();
            var tempRoot = new Node();
            for ( var i = 0; i < tempItems.Length; i++ ) {
                loadProgress = i / (float)tempItems.Length;
                var item = tempItems[i];
                var itemPath = item.content.text;
                var parts = itemPath.Split('/');
                Node current = tempRoot;
                var path = string.Empty;
                for ( var j = 0; j < parts.Length; j++ ) {
                    var part = parts[j];
                    path += '/' + part;
                    Node child = null;
                    if ( !current.children.TryGetValue(part, out child) ) {
                        child = new Node { name = part, parent = current };
                        child.fullPath = path;
                        current.children[part] = child;
                        if ( part == parts.Last() ) {
                            child.item = item;
                            tempLeafNodes.Add(child);
                        }
                    }
                    current = child;
                }
            }
            items = tempItems;
            leafNodes = tempLeafNodes;
            rootNode = tempRoot;
            currentNode = rootNode;
        }

        //generate search results and return in a Node (that should be set as root node)
        Node GenerateSearchResults() {
            var searchRootNode = new Node() { name = "Search Root" };
            searchRootNode.children = leafNodes
                .Where(x => ( filterFavorites ? x.isFavorite : true ) && StringUtils.SearchMatch(search, x.name, x.category))
                .OrderBy(x => StringUtils.ScoreSearchMatch(search, x.name, x.category) * ( x.isFavorite ? 0.5f : 1 ))
                .ToDictionary(x => x.fullPath, y => y);
            return searchRootNode;
        }


        //Show stuff
        public override void OnGUI(Rect rect) {

            var e = Event.current;
            EditorGUIUtility.SetIconSize(Vector2.zero);
            hoveringIndex = Mathf.Clamp(hoveringIndex, -1, currentNode.children.Count - 1);
            if ( EditorGUIUtility.isProSkin ) {
                Styles.Draw(rect, Styles.shadowedBackground);
            }

            var headerHeight = currentNode.parent != null ? 95 : 60;
            var headerRect = new Rect(0, 0, rect.width, headerHeight);
            DoHeader(headerRect, e);

            if ( ( treeGenerationThread != null && treeGenerationThread.ThreadState != System.Threading.ThreadState.Stopped ) || items == null || items.Length == 0 ) {
                var progressRect = new Rect(0, 0, 200, 20);
                progressRect.center = rect.center;
                EditorGUI.ProgressBar(progressRect, loadProgress, "Loading...");
                willRepaint = true;
            } else {
                var treeRect = Rect.MinMaxRect(0, headerHeight, rect.width, rect.height - HELP_RECT_HEIGHT);
                DoTree(treeRect, e);
            }

            var helpRect = Rect.MinMaxRect(2, rect.height - HELP_RECT_HEIGHT + 2, rect.width - 2, rect.height - 2);
            DoFooter(helpRect, e);

            //handle the events
            HandeEvents(e);

            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        //...
        void DoHeader(Rect headerRect, Event e) {
            //HEADER
            GUILayout.Space(5);
            GUILayout.Label(string.Format("<color=#{0}><size=14><b>{1}</b></size></color>", EditorGUIUtility.isProSkin ? "dddddd" : "222222", headerTitle), Styles.topCenterLabel);

            //SEARCH
            if ( e.keyCode == KeyCode.DownArrow ) { GUIUtility.keyboardControl = 0; }
            if ( e.keyCode == KeyCode.UpArrow ) { GUIUtility.keyboardControl = 0; }
            if ( e.keyCode == KeyCode.Return ) { GUIUtility.keyboardControl = 0; }
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            GUI.SetNextControlName("SearchToolbar");
            search = EditorUtils.SearchField(search);
            if ( currentKeyType != null ) {
                filterFavorites = EditorGUILayout.ToggleLeft("FavOnly", filterFavorites, GUILayout.Width(70));
            }
            GUILayout.EndHorizontal();
            EditorUtils.BoldSeparator();

            //BACK
            if ( currentNode.parent != null && string.IsNullOrEmpty(search) ) {
                GUILayout.BeginHorizontal("box");
                if ( GUILayout.Button(string.Format("<b><size=14>◄ {0}/{1}</size></b>", currentNode.parent.name, currentNode.name), Styles.leftLabel) ) {
                    currentNode = currentNode.parent;
                }
                GUILayout.EndHorizontal();
                var lastRect = GUILayoutUtility.GetLastRect();
                if ( lastRect.Contains(e.mousePosition) ) {
                    GUI.color = hoverColor;
                    GUI.DrawTexture(lastRect, EditorGUIUtility.whiteTexture);
                    GUI.color = Color.white;
                    willRepaint = true;
                    hoveringIndex = -1;
                }
            }
        }

        //THE TREE
        void DoTree(Rect treeRect, Event e) {

            if ( treeGenerationThread != null ) {
                return;
            }

            if ( search != lastSearch ) {
                lastSearch = search;
                hoveringIndex = -1;
                if ( !string.IsNullOrEmpty(search) ) {

                    //provide null reference thread (thus no aborting) so that results update all the time
                    searchGenerationThread = Threader.StartFunction(null, GenerateSearchResults, (resultNode) =>
                    {
                        currentNode = resultNode;
                        searchGenerationThread = null;
                        if ( current != null ) { willRepaint = true; }
                    });

                } else {
                    currentNode = rootNode;
                }
            }


            GUILayout.BeginArea(treeRect);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            GUILayout.BeginVertical();

            ///----------------------------------------------------------------------------------------------

            var i = 0;
            var itemAdded = false;
            string lastSearchCategory = null;
            var isSearch = !string.IsNullOrEmpty(search);
            foreach ( var childPair in currentNode.children ) {
                if ( isSearch && i >= 200 ) {
                    EditorGUILayout.HelpBox("There are more than 200 results. Please try refine your search input.", MessageType.Info);
                    break;
                }

                var node = childPair.Value;
                var memberInfo = node.isLeaf ? node.item.userData as MemberInfo : null;
                var isDisabled = node.isLeaf && node.item.func == null && node.item.func2 == null;
                var icon = node.isLeaf ? node.item.content.image : Icons.folderIcon;
                if ( icon == null && memberInfo != null ) {
                    icon = TypePrefs.GetTypeIcon(memberInfo);
                }

                //when within search, show category on top
                if ( isSearch ) {
                    var searchCategory = lastSearchCategory;
                    if ( memberInfo == null || memberInfo is System.Type ) {
                        searchCategory = node.parent.fullPath != null ? node.parent.fullPath.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() : null;
                    } else {
                        searchCategory = memberInfo.ReflectedType.FriendlyName();
                    }

                    if ( searchCategory != lastSearchCategory ) {
                        lastSearchCategory = searchCategory;
                        GUI.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        GUILayout.BeginHorizontal("box");
                        GUI.color = Color.white;
                        GUILayout.Label(searchCategory, Styles.leftLabel, GUILayout.Height(16));
                        GUILayout.EndHorizontal();
                    }
                }
                //


                if ( filterFavorites && !node.isFavorite && !node.HasAnyFavoriteChild() ) {
                    continue;
                }

                if ( node.isLeaf && node.item.separator ) {
                    if ( itemAdded ) {
                        EditorUtils.Separator();
                    }
                    continue;
                }

                itemAdded = true;

                GUI.color = Color.clear;
                GUILayout.BeginHorizontal("box");
                GUI.color = Color.white;

                //Prefix icon
                GUILayout.Label(icon, GUILayout.Width(22), GUILayout.Height(16));
                GUI.enabled = !isDisabled;

                //Favorite
                if ( currentKeyType != null ) {
                    GUI.color = node.isFavorite ? Color.white : ( node.HasAnyFavoriteChild() ? new Color(1, 1, 1, 0.2f) : new Color(0f, 0f, 0f, 0.4f) );
                    if ( GUILayout.Button(Icons.favoriteIcon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16)) ) {
                        node.ToggleFavorite();
                    }
                    GUI.color = Color.white;
                }

                //Content
                var label = node.name;
                var hexColor = EditorGUIUtility.isProSkin ? "#B8B8B8" : "#262626";
                hexColor = isDisabled ? "#666666" : hexColor;
                var text = string.Format("<color={0}><size=11>{1}</size></color>", hexColor, ( !node.isLeaf ? string.Format("<b>{0}</b>", label) : label ));
                GUILayout.Label(text, Styles.leftLabel, GUILayout.Width(0), GUILayout.ExpandWidth(true));
                GUILayout.Label(node.isLeaf ? "●" : "►", Styles.leftLabel, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                var elementRect = GUILayoutUtility.GetLastRect();
                if ( e.type == EventType.MouseDown && e.button == 0 && elementRect.Contains(e.mousePosition) ) {
                    e.Use();
                    if ( node.isLeaf ) {

                        ExecuteItemFunc(node.item);
                        break;

                    } else {

                        currentNode = node;
                        hoveringIndex = 0;
                        break;
                    }
                }

                if ( e.type == EventType.MouseMove && elementRect.Contains(e.mousePosition) ) {
                    hoveringIndex = i;
                }

                if ( hoveringIndex == i ) {
                    GUI.color = hoverColor;
                    GUI.DrawTexture(elementRect, EditorGUIUtility.whiteTexture);
                    GUI.color = Color.white;
                }

                i++;
                GUI.enabled = true;
            }

            if ( hoveringIndex != lastHoveringIndex ) {
                willRepaint = true;
                lastHoveringIndex = hoveringIndex;
            }

            if ( !itemAdded ) {
                GUILayout.Label("No results to display with current search and filter combination");
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        //HELP AREA
        void DoFooter(Rect helpRect, Event e) {
            helpRectRequiredHeight = 0;
            var hoveringNode = hoveringIndex >= 0 && currentNode.children.Count > 0 ? currentNode.children.Values.ToList()[hoveringIndex] : null;
            GUI.color = new Color(0, 0, 0, 0.3f);
            Styles.Draw(helpRect, GUI.skin.textField);
            GUI.color = Color.white;
            GUILayout.BeginArea(helpRect);
            GUILayout.BeginVertical();
            var doc = string.Empty;
            if ( hoveringNode != null && hoveringNode.isLeaf ) {
                doc = hoveringNode.item.content.tooltip;
                var memberInfo = hoveringNode.item.userData as MemberInfo;
                if ( memberInfo != null && string.IsNullOrEmpty(doc) ) {
                    if ( memberInfo is System.Type ) {
                        doc = TypePrefs.GetTypeDoc(memberInfo);
                    } else {
                        doc = XMLDocs.GetMemberSummary(memberInfo);
                    }
                }
            }

            GUILayout.Label(string.Format("<size=11>{0}</size>", doc), helpStyle);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        ///----------------------------------------------------------------------------------------------

        //Executes the item's registered delegate
        void ExecuteItemFunc(EditorUtils.MenuItemInfo item) {
            if ( item.func != null ) {
                item.func();
            } else {
                item.func2(item.userData);
            }
            base.editorWindow.Close();
        }

        //Handle events
        void HandeEvents(Event e) {

            //Go back with right click as well...
            if ( e.type == EventType.MouseDown && e.button == 1 ) {
                if ( currentNode.parent != null ) {
                    currentNode = currentNode.parent;
                }
                e.Use();
            }

            if ( e.type == EventType.KeyDown ) {

                if ( e.keyCode == KeyCode.RightArrow || e.keyCode == KeyCode.Return ) {
                    if ( hoveringIndex >= 0 ) {
                        var next = currentNode.children.Values.ToList()[hoveringIndex];
                        if ( e.keyCode == KeyCode.Return && next.isLeaf ) {
                            ExecuteItemFunc(next.item);
                        } else if ( !next.isLeaf ) {
                            currentNode = next;
                            hoveringIndex = 0;
                        }
                        e.Use();
                        return;
                    }
                }

                if ( e.keyCode == KeyCode.LeftArrow ) {
                    var previous = currentNode.parent;
                    if ( previous != null ) {
                        hoveringIndex = currentNode.parent.children.Values.ToList().IndexOf(currentNode);
                        currentNode = previous;
                    }
                    e.Use();
                    return;
                }

                if ( e.keyCode == KeyCode.DownArrow ) {
                    hoveringIndex++;
                    e.Use();
                    return;
                }

                if ( e.keyCode == KeyCode.UpArrow ) {
                    hoveringIndex = Mathf.Max(hoveringIndex - 1, 0);
                    e.Use();
                    return;
                }

                if ( e.keyCode == KeyCode.Escape ) {
                    base.editorWindow.Close();
                    e.Use();
                    return;
                }

                EditorGUI.FocusTextInControl("SearchToolbar");

            }
        }
    }
}

#endif