using System;
using ShanghaiWindy.Core;
using UnityEngine;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class SpawnBotVehicle : MonoBehaviour
    {
        [SHWLabelText("人机阵营 - Bot Team")] public TeamManager.Team botTeam = TeamManager.Team.blue;

        [SHWLabelText("是否 Idle - Is Idle")] public bool isIdle;

        [SHWLabelText("载具人机 - Bot Vehicle Info")]
        public VehicleInfo botVehicleInfo;

        private void Awake()
        {
            EditorModeStartup.OnInit += () =>
            {
                if (botVehicleInfo != null)
                {
                    if (VehicleInfoManager.Instance.GetVehicleInfo(botVehicleInfo.GetVehicleName()) != null)
                    {
                        CreateVehicleUtility.CreateLocalBot(botVehicleInfo.GetVehicleName(), transform.position, transform.rotation, botTeam, true, isIdle);
                    }
                }
            };
        }
    }
}