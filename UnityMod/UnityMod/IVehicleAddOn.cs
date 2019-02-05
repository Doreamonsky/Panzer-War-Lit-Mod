using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityMod
{
    public interface IVehicleAddOn
    {
        /// <summary>
        /// On New Vehicle Loaded. You need to find the Vehicle by InstaceID.
        /// </summary>
        /// <param name="instanceID"></param>
        void OnVehicleLoaded(int instanceID);   
    }
}
