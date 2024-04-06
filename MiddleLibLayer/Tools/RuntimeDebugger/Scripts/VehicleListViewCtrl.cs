using ShanghaiWindy.Core;
using UnityEngine;
using UnityEngine.UI;


namespace ShanghaiWindy.Editor.PlayMode
{
    public class VehicleListViewCtrl : MonoBehaviour
    {
        public Button killBtn;
        public Button refreshBtn;


        public void Awake()
        {
            EditorModeStartup.OnInit += () =>
            {
                GameModeManager.Instance.StartGameMode(new VehicleTestFieldGameMode(), null);
            };

            killBtn.onClick.AddListener(SHWEventManager.FastDispatchEvent<VehicleTestKillEvent>);
            refreshBtn.onClick.AddListener(SHWEventManager.FastDispatchEvent<VehicleTestReloadEvent>);
        }
    }
}