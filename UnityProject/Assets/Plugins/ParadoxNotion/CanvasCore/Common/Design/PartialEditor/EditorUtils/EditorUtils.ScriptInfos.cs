#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace ParadoxNotion.Design
{

    //reflection meta info
    partial class EditorUtils
    {

        [InitializeOnLoadMethod]
        static void Initialize_ScriptInfos() {
            TypePrefs.onPreferredTypesChanged -= FlushScriptInfos;
            TypePrefs.onPreferredTypesChanged += FlushScriptInfos;
        }

        //Flush script infos cache
        public static void FlushScriptInfos() {
            cachedInfos = null;
        }

        //For gathering script/type meta-information
        public struct ScriptInfo
        {
            public bool isValid { get; private set; }

            public Type originalType;
            public string originalName;
            public string originalCategory;

            public Type type;
            public string name;
            public string category;
            public int priority;

            public ScriptInfo(Type type, string name, string category, int priority) {
                this.isValid = true;
                this.originalType = type;
                this.originalName = name;
                this.originalCategory = category;

                this.type = type;
                this.name = name;
                this.category = category;
                this.priority = priority;
            }
        }

        ///<summary>Get a list of ScriptInfos of the baseType excluding: the base type, abstract classes, Obsolete classes and those with the DoNotList attribute, categorized as a list of ScriptInfo</summary>
        private static Dictionary<Type, List<ScriptInfo>> cachedInfos;
        public static List<ScriptInfo> GetScriptInfosOfType(Type baseType) {

            if ( cachedInfos == null ) { cachedInfos = new Dictionary<Type, List<ScriptInfo>>(); }

            List<ScriptInfo> infosResult;
            if ( cachedInfos.TryGetValue(baseType, out infosResult) ) {
                return infosResult.ToList();
            }

            infosResult = new List<ScriptInfo>();

            var subTypes = baseType.IsGenericTypeDefinition ? new Type[] { baseType } : ReflectionTools.GetImplementationsOf(baseType);
            foreach ( var subType in subTypes ) {

                if ( subType.IsAbstract || subType.RTIsDefined(typeof(DoNotListAttribute), true) || subType.RTIsDefined(typeof(ObsoleteAttribute), true) ) {
                    continue;
                }

                var isGeneric = subType.IsGenericTypeDefinition && subType.RTGetGenericArguments().Length == 1;
                var scriptName = subType.FriendlyName().SplitCamelCase();
                var scriptCategory = string.Empty;
                var scriptPriority = 0;

                var nameAttribute = subType.RTGetAttribute<NameAttribute>(true);
                if ( nameAttribute != null ) {
                    scriptPriority = nameAttribute.priority;
                    scriptName = nameAttribute.name;
                    if ( isGeneric && !scriptName.EndsWith("<T>") ) {
                        scriptName += " (T)";
                    }
                }

                var categoryAttribute = subType.RTGetAttribute<CategoryAttribute>(true);
                if ( categoryAttribute != null ) {
                    scriptCategory = categoryAttribute.category;
                }

                var info = new ScriptInfo(subType, scriptName, scriptCategory, scriptPriority);

                //add the generic types based on constrains and prefered types list
                if ( isGeneric ) {
                    var exposeAsBaseDefinition = NodeCanvas.Editor.Prefs.collapseGenericTypes || subType.RTIsDefined<ExposeAsDefinitionAttribute>(true);
                    if ( !exposeAsBaseDefinition ) {
                        var typesToWrap = TypePrefs.GetPreferedTypesList(true);
                        foreach ( var t in typesToWrap ) {
                            infosResult.Add(info.MakeGenericInfo(t, string.Format("/{0}/{1}", info.name, t.NamespaceToPath())));
                            infosResult.Add(info.MakeGenericInfo(typeof(List<>).MakeGenericType(t), string.Format("/{0}/{1}{2}", info.name, TypePrefs.LIST_MENU_STRING, t.NamespaceToPath()), -1));
                            infosResult.Add(info.MakeGenericInfo(typeof(Dictionary<,>).MakeGenericType(typeof(string), t), string.Format("/{0}/{1}{2}", info.name, TypePrefs.DICT_MENU_STRING, t.NamespaceToPath()), -2));
                        }
                        continue;
                    }
                }

                infosResult.Add(info);
            }

            infosResult = infosResult
            .Where(s => s.isValid)
            .OrderBy(s => s.originalCategory)
            .ThenBy(s => s.priority * -1)
            .ThenBy(s => s.originalName)
            .ToList();
            cachedInfos[baseType] = infosResult;
            return infosResult;
        }

        ///<summary>Makes and returns a closed generic ScriptInfo for targetType out of an existing ScriptInfo</summary>
        public static ScriptInfo MakeGenericInfo(this ScriptInfo info, Type targetType, string subCategory = null, int priorityShift = 0) {
            if ( !info.isValid || !info.originalType.IsGenericTypeDefinition ) {
                return default(ScriptInfo);
            }

            if ( info.originalType.TryMakeGeneric(targetType, out Type genericType) ) {
                var genericCategory = info.originalCategory + subCategory;
                var genericName = info.originalName.Replace("(T)", string.Format("({0})", targetType.FriendlyName()));
                var newInfo = new ScriptInfo(genericType, genericName, genericCategory, info.priority + priorityShift);
                newInfo.originalType = info.originalType;
                newInfo.originalName = info.originalName;
                newInfo.originalCategory = info.originalCategory;
                return newInfo;
            }
            return default(ScriptInfo);
        }

        ///<summary>Not really. Only for purposes of menus usage.</summary>
        static string NamespaceToPath(this Type type) {
            if ( type == null ) { return string.Empty; }
            return string.IsNullOrEmpty(type.Namespace) ? "No Namespace" : type.Namespace.Split('.').First();
        }
    }
}

#endif