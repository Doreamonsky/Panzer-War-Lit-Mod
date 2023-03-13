local PlayAnimationComponent = class("PlayAnimationComponent")

Propery = {

}

function PlayAnimationComponent:OnStarted()
    local animation = self.script:GetNativeComponent("Animation")
    local isPlayed = animation:Play()
    print("Animation isPlayed" .. tostring(isPlayed))
end

return PlayAnimationComponent
