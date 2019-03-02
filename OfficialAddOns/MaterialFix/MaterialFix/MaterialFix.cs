using ShanghaiWindy.Core;
using System.Linq;
using UnityEngine;
using UnityMod;

namespace MaterialFix
{
    public class MaterialFix : IVehicleAddOn
    {
        public void OnVehicleLoaded(int instanceID)
        {
            var playerVehicle = GameObject.FindObjectsOfType<TankInitSystem>().ToList().Find(x => x.gameObject.GetInstanceID() == instanceID);

            AssetShaderManager.ReplaceShader(playerVehicle.gameObject, true);
        }
    }
}
