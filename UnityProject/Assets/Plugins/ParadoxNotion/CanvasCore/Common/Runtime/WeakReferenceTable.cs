using System;
using System.Collections.Generic;

namespace ParadoxNotion
{
    ///<summary>A simple weak reference table (because Mono has a bug with ConditionalWeakTable)</summary>
    public class WeakReferenceTable<TKey, TValue> where TKey : class where TValue : IDisposable
    {
        private List<WeakReference<TKey>> keys;
        private List<TValue> values;

        public int Count => keys.Count;

        public WeakReferenceTable() {
            keys = new List<WeakReference<TKey>>();
            values = new List<TValue>();
        }

        public void Clear() {
            keys.Clear();
            values.Clear();
        }

        public void Add(TKey key, TValue value) {
            CheckCount();
            keys.Insert(0, new WeakReference<TKey>(key));
            values.Insert(0, value);
        }

        public void Remove(TKey key) {
            CheckCount();
            for ( var i = keys.Count; i-- > 0; ) {
                if ( keys[i].TryGetTarget(out TKey _k) && ReferenceEquals(_k, key) ) {
                    keys.RemoveAt(i);
                    values[i].Dispose();
                    values.RemoveAt(i);
                }
            }
        }

        public bool TryGetValueWithRefCheck(TKey key, out TValue value) {
            CheckCount();
            for ( var i = keys.Count; i-- > 0; ) {
                TKey _k;
                if ( !keys[i].TryGetTarget(out _k) ) {
                    keys.RemoveAt(i);
                    values[i].Dispose();
                    values.RemoveAt(i);
                }
                if ( ReferenceEquals(_k, key) ) {
                    value = values[i];
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        public void RemoveMissingReferences() {
            CheckCount();
            for ( var i = keys.Count; i-- > 0; ) {
                if ( !keys[i].TryGetTarget(out TKey _k) ) {
                    keys.RemoveAt(i);
                    values[i].Dispose();
                    values.RemoveAt(i);
                }
            }
        }

        void CheckCount() {
            if ( keys.Count != values.Count ) { throw new Exception("Mismatched indeces"); }
        }
    }
}