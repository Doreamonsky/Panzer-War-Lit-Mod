Behavior()
Property = {}
local GetVehicleHP = class("GetVehicleHP")

function GetVehicleHP:OnUpdated()
    local hp = self.vehicle.basePlayerState.hp:Get()
    print(hp)

    -- self.vehicle.basePlayerState.hp:Set(1500)
end

return GetVehicleHP
