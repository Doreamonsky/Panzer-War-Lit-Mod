---@class CSharpAPI
---@field isLocalPlayer fun():boolean 是否为本地玩家
---@field GetPlayer fun():BaseInitSystem 获得当前玩家
---@field GetMapData fun():MapData 获取当前地图
---@field isOfffline fun():boolean 是否单机
---@field LoadAssetBundle fun(abName:string,abVariant:string,callBack:function) 读取AssetBundle资源
---@field GetBotVehicleList fun(availableVehicleList:List<VehicleInfo>,maxRandomPoolCount:int) 获取 bot 可用载具列表
---@field RandomVehicleFromList fun(vehicleList:List<VehicleInfo>):VehicleInfo 从 vehicle 数组随机获取一个非火炮的载具
---@field CreateTankPlayer fun(vehicleName:string,pos:Vector3,rot:Quaternion):TankInitSystem 创建玩家坦克
---@field CreateFlightPlayer fun(vehicleName:string,pos:Vector3,rot:Quaternion,isSpawnOnAirPort:boolean):FlightInitSystem 创建玩家坦克
---@field CreateTankBot fun(vehicleName:string,pos:Vector3,rot:Quaternion,botTeam:TeamManager.Team):TankInitSystem 创建人机坦克\
---@field RequestScene fun(sceneName:string,onLoaded:System.Action):void 异步加载场景
---@field CreateDIYVehicle fun(userDefined:DIYUserDefined,onLoaded:System.Action<GameObject, VehicleTextData>):void 异步加载DIY载具
---@field GetGUID fun():string 创建 GUID
---@field SetRuleAsMain fun(userDefined:DIYUserDefined,mainRuleGuid:string) 设置 Rule 为 Main
---@field ExportShareCode fun(userDefined:DIYUserDefined,onComplete:System.Action<string>)
---@field ImportShareCode fun(shareId:string,onComplete:System.Action<DIYUserDefined>)
---@field OnEquipDetailClicked UnityEvent
---@field OnEquipInstallClicked UnityEvent
---@field OnEquipUninstallClicked UnityEvent
---@field OnLuaExitModeReq UnityEvent
CSharpAPI = CS.ShanghaiWindy.Core.LuaCallCSharpHelper

--- @class VehicleInfo
--- @field GetDisplayName fun():string 载具显示名称
--- @field rank int 等级
VehicleInfo = CS.ShanghaiWindy.Core.VehicleInfo

AssetLoader = CS.ShanghaiWindy.Core.AssetLoader
AssetBundleManager = CS.ShanghaiWindy.Core.AssetBundleManager
MapDataManager = CS.ShanghaiWindy.Core.MapDataManager

RenderPiplineType = CS.ShanghaiWindy.Core.VehicleInfo.RenderPiplineType
--- @class GameDataManager
--- @field PlayerTeam TeamManager.Team 玩家队伍
GameDataManager = CS.ShanghaiWindy.Core.GameDataManager

CommonDataManager = CS.ShanghaiWindy.Core.CommonDataManager
UserDIYDataManager = CS.ShanghaiWindy.Core.UserDIYDataManager

GameEventManager = CS.ShanghaiWindy.Core.GameEventManager

--- @field Team Team
TeamManager = CS.ShanghaiWindy.Core.TeamManager

VehicleInfoManager = CS.ShanghaiWindy.Core.VehicleInfoManager
AchievementManager = CS.ShanghaiWindy.Core.AchievementManager
MaskTextureManager = CS.ShanghaiWindy.Core.MaskTextureManager
PopMessageManager = CS.ShanghaiWindy.Core.PopMessageManager
DIYDataManager = CS.ShanghaiWindy.Core.DIYDataManager

URPMainUIManager = CS.ShanghaiWindy.URPCore.URPMainUIManager
URPCustomModeOfflineManager = CS.ShanghaiWindy.URPCore.URPCustomModeOfflineManager

Core = CS.ShanghaiWindy.Core
MouseLockModule = Core.MouseLockModule

--- @class DIYUserDefined
--- @field rules List<DIYRule>
--- @field overrideRank number
--- @field perferredRank number
--- @field definedName string
--- @field isApplyParentScale boolean
--- @field GetDeepCopied fun():DIYUserDefined 深拷贝自己
DIYUserDefined = CS.ShanghaiWindy.Core.Data.DIYUserDefined

--- @class DIYRule
--- @field ruleGuid string
--- @field itemGuid string
--- @field isMain boolean
--- @field parentRuleGuid string
--- @field targetSlotIndex number
--- @field scaleSize SerializeVector3
--- @field deltaPos SerializeVector3
--- @field localEulerAngles SerializeVector3
--- @field customPropertiesJson string
DIYRule = CS.ShanghaiWindy.Core.Data.DIYUserDefined.Rule

DIYDataEnum = CS.ShanghaiWindy.Core.Data.DIYDataEnum
SerializeVector3 = CS.ShanghaiWindy.Core.Data.SerializeVector3
uGUI_Localization = CS.ShanghaiWindy.Core.uGUI_Localization

OutlineHelper = CS.OutlineHelper
HitBox = CS.ShanghaiWindy.Core.HitBox
InternalModule = CS.ShanghaiWindy.Core.InternalModule

--- @class uGUI_Localsize
--- @field GetContent fun(_Key:string):void
uGUI_Localsize = CS.ShanghaiWindy.Core.uGUI_Localsize

--- @class GameObjectPool
--- @field Init fun(prototype:GameObject,count:int):void
--- @field InstantiateObject fun():GameObject
--- @field DestroyObject fun(o:GameObject):void
--- @field Dispose fun():void
GameObjectPool = CS.ShanghaiWindy.Core.Utils.GameObjectPool

-- UnityEngine
--- @class GameObject
--- @field Instantiate fun() 实例化
--- @field Destroy fun()
GameObject = CS.UnityEngine.GameObject
AudioListener = CS.UnityEngine.AudioListener
Vector2 = CS.UnityEngine.Vector2
Vector3 = CS.UnityEngine.Vector3
Color = CS.UnityEngine.Color

--- @class JsonUtility
--- @field ToJson fun(object:obj) Generate a JSON representation of the public fields of an object.
--- @field FromJsonOverwrite fun(json:string,overrideTarget:object)
JsonUtility = CS.UnityEngine.JsonUtility

-- UGUI
Text = CS.UnityEngine.UI.Text
Image = CS.UnityEngine.UI.Image
Canvas = CS.UnityEngine.Canvas

-- Web
WWWForm = CS.UnityEngine.WWWForm
UnityWebRequest = CS.UnityEngine.Networking.UnityWebRequest

-- Common
--- @class Application
--- @field lowMemory Action
Application = CS.UnityEngine.Application

Time = CS.UnityEngine.Time

MeshRenderer = CS.UnityEngine.MeshRenderer
