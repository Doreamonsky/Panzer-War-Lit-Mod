-- This is the hello world of lua in Panzer War
local TestClass = class("TestClass")

Behavior()

Propery = {
    rotateSpeed = {
        type = "number",
        value = 10
    },
    sceneCamera = {
        type = "object",
        value = nil
    },
}


function TestClass:OnStarted()
    print("On Start")
    print(self.IsLocalPlayer)
    print(self.sceneCamera.name)
    print("rotate speed" .. tostring(self.rotateSpeed))
end

function TestClass:OnUpdated()
    local r = Vector3.up * Time.deltaTime * self.rotateSpeed
    self.transform:Rotate(r)
end

function TestClass:OnDestroyed()
    print("On Destroyed")
end

return TestClass
