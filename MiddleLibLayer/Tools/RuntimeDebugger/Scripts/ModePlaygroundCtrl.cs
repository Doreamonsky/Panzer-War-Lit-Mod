using System.Collections.Generic;
using System.Linq;
using ShanghaiWindy.Core;
using ShanghaiWindy.Core.GameMode;
using ShanghaiWindy.Core.Lua;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShanghaiWindy.Editor.PlayMode
{
    public class ModePlaygroundCtrl : MonoBehaviour
    {
        public Button enterMode;
        public Button exitMode;

        public Dropdown modeDropdown;

        private List<ILuaGameMode> _luaGameModeInstanceDatas = new List<ILuaGameMode>();

        private ILuaGameMode _curGameModeMod = null;

        private void Start()
        {
            EditorModeStartup.OnInit += () =>
            {
#if UNITY_EDITOR
                foreach (var x in AssetDatabase.FindAssets($"t:{nameof(LuaGameModeInstanceData)}"))
                {
                    var asset =
                        AssetDatabase.LoadAssetAtPath<LuaGameModeInstanceData>(AssetDatabase.GUIDToAssetPath(x));
                    _luaGameModeInstanceDatas.Add(asset.GetInstanceGameMode());
                }
#endif

                // Add local modes
                modeDropdown.AddOptions(_luaGameModeInstanceDatas.Select(x => x.GetGameModeName("CN")).ToList());

                var modeIndex = 0;
                modeDropdown.onValueChanged.AddListener(index => { modeIndex = index; });

                enterMode.onClick.AddListener(() =>
                {
                    GameDataManager.PlayerTeam = TeamManager.Team.blue;

                    if (_curGameModeMod == null)
                    {
                        _curGameModeMod = _luaGameModeInstanceDatas[modeIndex];
                        Debug.Log($"Try running mode {_curGameModeMod.GetGameModeName("CN")}");
                        GameModeManager.Instance.StartGameMode(new LuaGameMode()
                        {
                            GameMode = _curGameModeMod
                        }, null);
                    }
                    else
                    {
                        Debug.LogError("Stop the game mode first before entering a new mode");
                    }
                });

                exitMode.onClick.AddListener(() =>
                {
                    if (_curGameModeMod != null)
                    {
                        // Exit Mode
                        Debug.Log($"Try exit mode {_curGameModeMod.GetGameModeName("CN")}");

                        _curGameModeMod.OnExitMode();
                        _curGameModeMod = null;
                    }
                    else
                    {
                        Debug.LogError("The game mode is not running");
                    }
                });
            };
        }

        private void Update()
        {
            if (_curGameModeMod != null)
            {
                _curGameModeMod.OnUpdated();
            }
        }
    }
}