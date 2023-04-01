local PlayAnimationComponent = class("PlayAnimationComponent")

Behavior()

Property = {

}

function PlayAnimationComponent:OnStarted()
    local animation = self.script:GetNativeComponent("Animation")
    local ret, vehicle = self.script:TryGetTankInitSystem()
    if ret then
        VehicleAPI.RegisterVehicleLoadedEvent(vehicle, function()
            VehicleAPI.RegisterBulletFiredEvent(vehicle, 0, function()
                local isPlayed = animation:Play()
                print("Animation isPlayed" .. tostring(isPlayed))
            end)
        end)
    end
end

return PlayAnimationComponent
