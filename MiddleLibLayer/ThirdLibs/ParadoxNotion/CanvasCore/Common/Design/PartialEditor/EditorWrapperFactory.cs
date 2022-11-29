#if UNITY_EDITOR

using System;
using System.Reflection;

namespace ParadoxNotion.Design
{

    ///<summary>Factory for EditorObjectWrappers</summary>
    public static class EditorWrapperFactory
    {

        private static WeakReferenceTable<object, EditorObjectWrapper> cachedEditors = new WeakReferenceTable<object, EditorObjectWrapper>();

        ///<summary>Returns a cached EditorObjectWrapper of type T for target object</summary>
        public static T GetEditor<T>(object target) where T : EditorObjectWrapper {
            EditorObjectWrapper wrapper;
            if ( cachedEditors.TryGetValueWithRefCheck(target, out wrapper) ) {
                return (T)wrapper;
            }
            wrapper = (T)( typeof(T).CreateObject() );
            wrapper.Enable(target);
            cachedEditors.Add(target, wrapper);
            return (T)wrapper;
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>Wrapper Editor for objects</summary>
    abstract public class EditorObjectWrapper : IDisposable
    {

        private WeakReference<object> _targetRef;
        //The target
        public object target {
            get
            {
                _targetRef.TryGetTarget(out object reference);
                return reference;
            }
        }

        //...
        void IDisposable.Dispose() { OnDisable(); }

        ///<summary>Init for target</summary>
        public void Enable(object target) {
            this._targetRef = new WeakReference<object>(target);
            OnEnable();
        }

        ///<summary>Create Property and Method wrappers here or other stuff.</summary>
        virtual protected void OnEnable() { }
        ///<summary>Cleanup</summary>
        virtual protected void OnDisable() { }

        ///<summary>Get a wrapped editor serialized field on target</summary>
        public EditorPropertyWrapper<T> CreatePropertyWrapper<T>(string name) {
            var type = target.GetType();
            var field = type.RTGetField(name, /*include private base*/ true);
            if ( field != null ) {
                var wrapper = (EditorPropertyWrapper<T>)typeof(EditorPropertyWrapper<>).MakeGenericType(typeof(T)).CreateObject();
                wrapper.Init(this, field);
                return wrapper;
            }
            return null;
        }

        ///<summary>Get a wrapped editor method on target</summary>
        public EditorMethodWrapper CreateMethodWrapper(string name) {
            var type = target.GetType();
            var method = type.RTGetMethod(name);
            if ( method != null ) {
                var wrapper = new EditorMethodWrapper();
                wrapper.Init(this, method);
                return wrapper;
            }
            return null;
        }
    }

    ///<summary>Wrapper Editor for objects</summary>
    abstract public class EditorObjectWrapper<T> : EditorObjectWrapper
    {
        new public T target { get { return (T)base.target; } }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>An editor wrapped field</summary>
    sealed public class EditorPropertyWrapper<T>
    {
        private EditorObjectWrapper editor { get; set; }
        private FieldInfo field { get; set; }
        public T value {
            get
            {
                var o = field.GetValue(editor.target);
                return o != null ? (T)o : default(T);
            }
            set
            {
                field.SetValue(editor.target, value);
            }
        }

        public void Init(EditorObjectWrapper editor, FieldInfo field) {
            this.editor = editor;
            this.field = field;
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>An editor wrapped method</summary>
    sealed public class EditorMethodWrapper
    {
        private EditorObjectWrapper editor { get; set; }
        private MethodInfo method { get; set; }
        public void Invoke(params object[] args) {
            method.Invoke(editor.target, args);
        }
        public void Init(EditorObjectWrapper editor, MethodInfo method) {
            this.editor = editor;
            this.method = method;
        }
    }
}

#endif