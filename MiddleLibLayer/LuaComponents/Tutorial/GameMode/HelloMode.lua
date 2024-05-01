GameMode()
Property = {
    enemyBotSpawnInterval = {
        type = "number",
        value = 30
    }
}

local HelloMode = class("HelloMode")

function HelloMode:ctor()
    self.timer = 0
end

function HelloMode:GetGameModeName(lang)
    if lang == "CN" then
        return "Hello World 模式"
    else
        return "Hello World Mode"
    end
end

function HelloMode:IsProxyBattle()
    return true
end

function HelloMode:GetLocalVehicle()
    local vehicleList = VehicleAPI.GetAllDriveableVehicleList(false)
    return vehicleList[0]
end

function HelloMode:OnStartMode()
    self.timer = self.enemyBotSpawnInterval

    TeamAPI.SetPlayerTeamAsBlueTeam()
    local vehicle = self:GetLocalVehicle() -- 获取车辆
    ---@type ShanghaiWindy.Core.AbstractBattlePlayer
    local mainPlayer = BattlePlayerAPI.CreateOfflineMainPlayer(0, nil)
    ModeAPI.AddBattlePlayer(mainPlayer)
    SpawnAPI.AsyncSpawn(mainPlayer:GetTeam(), function(trans)
        mainPlayer:CreateVehicle(vehicle, trans.position, trans.rotation)
    end)
end

function HelloMode:SpawnEnemyVehicle()
    -- local vehicle = self:GetLocalVehicle() -- 获取车辆
    -- SpawnVehicleAPI.CreateLocalBot(vehicle, Vector3(10, 0, 0), Quaternion.identity, TeamAPI.GetEnemyTeam(), false, false)
end

function HelloMode:OnUpdated()
    if self.timer >= self.enemyBotSpawnInterval then
        self:SpawnEnemyVehicle()
        self.timer = 0
    else
        self.timer = self.timer + Time.deltaTime
    end
end

function HelloMode:OnExitMode()
end

return HelloMode
