local RotateComponent = class("RotateComponent")

Behavior()

Propery = {
    rotateSpeed = {
        type = "number",
        value = 15
    },
}


function RotateComponent:OnUpdated()
    local r = Vector3.up * Time.deltaTime * self.rotateSpeed
    self.transform:Rotate(r)
end

return RotateComponent
