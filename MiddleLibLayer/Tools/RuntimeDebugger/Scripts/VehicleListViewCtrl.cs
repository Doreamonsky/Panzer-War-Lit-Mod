using System;
using System.Collections.Generic;
using ShanghaiWindy.Core;
using UnityEngine;
using UnityEngine.UI;


namespace ShanghaiWindy.Editor.PlayMode
{
    public class VehicleListViewCtrl : MonoBehaviour
    {
        public Dropdown vehicleDp;
        public Button driveBtn;
        public Button killBtn;
        public Button refreshBtn;
        public Toggle moduleToggle;

        private readonly List<VehicleInfo> _vehicleList = new List<VehicleInfo>();
        private BaseInitSystem _playerVehicle = null;

        public void Awake()
        {
            EditorModeStartup.OnInit += () =>
            {
                foreach (var x in VehicleInfoManager.Instance.GetAllDriveableVehicleList(true))
                {
                    RefershVehicleList(x);
                }

                VehicleInfoManager.OnNewVehicleAdded += (x) => { RefershVehicleList(x); };

                _vehicleList.Sort((a, b) => String.Compare(a.GetDisplayName(), b.GetDisplayName(), StringComparison.Ordinal));
                vehicleDp.options.Sort((a, b) => String.Compare(a.text, b.text, StringComparison.Ordinal));

                vehicleDp.value = PlayerPrefs.GetInt("VehicleDp");
                vehicleDp.onValueChanged.AddListener((val) => { PlayerPrefs.SetInt("VehicleDp", val); });
            };

            driveBtn.onClick.AddListener(() =>
            {
                var vehicleInfo = _vehicleList[vehicleDp.value];
                CreateVehicle(vehicleInfo, Vector3.zero, Quaternion.identity);
            });

            killBtn.onClick.AddListener(KillCurVehicle);

            refreshBtn.onClick.AddListener(() =>
            {
                if (_playerVehicle != null)
                {
                    var curVehicleInfo = VehicleInfoManager.Instance.GetVehicleInfo(_playerVehicle.VehicleName);
                    var pos = _playerVehicle.GetRigidbody().position;
                    var rot = _playerVehicle.GetRigidbody().rotation;

                    KillCurVehicle();
                    CreateVehicle(curVehicleInfo, pos, rot);
                }
            });

            moduleToggle.SetIsOnWithoutNotify(GameDataManager.IsModuleMode);
            moduleToggle.onValueChanged.AddListener(x => { GameDataManager.DamageMode = x ? EDamageMode.ModuleBased : EDamageMode.HealthBased; });
        }

        private void CreateVehicle(VehicleInfo vehicleInfo, Vector3 pos, Quaternion rot)
        {
            _playerVehicle = CreateVehicleUtility.CreatePlayer(vehicleInfo, pos, rot, p => { });
            MouseLockModule.Instance.Hide();
        }

        private void KillCurVehicle()
        {
            _playerVehicle.basePlayerState.OnDeadActionCalled();
            DestroyImmediate(_playerVehicle.gameObject);
        }

        public void RefershVehicleList(VehicleInfo x)
        {
            vehicleDp.AddOptions(new List<Dropdown.OptionData>()
            {
                new Dropdown.OptionData()
                {
                    text = x.GetDisplayName()
                }
            });

            _vehicleList.Add(x);
        }
    }
}