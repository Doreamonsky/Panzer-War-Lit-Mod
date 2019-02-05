using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityMod;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityModExample
{
    class Tests : IGeneralAddOn
    {
        public void OnFixedUpdate()
        {

        }

        public void OnInitialized()
        {
 
        }

        public void OnNewSceneLoaded(string name)
        {

        }

        public void OnUpdate()
        {
        
        }

        public void OnUpdateGUI()
        {
            /*
             * You can also draw GUI 
             */
            //GUILayout.Label("Mod Active. Author:Doreamonsky");
        }
    }

    class VehilceTests : IVehicleAddOn
    {
        public void OnVehicleLoaded(int instanceID)
        {
            //foreach(var vehicle in GameObject.FindObjectsOfType<TankInitSystem>())
            //{
            //    if(vehicle.gameObject.GetInstanceID() == instanceID)
            //    {
            //        Debug.Log(vehicle.name);
            //    }
            //}
        }
    }
}
