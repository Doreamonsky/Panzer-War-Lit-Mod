local RotateComponent = class("RotateComponent")

Behavior()

Property = {
    rotateSpeed = {
        type = "number",
        value = 15
    },
}


function RotateComponent:OnUpdated()
    print(self.rotateSpeed)
    local r = Vector3.up * Time.deltaTime * self.rotateSpeed
    self.transform:Rotate(r)
end

return RotateComponent