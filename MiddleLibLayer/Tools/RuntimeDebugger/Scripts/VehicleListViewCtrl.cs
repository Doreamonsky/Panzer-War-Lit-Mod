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

        private List<VehicleInfo> vehicleList = new List<VehicleInfo>();

        public void Awake()
        {
            EditorModeStartup.OnInit += () =>
            {
                foreach (var x in VehicleInfoManager.Instance.GetAllDriveableVehicleList(true))
                {
                    RefershVehicleList(x);
                }

                VehicleInfoManager.OnNewVehicleAdded += (x) => { RefershVehicleList(x); };

                vehicleList.Sort((a, b) => String.Compare(a.GetDisplayName(), b.GetDisplayName(), StringComparison.Ordinal));
                vehicleDp.options.Sort((a, b) => String.Compare(a.text, b.text, StringComparison.Ordinal));

                vehicleDp.value = PlayerPrefs.GetInt("VehicleDp");
                vehicleDp.onValueChanged.AddListener((val) => { PlayerPrefs.SetInt("VehicleDp", val); });
            };

            driveBtn.onClick.AddListener(() =>
            {
                var vehicleInfo = vehicleList[vehicleDp.value];


                switch (vehicleInfo.type)
                {
                    case VehicleInfo.Type.Ground:
                        CreateVehicleUtility.CreateTankPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, p => { });
                        break;
                    case VehicleInfo.Type.Aviation:
                        CreateVehicleUtility.CreateFlightPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, false, p => { });
                        break;
                    case VehicleInfo.Type.Army:
                        CreateVehicleUtility.CreateArmyPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, p => { });
                        break;
                }

                MouseLockModule.Instance.Hide();
            });
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

            vehicleList.Add(x);
        }
    }
}