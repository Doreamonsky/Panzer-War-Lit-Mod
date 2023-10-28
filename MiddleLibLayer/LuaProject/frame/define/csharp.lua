---@class ShanghaiWindy.Core.UIEnum
local UIEnum = {}

return UIEnum

---@class ShanghaiWindy.Data.ZoneChain
---@field fromZone System.String
---@field toZone System.String
local ZoneChain = {}

return ZoneChain

---@class ShanghaiWindy.Data.ZoneCapturePoint
---@field defaultOwner ShanghaiWindy.Core.TeamManager+Team
---@field pointName System.String
---@field position UnityEngine.Vector3
---@field radius System.Single
local ZoneCapturePoint = {}

return ZoneCapturePoint

---@class ShanghaiWindy.Data.CaptureZone
---@field zonePoints UnityEngine.Vector3[]
---@field zoneCapturePoint ShanghaiWindy.Data.ZoneCapturePoint
---@field spawnTransformInfos ShanghaiWindy.Data.TransformInfo[]
local CaptureZone = {}

return CaptureZone

---@class ShanghaiWindy.Data.CaptureZoneModeConfig
---@field captureZoneName System.String
---@field displayName ShanghaiWindy.Core.LocalizedName
---@field mapGuid System.String
---@field captureZones ShanghaiWindy.Data.CaptureZone[]
---@field zoneChains ShanghaiWindy.Data.ZoneChain[]
---@field backgroundCameraTransformInfo ShanghaiWindy.Data.TransformInfo
local CaptureZoneModeConfig = {}

return CaptureZoneModeConfig

---@class ShanghaiWindy.Data.TransformInfo
---@field pos UnityEngine.Vector3
---@field eulerAngle UnityEngine.Vector3
local TransformInfo = {}

return TransformInfo

---@class ShanghaiWindy.Data.WaveAttackInfo
---@field comment System.String
---@field vehicleGuid System.String
---@field canShowWayPoint System.Boolean
---@field waveWayPoints UnityEngine.Vector3[]
---@field waveTransformInfos ShanghaiWindy.Data.TransformInfo[]
local WaveAttackInfo = {}

return WaveAttackInfo

---@class ShanghaiWindy.Data.WaveInfo
---@field friendWaveList ShanghaiWindy.Data.WaveAttackInfo[]
---@field enemyWaveList ShanghaiWindy.Data.WaveAttackInfo[]
---@field destSize UnityEngine.Vector3
---@field destTransformInfo ShanghaiWindy.Data.TransformInfo
local WaveInfo = {}

return WaveInfo

---@class ShanghaiWindy.Data.WaveMissionConfig
---@field missionName System.String
---@field displayName ShanghaiWindy.Core.LocalizedName
---@field mapGuid System.String
---@field maxEscapeEnemyCount System.Int32
---@field playerTeam ShanghaiWindy.Core.TeamManager+Team
---@field playerVehicleType ShanghaiWindy.Data.EPlayerVehicleType
---@field playerPickMinRank System.Int32
---@field playerPickMaxRank System.Int32
---@field playerTransformInfo ShanghaiWindy.Data.TransformInfo
---@field waveInfos ShanghaiWindy.Data.WaveInfo[]
local WaveMissionConfig = {}

return WaveMissionConfig

---@class ShanghaiWindy.Data.MaterialConfig
local MaterialConfig = {}

return MaterialConfig

---飞行玩家状态类，继承自基础玩家状态类。
---FlightPlayerState class, inherits from the BasePlayerState.
---@class ShanghaiWindy.Core.FlightPlayerState
---@field vehicleName System.String @车辆名称。             Vehicle name.
---@field hp ShanghaiWindy.Core.PropValue`1[System.Int32] @玩家的生命值属性。             Player's health points property.
---@field defaultHP System.Int32 @默认生命值，用于初始化角色的生命值。Default Hit Points (HP) used for initializing the character's health.
---@field isFakeHP System.Boolean @是否为假生命值。             Whether it's fake health points.
---@field isGodMode ShanghaiWindy.Core.PropValue`1[System.Boolean] @是否开启上帝模式，若为 true，则角色无敌。             If God Mode is enabled (true), the character is invincible.
---@field lastBeHitPosition UnityEngine.Vector3 @上次受到伤害的位置。             The position where the character was last hit.
---@field totalDamage ShanghaiWindy.Core.PropValue`1[System.Int32] @角色总共造成的伤害。             The total damage the character has caused.
---@field totalDesytroyed ShanghaiWindy.Core.PropValue`1[System.Int32] @角色总共摧毁的物体数量。             The total number of objects the character has destroyed.
---@field totalBlockDamage ShanghaiWindy.Core.PropValue`1[System.Int32] @角色总共阻挡的伤害。             The total damage the character has blocked.
---@field DamageResistanceBuffedCoff ShanghaiWindy.Core.BuffProperty @伤害阻挡 Buff             Damage resistance buff
---@field OnDamaged UnityEngine.Events.UnityEvent`2[System.Int32,UnityEngine.Vector3]
---@field OnRichocheted UnityEngine.Events.UnityEvent
---@field OnNotBreakDowned UnityEngine.Events.UnityEvent
---@field isDestroyed System.Boolean
---@field ViewRangeBuffedCoff ShanghaiWindy.Core.BuffProperty
---@field RepairSpeedBuffedCoff ShanghaiWindy.Core.BuffProperty
local FlightPlayerState = {}

return FlightPlayerState

---@class ShanghaiWindy.Core.VehicleInfo
local VehicleInfo = {}

---@instance
---@function [VehicleInfo:GetDisplayName]
---@return System.String
function VehicleInfo:GetDisplayName() end
---@instance
---@function [VehicleInfo:GetVehicleName]
---@return System.String
function VehicleInfo:GetVehicleName() end
---@instance
---@function [VehicleInfo:GetRepairCost]
---@return System.Int32
---@param isMultiplayer System.Boolean
function VehicleInfo:GetRepairCost(isMultiplayer) end
---@instance
---@function [VehicleInfo:GetResearchExp]
---@return System.Int32
function VehicleInfo:GetResearchExp() end
---@instance
---@function [VehicleInfo:IsResearched]
---@return System.Boolean
function VehicleInfo:IsResearched() end
---@instance
---@function [VehicleInfo:IsSupportModuleMode]
---@return System.Boolean
function VehicleInfo:IsSupportModuleMode() end
---@instance
---@function [VehicleInfo:GetRank]
---@return System.Int32
function VehicleInfo:GetRank() end
---@instance
---@function [VehicleInfo:GetUUID]
---@return System.String
function VehicleInfo:GetUUID() end
return VehicleInfo

---@class ShanghaiWindy.Core.CaptureZoneTask
---@field ZoneId System.Int32
---@field Weight System.Int32
local CaptureZoneTask = {}

return CaptureZoneTask

---@class ShanghaiWindy.Core.BaseFireSystem
local BaseFireSystem = {}

return BaseFireSystem

---基本初始化系统类，用于游戏任何载具的基本初始化。该类实现了 IDynamicDataPatchable 接口和 IIndex 接口。
---The BaseInitSystem class is used for basic initialization of any vehicle. This class implements the IDynamicDataPatchable and IIndex interfaces.
---@class ShanghaiWindy.Core.BaseInitSystem
---@field basePlayerState ShanghaiWindy.Core.BasePlayerState @基础玩家状态类，用于处理玩家状态的改变，如受到伤害、击中反弹等事件。             Base player state class for handling changes in player states such as taking damage, ricocheting hits, etc.
---@field equipmentBuffDataList System.Collections.Generic.List`1[ShanghaiWindy.Core.EquipmentBuffData] @装备 Buff 数据列表             Equipment Buff Data List
---@field OnVehicleLoaded UnityEngine.Events.UnityEvent
---@field OnVehicleDestroyed UnityEngine.Events.UnityEvent
---@field OnGameObjectDestroyed UnityEngine.Events.UnityEvent
---@field OnDamagedInternalModule ShanghaiWindy.Core.BaseInitSystem+DamagedInternalModule
---@field OwnerTeam ShanghaiWindy.Core.TeamManager+Team
---@field IsDestroyed System.Boolean
---@field IsLoaded System.Boolean
local BaseInitSystem = {}

return BaseInitSystem

---基础玩家状态类，用于处理玩家状态的改变，如受到伤害、击中反弹等事件。
---Base player state class for handling changes in player states such as taking damage, ricocheting hits, etc.
---@class ShanghaiWindy.Core.BasePlayerState
---@field vehicleName System.String @车辆名称。             Vehicle name.
---@field hp ShanghaiWindy.Core.PropValue`1[System.Int32] @玩家的生命值属性。             Player's health points property.
---@field defaultHP System.Int32 @默认生命值，用于初始化角色的生命值。Default Hit Points (HP) used for initializing the character's health.
---@field isFakeHP System.Boolean @是否为假生命值。             Whether it's fake health points.
---@field isGodMode ShanghaiWindy.Core.PropValue`1[System.Boolean] @是否开启上帝模式，若为 true，则角色无敌。             If God Mode is enabled (true), the character is invincible.
---@field lastBeHitPosition UnityEngine.Vector3 @上次受到伤害的位置。             The position where the character was last hit.
---@field totalDamage ShanghaiWindy.Core.PropValue`1[System.Int32] @角色总共造成的伤害。             The total damage the character has caused.
---@field totalDesytroyed ShanghaiWindy.Core.PropValue`1[System.Int32] @角色总共摧毁的物体数量。             The total number of objects the character has destroyed.
---@field totalBlockDamage ShanghaiWindy.Core.PropValue`1[System.Int32] @角色总共阻挡的伤害。             The total damage the character has blocked.
---@field DamageResistanceBuffedCoff ShanghaiWindy.Core.BuffProperty @伤害阻挡 Buff             Damage resistance buff
---@field OnDamaged UnityEngine.Events.UnityEvent`2[System.Int32,UnityEngine.Vector3]
---@field OnRichocheted UnityEngine.Events.UnityEvent
---@field OnNotBreakDowned UnityEngine.Events.UnityEvent
---@field isDestroyed System.Boolean
---@field ViewRangeBuffedCoff ShanghaiWindy.Core.BuffProperty
---@field RepairSpeedBuffedCoff ShanghaiWindy.Core.BuffProperty
local BasePlayerState = {}

return BasePlayerState

---@class ShanghaiWindy.Core.ECaptureStage
local ECaptureStage = {}

return ECaptureStage

---@class ShanghaiWindy.Core.CaptureZoneInfo
---@field zoneName System.String
---@field point UnityEngine.Vector3
---@field radius System.Single
---@field currentCaptureProgress System.Single
---@field capturingTeam ShanghaiWindy.Core.TeamManager+Team
---@field captureStage ShanghaiWindy.Core.ECaptureStage
local CaptureZoneInfo = {}

---@instance
---@function [CaptureZoneInfo:IsComplete]
---@return System.Boolean
function CaptureZoneInfo:IsComplete() end
---@instance
---@function [CaptureZoneInfo:GetIndex]
---@return System.Int64
function CaptureZoneInfo:GetIndex() end
return CaptureZoneInfo

---@class ShanghaiWindy.Core.EIconPosition
local EIconPosition = {}

return EIconPosition

---战斗玩家数据
---Battle Player
---@class ShanghaiWindy.Core.AbstractBattlePlayer
---@field Uid System.Int32 @唯一 Id             Uid
---@field NickName System.String @昵称             Nick name
---@field VehicleInfo ShanghaiWindy.Core.VehicleInfo @载具信息             Vehicle Info
---@field Vehicle ShanghaiWindy.Core.BaseInitSystem @载具             Vehicle
---@field Info System.Object
---@field OnVehicleLoaded UnityEngine.Events.UnityEvent
---@field OnVehicleDestroyed UnityEngine.Events.UnityEvent
---@field OnGameObjectDestroyed UnityEngine.Events.UnityEvent
---@field IsAlive System.Boolean
local AbstractBattlePlayer = {}

---@instance
---@function [AbstractBattlePlayer:IsLocalPlayer]
---@return System.Boolean
function AbstractBattlePlayer:IsLocalPlayer() end
---@instance
---@function [AbstractBattlePlayer:GetVehicleName]
---@return System.String
function AbstractBattlePlayer:GetVehicleName() end
---获取总伤害
---Get total damaged
---@instance
---@function [AbstractBattlePlayer:GetTotalDamage]
---@return System.Int32
function AbstractBattlePlayer:GetTotalDamage() end
---获取总击毁数
---Get total destroyed number
---@instance
---@function [AbstractBattlePlayer:GetTotalDestroyed]
---@return System.Int32
function AbstractBattlePlayer:GetTotalDestroyed() end
---创建载具
---Create Vehicle
---@instance
---@function [AbstractBattlePlayer:CreateVehicle]
---@return ShanghaiWindy.Core.BaseInitSystem
---@param vehicleInfo ShanghaiWindy.Core.VehicleInfo
---@param pos UnityEngine.Vector3
---@param rot UnityEngine.Quaternion
function AbstractBattlePlayer:CreateVehicle(vehicleInfo, pos, rot) end
---获得队伍
---Get Team
---@instance
---@function [AbstractBattlePlayer:GetTeam]
---@return ShanghaiWindy.Core.TeamManager+Team
function AbstractBattlePlayer:GetTeam() end
return AbstractBattlePlayer

---@class ShanghaiWindy.Core.TeamManager
local TeamManager = {}

return TeamManager

---弹夹炮逻辑
---@class ShanghaiWindy.Core.TankFire
---@field mainBody UnityEngine.Transform @车体 Transform             Main Body Transform
---@field ffPoint UnityEngine.Transform @FF 点 Transform             FF Point Transform
---@field fireEffectPoint UnityEngine.Transform @射击特效点 Transform             Fire Effect Point Transform
---@field fireRecoilPoint UnityEngine.Transform @射击后坐力点 Transform             Fire Recoil Point Transform
---@field gunDym UnityEngine.Transform @Gun Transform
---@field attachedRigidbody UnityEngine.Rigidbody @附加刚体             Attached Rigidbody
---@field currentBulletId System.Int32 @当前炮弹 ID             Current Bullet ID
---@field isAutoCaclulateGravity System.Boolean @是否自动计算重力             Is Auto Calculate Gravity
---@field netType ShanghaiWindy.Core.InstanceNetType @网络类型             Instance Net Type
---@field isExtraTurret System.Boolean @是否为额外炮塔             Is Extra Turret
---@field canControl System.Boolean @是否可控制             Can Control
---@field useGravity System.Boolean @使用重力             Use Gravity
---@field projectile ShanghaiWindy.Core.ProjectileManager @投射物管理器             Projectile Manager
---@field calibratedPoint UnityEngine.Vector3 @校准点             Calibrated Point
---@field isPlayVFX System.Boolean @是否播放 VFX             Is Play VFX
---@field overrideVelocity UnityEngine.Vector3 @覆盖速度             Override Velocity
---@field bulletCountList System.Int32[]
---@field currentBullet ShanghaiWindy.Core.BulletData
local TankFire = {}

return TankFire

---坦克初始化系统
---Tank Initialization System
---@class ShanghaiWindy.Core.TankInitSystem
---@field InstanceMesh UnityEngine.GameObject @实例网格             Instance Mesh
---@field vehicleTextData ShanghaiWindy.Core.VehicleTextData @车辆文本数据             Vehicle Text Data
---@field referenceManager ShanghaiWindy.Core.VehicleComponentsReferenceManager @车辆组件引用管理器             Vehicle Components Reference Manager
---@field diyReferenceManager ShanghaiWindy.Core.DIYVehicleComponentsReferenceManager @DIY 车辆组件引用管理器             DIY Vehicle Components Reference Manager
---@field vehicleComponents ShanghaiWindy.Core.TankVehicleComponents @车辆组件集合             Vehicle Components Collection
---@field thinkLogic ShanghaiWindy.Core.BotLogic @机器人逻辑             Bot Logic
---@field vehicleInfo ShanghaiWindy.Core.VehicleInfo @车辆信息             Vehicle Information
---@field vehicleRemoveManagerModule ShanghaiWindy.Core.VehicleRemoveManagerModule @车辆移除管理模块             Vehicle Remove Manager Module
---@field basePlayerState ShanghaiWindy.Core.BasePlayerState @基础玩家状态类，用于处理玩家状态的改变，如受到伤害、击中反弹等事件。             Base player state class for handling changes in player states such as taking damage, ricocheting hits, etc.
---@field equipmentBuffDataList System.Collections.Generic.List`1[ShanghaiWindy.Core.EquipmentBuffData] @装备 Buff 数据列表             Equipment Buff Data List
---@field CurEngineParam ShanghaiWindy.Core.PlayerTankControllerParameter
---@field CurMainTankFireParam ShanghaiWindy.Core.TankFireParameter
---@field CurMainTurretParam ShanghaiWindy.Core.MouseTurretParameter
---@field CurPlayerStateParam ShanghaiWindy.Core.PlayerStateParameter
---@field OnVehicleLoaded UnityEngine.Events.UnityEvent
---@field OnVehicleDestroyed UnityEngine.Events.UnityEvent
---@field OnGameObjectDestroyed UnityEngine.Events.UnityEvent
---@field OnDamagedInternalModule ShanghaiWindy.Core.BaseInitSystem+DamagedInternalModule
---@field OwnerTeam ShanghaiWindy.Core.TeamManager+Team
---@field IsDestroyed System.Boolean
---@field IsLoaded System.Boolean
local TankInitSystem = {}

return TankInitSystem

---车辆组件
---Vehicle Components
---@class ShanghaiWindy.Core.TankVehicleComponents
---@field vehicleInputController ShanghaiWindy.Core.VehicleInputController @车辆输入控制器             Vehicle Input Controller
---@field tankTracksController ShanghaiWindy.Core.TankTracksController @坦克履带控制器             Tank Track Controller
---@field playerCamera ShanghaiWindy.Core.GroundCameraController @玩家相机控制器             Player Camera Controller
---@field mainCamera UnityEngine.Camera @主相机             Main Camera
---@field mainTankFireIndex ShanghaiWindy.Core.PropValue`1[System.Int32] @主坦克火力索引             Main Tank Fire Index
---@field fireSystemList System.Collections.Generic.List`1[ShanghaiWindy.Core.FireSystemConfigure] @火力系统配置列表             Fire System Configuration List
---@field tankfireList System.Collections.Generic.List`1[ShanghaiWindy.Core.TankFire] @坦克火力列表             Tank Fire List
---@field selfExplosionList System.Collections.Generic.List`1[ShanghaiWindy.Core.VehicleSelfExplosionFireSystem] @自爆火力系统列表             Vehicle Self-Explosion Fire System List
---@field turretControllerList System.Collections.Generic.List`1[ShanghaiWindy.Core.TurretController] @炮塔控制器列表             Turret Controller List
---@field identity ShanghaiWindy.Core.Identity @身份信息             Identity
---@field tankState ShanghaiWindy.Core.TankState @坦克状态             Tank State
---@field playerState ShanghaiWindy.Core.TankPlayerState @坦克玩家状态             Tank Player State
---@field basePlayerState ShanghaiWindy.Core.BasePlayerState @基本玩家状态             Base Player State
---@field damageStickManager ShanghaiWindy.Core.VehicleDamageStickManager @车辆伤害棒管理器             Vehicle Damage Stick Manager
---@field HitBoxes System.Collections.Generic.List`1[ShanghaiWindy.Core.HitBox] @碰撞箱列表             Hit Box List
---@field rigidbody UnityEngine.Rigidbody @刚体组件             Rigidbody Component
---@field vehicleFireCrossEffect ShanghaiWindy.Core.VehicleFireCrossEffect @车辆火力交叉特效             Vehicle Fire Cross Effect
---@field renderers UnityEngine.Renderer[] @渲染器数组             Renderer Array
---@field mainBodyVisiblity ShanghaiWindy.Core.MainBodyVisibity @主体可见性             Main Body Visibility
---@field mainBodyTransform UnityEngine.Transform @主体变换组件             Main Body Transform Component
---@field fireAssistComponent ShanghaiWindy.Core.FireAssistComponet @火力辅助组件             Fire Assist Component
---@field fireLockComponent ShanghaiWindy.Core.FireLockComponent @火力锁定组件             Fire Lock Component
---@field tracksControllers System.Collections.Generic.List`1[ShanghaiWindy.Core.TracksController] @履带控制器列表             Tracks Controller List
---@field tankFireFireGroupManager ShanghaiWindy.Core.TankFireFireGroupManager @坦克火力分组管理器             Tank Fire Group Manager
---@field mainTankFire ShanghaiWindy.Core.TankFire
---@field mainTurretController ShanghaiWindy.Core.TurretController
local TankVehicleComponents = {}

return TankVehicleComponents

---@class ShanghaiWindy.Core.TurretController
---@field turretParams ShanghaiWindy.Core.MouseTurretParameter @炮塔参数             Turret Parameters
---@field target UnityEngine.Transform @目标             Target
---@field turretTransform UnityEngine.Transform @炮塔 Transform             Turret Transform
---@field gunTransform UnityEngine.Transform @炮管 Transform             Gun Transform
---@field dymTransform UnityEngine.Transform @炮管伸缩 Transform             Dym Transform
---@field InitialTrans UnityEngine.Transform @初始变换             Initial Transform
---@field isTargetWithinRange System.Boolean @目标是否在射程内             Is Target Within Range
---@field isLocked System.Boolean @是否锁定             Is Locked
---@field fireAngle System.Single @射击角度             Fire Angle
---@field turretParamProvider ShanghaiWindy.Core.Interface.ITurretParamProvider
---@field isLocalPlayer System.Boolean
---@field TurretSpeedBuffedCoff ShanghaiWindy.Core.BuffProperty
---@field GunSpeedBuffedCoff ShanghaiWindy.Core.BuffProperty
local TurretController = {}

return TurretController

---Lua 模组接口，用于定义模组的基本信息。
---Lua mod interface, providing basic information about the mod.
---@class ShanghaiWindy.Core.Lua.ILuaBase
---@field modName System.String
---@field author System.String
---@field description System.String
---@field isPreview System.Boolean
local ILuaBase = {}

return ILuaBase

---@class ShanghaiWindy.Core.Lua.ILuaBehavior
local ILuaBehavior = {}

return ILuaBehavior

---@class ShanghaiWindy.Core.Lua.ILuaBuffCaster
local ILuaBuffCaster = {}

return ILuaBuffCaster

---Common() 定义下的类型接口。通用 Lua 模块接口，用于定义一组通用的游戏逻辑方法。
---Interrface for Common() define. Common Lua module interface, providing a set of general game logic methods.
---@class ShanghaiWindy.Core.Lua.ILuaCommon
local ILuaCommon = {}

---当模块启动时调用。
---Called when the module is started.
---@instance
---@function [ILuaCommon:OnStarted]
function ILuaCommon:OnStarted() end
---每帧更新时调用。
---Called on every frame update.
---@instance
---@function [ILuaCommon:OnUpdated]
function ILuaCommon:OnUpdated() end
---固定更新时调用。
---Called on fixed update.
---@instance
---@function [ILuaCommon:OnFixedUpdated]
function ILuaCommon:OnFixedUpdated() end
---当绘制图形用户界面时调用。
---Called when drawing the graphical user interface.
---@instance
---@function [ILuaCommon:OnGUI]
function ILuaCommon:OnGUI() end
---在每帧的最后更新时调用。
---Called on the late update of each frame.
---@instance
---@function [ILuaCommon:OnLateUpdated]
function ILuaCommon:OnLateUpdated() end
---当场景加载完成时调用。
---Called when a scene is loaded.
---@instance
---@function [ILuaCommon:OnSceneLoaded]
---@param levelName System.String
function ILuaCommon:OnSceneLoaded(levelName) end
---Lua 环境释放
---@instance
---@function [ILuaCommon:OnDispose]
function ILuaCommon:OnDispose() end
return ILuaCommon

---Vehicle() 定义下的类型接口。Lua 游戏载具模块接口，用于定义游戏载具的行为和逻辑。
---Interface for Vehicle() define. Lua vehicle module interface, providing vehicle behaviors and logic.
---@class ShanghaiWindy.Core.Lua.ILuaControllableVehicle
local ILuaControllableVehicle = {}

return ILuaControllableVehicle

---GameMode() 定义下的类型接口。Lua 游戏模式模块接口，用于定义游戏模式的行为和逻辑。
---Interface for GameMode() define. Lua game mode module interface, providing game mode behaviors and logic.
---@class ShanghaiWindy.Core.Lua.ILuaGameMode
---@field luaClass XLua.LuaTable
local ILuaGameMode = {}

---获取游戏模式的名称。
---Get the name of the game mode.
---@instance
---@function [ILuaGameMode:GetGameModeName]
---@return System.String 游戏模式名称 - Game mode name
---@param lang System.String
function ILuaGameMode:GetGameModeName(lang) end
---当游戏模式开始时调用。
---Called when the game mode starts.
---@instance
---@function [ILuaGameMode:OnStartMode]
function ILuaGameMode:OnStartMode() end
---每帧更新时调用。
---Called on every frame update.
---@instance
---@function [ILuaGameMode:OnUpdated]
function ILuaGameMode:OnUpdated() end
---当退出游戏模式时调用。
---Called when exiting the game mode.
---@instance
---@function [ILuaGameMode:OnExitMode]
function ILuaGameMode:OnExitMode() end
---是否让用户自己管理进入战斗的加载流程。
---Determine if the user should manage the battle loading process.
---@instance
---@function [ILuaGameMode:IsProxyBattle]
---@return System.Boolean 如果为 true，则用户自己管理进入战斗的加载流程。If true, the user manages the battle loading process.
function ILuaGameMode:IsProxyBattle() end
---是否启用占领点
---Is enable capture point
---@instance
---@function [ILuaGameMode:IsEnableCapturePoint]
---@return System.Boolean
function ILuaGameMode:IsEnableCapturePoint() end
---获取地图模式
---Get map mode
---@instance
---@function [ILuaGameMode:GetMapMode]
---@return System.Int32
function ILuaGameMode:GetMapMode() end
return ILuaGameMode

---@class ShanghaiWindy.Core.Lua.LuaBehaviorMono
---@field ClassName System.String
local LuaBehaviorMono = {}

---@instance
---@function [LuaBehaviorMono:TryGetTankInitSystem]
---@return System.Boolean
---@param tankInitSystem ShanghaiWindy.Core.TankInitSystem&
function LuaBehaviorMono:TryGetTankInitSystem(tankInitSystem) end
---@instance
---@function [LuaBehaviorMono:TryGetFlightInitSystem]
---@return System.Boolean
---@param flightInitSystem ShanghaiWindy.Core.FlightInitSystem&
function LuaBehaviorMono:TryGetFlightInitSystem(flightInitSystem) end
---@instance
---@function [LuaBehaviorMono:TryGetArmyInitSystem]
---@return System.Boolean
---@param armyInitSystem ShanghaiWindy.Core.FPS.ArmyInitSystem&
function LuaBehaviorMono:TryGetArmyInitSystem(armyInitSystem) end
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

---@class ShanghaiWindy.Core.Delegate.LoadPoolAssetDelegate
local LoadPoolAssetDelegate = {}

return LoadPoolAssetDelegate

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

---当载具物体被摧毁时触发全局游戏事件。
---Dispatch global event when vehicle gameobject is destroyed.
---@class ShanghaiWindy.Core.Delegate.OnGameVehicleGameObjectRemovedDelegate
local OnGameVehicleGameObjectRemovedDelegate = {}

return OnGameVehicleGameObjectRemovedDelegate

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

---@class ShanghaiWindy.Core.Delegate.OnKeyRegistered
local OnKeyRegistered = {}

return OnKeyRegistered

---@class ShanghaiWindy.Core.Delegate.OnKeyUnregistered
local OnKeyUnregistered = {}

return OnKeyUnregistered

---@class ShanghaiWindy.Core.Delegate.OnBattleSceneLoadedDelegate
local OnBattleSceneLoadedDelegate = {}

return OnBattleSceneLoadedDelegate

---@class ShanghaiWindy.Core.Delegate.OnBattleUILoadedDelegate
local OnBattleUILoadedDelegate = {}

return OnBattleUILoadedDelegate

---音频事件加载完成时的委托
---Post Event Loaded Delegate
---@class ShanghaiWindy.Core.Delegate.PostEventLoadedDelegate
local PostEventLoadedDelegate = {}

return PostEventLoadedDelegate

---@class ShanghaiWindy.Core.Delegate.OnQuarterTickDelegate
local OnQuarterTickDelegate = {}

return OnQuarterTickDelegate

---@class ShanghaiWindy.Core.Delegate.OnTickDelegate
local OnTickDelegate = {}

return OnTickDelegate

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

---载具缩略图加载完毕回调
---@class ShanghaiWindy.Core.Delegate.OnVehicleThumbnailLoadedDelegate
local OnVehicleThumbnailLoadedDelegate = {}

return OnVehicleThumbnailLoadedDelegate

---陆军相关 API
---Army-related API
---@class ShanghaiWindy.Core.API.ArmyAPI
local ArmyAPI = {}

---从陆军载具中获取所有武器控制器。
---Get all the weapon controllers from the army vehicle.
---@static
---@function [ArmyAPI.GetWeaponConrollerList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.FPS.WeaponController, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 武器控制器列表 List of weapon controllers
---@param vehicle ShanghaiWindy.Core.FPS.ArmyInitSystem
function ArmyAPI.GetWeaponConrollerList(vehicle) end
return ArmyAPI

---资源相关 API
---Asset-related API
---@class ShanghaiWindy.Core.API.AssetAPI
local AssetAPI = {}

---（弃用）加载资源包
---（Deprecated）Load Asset Bundle
---@static
---@function [AssetAPI.LoadAssetBundle]
---@param abName System.String
---@param format System.String
---@param callBack ShanghaiWindy.Core.Delegate.AssetLoadedDelegate
function AssetAPI.LoadAssetBundle(abName, format, callBack) end
---从对象池加载资源
---Load asset from object pool
---@static
---@function [AssetAPI.LoadPoolAsset]
---@param id System.String
---@param fileName System.String
---@param callBack ShanghaiWindy.Core.Delegate.LoadPoolAssetDelegate
function AssetAPI.LoadPoolAsset(id, fileName, callBack) end
---将对方返回对象池
---Release asset to object pool
---@static
---@function [AssetAPI.ReleasePoolAsset]
---@param assetPoolRef ShanghaiWindy.Core.AssetPoolRef
function AssetAPI.ReleasePoolAsset(assetPoolRef) end
---重新加载配置表
---Reload config
---@static
---@function [AssetAPI.ReloadConfig]
function AssetAPI.ReloadConfig() end
---强制解析资源包
---Force resolve package
---@static
---@function [AssetAPI.ForceResolvePackage]
function AssetAPI.ForceResolvePackage() end
return AssetAPI

---战斗玩家
---Battle Player
---@class ShanghaiWindy.Core.API.BattlePlayerAPI
local BattlePlayerAPI = {}

---创建离线单机 Bot
---Create offline bot
---@static
---@function [BattlePlayerAPI.CreateOfflineBotPlayer]
---@return ShanghaiWindy.Core.OfflineBotBattlePlayer
---@param uid System.Int32
---@param nickName System.String
---@param info System.Object
function BattlePlayerAPI.CreateOfflineBotPlayer(uid, nickName, info) end
---创建
---@static
---@function [BattlePlayerAPI.CreateOfflineMainPlayer]
---@return ShanghaiWindy.Core.OfflineMainBattlePlayer
---@param uid System.Int32
---@param info System.Object
function BattlePlayerAPI.CreateOfflineMainPlayer(uid, info) end
return BattlePlayerAPI

---@class ShanghaiWindy.Core.API.BotAPI
local BotAPI = {}

---@static
---@function [BotAPI.GetTankDefenceBotLogic]
---@return ShanghaiWindy.Core.DefenceBotLogic
function BotAPI.GetTankDefenceBotLogic() end
---@static
---@function [BotAPI.GetTankBotTaskLogic]
---@return ShanghaiWindy.Core.TankBotTaskLogic
function BotAPI.GetTankBotTaskLogic() end
---@static
---@function [BotAPI.AddCaptureTaskToBot]
---@return ShanghaiWindy.Core.CaptureZoneTask
---@param logic ShanghaiWindy.Core.TankBotTaskLogic
function BotAPI.AddCaptureTaskToBot(logic) end
return BotAPI

---Buff API
---@class ShanghaiWindy.Core.API.BuffAPI
local BuffAPI = {}

---为载具添加 Buff
---Add a buff to vehicle
---@static
---@function [BuffAPI.AddBuff]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param buffCaster ShanghaiWindy.Core.Lua.ILuaBuffCaster
function BuffAPI.AddBuff(vehicle, buffCaster) end
---为载具删除 Buff
---Remove a buff to vehicle
---@static
---@function [BuffAPI.RemoveBuff]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param buffCaster ShanghaiWindy.Core.Lua.ILuaBuffCaster
---@param isInterrupt System.Boolean
function BuffAPI.RemoveBuff(vehicle, buffCaster, isInterrupt) end
---@static
---@function [BuffAPI.TryGetBuffReceiverAsTank]
---@return System.Boolean
---@param buffReceiver ShanghaiWindy.Core.IBuffReceiver
---@param tankInitSystem ShanghaiWindy.Core.TankInitSystem&
function BuffAPI.TryGetBuffReceiverAsTank(buffReceiver, tankInitSystem) end
---@static
---@function [BuffAPI.TryGetBuffReceiverAsFlight]
---@return System.Boolean
---@param buffReceiver ShanghaiWindy.Core.IBuffReceiver
---@param flightInitSystem ShanghaiWindy.Core.FlightInitSystem&
function BuffAPI.TryGetBuffReceiverAsFlight(buffReceiver, flightInitSystem) end
---@static
---@function [BuffAPI.TryGetBuffReceiverAsArmy]
---@return System.Boolean
---@param buffReceiver ShanghaiWindy.Core.IBuffReceiver
---@param armyInitSystem ShanghaiWindy.Core.FPS.ArmyInitSystem&
function BuffAPI.TryGetBuffReceiverAsArmy(buffReceiver, armyInitSystem) end
return BuffAPI

---摄像机 API
---Camera API
---@class ShanghaiWindy.Core.API.CameraAPI
local CameraAPI = {}

---获取摄像机 Transform
---Get camera transform
---@static
---@function [CameraAPI.GetCameraTransform]
---@return UnityEngine.Transform
function CameraAPI.GetCameraTransform() end
---设置背景摄像机位置
---Set background camera position
---@static
---@function [CameraAPI.SetBackgroundCameraPosition]
---@param position UnityEngine.Vector3
function CameraAPI.SetBackgroundCameraPosition(position) end
---设置背景摄像机旋转
---Set background camera euler angles
---@static
---@function [CameraAPI.SetBackgroundCameraEulerAngles]
---@param eulerAngles UnityEngine.Vector3
function CameraAPI.SetBackgroundCameraEulerAngles(eulerAngles) end
---获取背景摄像机
---Get background camera
---@static
---@function [CameraAPI.GetBackGroundCamera]
---@return UnityEngine.Camera
function CameraAPI.GetBackGroundCamera() end
---屏幕位置转为世界坐标
---Screen point to world point
---@static
---@function [CameraAPI.ScreenToWorldPoint]
---@return UnityEngine.Vector3
---@param camera UnityEngine.Camera
---@param screenPoint UnityEngine.Vector3
function CameraAPI.ScreenToWorldPoint(camera, screenPoint) end
---世界坐标转为屏幕位置
---World point to screen point
---@static
---@function [CameraAPI.WorldToScreenPoint]
---@return UnityEngine.Vector3
---@param camera UnityEngine.Camera
---@param worldPoint UnityEngine.Vector3
function CameraAPI.WorldToScreenPoint(camera, worldPoint) end
---获取当前的游戏摄像机
---Get the current main game camera
---@static
---@function [CameraAPI.GetGameCamera]
---@return UnityEngine.Camera
function CameraAPI.GetGameCamera() end
---@static
---@function [CameraAPI.GetScreenPosition]
---@return UnityEngine.Vector3
---@param worldPos UnityEngine.Vector3
---@param iconPosition ShanghaiWindy.Core.EIconPosition&
function CameraAPI.GetScreenPosition(worldPos, iconPosition) end
return CameraAPI

---@class ShanghaiWindy.Core.API.CaptureZoneAPI
local CaptureZoneAPI = {}

---增加一个可占领区域
---Add a capture zone
---@static
---@function [CaptureZoneAPI.AddCaptureZone]
---@param zoneName System.String
---@param point UnityEngine.Vector3
---@param radius System.Single
function CaptureZoneAPI.AddCaptureZone(zoneName, point, radius) end
---@static
---@function [CaptureZoneAPI.CapturingZone]
---@return System.Void
---@param id System.Int32
---@param team ShanghaiWindy.Core.TeamManager+Team
---@param delta System.Single
function CaptureZoneAPI.CapturingZone(id, team, delta) end
---获取占领点
---Get capture zone
---@static
---@function [CaptureZoneAPI.GetCaptureZone]
---@return ShanghaiWindy.Core.CaptureZoneInfo
---@param id System.Int64
function CaptureZoneAPI.GetCaptureZone(id) end
---从名称获取占领点
---Get capture zone from index
---@static
---@function [CaptureZoneAPI.GetCaptureZoneFromName]
---@return ShanghaiWindy.Core.CaptureZoneInfo
---@param name System.String
function CaptureZoneAPI.GetCaptureZoneFromName(name) end
---销毁一个占领区域
---Remove a capture zone
---@static
---@function [CaptureZoneAPI.RemoveCaptureZone]
---@param id System.Int32
function CaptureZoneAPI.RemoveCaptureZone(id) end
---获取占领点列表
---Get capture zone infos
---@static
---@function [CaptureZoneAPI.GetCaptureZoneInfos]
---@return ShanghaiWindy.Core.CaptureZoneInfo[]
function CaptureZoneAPI.GetCaptureZoneInfos() end
return CaptureZoneAPI

---@class ShanghaiWindy.Core.API.ColorAPI
local ColorAPI = {}

---@static
---@function [ColorAPI.GetColor]
---@return UnityEngine.Color
---@param r System.Single
---@param g System.Single
---@param b System.Single
---@param a System.Single
function ColorAPI.GetColor(r, g, b, a) end
return ColorAPI

---@class ShanghaiWindy.Core.API.ComponentAPI
local ComponentAPI = {}

---根据类名获取指定 GameObject 上的 Lua 组件。
---Gets a Lua component with the specified class name on the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponent]
---@return XLua.LuaTable 指定的 Lua 组件，如果不存在则返回 null。The specified Lua component, or null if it does not exist.
---@param target UnityEngine.GameObject
---@param className System.String
function ComponentAPI.GetLuaComponent(target, className) end
---根据类名获取指定 GameObject 上的所有 Lua 组件。
---Gets all Lua components with the specified class name on the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponents]
---@return XLua.LuaTable[] 指定的 Lua 组件数组。An array of the specified Lua components.
---@param target UnityEngine.GameObject
---@param className System.String
function ComponentAPI.GetLuaComponents(target, className) end
---在指定 GameObject 的父级中，根据类名获取所有 Lua 组件。
---Gets all Lua components with the specified class name in the parent hierarchy of the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponentsInParent]
---@return XLua.LuaTable[] 指定的 Lua 组件数组。An array of the specified Lua components.
---@param target UnityEngine.GameObject
---@param className System.String
function ComponentAPI.GetLuaComponentsInParent(target, className) end
---在指定 GameObject 的子级中，根据类名获取所有 Lua 组件。
---Gets all Lua components with the specified class name in the children of the given GameObject.
---@static
---@function [ComponentAPI.GetLuaComponentsInChildren]
---@return XLua.LuaTable[] 指定的 Lua 组件数组。An array of the specified Lua components.
---@param target UnityEngine.GameObject
---@param className System.String
function ComponentAPI.GetLuaComponentsInChildren(target, className) end
---根据组件类型名获取指定 GameObject 上的原生组件。
---Gets the native component with the specified type name on the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponent]
---@return UnityEngine.Component 指定的原生组件。The specified native component.
---@param target UnityEngine.GameObject
---@param componentName System.String
function ComponentAPI.GetNativeComponent(target, componentName) end
---根据组件类型名获取指定 GameObject 上的所有原生组件。
---Gets all native components with the specified type name on the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponents]
---@return UnityEngine.Component[] 指定的原生组件数组。An array of the specified native components.
---@param target UnityEngine.GameObject
---@param componentName System.String
function ComponentAPI.GetNativeComponents(target, componentName) end
---在指定 GameObject 的父级中，根据组件类型名获取所有原生组件。
---Gets all native components with the specified type name in the parent hierarchy of the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponentsInParent]
---@return UnityEngine.Component[] 指定的原生组件数组。An array of the specified native components.
---@param target UnityEngine.GameObject
---@param componentName System.String
function ComponentAPI.GetNativeComponentsInParent(target, componentName) end
---在指定 GameObject 的子级中，根据组件类型名获取所有原生组件。
---Gets all native components with the specified type name in the children of the given GameObject.
---@static
---@function [ComponentAPI.GetNativeComponentsInChildren]
---@return UnityEngine.Component[] 指定的原生组件数组。An array of the specified native components.
---@param target UnityEngine.GameObject
---@param componentName System.String
function ComponentAPI.GetNativeComponentsInChildren(target, componentName) end
return ComponentAPI

---@class ShanghaiWindy.Core.API.ConfigAPI
local ConfigAPI = {}

---获取防守任务配置列表
---Get wave mission config list
---@static
---@function [ConfigAPI.GetWaveMissionConfigs]
---@return ShanghaiWindy.Data.WaveMissionConfig[]
function ConfigAPI.GetWaveMissionConfigs() end
---获取防守任务配置
---Get wave mission config
---@static
---@function [ConfigAPI.GetWaveMissionConfig]
---@return ShanghaiWindy.Data.WaveMissionConfig
---@param guid System.String
function ConfigAPI.GetWaveMissionConfig(guid) end
---获取占领点配置列表
---Get capture zone config list
---@static
---@function [ConfigAPI.GetCaptureZoneConfigs]
---@return ShanghaiWindy.Data.CaptureZoneModeConfig[]
function ConfigAPI.GetCaptureZoneConfigs() end
---获取占领区任务配置
---Get capture zone config
---@static
---@function [ConfigAPI.GetCaptureZoneConfig]
---@return ShanghaiWindy.Data.CaptureZoneModeConfig
---@param guid System.String
function ConfigAPI.GetCaptureZoneConfig(guid) end
return ConfigAPI

---将 Lua Table 转为 C# 对象
---Convert Lua Table to C# object
---@class ShanghaiWindy.Core.API.ConvertAPI
local ConvertAPI = {}

---将 Lua Class 转换为 IBuffCaster 对象
---Covert Lua Class to IBuffCaster Object
---@static
---@function [ConvertAPI.CovertToBuffCaster]
---@return ShanghaiWindy.Core.Lua.ILuaBuffCaster
---@param luaTable XLua.LuaTable
function ConvertAPI.CovertToBuffCaster(luaTable) end
return ConvertAPI

---自定义 Option UI
---Custom Option UI
---@class ShanghaiWindy.Core.API.CustomOptionUIAPI
local CustomOptionUIAPI = {}

---切换是否显示自定义 Option UI
---Toggle Option UI
---@static
---@function [CustomOptionUIAPI.ToggleUI]
---@param state System.Boolean
function CustomOptionUIAPI.ToggleUI(state) end
---添加选项
---Add option
---@static
---@function [CustomOptionUIAPI.AddOption]
---@param optionName System.String
---@param defaultValue System.String
---@param options System.String[]
---@param onValueChanged System.Action`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
function CustomOptionUIAPI.AddOption(optionName, defaultValue, options, onValueChanged) end
---增加滑条
---Add Slider
---@static
---@function [CustomOptionUIAPI.AddSlider]
---@param optionName System.String
---@param defaultValue System.Single
---@param minVal System.Single
---@param maxVal System.Single
---@param isWholeNumbers System.Boolean
---@param onValueChanged System.Action`1[[System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
function CustomOptionUIAPI.AddSlider(optionName, defaultValue, minVal, maxVal, isWholeNumbers, onValueChanged) end
---添加按钮
---Add Button
---@static
---@function [CustomOptionUIAPI.AddButton]
---@param optionName System.String
---@param buttonName System.String
---@param onValueChanged System.Action
function CustomOptionUIAPI.AddButton(optionName, buttonName, onValueChanged) end
---添加标题
---Add Title
---@static
---@function [CustomOptionUIAPI.AddTitle]
---@param optionName System.String
function CustomOptionUIAPI.AddTitle(optionName) end
---添加文本框
---Add text
---@static
---@function [CustomOptionUIAPI.AddTextField]
---@param optionName System.String
---@param onInputChanged System.Action`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
function CustomOptionUIAPI.AddTextField(optionName, onInputChanged) end
---清空 Options
---Clear Options
---@static
---@function [CustomOptionUIAPI.ClearOptions]
function CustomOptionUIAPI.ClearOptions() end
return CustomOptionUIAPI

---飞行器相关 API
---Flight-related API
---@class ShanghaiWindy.Core.API.FlightAPI
local FlightAPI = {}

---从飞行载具中获取所有飞行火力系统。
---Get all the flight fire systems from the flight vehicle.
---@static
---@function [FlightAPI.GetFlightFireList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.FlightFireSystem, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 飞行火力系统列表 List of flight fire systems
---@param vehicle ShanghaiWindy.Core.FlightInitSystem
function FlightAPI.GetFlightFireList(vehicle) end
return FlightAPI

---@class ShanghaiWindy.Core.API.FontAPI
local FontAPI = {}

---@static
---@function [FontAPI.ReplaceAsMainFont]
---@return System.Void
---@param instance UnityEngine.GameObject
function FontAPI.ReplaceAsMainFont(instance) end
return FontAPI

---游戏相关全局 API
---Game related global api
---@class ShanghaiWindy.Core.API.GameAPI
local GameAPI = {}

---注册车辆生成事件
---Register Vehicl eLoaded Event
---@static
---@function [GameAPI.RegisterVehicleLoadedEvent]
---@param gameVehicleLoadedDelegate ShanghaiWindy.Core.Delegate.OnGameVehicleLoadedDelegate
function GameAPI.RegisterVehicleLoadedEvent(gameVehicleLoadedDelegate) end
---注销车辆生成事件
---UnRegister Vehicle Loaded Event
---@static
---@function [GameAPI.UnRegisterVehicleLoadedEvent]
---@param gameVehicleLoadedDelegate ShanghaiWindy.Core.Delegate.OnGameVehicleLoadedDelegate
function GameAPI.UnRegisterVehicleLoadedEvent(gameVehicleLoadedDelegate) end
---注册车辆销毁事件
---Register Vehicle Destroyed Event
---@static
---@function [GameAPI.RegisterVehicleDestroyedEvent]
---@param gameVehicleDestroyedDelegate ShanghaiWindy.Core.Delegate.OnGameVehicleDestroyedDelegate
function GameAPI.RegisterVehicleDestroyedEvent(gameVehicleDestroyedDelegate) end
---注销车辆销毁事件
---UnRegister Vehicle Destroyed Event
---@static
---@function [GameAPI.UnRegisterVehicleDestroyedEvent]
---@param gameVehicleDestroyedDelegate ShanghaiWindy.Core.Delegate.OnGameVehicleDestroyedDelegate
function GameAPI.UnRegisterVehicleDestroyedEvent(gameVehicleDestroyedDelegate) end
---注册车辆物体销毁事件
---Register Vehicle GameObject Destroyed Event
---@static
---@function [GameAPI.RegisterVehicleGameObjectDestroyedEvent]
---@param gameVehicleGameObjectDestroyedDelegate ShanghaiWindy.Core.Delegate.OnGameVehicleGameObjectRemovedDelegate
function GameAPI.RegisterVehicleGameObjectDestroyedEvent(gameVehicleGameObjectDestroyedDelegate) end
---注销车辆物体销毁事件
---UnRegister Vehicle GameObject Destroyed Event
---@static
---@function [GameAPI.UnRegisterVehicleGameObjectDestroyedEvent]
---@param gameVehicleGameObjectDestroyedDelegate ShanghaiWindy.Core.Delegate.OnGameVehicleGameObjectRemovedDelegate
function GameAPI.UnRegisterVehicleGameObjectDestroyedEvent(gameVehicleGameObjectDestroyedDelegate) end
return GameAPI

---@class ShanghaiWindy.Core.API.GameObjectAPI
local GameObjectAPI = {}

---寻找路径物体
---Find path
---@static
---@function [GameObjectAPI.Find]
---@return UnityEngine.GameObject
---@param go UnityEngine.GameObject
---@param path System.String
function GameObjectAPI.Find(go, path) end
---切换场景时候保留物体
---Do not destroy the target Object when loading a new Scene
---@static
---@function [GameObjectAPI.DontDestroyOnLoad]
---@param go UnityEngine.GameObject
function GameObjectAPI.DontDestroyOnLoad(go) end
---复制物体
---Clone object
---@static
---@function [GameObjectAPI.Clone]
---@return UnityEngine.GameObject
---@param go UnityEngine.GameObject
function GameObjectAPI.Clone(go) end
---销毁物体
---Destroy object
---@static
---@function [GameObjectAPI.DestroyObject]
---@param go UnityEngine.GameObject
function GameObjectAPI.DestroyObject(go) end
---是否激活物体
---Set object active
---@static
---@function [GameObjectAPI.SetActive]
---@param go UnityEngine.GameObject
---@param isActive System.Boolean
function GameObjectAPI.SetActive(go, isActive) end
---是否显示物体
---Set object visible
---@static
---@function [GameObjectAPI.SetVisible]
---@param go UnityEngine.GameObject
---@param isVisible System.Boolean
function GameObjectAPI.SetVisible(go, isVisible) end
return GameObjectAPI

---输入处理 API
---Input Handler API
---@class ShanghaiWindy.Core.API.InputAPI
local InputAPI = {}

---注册按键输入
---Register Key Input
---@static
---@function [InputAPI.RegisterKeyInput]
---@param actionName System.String
---@param keyCode System.String
---@param keyPerformed ShanghaiWindy.Core.Delegate.OnKeyPerformed
---@param keyCanceled ShanghaiWindy.Core.Delegate.OnKeyCanceled
function InputAPI.RegisterKeyInput(actionName, keyCode, keyPerformed, keyCanceled) end
---取消注册按键输入
---Unregister Key Input
---@static
---@function [InputAPI.UnregisterKeyInput]
---@param actionName System.String
function InputAPI.UnregisterKeyInput(actionName) end
---@static
---@function [InputAPI.RegisterAddKeyInput]
---@return System.Void
---@param onKeyRegistered ShanghaiWindy.Core.Delegate.OnKeyRegistered
function InputAPI.RegisterAddKeyInput(onKeyRegistered) end
---@static
---@function [InputAPI.UnregisterAddKeyInput]
---@return System.Void
---@param onKeyRegistered ShanghaiWindy.Core.Delegate.OnKeyRegistered
function InputAPI.UnregisterAddKeyInput(onKeyRegistered) end
---@static
---@function [InputAPI.RegisterRemoveKeyInput]
---@return System.Void
---@param onKeyUnregistered ShanghaiWindy.Core.Delegate.OnKeyUnregistered
function InputAPI.RegisterRemoveKeyInput(onKeyUnregistered) end
---@static
---@function [InputAPI.UnregisterRemoveKeyInput]
---@return System.Void
---@param onKeyUnregistered ShanghaiWindy.Core.Delegate.OnKeyUnregistered
function InputAPI.UnregisterRemoveKeyInput(onKeyUnregistered) end
---@static
---@function [InputAPI.GetBindings]
---@return System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[ShanghaiWindy.Core.API.InputAPICache, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function InputAPI.GetBindings() end
return InputAPI

---Map api
---@class ShanghaiWindy.Core.API.MapAPI
local MapAPI = {}

---Get map from guid
---@static
---@function [MapAPI.GetMapDataByGuid]
---@return ShanghaiWindy.Core.MapData
---@param guid System.String
function MapAPI.GetMapDataByGuid(guid) end
return MapAPI

---@class ShanghaiWindy.Core.API.MaterialAPI
local MaterialAPI = {}

---@static
---@function [MaterialAPI.AsyncApplyMaterial]
---@return System.Void
---@param guid System.String
---@param instance UnityEngine.GameObject
function MaterialAPI.AsyncApplyMaterial(guid, instance) end
return MaterialAPI

---@class ShanghaiWindy.Core.API.MeshAPI
local MeshAPI = {}

---创建 mesh
---Create mesh
---@static
---@function [MeshAPI.CreateMesh]
---@return System.Int32
---@param points UnityEngine.Vector3[]
---@param height System.Single
function MeshAPI.CreateMesh(points, height) end
---获取 mesh
---Get mesh
---@static
---@function [MeshAPI.GetMesh]
---@return UnityEngine.GameObject
---@param meshId System.Int32
function MeshAPI.GetMesh(meshId) end
---删除 mesh
---Delete mesh
---@static
---@function [MeshAPI.DeleteMesh]
---@param meshId System.Int32
function MeshAPI.DeleteMesh(meshId) end
---@static
---@function [MeshAPI.CreateSphereTriggerBox]
---@return System.Int32
---@param position UnityEngine.Vector3
---@param radius System.Single
---@param onEnter System.Action`1[[UnityEngine.Collider, UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
---@param onExit System.Action`1[[UnityEngine.Collider, UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function MeshAPI.CreateSphereTriggerBox(position, radius, onEnter, onExit) end
---@static
---@function [MeshAPI.IsPointInPolygon]
---@return System.Boolean
---@param point UnityEngine.Vector3
---@param polygonPoints UnityEngine.Vector3[]
function MeshAPI.IsPointInPolygon(point, polygonPoints) end
return MeshAPI

---模式 API
---Mode API
---@class ShanghaiWindy.Core.API.ModeAPI
local ModeAPI = {}

---退出模式
---Exit Mode
---@static
---@function [ModeAPI.ExitMode]
function ModeAPI.ExitMode() end
---显示选择载具 UI
---Show pick vehicle ui
---@static
---@function [ModeAPI.ShowPickVehicleUI]
---@param isForcePick System.Boolean
function ModeAPI.ShowPickVehicleUI(isForcePick) end
---显示选择载具 UI (带列表)
---Show pick vehicle ui (with list)
---@static
---@function [ModeAPI.ShowPickVehicleUIWithList]
---@param isForcePick System.Boolean
---@param vehicleList System.Collections.Generic.List`1[[ShanghaiWindy.Core.VehicleInfo, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function ModeAPI.ShowPickVehicleUIWithList(isForcePick, vehicleList) end
---@static
---@function [ModeAPI.ShowPickVehicleListUI]
---@return System.Void
---@param callback System.Action`1[[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
function ModeAPI.ShowPickVehicleListUI(callback) end
---注册选中载具回调
---Register vehicle pick callabck
---@static
---@function [ModeAPI.RegisterPickVehicleCallBack]
---@param pickDelegate ShanghaiWindy.Core.EventDispatcherDelegate`1[[ShanghaiWindy.Core.VehiclePickEvent, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function ModeAPI.RegisterPickVehicleCallBack(pickDelegate) end
---取消注册选中载具回调
---UnRegister vehicle pick callabck
---@static
---@function [ModeAPI.UnRegisterPickVehicleCallBack]
---@param pickDelegate ShanghaiWindy.Core.EventDispatcherDelegate`1[[ShanghaiWindy.Core.VehiclePickEvent, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function ModeAPI.UnRegisterPickVehicleCallBack(pickDelegate) end
---加载战斗场景
---Load battle scene
---@static
---@function [ModeAPI.LoadBattleScene]
---@param mapData ShanghaiWindy.Core.MapData
---@param callback ShanghaiWindy.Core.Delegate.OnBattleSceneLoadedDelegate
function ModeAPI.LoadBattleScene(mapData, callback) end
---加载战斗 UI
---Load battle ui
---@static
---@function [ModeAPI.LoadBattleUI]
---@param callback ShanghaiWindy.Core.Delegate.OnBattleUILoadedDelegate
function ModeAPI.LoadBattleUI(callback) end
---添加玩家
---Add battle player
---@static
---@function [ModeAPI.AddBattlePlayer]
---@param battlePlayer ShanghaiWindy.Core.AbstractBattlePlayer
function ModeAPI.AddBattlePlayer(battlePlayer) end
---删除玩家
---Remove battle player
---@static
---@function [ModeAPI.RemoveBattlePlayer]
---@param uid System.Int32
function ModeAPI.RemoveBattlePlayer(uid) end
---更新比分
---Update score
---@static
---@function [ModeAPI.UpdateScore]
---@param friendTeamScore System.Int32
---@param enemyTeamScore System.Int32
---@param totalFriendTeamScore System.Int32
---@param totalEnemyTeamScore System.Int32
function ModeAPI.UpdateScore(friendTeamScore, enemyTeamScore, totalFriendTeamScore, totalEnemyTeamScore) end
---注册占领点事件
---Register capture point
---@static
---@function [ModeAPI.RegisterCapturePointCallback]
---@param captureDelegate ShanghaiWindy.Core.EventDispatcherDelegate`1[[ShanghaiWindy.Core.CaptureEvent, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function ModeAPI.RegisterCapturePointCallback(captureDelegate) end
---取消注册占领点事件
---Unregister capture point
---@static
---@function [ModeAPI.UnRegisterCapturePointCallback]
---@param captureDelegate ShanghaiWindy.Core.EventDispatcherDelegate`1[[ShanghaiWindy.Core.CaptureEvent, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function ModeAPI.UnRegisterCapturePointCallback(captureDelegate) end
---增加菜单自定义选项
---Add custom menu option
---@static
---@function [ModeAPI.AddCustomMenuOption]
---@param optionName System.String
---@param callback System.Action
function ModeAPI.AddCustomMenuOption(optionName, callback) end
---删除菜单自定义选项
---Remove custom menu option
---@static
---@function [ModeAPI.RemoveCustomMenuOption]
---@param optionName System.String
function ModeAPI.RemoveCustomMenuOption(optionName) end
---显示成功失败界面
---Show victory or default
---@static
---@function [ModeAPI.ShowVictoryOrDefeat]
---@param isVictory System.Boolean
function ModeAPI.ShowVictoryOrDefeat(isVictory) end
---显示与隐藏比分
---Show or hide score
---@static
---@function [ModeAPI.ToggleScore]
---@param isShownScore System.Boolean
function ModeAPI.ToggleScore(isShownScore) end
---开启作弊
---Enable cheat
---@static
---@function [ModeAPI.EnableCheat]
function ModeAPI.EnableCheat() end
---关闭作弊
---Disable cheat
---@static
---@function [ModeAPI.DisableCheat]
function ModeAPI.DisableCheat() end
---切换是否锁住载具
---Toggle lock vehicle state
---@static
---@function [ModeAPI.ToggleVehicleLockState]
---@param isLock System.Boolean
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
function ModeAPI.ToggleVehicleLockState(isLock, vehicle) end
---显示倒计时
---Enable count down
---@static
---@function [ModeAPI.EnableCountDown]
---@param countDown System.Int32
---@param countDownTitle System.String
---@param countDownDescription System.String
---@param callback System.Action
function ModeAPI.EnableCountDown(countDown, countDownTitle, countDownDescription, callback) end
---获取模式 lua class 的实例
---Get mode instance of lua class
---@static
---@function [ModeAPI.GetModeInstance]
---@return XLua.LuaTable
function ModeAPI.GetModeInstance() end
---@static
---@function [ModeAPI.StartRecord]
---@return System.Void
function ModeAPI.StartRecord() end
---@static
---@function [ModeAPI.EndRecord]
---@return System.Void
function ModeAPI.EndRecord() end
return ModeAPI

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

---随机 API
---Random API
---@class ShanghaiWindy.Core.API.RandomAPI
local RandomAPI = {}

---从载具列表随机一个载具
---Random vehicle from list
---@static
---@function [RandomAPI.GetRandomVehicleFromList]
---@return ShanghaiWindy.Core.VehicleInfo
---@param vehicleInfos System.Collections.Generic.List`1[[ShanghaiWindy.Core.VehicleInfo, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function RandomAPI.GetRandomVehicleFromList(vehicleInfos) end
return RandomAPI

---声音相关 API
---Sound-related API
---@class ShanghaiWindy.Core.API.SoundAPI
local SoundAPI = {}

---播放一次性音效
---Play One-Shot Sound
---@static
---@function [SoundAPI.PlayOneShot]
---@param guid System.String
---@param position UnityEngine.Vector3
function SoundAPI.PlayOneShot(guid, position) end
---发布音频事件
---Post Sound Event
---@static
---@function [SoundAPI.PostEvent]
---@param guid System.String
---@param position UnityEngine.Vector3
---@param postEventLoadedDelegate ShanghaiWindy.Core.Delegate.PostEventLoadedDelegate
function SoundAPI.PostEvent(guid, position, postEventLoadedDelegate) end
---释放音频事件
---Release Sound Event
---@static
---@function [SoundAPI.ReleaseEvent]
---@param eventInstance FMOD.Studio.EventInstance
function SoundAPI.ReleaseEvent(eventInstance) end
return SoundAPI

---重生点 API
---Spawn API
---@class ShanghaiWindy.Core.API.SpawnAPI
local SpawnAPI = {}

---@static
---@function [SpawnAPI.AsyncSpawn]
---@return System.Void
---@param team ShanghaiWindy.Core.TeamManager+Team
---@param onSpawnPoint System.Action`1[[UnityEngine.Transform, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function SpawnAPI.AsyncSpawn(team, onSpawnPoint) end
---异步寻找当前的出生点可用点
---Find spawn given point in async
---@static
---@function [SpawnAPI.AsyncSpawnGivenPoints]
---@param points UnityEngine.Transform[]
---@param onSpawnPoint System.Action`1[[UnityEngine.Transform, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function SpawnAPI.AsyncSpawnGivenPoints(points, onSpawnPoint) end
return SpawnAPI

---载具生成API
---Vehicle Spawn APIs
---@class ShanghaiWindy.Core.API.SpawnVehicleAPI
local SpawnVehicleAPI = {}

---@static
---@function [SpawnVehicleAPI.CreateLocalBot]
---@return System.Void
---@param vehicleInfo ShanghaiWindy.Core.VehicleInfo
---@param pos UnityEngine.Vector3
---@param rot UnityEngine.Quaternion
---@param botTeam ShanghaiWindy.Core.TeamManager+Team
---@param isKeepWreckage System.Boolean
---@param isIdle System.Boolean
function SpawnVehicleAPI.CreateLocalBot(vehicleInfo, pos, rot, botTeam, isKeepWreckage, isIdle) end
---创建一个本地玩家。您可以从GameDataManager.PlayerTeam设置玩家队伍
---Create a Local Player. You can set player team from GameDataManager.PlayerTeam
---@static
---@function [SpawnVehicleAPI.CreatePlayer]
---@return ShanghaiWindy.Core.BaseInitSystem 在生成位置创建的本地玩家 A created local player at spawn tranform
---@param vehicleInfo ShanghaiWindy.Core.VehicleInfo
---@param pos UnityEngine.Vector3
---@param rot UnityEngine.Quaternion
---@param onPreBind System.Action`1[[ShanghaiWindy.Core.BaseInitSystem, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
function SpawnVehicleAPI.CreatePlayer(vehicleInfo, pos, rot, onPreBind) end
---创建一个自由摄像机
---Create a free camera
---@static
---@function [SpawnVehicleAPI.CreateFreeCamera]
---@return ShanghaiWindy.Core.FreeCameraInitSystem 在生成位置创建的自由摄像机 A created free camera at spawn tranform
---@param pos UnityEngine.Vector3
---@param rot UnityEngine.Quaternion
function SpawnVehicleAPI.CreateFreeCamera(pos, rot) end
return SpawnVehicleAPI

---存储 API
---Storage API
---@class ShanghaiWindy.Core.API.StorageAPI
local StorageAPI = {}

---获得 Number
---Get Number
---@static
---@function [StorageAPI.GetNumberValue]
---@return System.Single
---@param group System.String
---@param key System.String
---@param defaultValue System.Single
function StorageAPI.GetNumberValue(group, key, defaultValue) end
---设置 Number
---Set Number
---@static
---@function [StorageAPI.SetNumberValue]
---@param group System.String
---@param key System.String
---@param value System.Single
function StorageAPI.SetNumberValue(group, key, value) end
---获得 String
---Get String
---@static
---@function [StorageAPI.GetStringValue]
---@return System.String
---@param group System.String
---@param key System.String
---@param defaultValue System.String
function StorageAPI.GetStringValue(group, key, defaultValue) end
---设置 String
---Set String
---@static
---@function [StorageAPI.SetStringValue]
---@param group System.String
---@param key System.String
---@param value System.String
function StorageAPI.SetStringValue(group, key, value) end
---获得 string array
---Get string array
---@static
---@function [StorageAPI.GetStringArrayValue]
---@return System.String[]
---@param group System.String
---@param key System.String
---@param defaultValue System.String[]
function StorageAPI.GetStringArrayValue(group, key, defaultValue) end
---设置 string array
---Set string array
---@static
---@function [StorageAPI.SetStringArrayValue]
---@param group System.String
---@param key System.String
---@param value System.String[]
function StorageAPI.SetStringArrayValue(group, key, value) end
---获得布尔
---Get Boolean
---@static
---@function [StorageAPI.GetBooleanValue]
---@return System.Boolean
---@param group System.String
---@param key System.String
---@param defaultValue System.Boolean
function StorageAPI.GetBooleanValue(group, key, defaultValue) end
---设置布尔
---Set Boolean
---@static
---@function [StorageAPI.SetBooleanValue]
---@param group System.String
---@param key System.String
---@param value System.Boolean
function StorageAPI.SetBooleanValue(group, key, value) end
---保存
---Save Storage
---@static
---@function [StorageAPI.SaveStorage]
function StorageAPI.SaveStorage() end
return StorageAPI

---坦克相关 API
---Tank-related API
---@class ShanghaiWindy.Core.API.TankAPI
local TankAPI = {}

---从坦克载具中获取所有坦克火力系统。
---Get all the tank fire systems from the tank vehicle.
---@static
---@function [TankAPI.GetTankFireList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.TankFire, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 坦克火力系统列表 List of tank fire systems
---@param vehicle ShanghaiWindy.Core.TankInitSystem
function TankAPI.GetTankFireList(vehicle) end
return TankAPI

---团队接口
---Team API
---@class ShanghaiWindy.Core.API.TeamAPI
local TeamAPI = {}

---@static
---@function [TeamAPI.SetPlayerTeam]
---@return System.Void
---@param playerTeam ShanghaiWindy.Core.TeamManager+Team
function TeamAPI.SetPlayerTeam(playerTeam) end
---设置自己队伍为 Red Team
---Set player team as red team
---@static
---@function [TeamAPI.SetPlayerTeamAsRedTeam]
function TeamAPI.SetPlayerTeamAsRedTeam() end
---设置自己的队伍为 Blue Team
---Set player team as blue team
---@static
---@function [TeamAPI.SetPlayerTeamAsBlueTeam]
function TeamAPI.SetPlayerTeamAsBlueTeam() end
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
---@static
---@function [TeamAPI.GetRedTeam]
---@return ShanghaiWindy.Core.TeamManager+Team
function TeamAPI.GetRedTeam() end
---@static
---@function [TeamAPI.GetBlueTeam]
---@return ShanghaiWindy.Core.TeamManager+Team
function TeamAPI.GetBlueTeam() end
---@static
---@function [TeamAPI.GetNoneTeam]
---@return ShanghaiWindy.Core.TeamManager+Team
function TeamAPI.GetNoneTeam() end
return TeamAPI

---@class ShanghaiWindy.Core.API.TimeAPI
local TimeAPI = {}

---@static
---@function [TimeAPI.RegisterQuarterTick]
---@return System.Void
---@param callback ShanghaiWindy.Core.Delegate.OnQuarterTickDelegate
function TimeAPI.RegisterQuarterTick(callback) end
---@static
---@function [TimeAPI.UnRegisterQuarterTick]
---@return System.Void
---@param callback ShanghaiWindy.Core.Delegate.OnQuarterTickDelegate
function TimeAPI.UnRegisterQuarterTick(callback) end
---@static
---@function [TimeAPI.RegisterLateFrameTick]
---@return System.Void
---@param callback ShanghaiWindy.Core.Delegate.OnTickDelegate
function TimeAPI.RegisterLateFrameTick(callback) end
---@static
---@function [TimeAPI.UnRegisterLateFrameTick]
---@return System.Void
---@param callback ShanghaiWindy.Core.Delegate.OnTickDelegate
function TimeAPI.UnRegisterLateFrameTick(callback) end
---@static
---@function [TimeAPI.RegisterFixedFrameTick]
---@return System.Void
---@param callback ShanghaiWindy.Core.Delegate.OnTickDelegate
function TimeAPI.RegisterFixedFrameTick(callback) end
---@static
---@function [TimeAPI.UnRegisterFixedFrameTick]
---@return System.Void
---@param callback ShanghaiWindy.Core.Delegate.OnTickDelegate
function TimeAPI.UnRegisterFixedFrameTick(callback) end
---@static
---@function [TimeAPI.GetDeltaTime]
---@return System.Single
function TimeAPI.GetDeltaTime() end
---@static
---@function [TimeAPI.GetFixedDeltaTime]
---@return System.Single
function TimeAPI.GetFixedDeltaTime() end
---@static
---@function [TimeAPI.GetTime]
---@return System.Single
function TimeAPI.GetTime() end
return TimeAPI

---@class ShanghaiWindy.Core.API.TransformAPI
local TransformAPI = {}

---From euler angle to rotation
---@static
---@function [TransformAPI.EulerToRot]
---@return UnityEngine.Quaternion
---@param eulerAngle UnityEngine.Vector3
function TransformAPI.EulerToRot(eulerAngle) end
---寻找路径物体
---Find path
---@static
---@function [TransformAPI.Find]
---@return UnityEngine.Transform
---@param trans UnityEngine.Transform
---@param path System.String
function TransformAPI.Find(trans, path) end
---创建 Transform
---Create transform
---@static
---@function [TransformAPI.CreateTransform]
---@return UnityEngine.Transform
---@param transformName System.String
function TransformAPI.CreateTransform(transformName) end
return TransformAPI

---模式 UI API
---Mode UI API
---@class ShanghaiWindy.Core.API.UIAPI
local UIAPI = {}

---显示内置 UI
---Show builtin ui
---@static
---@function [UIAPI.ShowUI]
---@param index ShanghaiWindy.Core.UIEnum
function UIAPI.ShowUI(index) end
---关闭内置 UI
---Remove builtin ui
---@static
---@function [UIAPI.RemoveUI]
---@param index ShanghaiWindy.Core.UIEnum
function UIAPI.RemoveUI(index) end
return UIAPI

---载具通用 API
---Vehicle Common APIs
---@class ShanghaiWindy.Core.API.VehicleAPI
local VehicleAPI = {}

---注册载具加载事件。
---Register an event dispatched when vehicle loaded.
---@static
---@function [VehicleAPI.RegisterVehicleLoadedEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param vehicleLoadedDelegate ShanghaiWindy.Core.Delegate.OnVehicleLoadedDelegate
function VehicleAPI.RegisterVehicleLoadedEvent(vehicle, vehicleLoadedDelegate) end
---取消注册载具加载事件。
---Unregister an event dispatched when vehicle loaded.
---@static
---@function [VehicleAPI.UnRegisterVehicleLoaedEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param vehicleLoadedDelegate ShanghaiWindy.Core.Delegate.OnVehicleLoadedDelegate
function VehicleAPI.UnRegisterVehicleLoaedEvent(vehicle, vehicleLoadedDelegate) end
---注册载具被摧毁事件。
---Register an event dispatched when vehicle destroyed.
---@static
---@function [VehicleAPI.RegisterVehicleDestroyedEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param vehicleDestroyedDelegate ShanghaiWindy.Core.Delegate.OnVehicleDestroyedDelegate
function VehicleAPI.RegisterVehicleDestroyedEvent(vehicle, vehicleDestroyedDelegate) end
---注销载具被摧毁时触发的事件。
---Unregister an event dispatched when vehicle destroyed.
---@static
---@function [VehicleAPI.UnRegisterVehicleDestroyedEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param vehicleDestroyedDelegate ShanghaiWindy.Core.Delegate.OnVehicleDestroyedDelegate
function VehicleAPI.UnRegisterVehicleDestroyedEvent(vehicle, vehicleDestroyedDelegate) end
---注册载具物体被摧毁时触发的事件。
---Register an event dispatched when vehicle gamobject destroyed.
---@static
---@function [VehicleAPI.RegisterVehicleGameObjectDestroyedEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param vehicleGameObjectDestroyed ShanghaiWindy.Core.Delegate.OnVehicleRemovedDelegate
function VehicleAPI.RegisterVehicleGameObjectDestroyedEvent(vehicle, vehicleGameObjectDestroyed) end
---注销载具物体被摧毁时触发的事件。
---Unregister an event dispatched when vehicle gamobject destroyed.
---@static
---@function [VehicleAPI.UnRegisterVehicleGameObjectDestroyedEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param vehicleGameObjectDestroyed ShanghaiWindy.Core.Delegate.OnVehicleRemovedDelegate
function VehicleAPI.UnRegisterVehicleGameObjectDestroyedEvent(vehicle, vehicleGameObjectDestroyed) end
---注册载具开火时触发的事件。
---Register an event dispatched when vehicle fired.
---@static
---@function [VehicleAPI.RegisterBulletFiredEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param index System.Int32
---@param tankFiredDelegate ShanghaiWindy.Core.Delegate.OnFiredDelegate
function VehicleAPI.RegisterBulletFiredEvent(vehicle, index, tankFiredDelegate) end
---注销载具开火时触发的事件。
---UnRegister an event dispatched when vehicle fired.
---@static
---@function [VehicleAPI.UnRegisterBulletFiredEvent]
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
---@param index System.Int32
---@param tankFiredDelegate ShanghaiWindy.Core.Delegate.OnFiredDelegate
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
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
function VehicleAPI.IsTankVehicle(vehicle) end
---判断载具是否为飞行器类型
---@static
---@function [VehicleAPI.IsFlightVehicle]
---@return System.Boolean 返回载具是否为飞行器类型 true if FlightVehicle, false otherwise
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
function VehicleAPI.IsFlightVehicle(vehicle) end
---判断载具是否为陆军类型
---@static
---@function [VehicleAPI.IsArmyVehicle]
---@return System.Boolean 返回载具是否为陆军类型 true if ArmyVehicle, false otherwise
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
function VehicleAPI.IsArmyVehicle(vehicle) end
---@static
---@function [VehicleAPI.TryGetBaseInitSystemFromGameObject]
---@return System.Boolean
---@param gameObject UnityEngine.GameObject
---@param baseInitSystem ShanghaiWindy.Core.BaseInitSystem&
function VehicleAPI.TryGetBaseInitSystemFromGameObject(gameObject, baseInitSystem) end
---@static
---@function [VehicleAPI.TryGetTankInitSystemFromGameObject]
---@return System.Boolean
---@param gameObject UnityEngine.GameObject
---@param tankInitSystem ShanghaiWindy.Core.TankInitSystem&
function VehicleAPI.TryGetTankInitSystemFromGameObject(gameObject, tankInitSystem) end
---@static
---@function [VehicleAPI.TryGetFlightInitSystemFromGameObject]
---@return System.Boolean
---@param gameObject UnityEngine.GameObject
---@param flightInitSystem ShanghaiWindy.Core.FlightInitSystem&
function VehicleAPI.TryGetFlightInitSystemFromGameObject(gameObject, flightInitSystem) end
---@static
---@function [VehicleAPI.TryGetArmyInitSystemFromGameObject]
---@return System.Boolean
---@param gameObject UnityEngine.GameObject
---@param armyInitSystem ShanghaiWindy.Core.FPS.ArmyInitSystem&
function VehicleAPI.TryGetArmyInitSystemFromGameObject(gameObject, armyInitSystem) end
---从载具中获取所有火力系统。
---Get all the fire system from vehicle.
---@static
---@function [VehicleAPI.GetFireList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.BaseFireSystem, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 火力系统数组 Array of fire system
---@param vehicle ShanghaiWindy.Core.BaseInitSystem
function VehicleAPI.GetFireList(vehicle) end
---获取载具信息
---Get Vehicle Info
---@static
---@function [VehicleAPI.GetVehicleInfoByName]
---@return ShanghaiWindy.Core.VehicleInfo 载具信息 Vehicle Info
---@param vehicleName System.String
function VehicleAPI.GetVehicleInfoByName(vehicleName) end
---获取载具信息
---Get vehicle info guid
---@static
---@function [VehicleAPI.GetVehicleInfoByGuid]
---@return ShanghaiWindy.Core.VehicleInfo
---@param guid System.String
function VehicleAPI.GetVehicleInfoByGuid(guid) end
---获取所有可驾驶载具列表
---Get All Driveable Vehicle List
---@static
---@function [VehicleAPI.GetAllDriveableVehicleList]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.VehicleInfo, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 可驾驶载具列表 Driveable Vehicle List
---@param ignoreModule System.Boolean
function VehicleAPI.GetAllDriveableVehicleList(ignoreModule) end
---过滤载具
---Filter Vehicle
---@static
---@function [VehicleAPI.GetFilteredVehicles]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.VehicleInfo, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] 载具列表 Vehicle List
---@param minRank System.Int32
---@param maxRank System.Int32
function VehicleAPI.GetFilteredVehicles(minRank, maxRank) end
---@static
---@function [VehicleAPI.GetFilteredBotVehicles]
---@return System.Collections.Generic.List`1[[ShanghaiWindy.Core.VehicleInfo, Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
---@param minRank System.Int32
---@param maxRank System.Int32
---@param allowArtillery System.Boolean
---@param vehicleType ShanghaiWindy.Core.VehicleInfo+Type
function VehicleAPI.GetFilteredBotVehicles(minRank, maxRank, allowArtillery, vehicleType) end
---加载缩略图
---@static
---@function [VehicleAPI.LoadVehicleThumbnail]
---@param vehicleInfo ShanghaiWindy.Core.VehicleInfo
---@param callback ShanghaiWindy.Core.Delegate.OnVehicleThumbnailLoadedDelegate
function VehicleAPI.LoadVehicleThumbnail(vehicleInfo, callback) end
return VehicleAPI

