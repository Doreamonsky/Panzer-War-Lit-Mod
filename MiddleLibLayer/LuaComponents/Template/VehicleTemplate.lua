Vehicle()
Property = {}
local VehicleTemplate = class("VehicleTemplate")

function VehicleTemplate:ctor()
    self.accelG = 0
    self.steerG = 0
    self.extraSteerG = 0
    self.reversing = false
    self.isEngineRunning = false
    self.canControl = false
    self.isTrackDamaged = false
    self.rigid = nil
    self.currentSpeed = 0
    self.hasNitrogen = false
    self.hasBrake = false
    self.rpm = 0
    self.gear = 0
    self.rpmPercentage = 0
    self.load = 0
    self.loadSwitch = 0
end

function VehicleTemplate:OnUpdated()
end

function VehicleTemplate:OnSpacePressChanged(isPressed)
end

function VehicleTemplate:OnNitrogenChanged(isPressed)
end

return VehicleTemplate
