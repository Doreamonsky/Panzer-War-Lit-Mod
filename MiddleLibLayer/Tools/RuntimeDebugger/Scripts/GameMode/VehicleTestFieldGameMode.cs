using ShanghaiWindy.Core;
using ShanghaiWindy.Core.GameMode;
using UnityEngine;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class VehicleTestReloadEvent : ISHWEvent
    {
    }

    public class VehicleTestKillEvent : ISHWEvent
    {
    }

    public class VehicleTestFieldGameMode : BaseGameMode
    {
        private OfflineMainBattlePlayer _localPlayer;
        private BaseInitSystem _playerVehicle => _localPlayer.Vehicle;

        public override void OnStartNode()
        {
            base.OnStartNode();
            GameDataManager.DamageMode = EDamageMode.ModuleBased;

            MapDataManager.Instance.currentMap = ScriptableObject.CreateInstance<MapData>();

            _localPlayer = OfflineBattlePlayerManager.Instance.CreateOfflineMainPlayer(1, null);
            GameModeManager.Instance.AddBattlePlayer(_localPlayer);
            ShowVehicle();
        }

        public override void AddListeners()
        {
            base.AddListeners();
            SHWEventManager.AddListener<VehiclePickEvent>(HandlePickVehicle);
            SHWEventManager.AddListener<VehicleTestReloadEvent>(HandleVehicleTestReloadEvent);
            SHWEventManager.AddListener<VehicleTestKillEvent>(HandleVehicleTestKillEvent);
        }


        public override void RemoveListeners()
        {
            base.RemoveListeners();
            SHWEventManager.RemoveListener<VehiclePickEvent>(HandlePickVehicle);
            SHWEventManager.RemoveListener<VehicleTestReloadEvent>(HandleVehicleTestReloadEvent);
            SHWEventManager.RemoveListener<VehicleTestKillEvent>(HandleVehicleTestKillEvent);
        }

        public override void OnLogicUpdated(float deltaTime)
        {
        }

        public override void OnFixedUpdated()
        {
        }

        public override void OnFrameUpdated()
        {
        }

        public override bool IsProxyBattle()
        {
            return false;
        }

        public override bool IsOfflineMode()
        {
            return true;
        }

        private void ShowVehicle()
        {
            UIManager.Instance.ShowUI<VehiclePickCtrl>(UIEnum.VEHICLE_PICK_UI,
                res => { res.SetForcePick(true); });
        }

        private void HandlePickVehicle(VehiclePickEvent evtData)
        {
            CreateVehicle(evtData.VehicleInfo, Vector3.zero, Quaternion.identity);
        }

        private void HandleVehicleTestReloadEvent(VehicleTestReloadEvent evtData)
        {
            ReloadVehicle();
        }

        private void HandleVehicleTestKillEvent(VehicleTestKillEvent evtData)
        {
            KillCurVehicle();
            ShowVehicle();
        }


        private void CreateVehicle(VehicleInfo vehicleInfo, Vector3 pos, Quaternion rot)
        {
            _localPlayer.CreateVehicle(vehicleInfo, pos, rot);
        }

        private void ReloadVehicle()
        {
            var curVehicleInfo = VehicleInfoManager.Instance.GetVehicleInfoByName(_playerVehicle.VehicleName);
            var pos = _playerVehicle.GetRigidbody().position;
            var rot = _playerVehicle.GetRigidbody().rotation;

            KillCurVehicle();
            CreateVehicle(curVehicleInfo, pos, rot);
        }

        private void KillCurVehicle()
        {
            _playerVehicle.basePlayerState.OnDeadActionCalled();
            Object.DestroyImmediate(_playerVehicle.gameObject);
        }
    }
}