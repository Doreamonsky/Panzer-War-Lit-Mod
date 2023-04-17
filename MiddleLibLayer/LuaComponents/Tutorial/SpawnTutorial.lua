Behavior()
Property = {}

-- 定义一个名为 SpawnTutorial 的类
-- Define a class called SpawnTutorial
local SpawnTutorial = class("SpawnTutorial")

-- 获取可驾驶的车辆
-- Get a drivable vehicle
function SpawnTutorial:GetVehicle()
    local vehicleList = VehicleAPI.GetAllDriveableVehicleList(false)
    return vehicleList[1]
end

-- 当 SpawnTutorial 开始时调用此函数
-- Call this function when SpawnTutorial starts
function SpawnTutorial:OnStarted()
    local pos = self.transform.position -- 获取位置
    local rot = self.transform.rotation -- 获取旋转

    local vehicle = self:GetVehicle() -- 获取车辆
    -- 创建一个本地的机器人驾驶车辆
    -- Create a local bot driving the vehicle
    SpawnVehicleAPI.CreateLocalBot(vehicle, pos, rot, TeamManager.Team.red, true, true)
end

-- 返回 SpawnTutorial 类
-- Return SpawnTutorial class
return SpawnTutorial

