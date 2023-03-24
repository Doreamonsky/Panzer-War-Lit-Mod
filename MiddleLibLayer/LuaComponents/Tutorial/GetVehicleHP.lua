Behavior()
Property = {}
local GetVehicleHP = class("GetVehicleHP")

function GetVehicleHP:OnStarted()
    self.OnTotalDamageChanged = function(oldVal, newVal)
        print(tostring(oldVal) .. " -> " .. tostring(newVal))
    end

    self.vehicle.basePlayerState.totalDamage:OnValueChanged("+", self.OnTotalDamageChanged)
end

function GetVehicleHP:OnUpdated()
    local hp = self.vehicle.basePlayerState.hp:Get()
    -- print(hp)

    local totalDamage = self.vehicle.basePlayerState.totalDamage:Get()
    -- print(totalDamage)
end

return GetVehicleHP
