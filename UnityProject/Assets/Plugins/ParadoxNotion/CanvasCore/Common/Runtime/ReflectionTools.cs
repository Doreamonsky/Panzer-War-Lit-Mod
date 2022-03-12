using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Logger = ParadoxNotion.Services.Logger;

namespace ParadoxNotion
{

    ///<summary>Reflection utility and extention methods</summary>
    public static class ReflectionTools
    {
        public const BindingFlags FLAGS_ALL = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const BindingFlags FLAGS_ALL_DECLARED = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        ///----------------------------------------------------------------------------------------------

        private static Assembly[] _loadedAssemblies;
        private static Type[] _allTypes;
        private static object[] _tempArgs;
        private static Dictionary<string, Type> _typesMap;
        private static Dictionary<Type, Type[]> _subTypesMap;
        private static Dictionary<MethodBase, MethodType> _methodSpecialType;
        private static Dictionary<Type, string> _typeFriendlyName;
        private static Dictionary<Type, string> _typeFriendlyNameCompileSafe;
        private static Dictionary<MethodBase, string> _methodSignatures;
        private static Dictionary<Type, ConstructorInfo[]> _typeConstructors;
        private static Dictionary<Type, MethodInfo[]> _typeMethods;
        private static Dictionary<Type, FieldInfo[]> _typeFields;
        private static Dictionary<Type, PropertyInfo[]> _typeProperties;
        private static Dictionary<Type, EventInfo[]> _typeEvents;
        private static Dictionary<Type, object[]> _typeAttributes;
        private static Dictionary<MemberInfo, object[]> _memberAttributes;
        private static Dictionary<MemberInfo, bool> _obsoleteCache;
        private static Dictionary<Type, MethodInfo[]> _typeExtensions;
        private static Dictionary<Type, Type[]> _genericArgsTypeCache;
        private static Dictionary<MethodInfo, Type[]> _genericArgsMathodCache;

        static ReflectionTools() { FlushMem(); }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        public static void FlushMem() {
            _loadedAssemblies = null;
            _allTypes = null;
            _tempArgs = new object[1];
            _typesMap = new Dictionary<string, Type>();
            _subTypesMap = new Dictionary<Type, Type[]>();
            _methodSpecialType = new Dictionary<MethodBase, MethodType>();
            _typeFriendlyName = new Dictionary<Type, string>();
            _typeFriendlyNameCompileSafe = new Dictionary<Type, string>();
            _methodSignatures = new Dictionary<MethodBase, string>();
            _typeConstructors = new Dictionary<Type, ConstructorInfo[]>();
            _typeMethods = new Dictionary<Type, MethodInfo[]>();
            _typeFields = new Dictionary<Type, FieldInfo[]>();
            _typeProperties = new Dictionary<Type, PropertyInfo[]>();
            _typeEvents = new Dictionary<Type, EventInfo[]>();
            _typeAttributes = new Dictionary<Type, object[]>();
            _memberAttributes = new Dictionary<MemberInfo, object[]>();
            _obsoleteCache = new Dictionary<MemberInfo, bool>();
            _typeExtensions = new Dictionary<Type, MethodInfo[]>();
            _genericArgsTypeCache = new Dictionary<Type, Type[]>();
            _genericArgsMathodCache = new Dictionary<MethodInfo, Type[]>();
        }

        ///----------------------------------------------------------------------------------------------

        private static Assembly[] loadedAssemblies {
            get { return _loadedAssemblies != null ? _loadedAssemblies : _loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies(); }
        }

        //Alternative to Type.GetType to work with FullName instead of AssemblyQualifiedName when looking up a type by string
        //This also handles Generics and their arguments, assembly changes and namespace changes to some extend.
        public static Type GetType(string typeFullName) { return GetType(typeFullName, false, null); }
        public static Type GetType(string typeFullName, Type fallbackAssignable) { return GetType(typeFullName, true, fallbackAssignable); }
        public static Type GetType(string typeFullName, bool fallbackNoNamespace = false, Type fallbackAssignable = null) {

            if ( string.IsNullOrEmpty(typeFullName) ) {
                return null;
            }

            Type type = null;
            if ( _typesMap.TryGetValue(typeFullName, out type) ) {
                return type;
            }

            //direct look up
            type = GetTypeDirect(typeFullName);
            if ( type != null ) {
                return _typesMap[typeFullName] = type;
            }

            //handle generics now
            type = TryResolveGenericType(typeFullName, fallbackNoNamespace, fallbackAssignable);
            if ( type != null ) {
                // Logger.LogWarning(string.Format("Type with name '{0}' was resolved using a fallback resolution (Generics).", typeFullName), "Type Request");
                return _typesMap[typeFullName] = type;
            }

            //make use of DeserializeFromAttribute
            type = TryResolveDeserializeFromAttribute(typeFullName);
            if ( type != null ) {
                // Logger.LogWarning(string.Format("Type with name '{0}' was resolved using a fallback resolution (DeserializeFromAttribute).", typeFullName), "Type Request");
                return _typesMap[typeFullName] = type;
            }

            //get type regardless namespace
            if ( fallbackNoNamespace ) {
                type = TryResolveWithoutNamespace(typeFullName, fallbackAssignable);
                if ( type != null ) {
                    // Logger.LogWarning(string.Format("Type with name '{0}' was resolved using a fallback resolution (NoNamespace).", typeFullName), "Type Request");
                    return _typesMap[typeFullName] = type;
                }
            }

            Logger.LogError(string.Format("Type with name '{0}' could not be resolved.", typeFullName), "Type Request");
            return _typesMap[typeFullName] = null;
        }

        //direct type look up with it's FullName
        static Type GetTypeDirect(string typeFullName) {
            var type = Type.GetType(typeFullName);
            if ( type != null ) {
                return type;
            }

            for ( var i = 0; i < loadedAssemblies.Length; i++ ) {
                var asm = loadedAssemblies[i];
                try { type = asm.GetType(typeFullName); }
                catch { continue; }
                if ( type != null ) {
                    return type;
                }
            }

            return null;
        }

        //Resolve generic types by their .FullName or .ToString
        //Remark: a generic's type .FullName returns a string where it's arguments only are instead printed as AssemblyQualifiedName.
        static Type TryResolveGenericType(string typeFullName, bool fallbackNoNamespace = false, Type fallbackAssignable = null) {

            //ensure that it is a generic type implementation, not a definition
            if ( typeFullName.Contains('`') == false || typeFullName.Contains('[') == false ) {
                return null;
            }

            try //big try/catch block cause maybe there is a bug. Hopefully not.
            {
                var quoteIndex = typeFullName.IndexOf('`');
                var genericTypeDefName = typeFullName.Substring(0, quoteIndex + 2);
                var genericTypeDef = GetType(genericTypeDefName, fallbackNoNamespace, fallbackAssignable);
                if ( genericTypeDef == null ) {
                    return null;
                }

                int argCount = Convert.ToInt32(typeFullName.Substring(quoteIndex + 1, 1));
                var content = typeFullName.Substring(quoteIndex + 2, typeFullName.Length - quoteIndex - 2);
                string[] split = null;
                if ( content.StartsWith("[[") ) { //this means that assembly qualified name is contained. Name was generated with FullName.
                    var startIndex = typeFullName.IndexOf("[[") + 2;
                    var endIndex = typeFullName.LastIndexOf("]]");
                    content = typeFullName.Substring(startIndex, endIndex - startIndex);
                    split = content.Split(new string[] { "],[" }, argCount, StringSplitOptions.RemoveEmptyEntries);
                } else { //this means that the name was generated with type.ToString().
                    var startIndex = typeFullName.IndexOf('[') + 1;
                    var endIndex = typeFullName.LastIndexOf(']');
                    content = typeFullName.Substring(startIndex, endIndex - startIndex);
                    split = content.Split(new char[] { ',' }, argCount, StringSplitOptions.RemoveEmptyEntries);
                }

                var argTypes = new Type[argCount];
                for ( var i = 0; i < split.Length; i++ ) {
                    var subName = split[i];
                    if ( !subName.Contains('`') && subName.Contains(',') ) { //remove assembly info since we work with FullName, but only if it's not yet another generic.
                        subName = subName.Substring(0, subName.IndexOf(','));
                    }

                    var argType = GetType(subName, true /*fallback no namespace*/);
                    if ( argType == null ) {
                        return null;
                    }
                    argTypes[i] = argType;
                }

                return genericTypeDef.RTMakeGenericType(argTypes);
            }

            catch ( Exception e ) {
                ParadoxNotion.Services.Logger.LogException(e, "Type Request Bug. Please report. :-(");
                return null;
            }
        }

        //uterly slow, but only happens when we have a null type
        static Type TryResolveDeserializeFromAttribute(string typeName) {
            var allTypes = GetAllTypes(true);
            for ( var i = 0; i < allTypes.Length; i++ ) {
                var t = allTypes[i];
                var att = t.GetCustomAttribute(typeof(Serialization.DeserializeFromAttribute), false) as Serialization.DeserializeFromAttribute;
                if ( att != null && att.previousTypeFullName == typeName ) {
                    return t;
                }
            }
            return null;
        }

        //fallback type look up with it's FullName. This is slow.
        static Type TryResolveWithoutNamespace(string typeName, Type fallbackAssignable = null) {

            //dont handle generic implementations this way (still handles definitions though).
            if ( typeName.Contains('`') && typeName.Contains('[') ) {
                return null;
            }

            //remove assembly info if any
            if ( typeName.Contains(',') ) {
                typeName = typeName.Substring(0, typeName.IndexOf(','));
            }

            //ensure strip namespace
            if ( typeName.Contains('.') ) {
                var dotIndex = typeName.LastIndexOf('.') + 1;
                typeName = typeName.Substring(dotIndex, typeName.Length - dotIndex);
            }

            //check all types
            var allTypes = GetAllTypes(true);
            for ( var i = 0; i < allTypes.Length; i++ ) {
                var t = allTypes[i];
                if ( t.Name == typeName && ( fallbackAssignable == null || fallbackAssignable.RTIsAssignableFrom(t) ) ) {
                    return t;
                }
            }
            return null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Get every single type in loaded assemblies</summary>
        public static Type[] GetAllTypes(bool includeObsolete) {
            if ( _allTypes != null ) {
                return _allTypes;
            }

            var result = new List<Type>();
            for ( var i = 0; i < loadedAssemblies.Length; i++ ) {
                var asm = loadedAssemblies[i];
                try { result.AddRange(asm.GetExportedTypes().Where(t => includeObsolete == true || !t.RTIsDefined<System.ObsoleteAttribute>(false))); }
                catch { continue; }
            }
            return _allTypes = result.OrderBy(t => t.Namespace).ThenBy(t => t.FriendlyName()).ToArray();
        }

        ///<summary>Get a collection of types assignable to provided type, excluding Abstract types</summary>
        public static Type[] GetImplementationsOf(Type baseType) {

            Type[] result = null;
            if ( _subTypesMap.TryGetValue(baseType, out result) ) {
                return result;
            }

            var temp = new List<Type>();
            var allTypes = GetAllTypes(false);
            for ( var i = 0; i < allTypes.Length; i++ ) {
                var type = allTypes[i];
                if ( baseType.RTIsAssignableFrom(type) && !type.RTIsAbstract() ) {
                    temp.Add(type);
                }
            }
            return _subTypesMap[baseType] = temp.ToArray();
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns an object[] with a single element, that can for example be used as method invocation args</summary>
        public static object[] SingleTempArgsArray(object arg) {
            _tempArgs[0] = arg;
            return _tempArgs;
        }

        ///----------------------------------------------------------------------------------------------

        //Method operator special name to friendly name map
        public readonly static Dictionary<string, string> op_FriendlyNamesLong = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"op_Equality", "Equal"},
            {"op_Inequality", "Not Equal"},
            {"op_GreaterThan", "Greater"},
            {"op_LessThan", "Less"},
            {"op_GreaterThanOrEqual", "Greater Or Equal"},
            {"op_LessThanOrEqual", "Less Or Equal"},
            {"op_Addition", "Add"},
            {"op_Subtraction", "Subtract"},
            {"op_Division", "Divide"},
            {"op_Multiply", "Multiply"},
            {"op_UnaryNegation", "Negate"},
            {"op_UnaryPlus", "Positive"},
            {"op_Increment", "Increment"},
            {"op_Decrement", "Decrement"},
            {"op_LogicalNot", "NOT"},
            {"op_OnesComplement", "Complements"},
            {"op_False", "FALSE"},
            {"op_True", "TRUE"},
            {"op_Modulus", "MOD"},
            {"op_BitwiseAnd", "AND"},
            {"op_BitwiseOR", "OR"},
            {"op_LeftShift", "Shift Left"},
            {"op_RightShift", "Shift Right"},
            {"op_ExclusiveOr", "XOR"},
            {"op_Implicit", "Convert"},
            {"op_Explicit", "Convert"},
        };

        //Method operator special name to friendly name map
        public readonly static Dictionary<string, string> op_FriendlyNamesShort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"op_Equality", "="},
            {"op_Inequality", "≠"},
            {"op_GreaterThan", ">"},
            {"op_LessThan", "<"},
            {"op_GreaterThanOrEqual", "≥"},
            {"op_LessThanOrEqual", "≤"},
            {"op_Addition", "+"},
            {"op_Subtraction", "-"},
            {"op_Division", "÷"},
            {"op_Multiply", "×"},
            {"op_UnaryNegation", "Negate"},
            {"op_UnaryPlus", "Positive"},
            {"op_Increment", "++"},
            {"op_Decrement", "--"},
            {"op_LogicalNot", "NOT"},
            {"op_OnesComplement", "~"},
            {"op_False", "FALSE"},
            {"op_True", "TRUE"},
            {"op_Modulus", "MOD"},
            {"op_BitwiseAnd", "AND"},
            {"op_BitwiseOR", "OR"},
            {"op_LeftShift", "<<"},
            {"op_RightShift", ">>"},
            {"op_ExclusiveOr", "XOR"},
            {"op_Implicit", "Convert"},
            {"op_Explicit", "Convert"},
        };

        ///<summary>Operator C# to friendly aliases</summary>
        public readonly static Dictionary<string, string> op_CSharpAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"!=", "≠"},
            {">=", "≥"},
            {"<=", "≤"},
            {"/", "÷"},
            {"*", "×"},
        };

        public const string METHOD_SPECIAL_NAME_GET = "get_";
        public const string METHOD_SPECIAL_NAME_SET = "set_";
        public const string METHOD_SPECIAL_NAME_ADD = "add_";
        public const string METHOD_SPECIAL_NAME_REMOVE = "remove_";
        public const string METHOD_SPECIAL_NAME_OP = "op_";

        public enum MethodType
        {
            Normal = 0,
            PropertyAccessor = 1,
            Event = 2,
            Operator = 3,
        }

        ///<summary>Returns the type of method case of accessor, operator or event.</summary>
        public static MethodType GetMethodSpecialType(this MethodBase method) {

            MethodType methodType;
            if ( _methodSpecialType.TryGetValue(method, out methodType) ) {
                return methodType;
            }

            var name = method.Name;
            if ( method.IsSpecialName ) {
                if ( name.StartsWith(METHOD_SPECIAL_NAME_GET) || name.StartsWith(METHOD_SPECIAL_NAME_SET) ) {
                    return _methodSpecialType[method] = MethodType.PropertyAccessor;
                }
                if ( name.StartsWith(METHOD_SPECIAL_NAME_ADD) || name.StartsWith(METHOD_SPECIAL_NAME_REMOVE) ) {
                    return _methodSpecialType[method] = MethodType.Event;
                }
                if ( name.StartsWith(METHOD_SPECIAL_NAME_OP) ) {
                    return _methodSpecialType[method] = MethodType.Operator;
                }
            }
            return _methodSpecialType[method] = MethodType.Normal;
        }

        ///<summary>Get a friendly name for the type</summary>
        public static string FriendlyName(this Type t, bool compileSafe = false) {

            if ( t == null ) {
                return null;
            }

            if ( !compileSafe && t.IsByRef ) {
                t = t.GetElementType();
            }

            if ( !compileSafe && t == typeof(UnityEngine.Object) ) {
                return "UnityObject";
            }

            string s;
            if ( !compileSafe && _typeFriendlyName.TryGetValue(t, out s) ) {
                return s;
            }

            if ( compileSafe && _typeFriendlyNameCompileSafe.TryGetValue(t, out s) ) {
                return s;
            }

            s = compileSafe ? t.FullName : t.Name;
            if ( !compileSafe ) {
                if ( s == "Single" ) { s = "Float"; }
                if ( s == "Single[]" ) { s = "Float[]"; }
                if ( s == "Int32" ) { s = "Integer"; }
                if ( s == "Int32[]" ) { s = "Integer[]"; }
            }

            if ( t.RTIsGenericParameter() ) {
                s = "T";
            }

            if ( t.RTIsGenericType() ) {
                s = compileSafe && !string.IsNullOrEmpty(t.Namespace) ? t.Namespace + "." + t.Name : t.Name;
                var args = t.RTGetGenericArguments();
                if ( args.Length != 0 ) {

                    s = s.Replace("`" + args.Length.ToString(), "");

                    s += compileSafe ? "<" : " (";
                    for ( var i = 0; i < args.Length; i++ ) {
                        s += ( i == 0 ? "" : ", " ) + args[i].FriendlyName(compileSafe);
                    }
                    s += compileSafe ? ">" : ")";
                }
            }

            if ( compileSafe ) {
                return _typeFriendlyNameCompileSafe[t] = s;
            }
            return _typeFriendlyName[t] = s;
        }

        ///<summary>Get a friendly name for member info</summary>
        public static string FriendlyName(this MemberInfo info) {
            if ( info == null ) { return null; }
            if ( info is Type ) { return FriendlyName((Type)info); }
            var type = info.ReflectedType.FriendlyName();
            return type + '.' + info.Name;
        }

        ///<summary>Get a friendly name of a methd which is the case for when it's a special name.</summary>
        public static string FriendlyName(this MethodBase method) { var specialType = MethodType.Normal; return method.FriendlyName(out specialType); }
        public static string FriendlyName(this MethodBase method, out MethodType specialNameType) {
            specialNameType = MethodType.Normal;
            var methodName = method.Name;
            if ( method.IsSpecialName ) {
                if ( methodName.StartsWith(METHOD_SPECIAL_NAME_GET) ) {
                    methodName = "Get " + methodName.Substring(METHOD_SPECIAL_NAME_GET.Length).CapitalizeFirst();
                    specialNameType = MethodType.PropertyAccessor;
                    return methodName;
                }
                if ( methodName.StartsWith(METHOD_SPECIAL_NAME_SET) ) {
                    methodName = "Set " + methodName.Substring(METHOD_SPECIAL_NAME_SET.Length).CapitalizeFirst();
                    specialNameType = MethodType.PropertyAccessor;
                    return methodName;
                }
                if ( methodName.StartsWith(METHOD_SPECIAL_NAME_ADD) ) {
                    methodName = methodName.Substring(METHOD_SPECIAL_NAME_ADD.Length) + " +=";
                    specialNameType = MethodType.Event;
                    return methodName;
                }
                if ( methodName.StartsWith(METHOD_SPECIAL_NAME_REMOVE) ) {
                    methodName = methodName.Substring(METHOD_SPECIAL_NAME_REMOVE.Length) + " -=";
                    specialNameType = MethodType.Event;
                    return methodName;
                }
                if ( methodName.StartsWith(METHOD_SPECIAL_NAME_OP) ) {
                    op_FriendlyNamesLong.TryGetValue(methodName, out methodName);
                    specialNameType = MethodType.Operator;
                    return methodName;
                }
            }
            return methodName;
        }

        ///<summary>Get a friendly full signature string name for a method</summary>
        public static string SignatureName(this MethodBase method) {
            string sig = null;
            if ( _methodSignatures.TryGetValue(method, out sig) ) {
                return sig;
            }

            var specialType = MethodType.Normal;
            var methodName = method.FriendlyName(out specialType);
            var parameters = method.GetParameters();
            if ( method is ConstructorInfo ) {
                sig = string.Format("new {0} (", method.DeclaringType.FriendlyName());
            } else {
                sig = string.Format("{0}{1} (", method.IsStatic && specialType != MethodType.Operator ? "static " : "", methodName);
            }
            for ( var i = 0; i < parameters.Length; i++ ) {
                var p = parameters[i];
                if ( p.IsParams(parameters) ) {
                    sig += "params ";
                }
                sig += ( p.ParameterType.IsByRef ? ( p.IsOut ? "out " : "ref " ) : "" ) + p.ParameterType.FriendlyName() + ( i < parameters.Length - 1 ? ", " : "" );
            }
            if ( method is MethodInfo ) {
                sig += ") : " + ( method as MethodInfo ).ReturnType.FriendlyName();
            } else {
                sig += ")";
            }
            return _methodSignatures[method] = sig;
        }

        ///<summary>for 1 arg only</summary>
        public static string FriendlyTypeName(string fullName) {
            if ( fullName.Contains("`1") ) {
                var argName = fullName.GetStringWithinInner('[', ',');
                var baseName = fullName.GetStringWithinInner('.', '`');
                return string.Format("{0}({1})", baseName, argName);
            }
            if ( fullName.Contains('.') ) {
                var idx = fullName.LastIndexOf('.') + 1;
                return fullName.Substring(idx, fullName.Length - idx);
            }
            return fullName;
        }

        ///----------------------------------------------------------------------------------------------

        public static Type RTReflectedOrDeclaredType(this MemberInfo member) {
            return member.ReflectedType != null ? member.ReflectedType : member.DeclaringType;
        }

        public static bool RTIsAssignableFrom(this Type type, Type other) {
            return type.IsAssignableFrom(other);
        }

        public static bool RTIsAssignableTo(this Type type, Type other) {
            return other.RTIsAssignableFrom(type);
        }

        public static bool RTIsAbstract(this Type type) {
            return type.IsAbstract;
        }

        public static bool RTIsValueType(this Type type) {
            return type.IsValueType;
        }

        public static bool RTIsArray(this Type type) {
            return type.IsArray;
        }

        public static bool RTIsInterface(this Type type) {
            return type.IsInterface;
        }

        public static bool RTIsSubclassOf(this Type type, Type other) {
            return type.IsSubclassOf(other);
        }

        public static bool RTIsGenericParameter(this Type type) {
            return type.IsGenericParameter;
        }

        public static bool RTIsGenericType(this Type type) {
            return type.IsGenericType;
        }

        public static MethodInfo RTGetGetMethod(this PropertyInfo prop) {
            return prop.GetGetMethod();
        }

        public static MethodInfo RTGetSetMethod(this PropertyInfo prop) {
            return prop.GetSetMethod();
        }

        public static MethodInfo RTGetDelegateMethodInfo(this Delegate del) {
            return del.Method;
        }

        public static Type RTMakeGenericType(this Type type, params Type[] typeArgs) {
            return type.MakeGenericType(typeArgs);
        }

        public static Type[] RTGetGenericArguments(this Type type) {
            Type[] result = null;
            if ( _genericArgsTypeCache.TryGetValue(type, out result) ) {
                return result;
            }
            return _genericArgsTypeCache[type] = result = type.GetGenericArguments();
        }

        public static Type[] RTGetGenericArguments(this MethodInfo method) {
            Type[] result = null;
            if ( _genericArgsMathodCache.TryGetValue(method, out result) ) {
                return result;
            }
            return _genericArgsMathodCache[method] = result = method.GetGenericArguments();
        }

        public static Type[] RTGetEmptyTypes() {
            return Type.EmptyTypes;
        }

        public static Type RTGetElementType(this Type type) {
            if ( type == null ) return null;
            return type.GetElementType();
        }

        public static bool RTIsByRef(this Type type) {
            if ( type == null ) return false;
            return type.IsByRef;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Create object of type</summary>
        public static object CreateObject(this Type type) {
            if ( type == null ) return null;
            return Activator.CreateInstance(type);
        }

        ///<summary>Create uninitialized object of type</summary>
		public static object CreateObjectUninitialized(this Type type) {
            if ( type == null ) return null;
            return FormatterServices.GetUninitializedObject(type);
        }

        ///----------------------------------------------------------------------------------------------

        public static ConstructorInfo RTGetDefaultConstructor(this Type type) {
            var ctors = type.RTGetConstructors();
            for ( var i = 0; i < ctors.Length; i++ ) {
                if ( ctors[i].GetParameters().Length == 0 ) {
                    return ctors[i];
                }
            }
            return null;
        }

        public static ConstructorInfo RTGetConstructor(this Type type, Type[] paramTypes) {
            var ctors = type.RTGetConstructors();
            for ( var i = 0; i < ctors.Length; i++ ) {
                var ctor = ctors[i];
                var parameters = ctor.GetParameters();
                if ( parameters.Length != paramTypes.Length ) {
                    continue;
                }
                var sequenceEquals = true;
                for ( var j = 0; j < parameters.Length; j++ ) {
                    if ( parameters[j].ParameterType != paramTypes[j] ) {
                        sequenceEquals = false;
                        break;
                    }
                }
                if ( sequenceEquals ) {
                    return ctor;
                }
            }
            return null;
        }

        ///----------------------------------------------------------------------------------------------

        //Utility used bellow
        private static bool MemberResolvedFromDeserializeAttribute(MemberInfo member, string targetName) {
            var att = member.RTGetAttribute<Serialization.DeserializeFromAttribute>(true);
            return att != null && att.previousTypeFullName == targetName;
        }

        public static MethodInfo RTGetMethod(this Type type, string name) {
            var methods = type.RTGetMethods();
            for ( var i = 0; i < methods.Length; i++ ) {
                var m = methods[i];
                if ( m.Name == name || MemberResolvedFromDeserializeAttribute(m, name) ) {
                    return m;
                }
            }
            Logger.LogError(string.Format("Method with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()), "Method Request");
            return null;
        }

        public static MethodInfo RTGetMethod(this Type type, string name, Type[] paramTypes, Type returnType = null, Type[] genericArgumentTypes = null) {
            var methods = type.RTGetMethods();
            for ( var i = 0; i < methods.Length; i++ ) {
                var m = methods[i];

                if ( m.Name == name || MemberResolvedFromDeserializeAttribute(m, name) ) {

                    if ( genericArgumentTypes != null && !m.IsGenericMethod ) {
                        continue;
                    }

                    var parameters = m.GetParameters();
                    if ( parameters.Length != paramTypes.Length ) {
                        continue;
                    }

                    if ( genericArgumentTypes != null ) {
                        m = m.MakeGenericMethod(genericArgumentTypes);
                        parameters = m.GetParameters();
                    }

                    if ( returnType != null && m.ReturnType != returnType ) {
                        continue;
                    }

                    var sequenceEquals = true;
                    for ( var j = 0; j < parameters.Length; j++ ) {
                        if ( parameters[j].ParameterType != paramTypes[j] ) {
                            sequenceEquals = false;
                            break;
                        }
                    }
                    if ( sequenceEquals ) {
                        return m;
                    }
                }
            }
            Logger.LogError(string.Format("Method with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()), "Method Request");
            return null;
        }

        public static FieldInfo RTGetField(this Type type, string name, bool includePrivateBase = false) {
            var current = type;
            while ( current != null ) {

                var fields = current.RTGetFields();
                for ( var i = 0; i < fields.Length; i++ ) {
                    var f = fields[i];
                    if ( f.Name == name || MemberResolvedFromDeserializeAttribute(f, name) ) {
                        return f;
                    }
                }

                if ( !includePrivateBase ) {
                    break;
                }

                current = current.BaseType;
            }

            Logger.LogError(string.Format("Field with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()), "Field Request");
            return null;
        }

        public static PropertyInfo RTGetProperty(this Type type, string name) {
            var props = type.RTGetProperties();
            for ( var i = 0; i < props.Length; i++ ) {
                var p = props[i];
                if ( p.Name == name || MemberResolvedFromDeserializeAttribute(p, name) ) {
                    return p;
                }
            }
            Logger.LogError(string.Format("Property with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()), "Property Request");
            return null;
        }

        ///<summary>returns either field or property member info </summary>
        public static MemberInfo RTGetFieldOrProp(this Type type, string name) {
            var fields = type.RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var f = fields[i];
                if ( f.Name == name || MemberResolvedFromDeserializeAttribute(f, name) ) {
                    return f;
                }
            }
            var props = type.RTGetProperties();
            for ( var i = 0; i < props.Length; i++ ) {
                var p = props[i];
                if ( p.Name == name || MemberResolvedFromDeserializeAttribute(p, name) ) {
                    return p;
                }
            }
            Logger.LogError(string.Format("Field Or Property with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()), "Field/Property Request");
            return null;
        }

        public static EventInfo RTGetEvent(this Type type, string name) {
            var events = type.RTGetEvents();
            for ( var i = 0; i < events.Length; i++ ) {
                var e = events[i];
                if ( e.Name == name || MemberResolvedFromDeserializeAttribute(e, name) ) {
                    return e;
                }
            }
            Logger.LogError(string.Format("Event with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()), "Event Request");
            return null;
        }

        ///<summary>return field or property value</summary>
        public static object RTGetFieldOrPropValue(this MemberInfo member, object instance, int index = -1) {
            if ( member is FieldInfo ) { return ( member as FieldInfo ).GetValue(instance); }
            if ( member is PropertyInfo ) { return ( member as PropertyInfo ).GetValue(instance, index == -1 ? null : SingleTempArgsArray(index)); }
            return null;
        }

        //set field or property value
        public static void RTSetFieldOrPropValue(this MemberInfo member, object instance, object value, int index = -1) {
            if ( member is FieldInfo ) { ( member as FieldInfo ).SetValue(instance, value); }
            if ( member is PropertyInfo ) { ( member as PropertyInfo ).SetValue(instance, value, index == -1 ? null : SingleTempArgsArray(index)); }
        }

        ///----------------------------------------------------------------------------------------------

        public static ConstructorInfo[] RTGetConstructors(this Type type) {
            ConstructorInfo[] constructors;
            if ( !_typeConstructors.TryGetValue(type, out constructors) ) {
                constructors = type.GetConstructors(FLAGS_ALL);
                _typeConstructors[type] = constructors;
            }

            return constructors;
        }

        public static MethodInfo[] RTGetMethods(this Type type) {
            MethodInfo[] methods;
            if ( !_typeMethods.TryGetValue(type, out methods) ) {
                methods = type.GetMethods(FLAGS_ALL);
                _typeMethods[type] = methods;
            }

            return methods;
        }

        public static FieldInfo[] RTGetFields(this Type type) {
            FieldInfo[] fields;
            if ( !_typeFields.TryGetValue(type, out fields) ) {
                fields = type.GetFields(FLAGS_ALL);
                _typeFields[type] = fields;
            }

            return fields;
        }

        public static PropertyInfo[] RTGetProperties(this Type type) {
            PropertyInfo[] properties;
            if ( !_typeProperties.TryGetValue(type, out properties) ) {
                properties = type.GetProperties(FLAGS_ALL);
                _typeProperties[type] = properties;
            }

            return properties;
        }

        public static EventInfo[] RTGetEvents(this Type type) {
            EventInfo[] events;
            if ( !_typeEvents.TryGetValue(type, out events) ) {
                events = type.GetEvents(FLAGS_ALL);
                _typeEvents[type] = events;
            }

            return events;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Get all attributes from type including inherited</summary>
        public static object[] RTGetAllAttributes(this Type type) {
            object[] attributes;
            if ( !_typeAttributes.TryGetValue(type, out attributes) ) {
                //put in try clause to avoid problems with some unity types
                try { attributes = type.GetCustomAttributes(true); }
                finally { _typeAttributes[type] = attributes; }
            }
            return attributes;
        }

        ///<summary>Is attribute defined?</summary>
        public static bool RTIsDefined<T>(this Type type, bool inherited) where T : Attribute { return type.RTIsDefined(typeof(T), inherited); }
        public static bool RTIsDefined(this Type type, Type attributeType, bool inherited) {
            return inherited ? type.RTGetAttribute(attributeType, inherited) != null : type.IsDefined(attributeType, false);
        }

        ///<summary>Get attribute from type of type T</summary>
        public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute { return (T)type.RTGetAttribute(typeof(T), inherited); }
        public static Attribute RTGetAttribute(this Type type, Type attributeType, bool inherited) {
            object[] attributes = RTGetAllAttributes(type);
            if ( attributes != null ) {
                for ( var i = 0; i < attributes.Length; i++ ) {
                    var att = (Attribute)attributes[i];
                    var attType = att.GetType();
                    if ( attType.RTIsAssignableTo(attributeType) ) {
                        if ( inherited || type.IsDefined(attType, false) ) {
                            return att;
                        }
                    }
                }
            }
            return null;
        }

        ///------------------------------------------

        ///<summary>Get all attributes from member including inherited</summary>
        public static object[] RTGetAllAttributes(this MemberInfo member) {
            object[] attributes;
            if ( !_memberAttributes.TryGetValue(member, out attributes) ) {
                attributes = member.GetCustomAttributes(true);
                _memberAttributes[member] = attributes;
            }
            return attributes;
        }

        ///<summary>Is attribute defined?</summary>
        public static bool RTIsDefined<T>(this MemberInfo member, bool inherited) where T : Attribute { return member.RTIsDefined(typeof(T), inherited); }
        public static bool RTIsDefined(this MemberInfo member, Type attributeType, bool inherited) {
            return inherited ? member.RTGetAttribute(attributeType, inherited) != null : member.IsDefined(attributeType, false);
        }

        ///<summary>Get attribute from member of type T</summary>
        public static T RTGetAttribute<T>(this MemberInfo member, bool inherited) where T : Attribute { return (T)member.RTGetAttribute(typeof(T), inherited); }
        public static Attribute RTGetAttribute(this MemberInfo member, Type attributeType, bool inherited) {
            object[] attributes = RTGetAllAttributes(member);
            for ( var i = 0; i < attributes.Length; i++ ) {
                var att = (Attribute)attributes[i];
                var attType = att.GetType();
                if ( attType.RTIsAssignableTo(attributeType) ) {
                    if ( inherited || member.IsDefined(attType, false) ) {
                        return att;
                    }
                }
            }
            return null;
        }

        ///<summary>Get all attributes of type T recursively up the type hierarchy</summary>
        public static IEnumerable<T> RTGetAttributesRecursive<T>(this Type type) where T : Attribute {
            var current = type;
            while ( current != null ) {
                var att = current.RTGetAttribute<T>(false);
                if ( att != null ) {
                    yield return att;
                }
                current = current.BaseType;
            }
        }

        ///----------------------------------------------------------------------------------------------

        public static ParameterInfo[] RTGetDelegateTypeParameters(this Type delegateType) {
            if ( delegateType == null || !delegateType.RTIsSubclassOf(typeof(Delegate)) ) {
                return new ParameterInfo[0];
            }
            var invokeMethod = delegateType.RTGetMethod("Invoke");
            return invokeMethod.GetParameters();
        }

        ///<summary>Create delegate</summary>
		public static T RTCreateDelegate<T>(this MethodInfo method, object instance) where T : Delegate {
            return (T)(object)method.RTCreateDelegate(typeof(T), instance);
        }

        ///<summary>Create delegate</summary>
		public static Delegate RTCreateDelegate(this MethodInfo method, Type type, object instance) {
            if ( instance != null ) {
                var instanceType = instance.GetType();
                if ( method.DeclaringType != instanceType ) {
                    method = instanceType.RTGetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
                }
            }
            return Delegate.CreateDelegate(type, instance, method);
        }

        ///<summary>Convert delegate</summary>
        public static Delegate ConvertDelegate(Delegate originalDelegate, Type targetDelegateType) {
            return Delegate.CreateDelegate(targetDelegateType, originalDelegate.Target, originalDelegate.Method);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Is the field read only?</summary>
        public static bool IsReadOnly(this FieldInfo field) {
            return field.IsInitOnly || field.IsLiteral;
        }

        ///<summary>Is the field a Constant?</summary>
        public static bool IsConstant(this FieldInfo field) {
            return field.IsReadOnly() && field.IsStatic;
        }

        ///<summary>Quicky to get if an event info is static.</summary>
        public static bool IsStatic(this EventInfo info) {
            var m = info.GetAddMethod();
            return m != null ? m.IsStatic : false;
        }

        ///<summary>Quicky to get if a property info is static.</summary>
        public static bool IsStatic(this PropertyInfo info) {
            var m = info.GetGetMethod();
            return m != null ? m.IsStatic : false;
        }

        ///<summary>Is the parameter provided a params array?</summary>
        public static bool IsParams(this ParameterInfo parameter, ParameterInfo[] parameters) {
            return parameter.Position == parameters.Length - 1 && parameter.IsDefined(typeof(ParamArrayAttribute), false);
        }

        ///<summary>Utility to determine obsolete members quicker. Also handles property accessor methods.</summary>
        public static bool IsObsolete(this MemberInfo member, bool inherited = true) {

            bool result;
            if ( _obsoleteCache.TryGetValue(member, out result) ) {
                return result;
            }

            var resultMember = member;
            if ( member is MethodInfo ) {
                var m = (MethodInfo)member;
                if ( m.IsPropertyAccessor() ) {
                    resultMember = m.GetAccessorProperty();
                }
            }
            var isObsolete = resultMember.RTIsDefined<System.ObsoleteAttribute>(inherited);
            return _obsoleteCache[member] = isObsolete;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>BaseDefinition for PropertyInfos.</summary>
	    public static PropertyInfo GetBaseDefinition(this PropertyInfo propertyInfo) {
            var method = propertyInfo.GetAccessors(true).FirstOrDefault();
            if ( method == null ) {
                return null;
            }

            var baseMethod = method.GetBaseDefinition();
            if ( baseMethod == method ) {
                return propertyInfo;
            }

            var arguments = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();
            return baseMethod.DeclaringType.GetProperty(propertyInfo.Name, FLAGS_ALL, null, propertyInfo.PropertyType, arguments, null);
        }

        ///<summary>BaseDefinition for FieldInfo. Not exactly correct but here for consistency.</summary>
        public static FieldInfo GetBaseDefinition(this FieldInfo fieldInfo) {
            return fieldInfo.DeclaringType.RTGetField(fieldInfo.Name);
        }

        ///<summary>Get a list of methods that extend the provided type</summary>
        public static MethodInfo[] GetExtensionMethods(this Type targetType) {
            MethodInfo[] methods = null;
            if ( _typeExtensions.TryGetValue(targetType, out methods) ) {
                return methods;
            }
            var result = new List<MethodInfo>();
            var allTypes = GetAllTypes(false);
            for ( var i = 0; i < allTypes.Length; i++ ) {
                var t = allTypes[i];
                if ( !t.IsSealed || t.IsGenericType || !t.RTIsDefined<System.Runtime.CompilerServices.ExtensionAttribute>(true) ) {
                    continue;
                }

                var typeMethods = t.RTGetMethods();
                for ( var j = 0; j < typeMethods.Length; j++ ) {
                    var m = typeMethods[j];
                    if ( m.IsExtensionMethod() && m.GetParameters()[0].ParameterType.RTIsAssignableFrom(targetType) ) {
                        result.Add(m);
                    }
                }
            }

            return _typeExtensions[targetType] = result.ToArray();
        }

        ///<summary>Helper to determine if method is extension quicker.</summary>
        public static bool IsExtensionMethod(this MethodInfo method) {
            return method.RTIsDefined<System.Runtime.CompilerServices.ExtensionAttribute>(true);
        }

        ///<summary>Returns if method is Get or Set method of a property.</summary>
        public static bool IsPropertyAccessor(this MethodInfo method) {
            return method.GetMethodSpecialType() == MethodType.PropertyAccessor;
        }

        ///<summary>Returns whether the property is an indexer.</summary>
        public static bool IsIndexerProperty(this PropertyInfo property) {
            return property.GetIndexParameters().Length != 0;
        }

        ///<summary>Returns if the property is auto.</summary>
        public static bool IsAutoProperty(this PropertyInfo property) {
            if ( !property.CanRead || !property.CanWrite ) { return false; }
            var backingFieldName = "<" + property.Name + ">k__BackingField";
            return property.DeclaringType.RTGetField(backingFieldName) != null;
        }

        ///<summary>Returns the equivalent property of a method that represents an accessor method.</summary>
        public static PropertyInfo GetAccessorProperty(this MethodInfo method) {
            if ( method.IsPropertyAccessor() ) {
                return method.RTReflectedOrDeclaredType().RTGetProperty(method.Name.Substring(4));
            }
            return null;
        }

        ///<summary>Is type a supported enumerable collection?</summary>
        public static bool IsEnumerableCollection(this Type type) {
            if ( type == null ) { return false; }
            return typeof(IEnumerable).RTIsAssignableFrom(type) && ( type.RTIsGenericType() || type.RTIsArray() );
        }

        ///<summary>Returns the element type of an enumerable type.</summary>
        public static Type GetEnumerableElementType(this Type type) {
            if ( type == null ) { return null; }

            if ( !typeof(IEnumerable).RTIsAssignableFrom(type) ) {
                return null;
            }

            if ( type.HasElementType || type.RTIsArray() ) {
                return type.GetElementType();
            }

            if ( type.RTIsGenericType() ) {
                //These are not exactly correct, but serve the purpose of usage.
                var args = type.RTGetGenericArguments();
                if ( args.Length == 1 ) {
                    return args[0];
                }
                //This is special. We only support Dictionary<string, T> and always consider 1st arg to be string.
                if ( typeof(IDictionary).RTIsAssignableFrom(type) && args.Length == 2 ) {
                    return args[1];
                }
            }
            /*
            var interfaces = type.GetInterfaces();
            for (var i = 0; i < interfaces.Length; i++){
                var iface = interfaces[i];
                if (!iface.RTIsGenericType()){
                    continue;
                }
                var genType = iface.GetGenericTypeDefinition();
                if (genType != typeof(IEnumerable<>)){
                    continue;
                }

                return iface.RTGetGenericArguments()[0];
            }
            */
            return null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns the first generic argument type if type is generic and has only a single (1) generic argument. Otherwise returns null.</summary>
        public static Type GetSingleGenericArgument(this Type type) {
            if ( type.RTIsGenericType() ) {
                var args = type.RTGetGenericArguments();
                return args.Length == 1 ? args[0] : null;
            }
            return null;
        }

        ///<summary>Returns the first argument parameter constraint. If no constraint, typeof(object) is returned.</summary>
        public static Type GetFirstGenericParameterConstraintType(this Type type) {
            if ( type == null || !type.RTIsGenericType() ) { return null; }
            type = type.GetGenericTypeDefinition();
            var arg1 = type.RTGetGenericArguments().First();
            var c1 = arg1.GetGenericParameterConstraints().FirstOrDefault();
            return c1 != null ? c1 : typeof(object);
        }

        ///<summary>Returns the first argument parameter constraint. If no constraint, typeof(object) is returned.</summary>
        public static Type GetFirstGenericParameterConstraintType(this MethodInfo method) {
            if ( method == null || !method.IsGenericMethod ) { return null; }
            method = method.GetGenericMethodDefinition();
            var arg1 = method.RTGetGenericArguments().First();
            var c1 = arg1.GetGenericParameterConstraints().FirstOrDefault();
            return c1 != null ? c1 : typeof(object);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Return true if def can be made generic with argType and outs the resulting generic type made</summary>
        public static bool TryMakeGeneric(this Type def, Type argType, out Type result) {
            result = null;
            if ( def == null || argType == null || !def.IsGenericType ) { return false; }
            try {
                result = def.GetGenericTypeDefinition().MakeGenericType(argType);
                return true;
            }
            catch { return false; }
        }

        ///<summary>Return true if def can be made generic with argType and outs the resulting generic method made</summary>
        public static bool TryMakeGeneric(this MethodInfo def, Type argType, out MethodInfo result) {
            result = null;
            if ( def == null || argType == null || !def.IsGenericMethod ) { return false; }
            try {
                result = def.GetGenericMethodDefinition().MakeGenericMethod(argType);
                return true;
            }
            catch { return false; }
        }

        ///<summary>Resize array of arbitrary element type. Creates a new instance.</summary>
        public static System.Array Resize(this System.Array array, int newSize) {
            if ( array == null ) { return null; }
            var oldSize = array.Length;
            var elementType = array.GetType().GetElementType();
            var newArray = System.Array.CreateInstance(elementType, newSize);
            var preserveLength = System.Math.Min(oldSize, newSize);
            if ( preserveLength > 0 ) {
                System.Array.Copy(array, newArray, preserveLength);
            }
            return newArray;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Check if conversion exists from -> to type and outs an expression able to do so.</summary>
        public static bool TryConvert(Type fromType, Type toType, out UnaryExpression exp) {
            try {
                // Throws an exception if there is no conversion fromType -> toType
                exp = Expression.Convert(Expression.Parameter(fromType, null), toType);
                return true;
            }
            catch {
                exp = null;
                return false;
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Dig instance fields provided predicate and callbacks on found object value. IList and IDictionary are handled (IDictionary.Values only). Recursion is *NOT* checked for performance reasons, so be careful.</summary>
        public static void DigFields(object root, Predicate<FieldInfo> move, Action<object> push, Action<object> pop) {

            if ( root == null ) { return; }

            var type = root.GetType();
            if ( type.IsPrimitive || type == typeof(string) ) {
                return;
            }

            if ( push != null ) { push(root); }

            var fields = type.RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var field = fields[i];
                if ( !field.IsStatic && !field.FieldType.IsPrimitive && field.FieldType != typeof(string) && move(field) ) {
                    var value = field.GetValue(root);
                    if ( value == null ) {
                        continue;
                    }
                    if ( value is IList ) {
                        foreach ( var item in (IList)value ) {
                            DigFields(item, move, push, pop);
                        }
                        continue;
                    }
                    if ( value is IDictionary ) {
                        foreach ( var item in ( (IDictionary)value ).Values ) {
                            DigFields(item, move, push, pop);
                        }
                        continue;
                    }
                    DigFields(value, move, push, pop);
                }
            }

            if ( pop != null ) { pop(root); }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Creates and returns an open instance getter for field.</summary>
        public static Func<T, TResult> GetFieldGetter<T, TResult>(FieldInfo info) {
#if !NET_STANDARD_2_0 && (UNITY_EDITOR || (!ENABLE_IL2CPP && (UNITY_STANDALONE || UNITY_ANDROID || UNITY_WSA)))
            var name = string.Format("__get_field_{0}_", info.Name);
            DynamicMethod fieldGetter = new DynamicMethod(name, typeof(TResult), new Type[] { typeof(T) }, typeof(T));
            ILGenerator il = fieldGetter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, info);
            il.Emit(OpCodes.Ret);
            return (Func<T, TResult>)fieldGetter.CreateDelegate(typeof(Func<T, TResult>));
#else
            return (T instance) => { return (TResult)info.GetValue(instance); };
#endif
        }

        ///<summary>Creates and returns an open instance setter for field.</summary>
        public static Action<T, TValue> GetFieldSetter<T, TValue>(FieldInfo info) {
#if !NET_STANDARD_2_0 && (UNITY_EDITOR || (!ENABLE_IL2CPP && (UNITY_STANDALONE || UNITY_ANDROID || UNITY_WSA)))
            var name = string.Format("__set_field_{0}_", info.Name);
            DynamicMethod m = new DynamicMethod(name, typeof(void), new Type[] { typeof(T), typeof(TValue) }, typeof(T));
            ILGenerator il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, info);
            il.Emit(OpCodes.Ret);
            return (Action<T, TValue>)m.CreateDelegate(typeof(Action<T, TValue>));
#else
            return (T instance, TValue value) => { info.SetValue(instance, value); };
#endif
        }

        ///----------------------------------------------------------------------------------------------
        // ///<summary>Can type be made generic by using target type as argument?</summary>
        // public static bool CanBeMadeGenericWith(this Type def, Type type) {
        //     if ( def == null || !def.RTIsGenericType() ) { return false; }
        //     return type.IsAllowedByGenericArgument(def.GetGenericTypeDefinition().RTGetGenericArguments().FirstOrDefault());
        // }

        // ///<summary>Can method be made generic by using target type as argument?</summary>
        // public static bool CanBeMadeGenericWith(this MethodInfo def, Type type) {
        //     if ( def == null || !def.IsGenericMethod ) { return false; }
        //     return type.IsAllowedByGenericArgument(def.GetGenericMethodDefinition().RTGetGenericArguments().FirstOrDefault());
        // }

        // ///<summary>Is type allowed to be assigned to target generic argument based on that argument's constaints?</summary>
        // public static bool IsAllowedByGenericArgument(this Type type, Type genericArgument) {

        //     if ( type == null || genericArgument == null ) { return false; }

        //     var constraints = genericArgument.GetGenericParameterConstraints();
        //     var attributes = genericArgument.GenericParameterAttributes;

        //     var result = true;
        //     for ( var i = 0; i < constraints.Length; i++ ) {
        //         var constraint = constraints[i];
        //         if ( constraint == typeof(ValueType) ) continue;
        //         if ( !result ) break;
        //         result &= constraint.RTIsAssignableFrom(type);
        //     }

        //     if ( result ) {
        //         if ( ( attributes & GenericParameterAttributes.DefaultConstructorConstraint ) ==
        //             GenericParameterAttributes.DefaultConstructorConstraint &&
        //             ( attributes & GenericParameterAttributes.NotNullableValueTypeConstraint ) !=
        //             GenericParameterAttributes.NotNullableValueTypeConstraint ) {
        //             var constructor = type.RTGetConstructors().FirstOrDefault(info => info.IsPublic && info.GetParameters().Length == 0);
        //             if ( constructor == null ) result = false;
        //         }
        //     }

        //     if ( result ) {
        //         if ( ( attributes & GenericParameterAttributes.ReferenceTypeConstraint ) ==
        //             GenericParameterAttributes.ReferenceTypeConstraint ) {
        //             if ( type.RTIsValueType() ) result = false;
        //         }
        //     }

        //     if ( result ) {
        //         if ( ( attributes & GenericParameterAttributes.NotNullableValueTypeConstraint ) ==
        //             GenericParameterAttributes.NotNullableValueTypeConstraint ) {
        //             if ( !type.RTIsValueType() ) result = false;
        //         }
        //     }
        //     return result;
        // }


    }
}