using System;
using System.Collections.Generic;

namespace ParadoxNotion
{

    ///<summary>A simple weak reference list</summary>
    public class WeakReferenceList<T> where T : class
    {
        private List<WeakReference<T>> list;

        public int Count => list.Count;

        public WeakReferenceList() {
            list = new List<WeakReference<T>>();
        }

        public WeakReferenceList(int capacity) {
            list = new List<WeakReference<T>>(capacity);
        }

        public T this[int i] {
            get
            {
                list[i].TryGetTarget(out T reference);
                return reference;
            }
            set
            {
                list[i].SetTarget(value);
            }
        }

        public void Add(T item) {
            list.Add(new WeakReference<T>(item));
        }

        public void Remove(T item) {
            for ( var i = list.Count; i-- > 0; ) {
                var element = list[i];
                if ( element.TryGetTarget(out T reference) && ReferenceEquals(reference, item) ) {
                    list.Remove(element);
                }
            }
        }

        public bool Contains(T item, out int index) {
            for ( var i = 0; i < list.Count; i++ ) {
                if ( list[i].TryGetTarget(out T target) && ReferenceEquals(target, item) ) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public void Clear() {
            list.Clear();
        }

        public List<T> ToReferenceList() {
            var result = new List<T>();
            for ( var i = 0; i < list.Count; i++ ) {
                var element = list[i];
                if ( element.TryGetTarget(out T reference) ) {
                    result.Add(reference);
                }
            }
            return result;
        }

        public static implicit operator WeakReferenceList<T>(List<T> value) {
            var result = new WeakReferenceList<T>(value.Count);
            for ( var i = 0; i < value.Count; i++ ) {
                result.Add(value[i]);
            }
            return result;
        }
    }
}