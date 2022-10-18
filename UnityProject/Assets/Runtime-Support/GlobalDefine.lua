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
---@field CreateTankBot fun(vehicleName:string,pos:Vector3,rot:Quaternion,botTeam:TeamManager.Team):TankInitSystem 创建人机坦克
---@field CreateFreeCamera fun(pos:Vector3,rot:Quaternion):FreeCameraInitSystem 创建自由摄像机
---@field RequestScene fun(sceneName:string,onLoaded:System.Action):void 异步加载场景
---@field RequestGarageScene fun():void 加载车库场景
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

DIYMapUserDefinedRule = CS.ShanghaiWindy.Core.Data.DIYMapUserDefined.Rule

--- @class UserDIYMapDataManager
--- @field Instance UserDIYMapDataManager
--- @field GetDIYUserDefines fun():List<DIYMapUserDefined>
UserDIYMapDataManager = CS.ShanghaiWindy.Core.UserDIYMapDataManager

--- @class DIYFileRecycleMgr
--- @field OnShareFile UnityEvent<string>
--- @field OnLoadFile UnityEvent<string>
--- @field OnPublishFile UnityEvent<string>
--- @field OnDeleteFile UnityEvent<string>
--- @field AddFileName fun(fileName:string):void
--- @field RemoveFileName fun(fileName:string):void
--- @field Refresh fun():void
--- @field Clean fun():void
DIYFileRecycleMgr = CS.ShanghaiWindy.Core.DIYFileRecycleMgr
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

DIYMapBaseReference = CS.ShanghaiWindy.Core.Data.DIYMapBaseReference

--- @class DIYMapItemComponent
--- @field transform Transfrom
--- @field GetData fun():DIYMapBaseData
--- @field GetJson fun():DIYMapItemComponentJson
DIYMapItemComponent = CS.ShanghaiWindy.Core.DIYMapItemComponent

--- @class DIYMapSerializationUtil
--- @field DeserializeToCurrentScene fun(userDefined:DIYMapUserDefined,isRuntimeMode:boolean):void
--- @field SerializeCurrentScene fun(definedName:string):DIYMapUserDefined
--- @field CleanScene fun():void
DIYMapSerializationUtil = CS.ShanghaiWindy.Core.DIYMapSerializationUtil

--- @class DIYMapCreateUtil
--- @field AutoPlaceItem fun(itemGuid:string,cameraPos:Vector3,cameraDir:Vector3):void
DIYMapCreateUtil = CS.ShanghaiWindy.Core.DIYMapCreateUtil

--- @class DIYMapBaseData
--- @field itemGUID string
DIYMapBaseData = CS.ShanghaiWindy.Core.DIYMapBaseData

SerializeVector3 = CS.ShanghaiWindy.Core.Data.SerializeVector3
uGUI_Localization = CS.ShanghaiWindy.Core.uGUI_Localization

OutlineHelper = CS.OutlineHelper
HitBox = CS.ShanghaiWindy.Core.HitBox
InternalModule = CS.ShanghaiWindy.Core.InternalModule

--- @class uGUI_Localsize
--- @field GetContent fun(_Key:string):void
uGUI_Localsize = CS.ShanghaiWindy.Core.uGUI_Localsize

eDIYControlType = CS.ShanghaiWindy.Core.eDIYControlType

--- @class GameObjectPool
--- @field Init fun(prototype:GameObject,count:int):void
--- @field InstantiateObject fun():GameObject
--- @field DestroyObject fun(o:GameObject):void
--- @field Dispose fun():void
GameObjectPool = CS.ShanghaiWindy.Core.Utils.GameObjectPool

--- @class RuntimeHandlesComponent
--- @field HandleScale float
RuntimeHandlesComponent = CS.Battlehub.RTHandles.RuntimeHandlesComponent

--- @class FreeCamera
--- @field CreateFreeCamera fun():void 创建摄像机
FreeCamera = CS.ShanghaiWindy.Core.FreeCamera

Joystick = CS.ShanghaiWindy.Core.Joystick

--- @class DIYHandleManager
--- @field Instance DIYHandleManager
--- @field isDragging boolean
DIYHandleManager = CS.ShanghaiWindy.Core.DIYHandleManager

--- @class DIYMapItemComponentDragPicker
--- @field Instance DIYMapItemComponentDragPicker
--- @field GetRayItem fun(screenPoint:Vector2)
DIYMapItemComponentDragPicker = CS.ShanghaiWindy.Core.DIYMapItemComponentDragPicker

--- @class DIYDragPicker
--- @field Instance DIYDragPicker
--- @field GetRayItem fun(screenPoint:Vector2)
DIYDragPicker = CS.ShanghaiWindy.Core.DIYDragPicker

--- @class DIYDataPackageManager
--- @field Instance DIYDataPackageManager
--- @field ExportUserDefine fun(userDefined:DIYUserDefined)
--- @field ExportMapUserDefine fun(userDefined:DIYMapUserDefined)
DIYDataPackageManager = CS.ShanghaiWindy.Core.DIYDataPackageManager

--- @class RuntimeInspector
--- @field Inspect fun(obj:object):void
RuntimeInspector = CS.RuntimeInspectorNamespace.RuntimeInspector

--- @class UIEventUtil
UIEventUtil = CS.ShanghaiWindy.Core.UIEventUtil

PositionHandle = CS.Battlehub.RTHandles.PositionHandle
RotationHandle = CS.Battlehub.RTHandles.RotationHandle
ScaleHandle = CS.Battlehub.RTHandles.ScaleHandle

--- @class UIManager
--- @field Instance UIManager
--- @field ShowUI fun(uiEnum:UIEnum,cb:function(ctrl))
UIManager = CS.ShanghaiWindy.Core.UIManager
UIEnum = CS.ShanghaiWindy.Core.UIEnum

-- UnityEngine
--- @class GameObject
--- @field Instantiate fun() 实例化
--- @field Destroy fun()
GameObject = CS.UnityEngine.GameObject
AudioListener = CS.UnityEngine.AudioListener
Vector2 = CS.UnityEngine.Vector2
Vector3 = CS.UnityEngine.Vector3
Quaternion = CS.UnityEngine.Quaternion
Color = CS.UnityEngine.Color
Mathf = CS.UnityEngine.Mathf
Handheld = CS.UnityEngine.Handheld
Physics = CS.UnityEngine.Physics

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
LogType = CS.UnityEngine.LogType

Time = CS.UnityEngine.Time

MeshRenderer = CS.UnityEngine.MeshRenderer

Input = CS.UnityEngine.Input
KeyCode = CS.UnityEngine.KeyCode
Camera = CS.UnityEngine.Camera

--- @class EventTrigger
EventTrigger = CS.UnityEngine.EventSystems.EventTrigger
EventTriggerType = CS.UnityEngine.EventSystems.EventTriggerType

GUIUtility = CS.UnityEngine.GUIUtility
Screen = CS.UnityEngine.Screen
