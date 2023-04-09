local RotateComponent = class("RotateComponent")

Behavior()

Property = {
    rotateSpeed = {
        type = "number",
        value = 15
    },
}

function RotateComponent:OnStarted()
    self.r = Vector3.up * self.rotateSpeed
end

function RotateComponent:OnUpdated()
    -- print("Hello World!")
    -- print(self.rotateSpeed)
    self.transform:Rotate(self.r * Time.deltaTime)
end

return RotateComponent
