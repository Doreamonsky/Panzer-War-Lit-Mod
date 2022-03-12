#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using System.Linq;

namespace NodeCanvas.Editor
{

    public static class BBParameterEditor
    {

        ///<summary>An editor for BBParameter type to let user choose either a constant value or link to a Blackboard Variable.</summary>
        public static BBParameter ParameterField(string name, BBParameter bbParam, UnityEngine.Object context) {
            return ParameterField(name, bbParam, false, context);
        }

        ///<summary>An editor for BBParameter type to let user choose either a constant value or link to a Blackboard Variable.</summary>
        public static BBParameter ParameterField(string name, BBParameter bbParam, bool blackboardOnly = false, UnityEngine.Object context = null) {
            var info = new InspectedFieldInfo();
            info.unityObjectContext = context;
            return ParameterField(string.IsNullOrEmpty(name) ? GUIContent.none : EditorUtils.GetTempContent(name), bbParam, blackboardOnly, false, info);
        }

        ///<summary>An editor for BBParameter type to let user choose either a constant value or link to a Blackboard Variable.</summary>
        public static BBParameter ParameterField(GUIContent content, BBParameter bbParam, bool blackboardOnly = false, bool required = false, InspectedFieldInfo info = default(InspectedFieldInfo)) {

            if ( bbParam == null ) {
                EditorGUILayout.LabelField(content, EditorUtils.GetTempContent("BBParameter is null"));
                return null;
            }

            //ensure we use blackboard if 'blackboardOnly'
            if ( blackboardOnly && !bbParam.useBlackboard ) {
                bbParam.useBlackboard = true;
            }

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    if ( bbParam.useBlackboard ) {

                        ParameterDropDown(content, bbParam);

                    } else {

                        //we mutate inspected info field and parentInstance, but keep attributes and of course unity object context
                        //this way, attributes work on internal field
                        info.field = bbParam.GetType().RTGetField("_value");
                        info.wrapperInstanceContext = info.parentInstanceContext;
                        info.parentInstanceContext = bbParam;
                        if ( info.attributes != null ) {
                            info.attributes = info.attributes.Where(a => !( a is DrawerAttribute ) || !( (DrawerAttribute)a ).isDecorator).ToArray();
                        }
                        GUILayout.BeginVertical();
                        bbParam.value = EditorUtils.ReflectedFieldInspector(content, bbParam.value, bbParam.varType, info);
                        GUILayout.EndVertical();
                    }

                    if ( !blackboardOnly ) {
                        bbParam.useBlackboard = EditorGUILayout.Toggle(bbParam.useBlackboard, EditorStyles.radioButton, GUILayout.Width(18));
                    }
                }
                GUILayout.EndHorizontal();


                //warn/error marks
                if ( content != GUIContent.none ) {
                    if ( bbParam.varRef == null && !string.IsNullOrEmpty(bbParam.name) ) {
                        if ( !bbParam.isPresumedDynamic ) { EditorUtils.MarkLastFieldError($"Missing Variable. Type of '{bbParam.varType.FriendlyName()}'"); }
                        if ( bbParam.isPresumedDynamic ) { EditorUtils.MarkLastFieldWarning($"Dynamic Variable. Type of '{bbParam.varType.FriendlyName()}'"); }
                    } else {
                        if ( required && bbParam.isNone ) { EditorUtils.MarkLastFieldError("An instance is required"); }
                        if ( required && !string.IsNullOrEmpty(bbParam.name) && !bbParam.isDefined && bbParam.isNull ) { EditorUtils.MarkLastFieldWarning("An instance is required but currently resolves to null. If it is set in runtime you can ignore this warning."); }
                    }
                }


                string textInfo = null;
                if ( bbParam.useBlackboard ) {
                    if ( bbParam.bb == null ) {
                        textInfo = "<i>No current Blackboard reference</i>";
                    } else
                    if ( bbParam.isNone ) {
                        textInfo = "Select a '" + bbParam.varType.FriendlyName() + "' Assignable Blackboard Variable";
                    } else
                    if ( bbParam.varRef != null && bbParam.varType != bbParam.refType ) {
                        var setPossible = bbParam.varRef.CanConvertFrom(bbParam.varType);
                        textInfo = string.Format("AutoConvert: ({0} ➲ {1}){2}", bbParam.refType.FriendlyName(), bbParam.varType.FriendlyName(), setPossible ? string.Empty : " [GET ONLY]");
                    }
                }

                if ( textInfo != null ) {
                    GUI.color = Color.white.WithAlpha(0.5f);
                    GUILayout.BeginVertical(GUI.skin.textField);
                    GUILayout.Label(textInfo, GUILayout.Width(0), GUILayout.ExpandWidth(true));
                    GUILayout.EndVertical();
                    GUILayout.Space(2);
                    GUI.color = Color.white;
                }
            }
            GUILayout.EndVertical();
            return bbParam;
        }

        //...
        static void ParameterDropDown(GUIContent content, BBParameter bbParam) {

            if ( bbParam.varRef == null && !string.IsNullOrEmpty(bbParam.name) ) {
                bbParam.name = EditorGUILayout.DelayedTextField(content, bbParam.name);
                GUI.backgroundColor = Color.white.WithAlpha(0.5f);
                if ( bbParam.bb != null && GUILayout.Button(EditorUtils.GetTempContent(Icons.plusIcon, "Promote To Variable"), Styles.centerLabel, GUILayout.Width(18), GUILayout.Height(16)) ) {
                    var menu = new GenericMenu();
                    foreach ( var bb in bbParam.bb.GetAllParents(true).Reverse() ) {
                        menu.AddItem(new GUIContent($"Promote Variable in '{bb.identifier}' Blackboard"), false, () =>
                       {
                           UndoUtility.RecordObject(bb.unityContextObject, "Promote Variable");
                           bbParam.PromoteToVariable(bb);
                           UndoUtility.SetDirty(bb.unityContextObject);
                       });
                    }
                    menu.ShowAsContext();
                }
                GUI.backgroundColor = Color.white;
                return;
            }

            GUI.color = new Color(0.9f, 0.9f, 1f, 1f);
            EditorGUILayout.PrefixLabel(content);
            var rect = EditorGUILayout.GetControlRect(false);
            rect.xMin += 2; // O.o
            var cachedContentTextForNew = string.IsNullOrEmpty(content.text) ? string.Format("new{0}", bbParam.varType.Name) : content.text;
            var displayName = bbParam.isNone ? "[NONE]" : bbParam.name;
            var pop = EditorGUI.DropdownButton(rect, EditorUtils.GetTempContent(displayName), FocusType.Passive);
            GUI.color = Color.white;
            if ( pop ) {

                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("[NONE]"), false, () => { bbParam.name = string.Empty; });

                foreach ( var globalBB in GlobalBlackboard.GetAll() ) {
                    var globalVars = globalBB.GetVariables(bbParam.varType);
                    foreach ( var variable in globalVars ) {
                        menu.AddItem(new GUIContent(globalBB.identifier + "/" + variable.name), bbParam.targetVariableID == variable.ID, () => { bbParam.SetTargetVariable(globalBB, variable); });
                    }

                    menu.AddSeparator(globalBB.identifier + "/");

                    menu.AddItem(new GUIContent(globalBB.identifier + "/(Create New)"), false, () =>
                    {
                        UndoUtility.RecordObject(( globalBB as IBlackboard ).unityContextObject, "New Variable");
                        if ( globalBB.AddVariable(cachedContentTextForNew, bbParam.varType) != null ) {
                            bbParam.name = globalBB.identifier + "/" + cachedContentTextForNew;
                        } else { bbParam.name = null; }
                        UndoUtility.SetDirty(( globalBB as IBlackboard ).unityContextObject);
                    });

                }

                if ( bbParam.bb != null ) {
                    foreach ( var actualBB in bbParam.bb.GetAllParents(true).Reverse() ) {
                        var variables = actualBB.variables.Values.Where(v => v.CanConvertTo(bbParam.varType));
                        foreach ( var variable in variables ) {
                            menu.AddItem(new GUIContent(actualBB.identifier + "/" + variable.name), bbParam.targetVariableID == variable.ID, () => { bbParam.SetTargetVariable(actualBB, variable); });
                        }

                        menu.AddSeparator(actualBB.identifier + "/");

                        menu.AddItem(new GUIContent(actualBB.identifier + "/(Create New)"), false, () =>
                        {
                            UndoUtility.RecordObject(actualBB.unityContextObject, "New Variable");
                            if ( actualBB.AddVariable(cachedContentTextForNew, bbParam.varType) != null ) {
                                bbParam.name = cachedContentTextForNew;
                            } else { bbParam.name = null; }
                            UndoUtility.SetDirty(actualBB.unityContextObject);
                        });
                    }
                }

                menu.AddSeparator(string.Empty);

                menu.AddItem(new GUIContent("(DynamicVar)"), false, () => { bbParam.name = "_"; });

                menu.DropDown(rect);
            }
        }

    }
}

#endif