using System;
using System.Collections.Generic;
using ParadoxNotion;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Framework
{

    ///<summary>Implementations for IBlackboards</summary>
    [ParadoxNotion.Design.SpoofAOT]
    public static class IBlackboardExtensions
    {

        ///<summary>Returns the root blackboard upwards the hierarchy</summary>
        public static IBlackboard GetRoot(this IBlackboard blackboard) {
            if ( blackboard.parent == null ) { return blackboard; }
            return blackboard.parent.GetRoot();
        }

        ///<summary>Returns all parent blackboard (optionaly including self) upwards</summary>
        public static IEnumerable<IBlackboard> GetAllParents(this IBlackboard blackboard, bool includeSelf) {
            if ( blackboard == null ) { yield break; }
            if ( includeSelf ) { yield return blackboard; }
            var current = blackboard.parent;
            while ( current != null ) {
                yield return current;
                current = current.parent;
            }
        }

        ///<summary>Is the blackboard parent of the target child blackboard upward hierarchy or the target itself</summary>
        public static bool IsPartOf(this IBlackboard blackboard, IBlackboard child) {
            if ( blackboard == null || child == null ) { return false; }
            if ( blackboard == child ) { return true; }
            return blackboard.IsPartOf(child.parent);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Adds a new Variable<T> with provided value and returns it.</summary>
        public static Variable<T> AddVariable<T>(this IBlackboard blackboard, string varName, T value) {
            var variable = blackboard.AddVariable<T>(varName);
            variable.value = value;
            return variable;
        }

        ///<summary>Adds a new Variable<T> with default T value and returns it</summary>
        public static Variable<T> AddVariable<T>(this IBlackboard blackboard, string varName) {
            return (Variable<T>)blackboard.AddVariable(varName, typeof(T));
        }

        ///<summary>Adds a new Variable in the blackboard</summary>
        public static Variable AddVariable(this IBlackboard blackboard, string varName, object value) {

            if ( value == null ) {
                Logger.LogError("You can't use AddVariable with a null value. Use AddVariable(string, Type) to add the new variable first", LogTag.BLACKBOARD, blackboard);
                return null;
            }

            var newVariable = blackboard.AddVariable(varName, value.GetType());
            if ( newVariable != null ) {
                newVariable.value = value;
            }

            return newVariable;
        }

        ///<summary>Adds a new Variable in the blackboard defining name and type instead of value</summary>
        public static Variable AddVariable(this IBlackboard blackboard, string varName, Type type) {

            if ( blackboard.variables.TryGetValue(varName, out Variable result) ) {
                if ( result.CanConvertTo(type) ) {
                    Logger.Log(string.Format("Variable with name '{0}' already exists in blackboard '{1}'. Returning existing instead of new.", varName, blackboard), LogTag.BLACKBOARD, blackboard);
                    return result;
                } else {
                    Logger.LogError(string.Format("Variable with name '{0}' already exists in blackboard '{1}', but is of a different type! Returning null instead of new.", varName, blackboard), LogTag.BLACKBOARD, blackboard);
                    return null;
                }
            }

            var variableType = typeof(Variable<>).RTMakeGenericType(new Type[] { type });
            var newVariable = (Variable)Activator.CreateInstance(variableType);
            newVariable.name = varName;
            blackboard.variables[varName] = newVariable;
            blackboard.TryInvokeOnVariableAdded(newVariable);
            return newVariable;
        }

        ///<summary>Deletes the Variable of name provided regardless of type and returns the deleted Variable object.</summary>
        public static Variable RemoveVariable(this IBlackboard blackboard, string varName) {
            Variable variable = null;
            if ( blackboard.variables.TryGetValue(varName, out variable) ) {
                blackboard.variables.Remove(varName);
                blackboard.TryInvokeOnVariableRemoved(variable);
                variable.OnDestroy();
            }
            return variable;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Gets the variable data value from the blackboard with provided name and type T.</summary>
        public static T GetVariableValue<T>(this IBlackboard blackboard, string varName) {

            var variable = GetVariable<T>(blackboard, varName);
            if ( variable == null ) {
                Logger.LogError(string.Format("No Variable of name '{0}' and type '{1}' exists on Blackboard '{2}'. Returning default T...", varName, typeof(T).FriendlyName(), blackboard), LogTag.BLACKBOARD, blackboard);
                return default(T);
            }

            if ( variable is Variable<T> ) {
                return ( variable as Variable<T> ).value;
            }

            try { return (T)variable.value; }
            catch { Logger.LogError(string.Format("Can't cast value of variable with name '{0}' to type '{1}'", varName, typeof(T).FriendlyName()), LogTag.BLACKBOARD, blackboard); }
            return default(T);
        }

        ///<summary>Set the value of the Variable variable defined by its name. If a data by that name and type doesnt exist, a new data is added by that name</summary>
        public static Variable SetVariableValue(this IBlackboard blackboard, string varName, object value) {

            Variable variable;
            if ( !blackboard.variables.TryGetValue(varName, out variable) ) {
                Logger.Log(string.Format("No Variable of name '{0}' and type '{1}' exists on Blackboard '{2}'. Adding new instead...", varName, value != null ? value.GetType().FriendlyName() : "null", blackboard), LogTag.BLACKBOARD, blackboard);
                variable = blackboard.AddVariable(varName, value);
                return variable;
            }

            try { variable.value = value; }
            catch {
                Logger.LogError(string.Format("Can't cast value '{0}' to blackboard variable of name '{1}' and type '{2}'", value != null ? value.ToString() : "null", varName, variable.varType.FriendlyName()), LogTag.BLACKBOARD, blackboard);
                return null;
            }

            return variable;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Initialize variables data binding for the target (gameobject is used). It can be null for static bindings</summary>
        public static void InitializePropertiesBinding(this IBlackboard blackboard, Component target, bool callSetter) {
            if ( blackboard.variables.Count == 0 ) { return; }
            foreach ( var data in blackboard.variables.Values ) {
                data.InitializePropertyBinding(target?.gameObject, callSetter);
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Generic version of GetVariable. Includes parent blackboards upwards the hierarchy.</summary>
        public static Variable<T> GetVariable<T>(this IBlackboard blackboard, string varName) {
            return (Variable<T>)blackboard.GetVariable(varName, typeof(T));
        }

        ///<summary>Gets the Variable object of a certain name and optional specified assignable type. Includes parent blackboards upwards the hierarchy.</summary>
        public static Variable GetVariable(this IBlackboard blackboard, string varName, Type ofType = null) {

            if ( blackboard.variables != null && varName != null ) {
                Variable variable;
                if ( blackboard.variables.TryGetValue(varName, out variable) ) {
                    if ( ofType == null || ofType == typeof(object) || variable.CanConvertTo(ofType) ) {
                        return variable;
                    }
                }
            }

            if ( blackboard.parent != null ) {
                var result = blackboard.parent.GetVariable(varName, ofType);
                if ( result != null ) { return result; }
            }

            return null;
        }

        ///<summary>Gets the Variable object of a certain ID. Includes parent blackboards upwards the hierarchy.</summary>
        public static Variable GetVariableByID(this IBlackboard blackboard, string ID) {

            if ( blackboard.variables != null && ID != null ) {
                foreach ( var pair in blackboard.variables ) {
                    if ( pair.Value.ID == ID ) { return pair.Value; }
                }
            }

            if ( blackboard.parent != null ) {
                var result = blackboard.parent.GetVariableByID(ID);
                if ( result != null ) { return result; }
            }

            return null;
        }

        ///<summary>Get all variables. Includes parent blackboards upwards the hierarchy.</summary>
        public static IEnumerable<Variable> GetVariables(this IBlackboard blackboard, Type ofType = null) {

            if ( blackboard.parent != null ) {
                foreach ( var subVariable in blackboard.parent.GetVariables(ofType) ) {
                    yield return subVariable;
                }
            }

            foreach ( var pair in blackboard.variables ) {
                if ( ofType == null || ofType == typeof(object) || pair.Value.CanConvertTo(ofType) ) {
                    yield return pair.Value;
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Change variable type (creates new instance and keeps name and ID)</summary>
        public static Variable ChangeVariableType(this IBlackboard blackboard, Variable target, Type newType) {
            var variableType = typeof(Variable<>).RTMakeGenericType(new Type[] { newType });
            var newVariable = (Variable)Activator.CreateInstance(variableType, new object[] { target.name, target.ID });
            blackboard.variables[target.name] = newVariable;
            blackboard.TryInvokeOnVariableAdded(newVariable);
            return newVariable;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Overwrite variables from source blackboard into this blackboard</summary>
        public static void OverwriteFrom(this IBlackboard blackboard, IBlackboard sourceBlackboard, bool removeMissingVariables = true) {
            foreach ( var pair in sourceBlackboard.variables ) {
                if ( blackboard.variables.ContainsKey(pair.Key) ) {
                    blackboard.SetVariableValue(pair.Key, pair.Value.value);
                } else {
                    blackboard.variables[pair.Key] = pair.Value;
                }
            }

            if ( removeMissingVariables ) {
                var keys = new List<string>(blackboard.variables.Keys);
                foreach ( string key in keys ) {
                    if ( !sourceBlackboard.variables.ContainsKey(key) ) {
                        blackboard.variables.Remove(key);
                    }
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

    }
}