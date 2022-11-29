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


                switch (vehicleInfo.type)
                {
                    case VehicleInfo.Type.Ground:
                        CreateVehicleUtility.CreateTankPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, p =>
                        {

                        });
                        break;
                    case VehicleInfo.Type.Aviation:
                        CreateVehicleUtility.CreateFlightPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, false, p =>
                        {

                        });
                        break;
                    case VehicleInfo.Type.Army:
                        CreateVehicleUtility.CreateArmyPlayer(vehicleInfo.GetVehicleName(), Vector3.zero, Quaternion.identity, p =>
                        {

                        });
                        break;
                }

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
