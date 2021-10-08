#if UNITY_EDITOR

using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ParadoxNotion.Design
{

    ///Provides object and attribute property drawers
    public static class PropertyDrawerFactory
    {

        //Type to drawer instance map
        private static Dictionary<Type, IObjectDrawer> objectDrawers = new Dictionary<Type, IObjectDrawer>();
        private static Dictionary<Type, IAttributeDrawer> attributeDrawers = new Dictionary<Type, IAttributeDrawer>();

        public static void FlushMem() {
            objectDrawers = new Dictionary<Type, IObjectDrawer>();
            attributeDrawers = new Dictionary<Type, IAttributeDrawer>();
        }

        ///Return an object drawer instance of target inspected type
        public static IObjectDrawer GetObjectDrawer(Type objectType) {
            IObjectDrawer result = null;
            if ( objectDrawers.TryGetValue(objectType, out result) ) {
                return result;
            }

            // look for specific drawer first
            Type fallbackDrawerType = null;
            foreach ( var drawerType in ReflectionTools.GetImplementationsOf(typeof(IObjectDrawer)) ) {
                if ( drawerType != typeof(DefaultObjectDrawer) ) {
                    var args = drawerType.BaseType.RTGetGenericArguments();
                    if ( args.Length == 1 ) {
                        if ( args[0].IsEquivalentTo(objectType) ) {
                            return objectDrawers[objectType] = Activator.CreateInstance(drawerType) as IObjectDrawer;
                        }
                        if ( args[0].IsAssignableFrom(objectType) ) { fallbackDrawerType = drawerType; }
                    }
                }
            }

            if ( fallbackDrawerType != null ) {
                return objectDrawers[objectType] = Activator.CreateInstance(fallbackDrawerType) as IObjectDrawer;
            }


            // foreach ( var drawerType in ReflectionTools.GetImplementationsOf(typeof(IObjectDrawer)) ) {
            //     if ( drawerType != typeof(DefaultObjectDrawer) ) {
            //         var args = drawerType.BaseType.RTGetGenericArguments();
            //         if ( args.Length == 1 && args[0].IsAssignableFrom(objectType) ) {
            //             return objectDrawers[objectType] = Activator.CreateInstance(drawerType) as IObjectDrawer;
            //         }
            //     }
            // }

            return objectDrawers[objectType] = new DefaultObjectDrawer(objectType);
        }

        ///Return an attribute drawer instance of target attribute instance
        public static IAttributeDrawer GetAttributeDrawer(DrawerAttribute att) { return GetAttributeDrawer(att.GetType()); }
        ///Return an attribute drawer instance of target attribute type
        public static IAttributeDrawer GetAttributeDrawer(Type attributeType) {
            IAttributeDrawer result = null;
            if ( attributeDrawers.TryGetValue(attributeType, out result) ) {
                return result;
            }

            foreach ( var drawerType in ReflectionTools.GetImplementationsOf(typeof(IAttributeDrawer)) ) {
                if ( drawerType != typeof(DefaultAttributeDrawer) ) {
                    var args = drawerType.BaseType.RTGetGenericArguments();
                    if ( args.Length == 1 && args[0].IsAssignableFrom(attributeType) ) {
                        return attributeDrawers[attributeType] = Activator.CreateInstance(drawerType) as IAttributeDrawer;
                    }
                }
            }

            return attributeDrawers[attributeType] = new DefaultAttributeDrawer(attributeType);
        }
    }

    ///----------------------------------------------------------------------------------------------

    public interface IObjectDrawer
    {
        object DrawGUI(GUIContent content, object instance, InspectedFieldInfo info);
        object MoveNextDrawer();
    }

    public interface IAttributeDrawer
    {
        object DrawGUI(IObjectDrawer objectDrawer, GUIContent content, object instance, DrawerAttribute attribute, InspectedFieldInfo info);
    }

    ///----------------------------------------------------------------------------------------------

    ///Derive this to create custom drawers for T assignable object types.
    abstract public class ObjectDrawer<T> : IObjectDrawer
    {
        ///info
        protected InspectedFieldInfo info { get; private set; }
        ///The GUIContent
        protected GUIContent content { get; private set; }
        ///The instance of the object being drawn
        protected T instance { get; private set; }

        ///The set of Drawer Attributes found on field
        protected DrawerAttribute[] attributes { get; private set; }
        ///Current attribute index drawn
        private int attributeIndex { get; set; }

        ///The reflected FieldInfo representation
        protected FieldInfo fieldInfo { get { return info.field; } }
        ///The parent object the instance is drawn within
        protected object context { get { return info.parentInstanceContext; } }
        ///The Unity object the instance serialized within
        protected UnityEngine.Object contextUnityObject { get { return info.unityObjectContext; } }


        ///Begin GUI
        object IObjectDrawer.DrawGUI(GUIContent content, object instance, InspectedFieldInfo info) {
            this.content = content;
            this.instance = (T)instance;
            this.info = info;

            this.attributes = info.attributes != null ? info.attributes.OfType<DrawerAttribute>().OrderBy(a => a.priority).ToArray() : null;

            this.attributeIndex = -1;
            var result = ( this as IObjectDrawer ).MoveNextDrawer();

            // //flush references
            this.info = default(InspectedFieldInfo);
            this.content = null;
            this.instance = default(T);
            this.attributes = null;

            return result;
        }

        ///Show the next attribute drawer in order, or the object drawer itself of no attribute drawer is left to show.
        object IObjectDrawer.MoveNextDrawer() {
            attributeIndex++;
            if ( attributes != null && attributeIndex < attributes.Length ) {
                var currentDrawerAttribute = attributes[attributeIndex];
                var drawer = PropertyDrawerFactory.GetAttributeDrawer(currentDrawerAttribute);
                return drawer.DrawGUI(this, content, instance, currentDrawerAttribute, info);
            }
            return OnGUI(content, instance);
        }

        ///Override to implement GUI. Return the modified instance at the end.
        abstract public T OnGUI(GUIContent content, T instance);
    }

    ///The default object drawer implementation able to inspect most types
    public class DefaultObjectDrawer : ObjectDrawer<object>
    {

        private Type objectType;

        public DefaultObjectDrawer(Type objectType) {
            this.objectType = objectType;
        }

        public override object OnGUI(GUIContent content, object instance) {
            return EditorUtils.DrawEditorFieldDirect(content, instance, objectType, info);
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///Derive this to create custom drawers for T DrawerAttribute.
    abstract public class AttributeDrawer<T> : IAttributeDrawer where T : DrawerAttribute
    {

        ///info
        protected InspectedFieldInfo info { get; private set; }

        ///The GUIContent
        protected GUIContent content { get; private set; }
        ///The instance of the object being drawn
        protected object instance { get; private set; }
        ///The reflected FieldInfo representation

        ///The attribute instance
        protected T attribute { get; private set; }
        ///The ObjectDrawer currently in use
        protected IObjectDrawer objectDrawer { get; private set; }

        protected FieldInfo fieldInfo { get { return info.field; } }
        ///The parent object the instance is drawn within
        protected object context { get { return info.parentInstanceContext; } }
        ///The Unity object the instance serialized within
        protected UnityEngine.Object contextUnityObject { get { return info.unityObjectContext; } }

        ///Begin GUI
        object IAttributeDrawer.DrawGUI(IObjectDrawer objectDrawer, GUIContent content, object instance, DrawerAttribute attribute, InspectedFieldInfo info) {
            this.objectDrawer = objectDrawer;
            this.content = content;
            this.instance = instance;
            this.attribute = (T)attribute;

            this.info = info;
            var result = OnGUI(content, instance);

            //flush references
            this.info = default(InspectedFieldInfo);
            this.content = null;
            this.instance = null;
            this.attribute = null;
            this.objectDrawer = null;

            return result;
        }

        ///Override to implement GUI. Return the modified instance at the end.
        abstract public object OnGUI(GUIContent content, object instance);
        ///Show the next attribute drawer in order, or the object drawer itself of no attribute drawer is left to show.
        protected object MoveNextDrawer() { return objectDrawer.MoveNextDrawer(); }
    }

    ///The default attribute drawer implementation for when an actual implementation is not found
    public class DefaultAttributeDrawer : AttributeDrawer<DrawerAttribute>
    {

        private Type attributeType;

        public DefaultAttributeDrawer(Type attributeType) {
            this.attributeType = attributeType;
        }

        public override object OnGUI(GUIContent content, object instance) {
            GUILayout.Label(string.Format("Implementation of '{0}' drawer attribute not found.", attributeType));
            return MoveNextDrawer();
        }
    }

}

#endif