#if UNITY_EDITOR

using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ParadoxNotion.Design
{

    partial class EditorUtils
    {

        public delegate void ReorderableListCallback(int index, bool isPicked);

        private static int pickedListIndex = -1;
        private static IList pickedList;

        public struct ReorderableListOptions
        {
            public delegate GenericMenu GetItemMenuDelegate(int i);
            public bool blockReorder;
            public bool allowAdd;
            public bool allowRemove;
            public UnityObject unityObjectContext;
            public GetItemMenuDelegate customItemMenu;
        }

        ///<summary> A simple reorderable list. Pass the list and a function to call for GUI. The callback comes with the current iterated element index in the list</summary>
        public static IList ReorderableList(IList list, ReorderableListCallback GUICallback) {
            return ReorderableList(list, default(ReorderableListOptions), GUICallback);
        }

        ///<summary> A simple reorderable list. Pass the list and a function to call for GUI. The callback comes with the current iterated element index in the list</summary>
        public static IList ReorderableList(IList list, ReorderableListOptions options, ReorderableListCallback GUICallback) {

            if ( list == null ) {
                return null;
            }

            var listType = list.GetType();
            var argType = listType.GetEnumerableElementType();
            if ( argType == null ) {
                return list;
            }

            var e = Event.current;

            if ( options.allowAdd ) {

                var dropRefs = DragAndDrop.objectReferences;

                //Drag And Drop.
                if ( dropRefs.Length > 0 ) {
                    if ( dropRefs.Any(r => argType.IsAssignableFrom(r.GetType()) || ( r.GetType() == typeof(GameObject) && typeof(Component).IsAssignableFrom(argType) )) ) {
                        var dropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                        dropRect.xMin += 5;
                        dropRect.xMax -= 5;
                        Styles.Draw(dropRect, Styles.roundedBox);
                        GUI.Box(dropRect, "Drop Here to Enlist", Styles.centerLabel);
                        if ( dropRect.Contains(e.mousePosition) ) {
                            if ( e.type == EventType.DragUpdated ) {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                e.Use();
                            }
                            if ( e.type == EventType.DragPerform ) {
                                for ( var i = 0; i < dropRefs.Length; i++ ) {
                                    var dropRef = dropRefs[i];
                                    var dropRefType = dropRef.GetType();
                                    if ( argType.IsAssignableFrom(dropRefType) ) {
                                        UndoUtility.RecordObject(options.unityObjectContext, "Drag Add Item");
                                        list.Add(dropRef);
                                        GUI.changed = true;
                                        UndoUtility.SetDirty(options.unityObjectContext);
                                        continue;
                                    }
                                    if ( dropRefType == typeof(GameObject) && typeof(Component).IsAssignableFrom(argType) ) {
                                        var componentToAdd = ( dropRef as GameObject ).GetComponent(argType);
                                        if ( componentToAdd != null ) {
                                            UndoUtility.RecordObject(options.unityObjectContext, "Drag Add Item");
                                            list.Add(componentToAdd);
                                            GUI.changed = true;
                                            UndoUtility.SetDirty(options.unityObjectContext);
                                        }
                                        continue;
                                    }
                                }
                                e.Use();
                            }
                        }
                    }
                }

                //Add new default element
                if ( dropRefs.Length == 0 ) {
                    if ( GUILayout.Button("Add Element") ) {
                        UndoUtility.RecordObject(options.unityObjectContext, "Add Item");
                        var o = argType.IsValueType ? argType.CreateObjectUninitialized() : null;
                        if ( listType.IsArray ) {
                            list = ReflectionTools.Resize((System.Array)list, list.Count + 1);
                        } else {
                            list.Add(o);
                        }
                        GUI.changed = true;
                        UndoUtility.SetDirty(options.unityObjectContext);
                        return list;
                    }
                }

            }

            if ( list.Count == 0 ) {
                return list;
            }

            for ( var i = 0; i < list.Count; i++ ) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.BeginVertical();
                GUICallback(i, pickedListIndex == i && pickedList == list);
                GUILayout.EndVertical();
                var lastRect = GUILayoutUtility.GetLastRect();
                var pickRect = Rect.MinMaxRect(lastRect.xMin - 16, lastRect.yMin, lastRect.xMin, lastRect.yMax);
                GUI.color = new Color(1, 1, 1, 0.5f);
                GUI.Label(pickRect, options.blockReorder ? EditorUtils.GetTempContent("■", null, "Re-Ordering Is Disabled") : EditorUtils.GetTempContent("☰"), Styles.centerLabel);
                GUI.color = Color.white;
                if ( options.customItemMenu != null ) {
                    GUILayout.Space(18);
                    var buttonRect = Rect.MinMaxRect(lastRect.xMax, lastRect.yMin, lastRect.xMax + 22, lastRect.yMax + 1);
                    EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
                    GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.grey;
                    if ( GUI.Button(buttonRect, Icons.gearPopupIcon, Styles.centerLabel) ) {
                        UndoUtility.RecordObject(options.unityObjectContext, "Menu Item");
                        options.customItemMenu(i).ShowAsContext();
                        GUI.changed = true;
                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UndoUtility.SetDirty(options.unityObjectContext);
                    }
                    GUI.color = Color.white;
                }
                if ( options.allowRemove ) {
                    GUILayout.Space(20);
                    var buttonRect = Rect.MinMaxRect(lastRect.xMax + 2, lastRect.yMin, lastRect.xMax + 20, lastRect.yMax);
                    if ( GUI.Button(buttonRect, "X") ) {
                        UndoUtility.RecordObject(options.unityObjectContext, "Remove Item");
                        if ( listType.IsArray ) {
                            list = ReflectionTools.Resize((System.Array)list, list.Count - 1);
                        } else {
                            list.RemoveAt(i);
                        }
                        GUI.changed = true;
                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UndoUtility.SetDirty(options.unityObjectContext);
                    }
                }
                GUILayout.EndHorizontal();

                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                if ( !options.blockReorder ) { EditorGUIUtility.AddCursorRect(pickRect, MouseCursor.MoveArrow); }
                var boundRect = GUILayoutUtility.GetLastRect();

                if ( pickRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && !options.blockReorder ) {
                    pickedList = list;
                    pickedListIndex = i;
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    e.Use();
                }

                if ( pickedList == list ) {
                    if ( pickedListIndex == i ) {
                        GUI.Box(boundRect, string.Empty);
                    }

                    if ( pickedListIndex != -1 && pickedListIndex != i && boundRect.Contains(e.mousePosition) ) {

                        var markRect = new Rect(boundRect.x, boundRect.y - 2, boundRect.width, 2);
                        if ( pickedListIndex < i ) {
                            markRect.y = boundRect.yMax - 2;
                        }

                        GUI.DrawTexture(markRect, Texture2D.whiteTexture);
                        if ( e.type == EventType.MouseUp ) {
                            UndoUtility.RecordObject(options.unityObjectContext, "Reorder Item");
                            var pickObj = list[pickedListIndex];
                            list.RemoveAt(pickedListIndex);
                            list.Insert(i, pickObj);
                            GUI.changed = true;
                            UndoUtility.SetDirty(options.unityObjectContext);
                            pickedList = null;
                            pickedListIndex = -1;
                            e.Use();
                        }
                    }
                }

            }

            //just rest in case out of rect
            if ( e.rawType == EventType.MouseUp ) {
                if ( list == pickedList ) {
                    pickedList = null;
                    pickedListIndex = -1;
                }
            }

            return list;
        }
    }
}

#endif