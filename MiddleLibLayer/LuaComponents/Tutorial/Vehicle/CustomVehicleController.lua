Vehicle()
Property = {}
local CustomVehicleController = class("CustomVehicleController")

function CustomVehicleController:ctor()
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

    self.curSpeed = Vector3.zero
    self.curRot = Vector3.zero
end

function CustomVehicleController:OnStarted()
    self.rigid.useGravity = false
end

function CustomVehicleController:OnUpdated()
    self.curSpeed = Vector3.Lerp(self.curSpeed, Vector3.up * self.accelG * 10, Time.deltaTime)
    self.curRot = Vector3.Lerp(self.curRot, Vector3.up * self.steerG * 35, Time.deltaTime)

    self.transform:Translate(self.curSpeed * Time.deltaTime)
    self.transform:Rotate(self.curRot * Time.deltaTime)

    local stabilizeTorque = Vector3.Cross(self.transform.up, Vector3.up) * 5000;
    self.rigid:AddTorque(stabilizeTorque);
end

function CustomVehicleController:OnSpacePressChanged(isPressed)
end

function CustomVehicleController:OnNitrogenChanged(isPressed)
end

return CustomVehicleController
