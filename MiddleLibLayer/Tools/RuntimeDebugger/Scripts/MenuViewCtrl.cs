using ShanghaiWindy.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class MenuViewCtrl : MonoBehaviour
    {
        public Button menuBtn;

        private void Start()
        {
            menuBtn.onClick.AddListener(() => { UIManager.Instance.ShowUI(UIEnum.GAME_SETTING_UI); });
        }
    }
}