---飞行玩家状态类，继承自基础玩家状态类。
---FlightPlayerState class, inherits from the BasePlayerState.
---@class ShanghaiWindy.Core.FlightPlayerState
local FlightPlayerState = {}

return FlightPlayerState

---@class ShanghaiWindy.Core.BaseFireSystem
local BaseFireSystem = {}

return BaseFireSystem

---基本初始化系统类，用于游戏任何载具的基本初始化。该类实现了 IDynamicDataPatchable 接口和 IIndex 接口。
---The BaseInitSystem class is used for basic initialization of any vehicle. This class implements the IDynamicDataPatchable and IIndex interfaces.
---@class ShanghaiWindy.Core.BaseInitSystem
local BaseInitSystem = {}

return BaseInitSystem

---基础玩家状态类，用于处理玩家状态的改变，如受到伤害、击中反弹等事件。
---Base player state class for handling changes in player states such as taking damage, ricocheting hits, etc.
---@class ShanghaiWindy.Core.BasePlayerState
local BasePlayerState = {}

return BasePlayerState

---@class ShanghaiWindy.Core.TeamManager
local TeamManager = {}

return TeamManager

---弹夹炮逻辑
---@class ShanghaiWindy.Core.TankFire
local TankFire = {}

return TankFire

---车辆组件
---Vehicle Components
---@class ShanghaiWindy.Core.TankVehicleComponents
local TankVehicleComponents = {}

return TankVehicleComponents

---坦克初始化系统
---Tank Initialization System
---@class ShanghaiWindy.Core.TankInitSystem
local TankInitSystem = {}

return TankInitSystem

---@class ShanghaiWindy.Core.TurretController
local TurretController = {}

return TurretController

---Lua 模组接口，用于定义模组的基本信息。
---Lua mod interface, providing basic information about the mod.
---@class ShanghaiWindy.Core.Lua.ILuaBase
local ILuaBase = {}

return ILuaBase

---@class ShanghaiWindy.Core.Lua.ILuaBehavior
local ILuaBehavior = {}

return ILuaBehavior

---Common() 定义下的类型接口。通用 Lua 模块接口，用于定义一组通用的游戏逻辑方法。
---Interrface for Common() define. Common Lua module interface, providing a set of general game logic methods.
---@class ShanghaiWindy.Core.Lua.ILuaCommon
local ILuaCommon = {}

---当模块启动时调用。
---Called when the module is started.
---@instance
---@function [ILuaCommon.OnStarted]
function ILuaCommon.OnStarted() end
---每帧更新时调用。
---Called on every frame update.
---@instance
---@function [ILuaCommon.OnUpdated]
function ILuaCommon.OnUpdated() end
---固定更新时调用。
---Called on fixed update.
---@instance
---@function [ILuaCommon.OnFixedUpdated]
function ILuaCommon.OnFixedUpdated() end
---当绘制图形用户界面时调用。
---Called when drawing the graphical user interface.
---@instance
---@function [ILuaCommon.OnGUI]
function ILuaCommon.OnGUI() end
---在每帧的最后更新时调用。
---Called on the late update of each frame.
---@instance
---@function [ILuaCommon.OnLateUpdated]
function ILuaCommon.OnLateUpdated() end
---当场景加载完成时调用。
---Called when a scene is loaded.
---@instance
---@function [ILuaCommon.OnSceneLoaded]
function ILuaCommon.OnSceneLoaded(levelName) end
return ILuaCommon

---Vehicle() 定义下的类型接口。Lua 游戏载具模块接口，用于定义游戏载具的行为和逻辑。
---Interface for Vehicle() define. Lua vehicle module interface, providing vehicle behaviors and logic.
---@class ShanghaiWindy.Core.Lua.ILuaControllableVehicle
local ILuaControllableVehicle = {}

return ILuaControllableVehicle

---GameMode() 定义下的类型接口。Lua 游戏模式模块接口，用于定义游戏模式的行为和逻辑。
---Interface for GameMode() define. Lua game mode module interface, providing game mode behaviors and logic.
---@class ShanghaiWindy.Core.Lua.ILuaGameMode
local ILuaGameMode = {}

---获取游戏模式的名称。
---Get the name of the game mode.
---@instance
---@function [ILuaGameMode.GetGameModeName]
---@return System.String 游戏模式名称 - Game mode name
function ILuaGameMode.GetGameModeName(lang) end
---当游戏模式开始时调用。
---Called when the game mode starts.
---@instance
---@function [ILuaGameMode.OnStartMode]
function ILuaGameMode.OnStartMode() end
---每帧更新时调用。
---Called on every frame update.
---@instance
---@function [ILuaGameMode.OnUpdated]
function ILuaGameMode.OnUpdated() end
---当退出游戏模式时调用。
---Called when exiting the game mode.
---@instance
---@function [ILuaGameMode.OnExitMode]
function ILuaGameMode.OnExitMode() end
---是否让用户自己管理进入战斗的加载流程。
---Determine if the user should manage the battle loading process.
---@instance
---@function [ILuaGameMode.IsProxyBattle]
---@return System.Boolean 如果为 true，则用户自己管理进入战斗的加载流程。If true, the user manages the battle loading process.
function ILuaGameMode.IsProxyBattle() end
return ILuaGameMode

---@class ShanghaiWindy.Core.Lua.LuaBehaviorMono
local LuaBehaviorMono = {}

---@instance
---@function [LuaBehaviorMono.TryGetTankInitSystem]
function LuaBehaviorMono.TryGetTankInitSystem(tankInitSystem) end
---@instance
---@function [LuaBehaviorMono.TryGetFlightInitSystem]
function LuaBehaviorMono.TryGetFlightInitSystem(flightInitSystem) end
---@instance
---@function [LuaBehaviorMono.TryGetArmyInitSystem]
function LuaBehaviorMono.TryGetArmyInitSystem(armyInitSystem) end
return LuaBehaviorMono

---子弹发射信息
---Bullet Fired Info
---@class ShanghaiWindy.Core.Data.BulletFiredInfo
local BulletFiredInfo = {}

return BulletFiredInfo

---资源加载完成时的委托
---Asset Loaded Delegate
---@class ShanghaiWindy.Core.Delegate.AssetLoadedDelegate
local AssetLoadedDelegate = {}

return AssetLoadedDelegate

---当载具资源和组件加载时触发全局游戏事件。
---Dispatch global event when vehicle assets and components are loaded.
---@class ShanghaiWindy.Core.Delegate.OnGameVehicleLoadedDelegate
local OnGameVehicleLoadedDelegate = {}

return OnGameVehicleLoadedDelegate

---当载具被摧毁时触发全局游戏事件。
---Dispatch global event when vehicle is destroyed.
---@class ShanghaiWindy.Core.Delegate.OnGameVehicleDestroyedDelegate
local OnGameVehicleDestroyedDelegate = {}

return OnGameVehicleDestroyedDelegate

---按键执行时的委托
---Key Performed Delegate
---@class ShanghaiWindy.Core.Delegate.OnKeyPerformed
local OnKeyPerformed = {}

return OnKeyPerformed

---按键取消时的委托
---Key Canceled Delegate
---@class ShanghaiWindy.Core.Delegate.OnKeyCanceled
local OnKeyCanceled = {}

return OnKeyCanceled

---音频事件加载完成时的委托
---Post Event Loaded Delegate
---@class ShanghaiWindy.Core.Delegate.PostEventLoadedDelegate
local PostEventLoadedDelegate = {}

return PostEventLoadedDelegate

---当载具资源和组件加载时触发事件。
---Dispatch event when vehicle assets and components are loaded.
---@class ShanghaiWindy.Core.Delegate.OnVehicleLoadedDelegate
local OnVehicleLoadedDelegate = {}

return OnVehicleLoadedDelegate

---当载具被摧毁时触发事件。
---Dispatch event when vehicle is destroyed.
---@class ShanghaiWindy.Core.Delegate.OnVehicleDestroyedDelegate
local OnVehicleDestroyedDelegate = {}

return OnVehicleDestroyedDelegate

---当载具从场景中移除时触发事件。
---Dispatch event when vehicle is removed from scene.
---@class ShanghaiWindy.Core.Delegate.OnVehicleRemovedDelegate
local OnVehicleRemovedDelegate = {}

return OnVehicleRemovedDelegate

---当载具开火时触发事件。
---Dispatch event when vehicle is fired.
---@class ShanghaiWindy.Core.Delegate.OnFiredDelegate
local OnFiredDelegate = {}

return OnFiredDelegate

---陆军相关 API
---Army-related API
---@class ShanghaiWindy.Core.API.ArmyAPI
local ArmyAPI = {}

---从陆军载具中获取所有武器控制器。
---Get all the weapon controllers from the army vehicle.
---@static
---@function [ArmyAPI.GetWeaponConrollerList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.FPS.WeaponController, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 武器控制器列表 List of weapon controllers
function ArmyAPI.GetWeaponConrollerList(vehicle) end
return ArmyAPI

---资源相关 API
---Asset-related API
---@class ShanghaiWindy.Core.API.AssetAPI
local AssetAPI = {}

---加载资源包
---Load Asset Bundle
---@static
---@function [AssetAPI.LoadAssetBundle]
function AssetAPI.LoadAssetBundle(abName, format, callBack) end
---强制解析资源包
---Force Resolve Package
---@static
---@function [AssetAPI.ForceResolvePackage]
function AssetAPI.ForceResolvePackage() end
return AssetAPI

---@class ShanghaiWindy.Core.API.ComponentAPI
local ComponentAPI = {}

---根据类名获取指定 GameObject 上的 Lua 组件。
---Gets a Lua component with the specified class name on the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponent]
---@return XLua.LuaTable 指定的 Lua 组件，如果不存在则返回 null。The specified Lua component, or null if it does not exist.
function ComponentAPI.GetLuaComponent(target, className) end
---根据类名获取指定 GameObject 上的所有 Lua 组件。
---Gets all Lua components with the specified class name on the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponents]
---@return XLua.LuaTable[] 指定的 Lua 组件数组。An array of the specified Lua components.
function ComponentAPI.GetLuaComponents(target, className) end
---在指定 GameObject 的父级中，根据类名获取所有 Lua 组件。
---Gets all Lua components with the specified class name in the parent hierarchy of the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponentsInParent]
---@return XLua.LuaTable[] 指定的 Lua 组件数组。An array of the specified Lua components.
function ComponentAPI.GetLuaComponentsInParent(target, className) end
---在指定 GameObject 的子级中，根据类名获取所有 Lua 组件。
---Gets all Lua components with the specified class name in the children of the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponentsInChildren]
---@return XLua.LuaTable[] 指定的 Lua 组件数组。An array of the specified Lua components.
function ComponentAPI.GetLuaComponentsInChildren(target, className) end
---根据组件类型名获取指定 GameObject 上的原生组件。
---Gets the native component with the specified type name on the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponent]
---@return UnityEngine.Component 指定的原生组件。The specified native component.
function ComponentAPI.GetNativeComponent(target, componentName) end
---根据组件类型名获取指定 GameObject 上的所有原生组件。
---Gets all native components with the specified type name on the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponents]
---@return UnityEngine.Component[] 指定的原生组件数组。An array of the specified native components.
function ComponentAPI.GetNativeComponents(target, componentName) end
---在指定 GameObject 的父级中，根据组件类型名获取所有原生组件。
---Gets all native components with the specified type name in the parent hierarchy of the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponentsInParent]
---@return UnityEngine.Component[] 指定的原生组件数组。An array of the specified native components.
function ComponentAPI.GetNativeComponentsInParent(target, componentName) end
---在指定 GameObject 的子级中，根据组件类型名获取所有原生组件。
---Gets all native components with the specified type name in the children of the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponentsInChildren]
---@return UnityEngine.Component[] 指定的原生组件数组。An array of the specified native components.
function ComponentAPI.GetNativeComponentsInChildren(target, componentName) end
return ComponentAPI

---飞行器相关 API
---Flight-related API
---@class ShanghaiWindy.Core.API.FlightAPI
local FlightAPI = {}

---从飞行载具中获取所有飞行火力系统。
---Get all the flight fire systems from the flight vehicle.
---@static
---@function [FlightAPI.GetFlightFireList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.FlightFireSystem, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 飞行火力系统列表 List of flight fire systems
function FlightAPI.GetFlightFireList(vehicle) end
return FlightAPI

---游戏相关全局 API
---Game related global api
---@class ShanghaiWindy.Core.API.GameAPI
local GameAPI = {}

---注册车辆生成事件
---Register Vehicl eLoaded Event
---@static
---@function [GameAPI.RegisterVehicleLoadedEvent]
function GameAPI.RegisterVehicleLoadedEvent(gameVehicleLoadedDelegate) end
---注销车辆生成事件
---UnRegister Vehicle Loaded Event
---@static
---@function [GameAPI.UnRegisterVehicleLoadedEvent]
function GameAPI.UnRegisterVehicleLoadedEvent(gameVehicleLoadedDelegate) end
---注册车辆销毁事件
---Register Vehicle Destroyed Event
---@static
---@function [GameAPI.RegisterVehicleDestroyedEvent]
function GameAPI.RegisterVehicleDestroyedEvent(gameVehicleDestroyedDelegate) end
---注销车辆销毁事件
---UnRegister Vehicle Destroyed Event
---@static
---@function [GameAPI.UnRegisterVehicleDestroyedEvent]
function GameAPI.UnRegisterVehicleDestroyedEvent(gameVehicleDestroyedDelegate) end
return GameAPI

---输入处理 API
---Input Handler API
---@class ShanghaiWindy.Core.API.InputAPI
local InputAPI = {}

---注册按键输入
---Register Key Input
---@static
---@function [InputAPI.RegisterKeyInput]
function InputAPI.RegisterKeyInput(actionName, keyCode, keyPerformed, keyCanceled) end
---取消注册按键输入
---Unregister Key Input
---@static
---@function [InputAPI.UnregisterKeyInput]
function InputAPI.UnregisterKeyInput(actionName) end
return InputAPI

---位置点 API
---Point API
---@class ShanghaiWindy.Core.API.PointAPI
local PointAPI = {}

---获取 A 队起始点
---Get Team A Start Points
---@static
---@function [PointAPI.GetTeamAStartPoints]
---@return UnityEngine.GameObject[] 包含 A 队起始点的 GameObject 数组
function PointAPI.GetTeamAStartPoints() end
---获取 B 队起始点
---Get Team B Start Points
---@static
---@function [PointAPI.GetTeamBStartPoints]
---@return UnityEngine.GameObject[] 包含 B 队起始点的 GameObject 数组
function PointAPI.GetTeamBStartPoints() end
---获取巡逻点
---Get Patrol Points
---@static
---@function [PointAPI.GetPatrolPoints]
---@return UnityEngine.GameObject[] 包含巡逻点的 GameObject 数组
function PointAPI.GetPatrolPoints() end
return PointAPI

---声音相关 API
---Sound-related API
---@class ShanghaiWindy.Core.API.SoundAPI
local SoundAPI = {}

---播放一次性音效
---Play One-Shot Sound
---@static
---@function [SoundAPI.PlayOneShot]
function SoundAPI.PlayOneShot(guid, position) end
---发布音频事件
---Post Sound Event
---@static
---@function [SoundAPI.PostEvent]
function SoundAPI.PostEvent(guid, postEventLoadedDelegate) end
---释放音频事件
---Release Sound Event
---@static
---@function [SoundAPI.ReleaseEvent]
function SoundAPI.ReleaseEvent(eventInstance) end
return SoundAPI

---载具生成API
---Vehicle Spawn APIs
---@class ShanghaiWindy.Core.API.SpawnVehicleAPI
local SpawnVehicleAPI = {}

---@static
---@function [SpawnVehicleAPI.CreateLocalBot]
function SpawnVehicleAPI.CreateLocalBot(vehicleInfo, pos, rot, botTeam, isKeepWreckage, isIdle) end
---创建一个本地玩家。您可以从GameDataManager.PlayerTeam设置玩家队伍
---Create a Local Player. You can set player team from GameDataManager.PlayerTeam
---@static
---@function [SpawnVehicleAPI.CreatePlayer]
---@return ShanghaiWindy.Core.BaseInitSystem 在生成位置创建的本地玩家 A created local player at spawn tranform
function SpawnVehicleAPI.CreatePlayer(vehicleInfo, pos, rot, onPreBind) end
---创建一个自由摄像机
---Create a free camera
---@static
---@function [SpawnVehicleAPI.CreateFreeCamera]
---@return ShanghaiWindy.Core.FreeCameraInitSystem 在生成位置创建的自由摄像机 A created free camera at spawn tranform
function SpawnVehicleAPI.CreateFreeCamera(pos, rot) end
return SpawnVehicleAPI

---坦克相关 API
---Tank-related API
---@class ShanghaiWindy.Core.API.TankAPI
local TankAPI = {}

---从坦克载具中获取所有坦克火力系统。
---Get all the tank fire systems from the tank vehicle.
---@static
---@function [TankAPI.GetTankFireList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.TankFire, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 坦克火力系统列表 List of tank fire systems
function TankAPI.GetTankFireList(vehicle) end
return TankAPI

---团队接口
---Team API
---@class ShanghaiWindy.Core.API.TeamAPI
local TeamAPI = {}

---获取玩家所在团队
---Get Player Team
---@static
---@function [TeamAPI.GetPlayerTeam]
---@return ShanghaiWindy.Core.TeamManager+Team 玩家所在团队 Player Team
function TeamAPI.GetPlayerTeam() end
---获取敌对团队
---Get Enemy Team
---@static
---@function [TeamAPI.GetEnemyTeam]
---@return ShanghaiWindy.Core.TeamManager+Team 敌对团队 Enemy Team
function TeamAPI.GetEnemyTeam() end
return TeamAPI

---载具通用 API
---Vehicle Common APIs
---@class ShanghaiWindy.Core.API.VehicleAPI
local VehicleAPI = {}

---注册载具加载事件。
---Register an event dispatched when vehicle loaded.
---@static
---@function [VehicleAPI.RegisterVehicleLoadedEvent]
function VehicleAPI.RegisterVehicleLoadedEvent(vehicle, vehicleLoadedDelegate) end
---取消注册载具加载事件。
---Unregister an event dispatched when vehicle loaded.
---@static
---@function [VehicleAPI.UnRegisterVehicleLoaedEvent]
function VehicleAPI.UnRegisterVehicleLoaedEvent(vehicle, vehicleLoadedDelegate) end
---注册载具被摧毁事件。
---Register an event dispatched when vehicle destroyed.
---@static
---@function [VehicleAPI.RegisterVehicleDestroyedEvent]
function VehicleAPI.RegisterVehicleDestroyedEvent(vehicle, vehicleDestroyedDelegate) end
---注销载具被摧毁时触发的事件。
---Unregister an event dispatched when vehicle destroyed.
---@static
---@function [VehicleAPI.UnRegisterVehicleDestroyedEvent]
function VehicleAPI.UnRegisterVehicleDestroyedEvent(vehicle, vehicleDestroyedDelegate) end
---注册载具物体被摧毁时触发的事件。
---Register an event dispatched when vehicle gamobject destroyed.
---@static
---@function [VehicleAPI.RegisterVehicleGameObjectDestroyedEvent]
function VehicleAPI.RegisterVehicleGameObjectDestroyedEvent(vehicle, vehicleGameObjectDestroyed) end
---注销载具物体被摧毁时触发的事件。
---Unregister an event dispatched when vehicle gamobject destroyed.
---@static
---@function [VehicleAPI.UnRegisterVehicleGameObjectDestroyedEvent]
function VehicleAPI.UnRegisterVehicleGameObjectDestroyedEvent(vehicle, vehicleGameObjectDestroyed) end
---注册载具开火时触发的事件。
---Register an event dispatched when vehicle fired.
---@static
---@function [VehicleAPI.RegisterBulletFiredEvent]
function VehicleAPI.RegisterBulletFiredEvent(vehicle, index, tankFiredDelegate) end
---注销载具开火时触发的事件。
---UnRegister an event dispatched when vehicle fired.
---@static
---@function [VehicleAPI.UnRegisterBulletFiredEvent]
function VehicleAPI.UnRegisterBulletFiredEvent(vehicle, index, tankFiredDelegate) end
---获取玩家的载具。如果玩家未出现，则可能为空。
---Get the player vehicle. It can be null if player is not spawned.
---@static
---@function [VehicleAPI.GetPlayerVehicle]
---@return ShanghaiWindy.Core.BaseInitSystem 一个已存在的本地玩家 A exist local player
function VehicleAPI.GetPlayerVehicle() end
---判断载具是否为坦克类型
---@static
---@function [VehicleAPI.IsTankVehicle]
---@return System.Boolean 返回载具是否为坦克类型 true if TankVehicle, false otherwise
function VehicleAPI.IsTankVehicle(vehicle) end
---判断载具是否为飞行器类型
---@static
---@function [VehicleAPI.IsFlightVehicle]
---@return System.Boolean 返回载具是否为飞行器类型 true if FlightVehicle, false otherwise
function VehicleAPI.IsFlightVehicle(vehicle) end
---判断载具是否为陆军类型
---@static
---@function [VehicleAPI.IsArmyVehicle]
---@return System.Boolean 返回载具是否为陆军类型 true if ArmyVehicle, false otherwise
function VehicleAPI.IsArmyVehicle(vehicle) end
---@static
---@function [VehicleAPI.TryGetTankInitSystemFromGameObject]
function VehicleAPI.TryGetTankInitSystemFromGameObject(gameObject, tankInitSystem) end
---@static
---@function [VehicleAPI.TryGetFlightInitSystemFromGameObject]
function VehicleAPI.TryGetFlightInitSystemFromGameObject(gameObject, flightInitSystem) end
---@static
---@function [VehicleAPI.TryGetArmyInitSystemFromGameObject]
function VehicleAPI.TryGetArmyInitSystemFromGameObject(gameObject, armyInitSystem) end
---从载具中获取所有火力系统。
---Get all the fire system from vehicle.
---@static
---@function [VehicleAPI.GetFireList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.BaseFireSystem, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 火力系统数组 Array of fire system
function VehicleAPI.GetFireList(vehicle) end
---获取载具信息
---Get Vehicle Info
---@static
---@function [VehicleAPI.GetVehicleInfo]
---@return ShanghaiWindy.Core.VehicleInfo 载具信息 Vehicle Info
function VehicleAPI.GetVehicleInfo(vehicleName) end
---获取所有可驾驶载具列表
---Get All Driveable Vehicle List
---@static
---@function [VehicleAPI.GetAllDriveableVehicleList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.VehicleInfo, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 可驾驶载具列表 Driveable Vehicle List
function VehicleAPI.GetAllDriveableVehicleList(ignoreModule) end
return VehicleAPI

