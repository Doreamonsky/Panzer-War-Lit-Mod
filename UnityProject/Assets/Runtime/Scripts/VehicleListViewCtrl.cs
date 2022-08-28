using ShanghaiWindy.Core.RuntimeEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShanghaiWindy.Core
{
    public class VehicleListViewCtrl : MonoBehaviour
    {
        public EditorStartup editorStartup;
        public Dropdown vehicleDp;
        public Button driveBtn;

        private List<VehicleInfo> vehicleList = new List<VehicleInfo>();

        public void Awake()
        {
            editorStartup.OnInit += () =>
            {
                foreach (var x in VehicleInfoManager.Instance.GetAllDriveableVehicleList(true))
                {
                    RefershVehicleList(x);
                }

                VehicleInfoManager.OnNewVehicleAdded += (x) =>
                {
                    RefershVehicleList(x);
                };
            };

            driveBtn.onClick.AddListener(() =>
            {
                var vehicleInfo = vehicleList[vehicleDp.value];

                GameDataManager.isCoreRP = false;
                CreateVehicleUtility.CreateTankPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, p =>
                {

                });
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
