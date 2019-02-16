
using ShanghaiWindy.Core;
using UnityEngine;

namespace Multiplayer
{
    public class ClientOtherPlayerVehicleLogic : BotLogic
    {
        private GameObject virutalTarget;

        public override void Initialize(BotThinkData _thinkData)
        {
            virutalTarget = new GameObject("virutalTarget");

            _thinkData.tankInitSystem.vehicleComponents.mainTurretController.target = virutalTarget.transform;

            _thinkData.ptc.enabled = false;
        }

        public override void OnAttacked(GameObject Attacker)
        {

        }

        public override void Think(BotThinkData botThink)
        {

        }
    }
}
