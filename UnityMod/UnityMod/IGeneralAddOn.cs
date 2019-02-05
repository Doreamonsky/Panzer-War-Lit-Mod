using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityMod
{
    public interface IGeneralAddOn
    {
        /// <summary>
        /// On AddOn is loaded
        /// </summary>
        void OnInitialized();
        /// <summary>
        /// On Unity Monobehaviour Call Update
        /// </summary>
        void OnUpdate();
        /// <summary>
        /// On Unity Monobehaviour  Call FixedUpdate
        /// </summary>
        void OnFixedUpdate();
        /// <summary>
        /// On New Scene is loaded
        /// </summary>
        void OnNewSceneLoaded(string name);
        /// <summary>
        /// On Unity Monobehaviour Call OnGUI
        /// </summary>
        void OnUpdateGUI();
    }
}
