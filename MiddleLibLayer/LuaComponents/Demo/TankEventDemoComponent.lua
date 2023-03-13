local TankEventDemoComponent = class("TankEventDemoComponent")

Propery = {

}

function TankEventDemoComponent:OnStarted()
    local ret, tank = self.script:GetTankInitSystem()

    if ret then
        tank.OnVehicleLoaded:AddListener(function()
            print("Vehicle is loaded")

            -- This is the time when vehicle is OnVehicleLoaded
            tank.vehicleComponents.mainTankFire.OnFired:AddListener(function()
                print("On Fired")
            end)
        end)
    else
        print("This is not a valid tank")
    end
end

function TankEventDemoComponent:OnUpdated()

end

function TankEventDemoComponent:OnDestroyed()

end

return TankEventDemoComponent
