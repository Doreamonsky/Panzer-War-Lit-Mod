Behavior()
Property = {
    angle = {
    }
}
local T26E5 = class("T26E5")

function T26E5:OnStarted()
    local ret, vehicle = self.script:TryGetTankInitSystem()

    if ret then
        self.vehicle = vehicle
    end
end

function T26E5:OnUpdated()
    local forward = self.vehicle.vehicleComponents.mainTurretController.gunTransform.forward
    local detlaRot = Quaternion.Euler(90, 0, 0);
    self.transform.rotation = Quaternion.LookRotation(forward) * detlaRot
end

return T26E5
