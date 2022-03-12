using System.Collections.Generic;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Framework
{

    ///<summary> Global Blackboards are accessible from any BBParameter.</summary>
    [ExecuteInEditMode]
    public class GlobalBlackboard : Blackboard, IGlobalBlackboard
    {
        public enum SingletonMode
        {
            DestroyComponentOnly,
            DestroyEntireGameObject
        }

        [SerializeField] private string _UID = System.Guid.NewGuid().ToString();
        [Tooltip("A *unique* identifier of this Global Blackboard")]
        [SerializeField] private string _identifier;
        [Tooltip("If a duplicate with the same identifier is encountered, destroy the previous Global Blackboard component only, or the previous Global Blackboard gameobject entirely?")]
        [SerializeField] private SingletonMode _singletonMode = SingletonMode.DestroyComponentOnly;
        [Tooltip("If true, the Global Blackboard will not be destroyed when another scene is loaded.")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private static List<GlobalBlackboard> _allGlobals = new List<GlobalBlackboard>();

        public string identifier => _identifier;
        public string UID => _UID;
        new public string name => identifier;

        ///----------------------------------------------------------------------------------------------

        ///<summary>A collection of all the current active global blackboards in the scene</summary>
        public static IEnumerable<GlobalBlackboard> GetAll() {
            return _allGlobals;
        }

        ///<summary>Create a global blackboard</summary>
        public static GlobalBlackboard Create() {
            var bb = new GameObject("@GlobalBlackboard").AddComponent<GlobalBlackboard>();
            bb._identifier = "Global";
            return bb;
        }

        ///<summary>Get a global blackboard by it's name</summary>
        public static GlobalBlackboard Find(string name) {
            return _allGlobals.Find(b => b.identifier == name);
        }

        //...
        protected void OnEnable() {
            if ( IsPrefabAsset() ) { return; }
            if ( string.IsNullOrEmpty(_identifier) ) { _identifier = gameObject.name; }
            if ( Application.isPlaying ) {
                if ( Find(identifier) != null ) {
                    Logger.Log(string.Format("There exist more than one Global Blackboards with same identifier name '{0}'. The old one will now be destroyed.", identifier), LogTag.BLACKBOARD, this);
                    if ( _singletonMode == SingletonMode.DestroyComponentOnly ) { Destroy(this); }
                    if ( _singletonMode == SingletonMode.DestroyEntireGameObject ) { Destroy(this.gameObject); }
                    return;
                }
                if ( _dontDestroyOnLoad ) { DontDestroyOnLoad(this.gameObject); }
                this.InitializePropertiesBinding(( (IBlackboard)this ).propertiesBindTarget, false);
            }
            if ( !_allGlobals.Contains(this) ) { _allGlobals.Add(this); }
        }

        //...
        protected void OnDisable() {
            if ( IsPrefabAsset() ) { return; }
            _allGlobals.Remove(this);
        }

        //...
        protected override void OnValidate() {
            base.OnValidate();

#if UNITY_EDITOR
            if ( UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null ) { return; }
#endif

            if ( Application.isPlaying || IsPrefabAsset() ) { return; }
            if ( !_allGlobals.Contains(this) ) { _allGlobals.Add(this); }
            if ( string.IsNullOrEmpty(_identifier) ) { _identifier = gameObject.name; }
            var existing = Find(identifier);
            if ( existing != this && existing != null ) {
                Logger.LogError(string.Format("Another blackboard with the same identifier name '{0}' exists. Please rename either.", identifier), LogTag.BLACKBOARD, this);
            }
        }

        public override string ToString() { return identifier; }

        ///----------------------------------------------------------------------------------------------

        //...
        bool IsPrefabAsset() {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.IsPersistent(this);
#else
            return false;
#endif
        }
    }
}