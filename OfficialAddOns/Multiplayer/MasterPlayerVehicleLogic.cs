
using ShanghaiWindy.Core;
using UnityEngine;

namespace Multiplayer
{
    public class MasterPlayerVehicleLogic : BotLogic
    {
        private GameObject virutalTarget;

        public override void Initialize(BotThinkData _thinkData)
        {
            virutalTarget = new GameObject("virutalTarget");

            _thinkData.tankInitSystem.vehicleComponents.mainTurretController.target = virutalTarget.transform;
        }

        public override void OnAttacked(GameObject Attacker)
        {

        }

        public override void Think(BotThinkData botThink)
        {

        }
    }
}
