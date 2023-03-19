Behavior()
Property = {
    printNumber = {
        type = "number",
        value = 123
    },
    spawnPoint = {
        type = "object",
        value = nil
    }
}
local HelloWorld = class("HelloWorld")

function HelloWorld:GetVehicle()
    local vehicleList = VehicleAPI.GetAllDriveableVehicleList(false)
    return vehicleList[0]
end

function HelloWorld:OnStarted()
    print("Hello World! 你好世界！")

    local pos = self.spawnPoint.transform.position -- 获取位置
    local rot = self.spawnPoint.transform.rotation -- 获取旋转

    local vehicle = self:GetVehicle()              -- 获取车辆
    SpawnVehicleAPI.CreatePlayer(vehicle, pos, rot, function(vehicle)
        print("on prebind")
    end)

    -- 创建一个本地的机器人驾驶车辆
    -- Create a local bot driving the vehicle
    -- SpawnVehicleAPI.CreateLocalBot(vehicle, pos, rot, TeamManager.Team.red, true, true)
end

function HelloWorld:OnUpdated()
    print(self.printNumber)
end

function HelloWorld:OnDestroyed()
    print("I am destroyed 我被销毁了")
end

return HelloWorld
