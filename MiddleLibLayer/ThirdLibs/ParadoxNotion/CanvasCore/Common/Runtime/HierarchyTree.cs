using System.Collections.Generic;

namespace ParadoxNotion
{

    ///<summary>A simple general purpose hierarchical tree.</summary>
    public class HierarchyTree
    {
        //..with nothing inside right now

        ///<summary>A simple general purpose hierarchical tree element.</summary>
        public class Element
        {

            private object _reference;
            private Element _parent;
            private List<Element> _children;

            public object reference => _reference;
            public Element parent => _parent;
            public IEnumerable<Element> children => _children;

            public Element(object reference) {
                this._reference = reference;
            }

            ///<summary>Add a child element</summary>
            public Element AddChild(Element child) {
                if ( _children == null ) { _children = new List<Element>(); }
                child._parent = this;
                _children.Add(child);
                return child;
            }

            ///<summary>Remove a child element</summary>
            public void RemoveChild(Element child) {
                if ( _children != null ) {
                    _children.Remove(child);
                }
            }

            ///<summary>Get root element</summary>
            public Element GetRoot() {
                var current = _parent;
                while ( current != null ) {
                    current = current._parent;
                }
                return current;
            }

            ///<summary>Returns the first found Element that references target object</summary>
            public Element FindReferenceElement(object target) {
                if ( this._reference == target ) { return this; }
                if ( _children == null ) { return null; }
                for ( var i = 0; i < _children.Count; i++ ) {
                    var sub = _children[i].FindReferenceElement(target);
                    if ( sub != null ) {
                        return sub;
                    }
                }
                return null;
            }

            ///<summary>Get first parent reference of type T, including self element</summary>
            public T GetFirstParentReferenceOfType<T>() {
                if ( this._reference is T ) { return (T)_reference; }
                return _parent != null ? _parent.GetFirstParentReferenceOfType<T>() : default(T);
            }

            ///<summary>Get all children references of type T recursively</summary>
            public IEnumerable<T> GetAllChildrenReferencesOfType<T>() {
                if ( _children == null ) { yield break; }
                for ( var i = 0; i < _children.Count; i++ ) {
                    var element = _children[i];
                    if ( element._reference is T ) {
                        yield return (T)element._reference;
                    }
                    foreach ( var deep in element.GetAllChildrenReferencesOfType<T>() ) {
                        yield return deep;
                    }
                }
            }
        }
    }
}